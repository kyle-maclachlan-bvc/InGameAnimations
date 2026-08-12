using UnityEngine;
using UnityEngine.InputSystem;
 
public class PlayerController : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] InputActionReference moveInputAction;
    [SerializeField] InputActionReference runInputAction;
    [SerializeField] InputActionReference jumpInputAction;
    //[SerializeField] InputActionReference interactInputAction;
        
    [Header("Running Settings")]
    [SerializeField] float movementSpeed;
    [SerializeField] float rotationSpeed;
    [SerializeField] float runningSpeedMulitplier;
    
    [SerializeField] Transform cameraTransform;
    
    [Header("Jump Settings")]
    [SerializeField] float jumpForce;
    [SerializeField] Transform groundCheckPoint;
    [SerializeField] float groundCheckRadius;
    [SerializeField] LayerMask groundLayer;
    
    Vector2 moveInput;
 
    Rigidbody rb;
 
    Animator anim;
 
    float activeRunningSpeedMultiplier = 1.5f;

    bool jumpRequested;
    bool isGrounded;
 
    readonly int walkingAnimatorHash = Animator.StringToHash("Walking");
    readonly int runningAnimatorHash = Animator.StringToHash("Running");
    readonly int jumpAnimatorHash = Animator.StringToHash("JumpRequested");
    readonly int isGroundedAnimatorHash = Animator.StringToHash("IsGrounded");
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    void OnEnable()
    {
        jumpInputAction.action.performed += OnJumpPerformed;
    }

    void OnDisable()
    {
        jumpInputAction.action.performed -= OnJumpPerformed;
    }
    
    void Update()
    {
       moveInput = moveInputAction.action.ReadValue<Vector2>();
    }
 
    private void FixedUpdate()
    {
        // Ground Check for Jumping
        isGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer);
        anim.SetBool(isGroundedAnimatorHash, isGrounded);
        
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
 
        cameraForward.y = 0;
        cameraRight.y = 0;
 
        cameraForward.Normalize();
        cameraRight.Normalize();
 
        Vector3 moveDirection = cameraForward * moveInput.y + cameraRight * moveInput.x;
 
        Vector3 velocity = moveDirection * movementSpeed * activeRunningSpeedMultiplier;
 
       rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
 
       // Handles the jump request, consuming it so it only fires once per press
       
       //jumpRequested = false;
       anim.SetBool(jumpAnimatorHash, jumpRequested);
       
       //Checks if the character is moving
        if (moveDirection == Vector3.zero)
        {
            anim.SetBool(walkingAnimatorHash, false);
            anim.SetBool(runningAnimatorHash, false);
           return;
        }
 
        anim.SetBool(walkingAnimatorHash, true);
 
        //Checking if 'Shift' is being pressed to execute running. If not, then the characteris walking.
        if (runInputAction.action.IsPressed())
        {
            anim.SetBool(runningAnimatorHash, true);
            activeRunningSpeedMultiplier = runningSpeedMulitplier;
        }
        else
        {
           anim.SetBool(runningAnimatorHash, false);
           activeRunningSpeedMultiplier = 1;
        }
 
       Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
 
       Quaternion finalRotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed);
 
       rb.MoveRotation(finalRotation);
    }

    void OnJumpPerformed(InputAction.CallbackContext context)
    {
        jumpRequested = true;
        anim.SetBool(jumpAnimatorHash, true);
        
        if (jumpRequested && isGrounded)
        {
            jumpRequested = false;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}