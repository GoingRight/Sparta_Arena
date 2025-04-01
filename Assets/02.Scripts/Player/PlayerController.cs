using System;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    private Player player;
    public float speed;
    internal Vector2 curMoveInput;
    internal Rigidbody _rigidbody;
    internal bool isSprint;

    [Header("Look")]
    private Vector2 curLookInput;
    [SerializeField] private Transform camContainer;
    [SerializeField] private bool CursurLockState;
    private float curCamX;

    [Header("Jump")]
    private float jumpForce;
    public Action jumpTrigger;

    private void Awake()
    {
        // Move
        _rigidbody = GetComponent<Rigidbody>();
        player = GetComponent<Player>() ?? throw new System.NullReferenceException($"player 클래스를 가지지 않음 : {this.gameObject.name}");

        // Look
        Cursor.lockState = (CursurLockState) ? CursorLockMode.Locked : CursorLockMode.None;

        // Jump
        jumpForce = player.Data.AirData.JumpForce;
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void LateUpdate()
    {
        RotateTarget();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            curMoveInput = context.ReadValue<Vector2>();

        else if (context.phase == InputActionPhase.Canceled)
            curMoveInput = Vector2.zero;
    }

    public Vector2 ReturnMoveInput()
    {
        return curMoveInput;
    }

    private void Move()
    {
        Vector3 camForward = camContainer.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = camContainer.right;
        camRight.y = 0;
        camRight.Normalize();

        Vector3 moveDirection = (camForward * curMoveInput.y + camRight * curMoveInput.x).normalized;

        speed = isSprint ? player.stat.Speed + 2f : player.stat.Speed;
        Vector3 velocity = moveDirection * speed;
        velocity.y = _rigidbody.velocity.y;

        _rigidbody.velocity = velocity;

        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * 5f
            );
        }
    }

    public void RotateTarget()
    {
        // 목표 회전 값 가져오기
        Quaternion targetRotation = camContainer.rotation;

        // Y축 회전만 유지 (X와 Z축은 고정)
        Vector3 eulerAngles = targetRotation.eulerAngles;
        eulerAngles.x = 0f; // X축 고정
        eulerAngles.z = 0f; // Z축 고정

        targetRotation = Quaternion.Euler(eulerAngles);

        if (camContainer.rotation.y - targetRotation.y > 1f)
        {
            Debug.Log("차이 발생");
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2f); // 회전 속도 조절
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        isSprint = (context.phase == InputActionPhase.Performed) ? true : false;
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
