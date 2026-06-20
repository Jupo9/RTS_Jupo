using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private Rigidbody cameraTarget;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private new Camera camera;
    [SerializeField] private LayerMask selectableUnitsLayers;
    [SerializeField] private LayerMask interactableLayers;
    [SerializeField] private LayerMask floorLayers;
    [SerializeField] private RectTransform selectionBox;
    //Script for reference to camera configuration
    [SerializeField] private CameraConfig cameraConfig;

    private Vector2 startingMousePosition;

    private BaseAction activeAction;
    private GameObject ghostInstance;
    private bool wasMouseDownOnUI;
    private CinemachineFollow cinemachineFollow;
    private float zoomStartTime;
    private float rotationStartTime;
    private Vector3 startingFollowOffset;
    private float maxRotationAmount;
    private HashSet<AbstractUnit> aliveUnits = new(100);
    private HashSet<AbstractUnit> addedUnits = new(24);
    private List<ISelectable> selectedUnits = new(12);



    private void Awake()
    {
       //cinemachineFollow = cinemachineCamera.GetComponent<CinemachineFollow>();
       if (!cinemachineCamera.TryGetComponent(out cinemachineFollow))
        {
            Debug.LogError("Cinemachine Camera did not have CinemachineFollow component. Zoom will not work!");
        }

        startingFollowOffset = cinemachineFollow.FollowOffset;
        maxRotationAmount = Mathf.Abs(cinemachineFollow.FollowOffset.z);

        Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
        Bus<UnitDeselectEvent>.OnEvent += HandleUnitDeselect;
        Bus<UnitSpawnEvent>.OnEvent += HandleUnitSpawn;
        Bus<ActionSelectedEvent>.OnEvent += HandleActionSelected;
        Bus<UnitDeathEvent>.OnEvent += HandleUnitDeath;
    }

    private void OnDestroy()
    {
        Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
        Bus<UnitDeselectEvent>.OnEvent -= HandleUnitDeselect;
        Bus<UnitSpawnEvent>.OnEvent -= HandleUnitSpawn;
        Bus<ActionSelectedEvent>.OnEvent -= HandleActionSelected;
        Bus<UnitDeathEvent>.OnEvent += HandleUnitDeath;
    }

    /* alternative version:
    private void HandleUnitSelected(UnitSelectedEvent evt)
    {
        selectedUnits.Add(evt.Unit);
    }
    */
    private void HandleUnitSelected(UnitSelectedEvent evt)
    {
        if (!selectedUnits.Contains(evt.Unit))
        { 
            selectedUnits.Add(evt.Unit);
        }
    }
    private void HandleUnitDeselect(UnitDeselectEvent evt) => selectedUnits.Remove(evt.Unit);
    private void HandleUnitSpawn(UnitSpawnEvent evt) => aliveUnits.Add(evt.Unit);
    private void HandleActionSelected(ActionSelectedEvent evt)
    {
        activeAction = evt.Action;
        if (!activeAction.RequiresClickToActive)
        {
            // immediately handle the action
            ActivateAction(new RaycastHit());
        }
        else if (activeAction.GhostPrefab != null)
        {
            ghostInstance = Instantiate(activeAction.GhostPrefab);
        }
    }

    private void HandleUnitDeath(UnitDeathEvent evt)
    {
        selectedUnits.Remove(evt.Unit);
        aliveUnits.Remove(evt.Unit);
    }

    private void Update()
    {
        CameraMovement();
        CameraZoom();
        CameraRotation();
        HandleGhost();
        HandleRightClick();
        HandleDragSelect();
    }

    private void CameraMovement()
    {
        Vector2 moveAmount = GetKeyboardMoveAround();
        moveAmount += GetMouseMoveAmount();

        cameraTarget.linearVelocity = new Vector3(moveAmount.x, 0, moveAmount.y);
    }

    private Vector2 GetKeyboardMoveAround()
    {
        Vector2 moveAmount = Vector2.zero;

        if (Keyboard.current.upArrowKey.isPressed
            || Keyboard.current.wKey.isPressed)
        {
            moveAmount.y += cameraConfig.KeyboardPanSpeed;
        }
        if (Keyboard.current.leftArrowKey.isPressed
            || Keyboard.current.aKey.isPressed)
        {
            moveAmount.x -= cameraConfig.KeyboardPanSpeed;
        }
        if (Keyboard.current.downArrowKey.isPressed
            || Keyboard.current.sKey.isPressed)
        {
            moveAmount.y -= cameraConfig.KeyboardPanSpeed;
        }
        if (Keyboard.current.rightArrowKey.isPressed
            || Keyboard.current.dKey.isPressed)
        {
            moveAmount.x += cameraConfig.KeyboardPanSpeed;
        }

        return moveAmount;
    }

    private Vector2 GetMouseMoveAmount()
    {
        Vector2 moveAmount = Vector2.zero;

        if (!cameraConfig.EnableEdgePan)
        {
            return moveAmount;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        int screenWidth = Screen.width;     // 1920
        int screenHeight = Screen.height;   // 1080

        if (mousePosition.x <= cameraConfig.EdgePanSize)
        {
            moveAmount.x -= cameraConfig.MousePanSpeed;
        }
        else if (mousePosition.x >= screenWidth - cameraConfig.EdgePanSize)
        {
            moveAmount.x += cameraConfig.MousePanSpeed;
        }

        if (mousePosition.y >= screenHeight - cameraConfig.EdgePanSize)
        {
            moveAmount.y += cameraConfig.MousePanSpeed;
        }
        else if (mousePosition.y <= cameraConfig.EdgePanSize)
        {
            moveAmount.y -= cameraConfig.MousePanSpeed;
        }

        return moveAmount;
    }

    private void CameraZoom()
    {
        if (ShouldZoomTimeStart())
        {
            zoomStartTime = Time.time;
        }

        float zoomTime = Mathf.Clamp01((Time.time - zoomStartTime) * cameraConfig.ZoomSpeed);
        Vector3 targetFollowOffset;

        if (Keyboard.current.endKey.isPressed)
        {
            targetFollowOffset = new Vector3(cinemachineFollow.FollowOffset.x, cameraConfig.MinZoomDistance, cinemachineFollow.FollowOffset.z);
        }
        else
        {
            targetFollowOffset = new Vector3(cinemachineFollow.FollowOffset.x, startingFollowOffset.y, cinemachineFollow.FollowOffset.z);
            cinemachineFollow.FollowOffset = Vector3.Slerp(targetFollowOffset, startingFollowOffset, zoomTime);
        }

        cinemachineFollow.FollowOffset = Vector3.Slerp(cinemachineFollow.FollowOffset, targetFollowOffset, zoomTime);
    }

    private bool ShouldZoomTimeStart()
    {
        return Keyboard.current.endKey.wasPressedThisFrame 
            || Keyboard.current.endKey.wasReleasedThisFrame;
    }

    private void CameraRotation()
    {
        if (ShouldRotationTimeStart())
        {
            rotationStartTime = Time.time;
        }

        float rotationTime = Mathf.Clamp01((Time.time - rotationStartTime) * cameraConfig.RotationSpeed);

        Vector3 targetFollowOffset;

        if (Keyboard.current.pageDownKey.isPressed
            || Keyboard.current.qKey.isPressed)
        {
            targetFollowOffset = new Vector3(maxRotationAmount, cinemachineFollow.FollowOffset.y, 0);
        }
        else if (Keyboard.current.pageUpKey.isPressed
            || Keyboard.current.rKey.isPressed)
        {
            targetFollowOffset = new Vector3(-maxRotationAmount, cinemachineFollow.FollowOffset.y, 0);
        }
        else
        {
            targetFollowOffset = new Vector3(startingFollowOffset.x, cinemachineFollow.FollowOffset.y, startingFollowOffset.z);
        }

            cinemachineFollow.FollowOffset = Vector3.Slerp(cinemachineFollow.FollowOffset, targetFollowOffset, rotationTime);
    }

    private bool ShouldRotationTimeStart()
    {
        return Keyboard.current.pageUpKey.wasPressedThisFrame 
            || Keyboard.current.pageDownKey.wasPressedThisFrame 
            || Keyboard.current.pageUpKey.wasReleasedThisFrame 
            || Keyboard.current.pageDownKey.wasReleasedThisFrame
            || Keyboard.current.rKey.wasPressedThisFrame
            || Keyboard.current.qKey.wasPressedThisFrame
            || Keyboard.current.rKey.wasReleasedThisFrame
            || Keyboard.current.qKey.wasReleasedThisFrame;
    }

    private void HandleLeftClick()
    {
        if (camera == null)
        {
            return;
        }

        Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (activeAction == null
            && Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, selectableUnitsLayers)
            && hit.collider.TryGetComponent(out ISelectable selectable))
        {
            selectable.Select();
        }
        else if (activeAction != null
            && !EventSystem.current.IsPointerOverGameObject()
            && Physics.Raycast(cameraRay, out hit, float.MaxValue, interactableLayers | floorLayers))
        {
            ActivateAction(hit);
        }
    }

    private void ActivateAction(RaycastHit hit)
    {
        if (ghostInstance != null)
        {
            Destroy(ghostInstance);
            ghostInstance = null;
        }

        List<AbstractCommandable> abstractCommandables = selectedUnits
            .Where((unit) => unit is AbstractCommandable)
            .Cast<AbstractCommandable>()
            .ToList();

        for (int i = 0; i < abstractCommandables.Count; i++)
        {
            CommandContext context = new(abstractCommandables[i], hit, i);
            activeAction.Handle(context);
        }

        activeAction = null;
    }

    private void HandleGhost()
    {
        if (ghostInstance == null)
        {
            return;
        }

        if (Keyboard.current.escapeKey.wasReleasedThisFrame)
        {
            Destroy(ghostInstance);
            ghostInstance = null;
            activeAction = null;
            return;
        }

        Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, floorLayers))
        {
            ghostInstance.transform.position = hit.point;
        }

    }

    private void HandleRightClick()
    {
        if (selectedUnits.Count == 0)
        {
            return;
        }

        Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Mouse.current.rightButton.wasReleasedThisFrame
            && Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, interactableLayers | floorLayers))
        {
            List<AbstractUnit> abstractUnits = new List<AbstractUnit>(selectedUnits.Count);

            foreach (ISelectable selectable in selectedUnits)
            {
                if (selectable is AbstractUnit unit)
                { 
                    abstractUnits.Add(unit);
                }
            }

            for (int i = 0; i < abstractUnits.Count; i++)
            {
                CommandContext context = new(abstractUnits[i], hit, i);

                foreach (ICommand command in GetAvailableCommands(abstractUnits[i]))
                {
                    if (command.CanHandle(context))
                    {
                        command.Handle(context);
                        break;
                    }
                }
            }
        }
    }


    private List<BaseAction> GetAvailableCommands(AbstractUnit unit)
    {
        OverrideCommandsCommand[] overrideCommandsCommands = unit.AvailableCommands
            .Where(command => command is OverrideCommandsCommand)
            .Cast<OverrideCommandsCommand>()
            .ToArray();

        List<BaseAction> allAvailableCommands = new();
        foreach (OverrideCommandsCommand overrideCommand in overrideCommandsCommands)
        {
            allAvailableCommands.AddRange(overrideCommand.Commands
                .Where(command => command is not OverrideCommandsCommand)
            );
        }

        allAvailableCommands.AddRange(unit.AvailableCommands
            .Where(command => command is not OverrideCommandsCommand)
        );

        return allAvailableCommands;
    }

    private void HandleDragSelect()
    {
        if (selectionBox == null)
        {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleMouseDown();
        }
        else if (Mouse.current.leftButton.isPressed
            && !Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleMouseDrag();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            HandleMouseUp();
        }
    }

    private void HandleMouseDown()
    {
        selectionBox.sizeDelta = Vector2.zero;                              // reset size
        selectionBox.gameObject.SetActive(true);                            // enable selection box ui
        startingMousePosition = Mouse.current.position.ReadValue();         // store start position
        addedUnits.Clear();                                                 // clear added units
        wasMouseDownOnUI = EventSystem.current.IsPointerOverGameObject();   // check if mouse was pressed on UI
    }

    private void HandleMouseDrag()
    {
        if (activeAction != null || wasMouseDownOnUI)
        {
            return;
        }

        Bounds selectionBoxBounds = ResizeSelectionBox();

        foreach (AbstractUnit unit in aliveUnits)
        {
            Vector2 unitPosition = camera.WorldToScreenPoint(unit.transform.position);

            if (selectionBoxBounds.Contains(unitPosition))
            {
                //select this unit when the mouse is released
                addedUnits.Add(unit);
            }
        }
    }

    private void HandleMouseUp()
    {
        if (!wasMouseDownOnUI && activeAction == null && !Keyboard.current.shiftKey.isPressed)
        {
            DeselectAllUnits();
        }
        HandleLeftClick();
        // Select units within box
        foreach (AbstractUnit unit in addedUnits)
        {
            unit.Select();
        }

        selectionBox.gameObject.SetActive(false);   // disable the ui
    }

    private void DeselectAllUnits()
    {
        // deselect selected units if not in box
        ISelectable[] currentlySelectedUnits = selectedUnits.ToArray();
        foreach (ISelectable selectable in currentlySelectedUnits)
        {
            selectable.Deselect();
        }
    }


    private Bounds ResizeSelectionBox()
    {
        // resize selection box

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        float width = mousePosition.x - startingMousePosition.x;
        float height = mousePosition.y - startingMousePosition.y;

        selectionBox.anchoredPosition = startingMousePosition + new Vector2(width / 2, height / 2);
        selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));

        return new Bounds(selectionBox.anchoredPosition, selectionBox.sizeDelta);
    }
}
