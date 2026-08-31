using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour {
    [SerializeField] private Character targetCharacter;
    [SerializeField] private Vector2 targetOffset = new Vector3(1.0f, 1.0f);
    [SerializeField] private float targetDistance = 5.0f;
    [SerializeField] private float rotationSpeed = 5.0f;
    [SerializeField] private float rotationLerpFactor = 0.2f;
    [SerializeField] private float wallPenetrationThreshold = 0.1f;

    [Header("Input")]
    [SerializeField] private InputActionReference lookAction;

    [Header("Internal State")]
    [SerializeField, SerializeReadOnly] private float targetYaw;
    [SerializeField, SerializeReadOnly] private float currentYaw;
    [SerializeField, SerializeReadOnly] private float targetPitch;
    [SerializeField, SerializeReadOnly] private float currentPitch;

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate() {
        var lookInput = lookAction.action.ReadValue<Vector2>();

        targetPitch -= lookInput.y * rotationSpeed;
        targetPitch = Mathf.Clamp(targetPitch, -90.0f, 90.0f);
        currentPitch = Mathf.LerpAngle(currentPitch, targetPitch, rotationLerpFactor);

        targetYaw += lookInput.x * rotationSpeed;
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
        }

        // Don't interpolate, this causes hiccups because of fluctuations in deltaTime
        transform.position = targetPosition;
    }
}
