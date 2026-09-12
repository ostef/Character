using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour {
    [SerializeField] private Character targetCharacter;
    [SerializeField] private Vector2 targetOffset = new Vector3(1.0f, 1.0f);
    [SerializeField] private float targetDistance = 5.0f;
    [SerializeField] private float rotationSpeed = 5.0f;
    [SerializeField] private float rotationLerpFactor = 0.2f;
    [SerializeField] private float wallPenetrationThreshold = 0.1f;
    [SerializeField, Range(0.0f, 90.0f)]
    private float fovMaxZoomWhenCollide = 10.0f;

    [Header("Input")]
    [SerializeField] private InputActionReference lookAction;

    [Header("Internal State")]
    [SerializeField, SerializeReadOnly] private float targetYaw;
    [SerializeField, SerializeReadOnly] private float currentYaw;
    [SerializeField, SerializeReadOnly] private float targetPitch;
    [SerializeField, SerializeReadOnly] private float currentPitch;
    private float baseFov;
    private Camera camera;

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        camera = GetComponent<Camera>();
        baseFov = camera.fieldOfView;
    }

    void LateUpdate() {
        var lookInput = lookAction.action.ReadValue<Vector2>();

        targetPitch -= lookInput.y * rotationSpeed;
        targetPitch = Mathf.Clamp(targetPitch, -90.0f, 90.0f);
        currentPitch = Mathf.LerpAngle(currentPitch, targetPitch, rotationLerpFactor);

        targetYaw += lookInput.x * rotationSpeed;
        targetYaw %= 360.0f;
        currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, rotationLerpFactor);

        targetCharacter.globalHeading = currentYaw;

        transform.rotation = Quaternion.Euler(currentPitch, currentYaw, 0);

        // Check if a wall is between the camera and the player
        var lookAtPosition = targetCharacter.transform.position
            + transform.right * targetOffset.x
            + transform.up * targetOffset.y;

        var targetPosition = lookAtPosition - transform.forward * targetDistance;

        RaycastHit raycastResult;
        if (Physics.Raycast(lookAtPosition, -transform.forward, out raycastResult, targetDistance - wallPenetrationThreshold)) {
            targetPosition = raycastResult.point + transform.forward * wallPenetrationThreshold;

            // Cool effect: make the FOV zoom when colliding
            var distanceT = Vector3.Distance(lookAtPosition, targetPosition) / targetDistance;
            distanceT = Mathf.Clamp01(1 - distanceT);
            camera.fieldOfView = Mathf.Lerp(baseFov, baseFov - fovMaxZoomWhenCollide, distanceT);
        }

        // Don't interpolate, this causes hiccups because of fluctuations in deltaTime
        transform.position = targetPosition;
    }
}
