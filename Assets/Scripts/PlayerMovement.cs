using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 12f;
    public float rotationSpeed = 20f;
    public float jumpForce = 12f;
    
    [Tooltip("Multiplier for physics gravity. Values between 2.0 and 3.5 feel snappy and arcade-like.")]
    public float gravityScale = 2.5f;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundMask;

    [Header("References")]
    public Transform cameraTransform;
    public Animator animator;

    [Header("Animator Parameter Names")]
    public string speedFloatParam = "Speed";
    public string isMovingBoolParam = "isMoving";
    public string isGroundedBoolParam = "isGrounded";
    public string jumpTriggerParam = "Jump";

    private Rigidbody rb;
    private InputSystem_Actions inputActions;
    private bool isGrounded;
    private bool canMove = true;

    // Animator Hashes
    private int speedHash;
    private int isMovingHash;
    private int isGroundedHash;
    private int jumpTriggerHash;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        inputActions = new InputSystem_Actions();

        if (animator != null)
        {
            speedHash = Animator.StringToHash(speedFloatParam);
            isMovingHash = Animator.StringToHash(isMovingBoolParam);
            isGroundedHash = Animator.StringToHash(isGroundedBoolParam);
            jumpTriggerHash = Animator.StringToHash(jumpTriggerParam);
        }
    }

    private void OnEnable() => inputActions.Player.Enable();
    private void OnDisable() => inputActions.Player.Disable();

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        // 1. Ground check
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }

        // 2. Read Jump input
        if (canMove && inputActions.Player.Jump.triggered && isGrounded)
        {
            Jump();
        }

        // 3. Update Animator
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (canMove)
        {
            MovePlayer();
        }

        ApplyCustomGravity();
    }

    private void MovePlayer()
    {
        Vector2 inputVector = inputActions.Player.Move.ReadValue<Vector2>();

        Vector3 inputDirection = new Vector3(inputVector.x, 0f, inputVector.y);
        bool isMoving = inputDirection.magnitude >= 0.1f;

        if (isMoving)
        {
            // Camera-relative calculations
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDirection = (camForward * inputDirection.z) + (camRight * inputDirection.x);

            // Directly set velocity to single move speed (Run)
            Vector3 targetVelocity = moveDirection * moveSpeed;
            rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

            // Smooth rotation
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
        else
        {
            // Stop horizontal momentum instantly when no input is given
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }

    private void ApplyCustomGravity()
    {
        if (gravityScale != 1f)
        {
            Vector3 extraGravity = Physics.gravity * (gravityScale - 1f);
            rb.AddForce(extraGravity, ForceMode.Acceleration);
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        if (animator != null)
        {
            animator.SetTrigger(jumpTriggerHash);
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        Vector2 inputVector = canMove ? inputActions.Player.Move.ReadValue<Vector2>() : Vector2.zero;
        bool isMoving = canMove && (inputVector.magnitude >= 0.1f);

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float currentSpeed = isMoving ? horizontalVelocity.magnitude : 0f;

        animator.SetBool(isMovingHash, isMoving);
        animator.SetBool(isGroundedHash, isGrounded);
        animator.SetFloat(speedHash, currentSpeed, 0.1f, Time.deltaTime);
    }

    public void ApplyKnockback(Vector3 impulseForce, float stunDuration)
    {
        rb.AddForce(impulseForce, ForceMode.Impulse);
        StartCoroutine(StunRoutine(stunDuration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        canMove = false;
        yield return new WaitForSeconds(duration);
        canMove = true;
    }

    public void SetCanMove(bool state)
    {
        canMove = state;
    }
}