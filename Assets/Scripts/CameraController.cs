using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour {
    [SerializeField] private Character targetCharacter;
    [SerializeField] private float targetDistance = 5.0f;
    [SerializeField] private float rotationSpeed = 5.0f;
    [SerializeField] private float rotationLerpFactor = 0.2f;

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

        transform.rotation = Quaternion.Euler(currentPitch, currentYaw, 0);

        transform.position = targetCharacter.transform.position - transform.forward * targetDistance;

        targetCharacter.globalHeading = currentYaw;
    }
}
