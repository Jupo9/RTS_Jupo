using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private Rigidbody cameraTarget;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private new Camera camera;
    [SerializeField] private LayerMask selectableUnitsLayers;
    [SerializeField] private LayerMask floorLayers;
    [SerializeField] private RectTransform selectionBox;
    //Script for reference to camera configuration
    [SerializeField] private CameraConfig cameraConfig;

    private Vector2 startingMousePosition;

    private CinemachineFollow cinemachineFollow;
    private float zoomStartTime;
    private float rotationStartTime;
    private Vector3 startingFollowOffset;
    private float maxRotationAmount;
    private ISelectable selectedUnit;



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
    }

    private void OnDestroy()
    {
        Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
    }

    private void HandleUnitSelected(UnitSelectedEvent evt)
    {
        selectedUnit = evt.Unit;
    }

    private void HandleUnitDeselect(UnitDeselectEvent evt)
    {
        selectedUnit = null;
    }

    private void Update()
    {
        CameraMovement();
        CameraZoom();
        CameraRotation();
        HandleLeftClick();
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
        if (camera ==null)
        {
            return;
        }

        Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (selectedUnit != null)
            {
                selectedUnit.Deselect();
            }

            if (Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, selectableUnitsLayers)
            && hit.collider.TryGetComponent(out ISelectable selectable))
            {
                selectable.Select();
            }
        }
    }

    private void HandleRightClick()
    {
        if (selectedUnit == null || selectedUnit is not IMoveable moveable)
        {
            return;
        }

        Ray cameraRay = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Mouse.current.rightButton.wasReleasedThisFrame
            && Physics.Raycast(cameraRay, out RaycastHit hit, float.MaxValue, floorLayers))
        {
            moveable.MoveTo(hit.point);
        }
    }

    private void HandleDragSelect()
    {
        if (selectionBox == null)
        {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            selectionBox.gameObject.SetActive(true);                    // enable selection box ui
            startingMousePosition = Mouse.current.position.ReadValue(); // store start position
        }
        else if (Mouse.current.leftButton.isPressed
            && !Mouse.current.leftButton.wasPressedThisFrame)
        {
            ResizeSelectionBox();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            // Select units within box
            // deselect selected units if not in box
            selectionBox.gameObject.SetActive(false);   // disable the ui
        }
    }

    private void ResizeSelectionBox()
    {
        // resize selection box

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        float width = mousePosition.x - startingMousePosition.x;
        float height = mousePosition.y - startingMousePosition.y;

        selectionBox.anchoredPosition = startingMousePosition + new Vector2(width / 2, height / 2);
        selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
    }
}
