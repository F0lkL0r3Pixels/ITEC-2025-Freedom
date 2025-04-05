using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Controller")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float normalJumpForce = 10f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private Transform groundCheckTransform;
    [SerializeField] private LayerMask groundLayer;
    private float lastGroundedTime;
    private bool hasJumped;

    [Header("Camera FX")]
    [SerializeField] private float stepInterval = 0.5f;
    [SerializeField] private CinemachineImpulseSource impulseSource;
    private float stepTimer = 0f;

    [Header("Abilities")]
    [SerializeField] private bool CanChargeJump;
    [SerializeField] private bool CanDash;
    [Space]
    [SerializeField] private float maxStamina = 100;
    [SerializeField] private float overheatThreshold = 20;
    [SerializeField] private float staminaRechargeMultiplier;
    [Space]
    [SerializeField] private float maxChargeJumpForce;
    [SerializeField] private float chargeJumpMultiplier;
    [SerializeField] private float chargeJumpDrainMultiplier;
    [Space]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashDrainMultiplier;


    [Header("Debug")]
    public float stamina;
    public float currentJumpForce;
    public bool isRechargingStamina;
    public bool isOverheated;
    public bool isChargingJump;
    public bool isDashing;

    private Transform cameraTransform;
    private Rigidbody rb;
    private CapsuleCollider capsule;
    private Vector2 moveInput;

    private void Awake()
    {
        cameraTransform = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        stamina = maxStamina;
        currentJumpForce = normalJumpForce;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Start()
    {
        GameManager.UIManager.staminaBarUI.SetCriticalThresholdPercentage(overheatThreshold / 100);
    }

    private void Update()
    {
        HandleStamina();
        HandleJump();
        if (CanDash && !isChargingJump) HandleDash();

        if (IsGrounded())
        {
            hasJumped = false;
            lastGroundedTime = Time.time;
            PlayerManager.Instance.Grappling.SetCanGrapple(true);
        }
    }

    private void FixedUpdate()
    {
        if (!isChargingJump && !isDashing) HandleMovement();
    }

    private void HandleMovement()
    {
        moveInput = GameManager.InputMaster.Player.Move.ReadValue<Vector2>();
        Vector3 forward = new Vector3(-cameraTransform.right.z, 0.0f, cameraTransform.right.x);
        Vector3 move = (forward * moveInput.y + cameraTransform.right * moveInput.x) * walkSpeed;

        if (!IsOnSteepSlope())
        {
            rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
        }

        // Head bob
        if (IsMoving() && IsGrounded() && !isDashing)
        {
            stepTimer -= Time.fixedDeltaTime;

            if (stepTimer <= 0f)
            {
                impulseSource.GenerateImpulse(new Vector3(0, -0.1f, 0));
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }


    private void HandleStamina()
    {
        stamina = Mathf.Clamp(stamina, 0, maxStamina);

        GameManager.UIManager.staminaBarUI.UpdateStamina(stamina, maxStamina);

        if (stamina <= 0)
        {
            isOverheated = true;
            isChargingJump = false;
            isDashing = false;

            if (IsGrounded())
            {
                isRechargingStamina = true;
            }
        }

        if (isChargingJump)
        {
            stamina -= Time.deltaTime * chargeJumpDrainMultiplier;
        }
        else if (isDashing)
        {
            stamina -= Time.deltaTime * dashDrainMultiplier;
        }
        else if (stamina < maxStamina && IsGrounded())
        {
            stamina += Time.deltaTime * staminaRechargeMultiplier;
        }

        if (stamina > overheatThreshold)
        {
            isOverheated = false;
        }

        if (stamina >= maxStamina)
        {
            isRechargingStamina = false;
        }
    }


    private void HandleJump()
    {
        if ((Time.time - lastGroundedTime > coyoteTime) || hasJumped ||IsOnSteepSlope()) return;

        if (!CanChargeJump)
        {
            if (GameManager.InputMaster.Player.Jump.WasPressedThisFrame())
            {
                currentJumpForce = normalJumpForce;
                Jump();
            }
            return;
        }

        if (isChargingJump && stamina <= 0)
        {
            Jump();
        }

        if (GameManager.InputMaster.Player.Jump.IsInProgress() && CanUseAbility())
        {
            isChargingJump = true;
            currentJumpForce += Time.deltaTime * chargeJumpMultiplier;
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }

        if (isChargingJump && GameManager.InputMaster.Player.Jump.WasReleasedThisFrame())
        {
            Jump();
        }
    }

    private void Jump()
    {
        rb.AddForce(Vector3.up * currentJumpForce, ForceMode.Impulse);
        hasJumped = true;

        if (isChargingJump)
        {
            isChargingJump = false;
            currentJumpForce = normalJumpForce;
            impulseSource.GenerateImpulse(new Vector3(0, -0.5f, 0));
        }
        else
        {
            impulseSource.GenerateImpulse(new Vector3(0, -0.4f, 0));
        }
    }

    private void HandleDash()
    {
        if (!CanUseAbility())
        {
            isDashing = false;
            return;
        }

        if (GameManager.InputMaster.Player.Sprint.WasPerformedThisFrame())
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }

        if (GameManager.InputMaster.Player.Sprint.IsInProgress())
        {
            isDashing = true;

            Vector3 forward = new Vector3(-cameraTransform.right.z, 0.0f, cameraTransform.right.x);
            Vector3 dash = forward * dashSpeed;

            if (!IsOnSteepSlope())
            {
                rb.linearVelocity = new Vector3(dash.x, rb.linearVelocity.y, dash.z);
            }
        }
        else
        {
            isDashing = false;
        }
    }

    private bool IsGrounded()
    {
        return Physics.CheckSphere(groundCheckTransform.position, groundCheckRadius, groundLayer);
    }

    private bool IsMoving()
    {
        return moveInput.sqrMagnitude > 0.1f;
    }

    private bool CanUseAbility()
    {
        return stamina > 0 && !isOverheated;
    }

    private bool IsOnSteepSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, capsule.height / 2 + 0.5f, groundLayer))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            return slopeAngle > 30f; // You can tweak this angle
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        if (groundCheckTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckTransform.position, groundCheckRadius);
        }
    }
}