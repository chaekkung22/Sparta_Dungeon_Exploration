using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")] [SerializeField] private float speed;
    [SerializeField] private float jumpForce;
    [SerializeField] private LayerMask groundLayer;
    private Vector2 currentMovementInput;
    private float defaultSpeed;

    [Header("Look")] [SerializeField] private Transform cameraContainer;
    [SerializeField] private float minXLook;
    [SerializeField] private float maxXLook;
    [SerializeField] private float lookSensitivity;
    [SerializeField] private bool canLook = true;
    private Vector2 mouseDelta;
    private float camCurXRotation;
    private Animator animator;

    private bool isJumping;
    private float jumpStartTime;
    [SerializeField] private float jumpAnimLength = 1.53f;

    public Action inventory;
    private Rigidbody _rigidbody;
    private CapsuleCollider capsuleCollider;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        defaultSpeed = speed;
    }

    void Update()
    {
        if (isJumping && !IsGrounded())
        {
            float jumpDuration = Time.time - jumpStartTime;
            float calculatedSpeed = jumpAnimLength / jumpDuration;
            animator.speed = Mathf.Clamp(calculatedSpeed, 0.3f, 2f);
            isJumping = false;

            Invoke("ResetAnimatorSpeed", 0.1f);
        }
    }

    void ResetAnimatorSpeed()
    {
        animator.speed = 1f;
    }

    void FixedUpdate()
    {
        Move();
    }

    void LateUpdate()
    {
        if (canLook)
        {
            CameraLook();
        }
    }

    void Move()
    {
        Vector3 dir = transform.forward * currentMovementInput.y + transform.right * currentMovementInput.x;
        dir *= speed;
        dir.y = _rigidbody.velocity.y;

        _rigidbody.velocity = dir;
    }

    void CameraLook()
    {
        camCurXRotation += mouseDelta.y * lookSensitivity;
        camCurXRotation = Mathf.Clamp(camCurXRotation, minXLook, maxXLook);
        cameraContainer.localEulerAngles = new Vector3(-camCurXRotation, 0, 0);

        transform.eulerAngles += new Vector3(0, mouseDelta.x * lookSensitivity, 0);
    }


    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            currentMovementInput = context.ReadValue<Vector2>();
            animator.SetBool("IsWalk", true);
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            currentMovementInput = Vector2.zero;
            animator.SetBool("IsWalk", false);
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        mouseDelta = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started && IsGrounded())
        {
            if (CharacterManager.Instance.Player.condition.UseStamina(20))
            {
                _rigidbody.AddForce(Vector2.up * jumpForce, ForceMode.Impulse);
                animator.SetTrigger("Jump");
                jumpStartTime = Time.time;
                isJumping = true;
            }
        }
    }

    private bool IsGrounded()
    {
        Ray[] rays = new Ray[4]
        {
            new Ray(
                transform.TransformPoint(capsuleCollider.center) - new Vector3(0, capsuleCollider.center.y, 0) +
                (transform.forward * 0.2f) +
                (transform.up * 0.01f), Vector3.down),
            new Ray(
                transform.TransformPoint(capsuleCollider.center) - new Vector3(0, capsuleCollider.center.y, 0) +
                (-transform.forward * 0.2f) +
                (transform.up * 0.01f), Vector3.down),
            new Ray(
                transform.TransformPoint(capsuleCollider.center) - new Vector3(0, capsuleCollider.center.y, 0) +
                (transform.right * 0.2f) +
                (transform.up * 0.01f), Vector3.down),
            new Ray(
                transform.TransformPoint(capsuleCollider.center) - new Vector3(0, capsuleCollider.center.y, 0) +
                (-transform.right * 0.2f) +
                (transform.up * 0.01f), Vector3.down)
        };

        for (int i = 0; i < rays.Length; i++)
        {
            if (Physics.Raycast(rays[i], 0.1f, groundLayer))
            {
                return true;
            }
        }

        return false;
    }

    public void OnInventory(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            inventory?.Invoke();
            ToggleCursor();
        }
    }

    void ToggleCursor()
    {
        bool toggle = Cursor.lockState == CursorLockMode.Locked;
        Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Locked;
        canLook = !toggle;
    }

    public void ChangeSpeed(float amount)
    {
        speed *= amount;
        Invoke("ResetSpeed", 3f);
    }

    void ResetSpeed()
    {
        speed = defaultSpeed;
    }
}