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
    private CharacterAnimController animController;
    private Animator animator;

    void Start() {
        characterController = GetComponent<CharacterController>();
        animController = GetComponentInChildren<CharacterAnimController>();
        animator = GetComponentInChildren<Animator>();
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

        if (wantsCrouch) {
            if (movementStance == MovementStance.Crouching) {
                movementStance = MovementStance.Standing;
                // animController.IsCrouched = false;
            } else {
                movementStance = MovementStance.Crouching;
                // animController.IsCrouched = true;
            }
        }

        if (wantsSprint) {
            movementGait = MovementGait.Sprint;
        } else if (wantsWalk) {
            movementGait = MovementGait.Walk;
        } else {
            movementGait = MovementGait.Run;
        }

        if (animController != null) {
            if (movementGait == MovementGait.Walk) {
                animController.WalkRun = 0.0f;
            } else {
                animController.WalkRun = 1.0f;
            }
        }
        if (animator != null) {
            if (movementGait == MovementGait.Walk) {
                animator.SetFloat("WalkRun", 0.0f);
            } else {
                animator.SetFloat("WalkRun", 1.0f);
            }
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

        if (animController != null) {
            animController.Stride = moveInput.magnitude;
        }
        if (animator != null) {
            animator.SetFloat("Stride", moveInput.magnitude);
            animator.SetBool("IsMoving", moveInput.magnitude > 0.0f);
        }

        transform.rotation = Quaternion.Euler(0, currentHeading, 0);

        var velocityY = velocity.y;
        if (characterController.isGrounded) {
            velocityY = -gravity * groundedGravityMult;
        } else {
            velocityY -= gravity * gravityMult * Time.deltaTime;
        }

        velocity = movement + Vector3.up * velocityY;

        if (animController != null) {
            animController.velocity = velocity;
            animController.heading = currentHeading;
        }
        if (animator != null) {
            var dir = new Vector3(movement.x, 0, movement.z);
            if (dir.sqrMagnitude > 0.0f) {
                dir = dir.normalized;
                dir = Quaternion.Euler(0, -currentHeading, 0) * dir;
            }

            var magnitude = movementGait == MovementGait.Walk ? 0.2f : 1.0f;

            animator.SetBool("IsWalking", movementGait == MovementGait.Walk);
            animator.SetFloat("LeanVelocityX", dir.x * magnitude);
            animator.SetFloat("LeanVelocityY", dir.z * magnitude);
        }

        characterController.Move(velocity * Time.deltaTime);
    }
}
