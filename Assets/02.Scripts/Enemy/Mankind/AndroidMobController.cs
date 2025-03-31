using UnityEngine;
using Akasha;

public class AndroidMobController : BaseController<AndroidMobEntity>
{
    [Header("Movement")]
    public float MoveSpeed = 2f;
    public float Gravity = -15f;

    [Header("Ground Check")]
    public LayerMask GroundLayers;
    private bool Grounded;
    private float _verticalVelocity;
    private float _fallTimeout = 0.15f;
    private float _fallTimeoutDelta;

    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator;

    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;

    protected override void OnControllerEnable()
    {
        _fallTimeoutDelta = _fallTimeout;

        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
    }

    private void Update()
    {
        GroundedCheck();
        ApplyGravity();
    }

    public void Move(Vector3 direction)
    {
        if (direction.sqrMagnitude > 0f)
            direction.y = 0;

        characterController.Move(
            direction.normalized * MoveSpeed * Time.deltaTime +
            new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime
        );
    }

    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - 0.14f, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, 0.28f, GroundLayers, QueryTriggerInteraction.Ignore);

        if (animator)
            animator.SetBool(_animIDGrounded, Grounded);
    }

    private void ApplyGravity()
    {
        if (Grounded)
        {
            _fallTimeoutDelta = _fallTimeout;

            if (_verticalVelocity < 0f)
                _verticalVelocity = -2f;

            if (animator)
            {
                animator.SetBool(_animIDJump, false);
                animator.SetBool(_animIDFreeFall, false);
            }
        }
        else
        {
            _fallTimeoutDelta -= Time.deltaTime;

            if (_fallTimeoutDelta <= 0f && animator)
                animator.SetBool(_animIDFreeFall, true);

            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }
}