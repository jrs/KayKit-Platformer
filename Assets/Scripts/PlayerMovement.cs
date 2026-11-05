using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 7.0f;
    public float rotationSpeed = 10f;
    public float jumpHeight = 1.5f;
    public float gravity = -19.62f; // -19.62f feels snappier/more arcadey than -9.81f

    [Header("References")]
    public Transform cameraTransform;
    public Animator animator;

    [Header("Animator Parameter Names")]
    public string speedFloatParam = "Speed";
    public string isMovingBoolParam = "isMoving";
    public string isRunningBoolParam = "isRunning";
    public string isGroundedBoolParam = "isGrounded";
    public string jumpTriggerParam = "Jump";

    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool isGrounded;
    private bool canMove = true;

    private InputSystem_Actions inputActions;

    // Animator Hash IDs
    private int speedHash;
    private int isMovingHash;
    private int isRunningHash;
    private int isGroundedHash;
    private int jumpTriggerHash;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputActions = new InputSystem_Actions();

        if (animator != null)
        {
            speedHash = Animator.StringToHash(speedFloatParam);
            isMovingHash = Animator.StringToHash(isMovingBoolParam);
            isRunningHash = Animator.StringToHash(isRunningBoolParam);
            isGroundedHash = Animator.StringToHash(isGroundedBoolParam);
            jumpTriggerHash = Animator.StringToHash(jumpTriggerParam);
        }
    }

    private void OnEnable() => inputActions.Player.Enable();
    private void OnDisable() => inputActions.Player.Disable();

    void Start()
    {
        // Lock the cursor to the center of the game screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        // 1. Ground check & velocity reset
        isGrounded = controller.isGrounded;
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -0.5f;
        }

        // 2. Read Inputs
        Vector2 inputVector = Vector2.zero;
        bool isRunPressed = false;
        bool jumpTriggered = false;

        if (canMove)
        {
            inputVector = inputActions.Player.Move.ReadValue<Vector2>();
            isRunPressed = inputActions.Player.Sprint.IsPressed();
            jumpTriggered = inputActions.Player.Jump.triggered;
        }

        // 3. Jump Logic
        if (jumpTriggered && isGrounded)
        {
            // Physics formula to calculate initial jump velocity: v = sqrt(height * -2 * gravity)
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (animator != null)
            {
                animator.SetTrigger(jumpTriggerHash);
            }
        }

        // 4. Horizontal Movement Logic
        Vector3 inputDirection = new Vector3(inputVector.x, 0f, inputVector.y);
        bool isMoving = canMove && (inputDirection.magnitude >= 0.1f);
        float currentSpeed = 0f;
        bool isRunning = false;

        if (isMoving)
        {
            isRunning = isRunPressed;
            currentSpeed = isRunning ? runSpeed : walkSpeed;

            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDirection = (camForward * inputDirection.z) + (camRight * inputDirection.x);

            controller.Move(moveDirection * currentSpeed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 5. Update Animator Parameters
        UpdateAnimator(isMoving, isRunning, currentSpeed);

        // 6. Apply Gravity & Vertical Movement
        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    private void UpdateAnimator(bool isMoving, bool isRunning, float currentSpeed)
    {
        if (animator == null) return;

        animator.SetBool(isMovingHash, isMoving);
        animator.SetBool(isRunningHash, isRunning);
        animator.SetBool(isGroundedHash, isGrounded);
        animator.SetFloat(speedHash, currentSpeed, 0.1f, Time.deltaTime);
    }

    public void SetCanMove(bool state)
    {
        canMove = state;
    }
}