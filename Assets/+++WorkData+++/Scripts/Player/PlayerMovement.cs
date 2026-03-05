using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;

    public float groundDrag;
    
    [Header("Sprinting")]
    public float sprintSpeed;
    public float maxStamina = 5f;
    public float staminaDrainPerSecond = 1.2f;
    public float staminaRegenPerSecond = 0.9f;
    public float regenDelayAfterSprint = 0.7f;
    private float lastSprintTime;
    [SerializeField] private float stamina;
    public bool regenOnlyWhenGrounded = true;
    private bool canSprint => stamina > 0.05f;
    
    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;
    
    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;
    
    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    
    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;
    
    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;
    
    public Transform orientation;
    
    float horizontalInput;
    float verticalInput;
    
    Vector3 moveDirection;
    
    Rigidbody rb;
    
    public MovementState state;

    public enum MovementState
    {
        walking, sprinting, crouching, air
    }
    void Start()
    {
        readyToJump = true;
        rb = GetComponent<Rigidbody>();
        
        rb.freezeRotation = true;
        
        startYScale = transform.localScale.y;
        
        stamina = maxStamina;
    }

    void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        
        MyInput();
        SpeedControl();
        StateHandler();
        HandleStamina();
        
        // handle drag
        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
        
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //jump
        if (Input.GetKeyDown(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            
            Jump();
            
            Invoke(nameof(ResetJump), jumpCooldown);
        }
        
        //crouch
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        //stop crouch
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void HandleStamina()
    {
        bool isSprinting = (state == MovementState.sprinting);

        if (isSprinting)
        {
            stamina -= staminaDrainPerSecond * Time.deltaTime;
            if (stamina < 0f) stamina = 0f;
            
            lastSprintTime = Time.time;
            
            if (stamina <= 0f)
            {
                stamina = 0f;
            }
        }
        else
        {
            bool delayPassed = Time.time >= lastSprintTime + regenDelayAfterSprint;
            
            if (delayPassed && (!regenOnlyWhenGrounded || grounded))
            {
                stamina += staminaRegenPerSecond * Time.deltaTime;
                if (stamina > maxStamina) stamina = maxStamina;
            }
        }
    }

    private void StateHandler()
    {
        //crouch
        if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            Debug.Log("Crouching");
            moveSpeed = crouchSpeed;
            return;
        }
        
        bool hasMoveInput = (horizontalInput != 0f || verticalInput != 0f);
        //sprint
        if (grounded && Input.GetKey(sprintKey) && canSprint && hasMoveInput)
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }
        
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }

        else
        {
            state = MovementState.air;
        }
    }

    private void MovePlayer()
    {
        //calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);
            
            if(rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        
        if(grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        else if(!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        
        //gravity off on slope
        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        //speedlimit on slope
        if (OnSlope() && !exitingSlope)
        {
            if(rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }

        //speedlimit on ground or air
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        
            //limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;
        
        //reset y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        
        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}
