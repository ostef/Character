using UnityEngine;
using UnityEngine.InputSystem;

public class Character : MonoBehaviour {
    public enum MovementMode {
        RotateTowardsMovement,
        LookTowardsCamera,
    }

    public enum MovementStance {
        Standing,
        Crouching,
    }

    public enum MovementGait {
        Walk,
        Run,
        Sprint,

        Count,
    }

    [System.Serializable]
    public struct MovementGaitInfo {
        public float baseSpeed;
        [Range(0.001f, 1.0f)]
        public float headingLerpFactor;
        public float turnRate;
        [Range(0.0f, 180.0f)]
        public float sharpTurnThreshold;
    }

    [System.Serializable]
    public struct MovementGaitInfos {
        public MovementGaitInfo walk;
        public MovementGaitInfo run;
        public MovementGaitInfo sprint;

        public MovementGaitInfo Get(MovementGait gait) {
            switch (gait) {
            case MovementGait.Walk: return walk;
            case MovementGait.Run: return run;
            case MovementGait.Sprint: return sprint;
            }
            return new MovementGaitInfo();
        }
    }

    [SerializeField] private MovementGaitInfos movementGaitInfos;
    public float gravityMult = 1.0f;
    [SerializeField] private float groundedGravityMult = 0.1f;

    public const float gravity = 9.81f;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private InputActionReference walkToggleAction;
    [SerializeField] private InputActionReference crouchAction;
    [SerializeField] private InputActionReference aimAction;

    [Header("Internal State")]
    [SerializeField, SerializeReadOnly] private MovementMode movementMode;
    [SerializeField, SerializeReadOnly] private MovementStance movementStance;
    [SerializeField, SerializeReadOnly] private MovementGait movementGait;
    [SerializeField, SerializeReadOnly] private Vector3 velocity;
    [SerializeField, SerializeReadOnly] private float targetHeading;
    [SerializeField, SerializeReadOnly] private float currentHeading;
    [SerializeReadOnly] public float globalHeading;

    [SerializeField, SerializeReadOnly] private Vector2 moveInput;
    [SerializeField, SerializeReadOnly] private Vector2 lookInput;
    [SerializeField, SerializeReadOnly] private bool wantsSprint;
    [SerializeField, SerializeReadOnly] private bool wantsWalk;
    [SerializeField, SerializeReadOnly] private bool wantsCrouch;
    [SerializeField, SerializeReadOnly] private bool isAiming;

    private CharacterController characterController;

    void Start() {
        characterController = GetComponent<CharacterController>();
    }

    void OnEnable() {
        walkToggleAction.action.performed += OnWalkToggle;
    }

    void OnDisable() {
        walkToggleAction.action.performed -= OnWalkToggle;
    }

    void OnWalkToggle(InputAction.CallbackContext context) {
        wantsWalk = !wantsWalk;
    }

    void Update() {
        var moveInputLastFrame = moveInput;

        wantsSprint = sprintAction.action.IsPressed();
        wantsCrouch = crouchAction.action.IsPressed();
        isAiming = aimAction.action.IsPressed();
        moveInput = moveAction.action.ReadValue<Vector2>();
        lookInput = lookAction.action.ReadValue<Vector2>();

        if (wantsSprint) {
            movementGait = MovementGait.Sprint;
        } else if (wantsWalk) {
            movementGait = MovementGait.Walk;
        } else {
            movementGait = MovementGait.Run;
        }

        if (isAiming) {
            movementMode = MovementMode.LookTowardsCamera;
        } else {
            movementMode = MovementMode.RotateTowardsMovement;
        }

        var gait = movementGaitInfos.Get(movementGait);

        var movement = Vector3.zero;
        switch (movementMode) {
        case MovementMode.LookTowardsCamera:
            movement = (transform.right * moveInput.x + transform.forward * moveInput.y) * gait.baseSpeed;
            targetHeading = globalHeading;
            currentHeading = Mathf.LerpAngle(currentHeading, targetHeading, gait.headingLerpFactor);
            break;

        case MovementMode.RotateTowardsMovement:
            if (moveInput.magnitude > 0) {
                movement = transform.forward * gait.baseSpeed;

                targetHeading = globalHeading + Vector3.SignedAngle(Vector3.forward, new Vector3(moveInput.x, 0, moveInput.y), Vector3.up);

                var startedMoving = moveInputLastFrame == Vector2.zero;
                var sharpTurn = Mathf.Abs(Mathf.DeltaAngle(targetHeading, currentHeading)) >= gait.sharpTurnThreshold;
                if (sharpTurn || startedMoving) {
                    currentHeading = targetHeading;
                } else {
                    currentHeading = Mathf.MoveTowardsAngle(currentHeading, targetHeading, gait.turnRate * Time.deltaTime);
                }
            }

            break;
        }

        transform.rotation = Quaternion.Euler(0, currentHeading, 0);

        var velocityY = velocity.y;
        if (characterController.isGrounded) {
            velocityY = -gravity * groundedGravityMult;
        } else {
            velocityY -= gravity * gravityMult * Time.deltaTime;
        }

        velocity = movement + Vector3.up * velocityY;

        characterController.Move(velocity * Time.deltaTime);
    }
}
