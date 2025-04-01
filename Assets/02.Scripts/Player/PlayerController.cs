using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Transform camPivot; // 카메라 피벗
    [SerializeField] private Transform characterModel; // 캐릭터 모델 (회전 대상)
    private Player player;
    public float speed;
    internal Vector2 curMoveInput;
    internal Rigidbody _rigidbody;
    internal bool isSprint;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 15f; // 모델 회전 속도

    [Header("Jump")]
    private float jumpForce;
    public Action jumpTrigger;
    public bool isGrounded;
    public LayerMask groundMask;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        player = GetComponent<Player>() ?? throw new NullReferenceException($"player 클래스를 가지지 않음 : {this.gameObject.name}");
        jumpForce = player.Data.AirData.JumpForce;

        if (characterModel == null)
            characterModel = transform.GetChild(0);
    }

    private void FixedUpdate()
    {
        Move();
        RotateCharacterModel();
        isGrounded = Physics.Raycast(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z), Vector3.down, 1.5f, groundMask);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            curMoveInput = context.ReadValue<Vector2>();
        else if (context.phase == InputActionPhase.Canceled)
            curMoveInput = Vector2.zero;
    }

    private void Move()
    {
        if (camPivot == null) return;

        // 카메라 기준 방향 계산
        Vector3 camForward = camPivot.forward;
        Vector3 camRight = camPivot.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // 이동 방향 계산
        Vector3 moveDirection = (camForward * curMoveInput.y + camRight * curMoveInput.x).normalized;

        // 속도 적용
        speed = isSprint ? player.stat.Speed + 2f : player.stat.Speed;
        Vector3 velocity = moveDirection * speed;
        velocity.y = _rigidbody.velocity.y;

        _rigidbody.velocity = velocity;
    }

    private void RotateCharacterModel()
    {
        if (characterModel == null || curMoveInput.magnitude < 0.1f) return;

        // 카메라 기준 이동 방향 계산
        Vector3 camForward = camPivot.forward;
        Vector3 camRight = camPivot.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = (camForward * curMoveInput.y + camRight * curMoveInput.x).normalized;

        // 이동 방향으로 부드럽게 회전
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        characterModel.rotation = Quaternion.Slerp(characterModel.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        isSprint = (context.phase == InputActionPhase.Performed);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started && isGrounded)
        {
            _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpTrigger?.Invoke();
            Debug.Log("Jump");
        }
    }

    public Vector2 ReturnMoveInput()
    {
        return curMoveInput;
    }
}