using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private Rigidbody cameraTarget;
    [SerializeField] private CinemachineCamera cinemachineCamera;

    //Script for reference to camera configuration
    [SerializeField] private CameraConfig cameraConfig;

    private CinemachineFollow cinemachineFollow;
    private float zoomStartTime;
    private float rotationStartTime;
    private Vector3 startingFollowOffset;
    private float maxRotationAmount;

    private void Awake()
    {
       //cinemachineFollow = cinemachineCamera.GetComponent<CinemachineFollow>();
       if (!cinemachineCamera.TryGetComponent(out cinemachineFollow))
        {
            Debug.LogError("Cinemachine Camera did not have CinemachineFollow component. Zoom will not work!");
        }

        startingFollowOffset = cinemachineFollow.FollowOffset;
        maxRotationAmount = Mathf.Abs(cinemachineFollow.FollowOffset.z);
    }

    private void Update()
    {
        CameraMovement();
        CameraZoom();
        CameraRotation();
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
        int screenWidth = Screen.width; // 1920
        int screenHeight = Screen.height; // 1080

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
}
