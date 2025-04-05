using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;

    [SerializeField, Space] private float groundCheckRadius = 0.3f;
    [SerializeField] private Transform groundCheckTransform;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField, Space] private float stepInterval = 0.5f;
    [SerializeField] private CinemachineImpulseSource impulseSource;

    private float stepTimer = 0f;

    private Transform cameraTransform;
    private Rigidbody rb;
    private CapsuleCollider capsule;
    private Vector2 moveInput;

    private void Awake()
    {
        cameraTransform = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Start()
    {
        GameManager.InputMaster.Player.Move.performed += OnMove;
        GameManager.InputMaster.Player.Move.canceled += OnMove;
        GameManager.InputMaster.Player.Jump.performed += OnJump;
    }

    private void OnDisable()
    {
        GameManager.InputMaster.Player.Move.performed -= OnMove;
        GameManager.InputMaster.Player.Move.canceled -= OnMove;
        GameManager.InputMaster.Player.Jump.performed -= OnJump;
    }

    private void FixedUpdate()
    {
        Vector3 forward = new Vector3(-cameraTransform.right.z, 0.0f, cameraTransform.right.x);
        Vector3 move = (forward * moveInput.y + cameraTransform.right * moveInput.x) * walkSpeed;
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);

        if (IsMoving() && IsGrounded())
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

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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

    private void OnDrawGizmos()
    {
        if (groundCheckTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckTransform.position, groundCheckRadius);
        }
    }
}