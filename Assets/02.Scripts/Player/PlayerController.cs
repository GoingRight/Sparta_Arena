using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    private Player player;
    public float speed;
    internal Vector2 curMoveInput;
    internal Rigidbody _rigidbody;
    internal bool isSprint;

    [Header("Jump")]
    private float jumpForce;
    public Action jumpTrigger;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        player = GetComponent<Player>() ?? throw new NullReferenceException($"player 클래스를 가지지 않음 : {this.gameObject.name}");
        jumpForce = player.Data.AirData.JumpForce;
    }

    private void FixedUpdate()
    {
        Move();
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
        // 입력 방향을 3D 벡터로 변환 (y축은 0)
        Vector3 moveDirection = new Vector3(curMoveInput.x, 0, curMoveInput.y).normalized;

        // 스프린트 여부에 따라 속도 결정
        speed = isSprint ? player.stat.Speed + 2f : player.stat.Speed;

        // 속도 적용 (y축은 기존 물리 속도 유지)
        Vector3 velocity = moveDirection * speed;
        velocity.y = _rigidbody.velocity.y;

        _rigidbody.velocity = velocity;
    }

    public Vector2 ReturnMoveInput()
    {
        return curMoveInput;
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        isSprint = (context.phase == InputActionPhase.Performed);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpTrigger?.Invoke();
        }
    }
}