using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : StateMachine
{
    public Player Player { get; }
    public PlayerController PlayerController { get; }
    public float RotationDamping { get; private set; }
    public float JumpForce { get; set; } = 500f;

    public bool isAttacking { get; set; }
    public int ComboIndex { get; set; }
    public Transform MainCamTransform { get; private set; }
    public PlayerIdleState IdleState { get; set; }
    public PlayerWalkState WalkState { get; set; }
    public PlayerRunState RunState { get; set; }
    public PlayerJumpState JumpState { get; set; }
    public PlayerFallState FallState { get; set; }
    public PlayerComboAttackState ComboAttackState { get; set; }
    public PlayerTakeDamagedState TakeDamagedState { get; set; }
    public PlayerStateMachine(Player player)
    {
        this.Player = player;

        MainCamTransform = Camera.main.transform;

        IdleState = new PlayerIdleState(this);
        WalkState = new PlayerWalkState(this);
        RunState = new PlayerRunState(this);
        TakeDamagedState = new PlayerTakeDamagedState(this);

        JumpState = new PlayerJumpState(this);
        FallState = new PlayerFallState(this);

        ComboAttackState = new PlayerComboAttackState(this);
        TakeDamagedState = new PlayerTakeDamagedState(this);

        RotationDamping = player.Data.GroundData.BaseRotationDamping;

        PlayerController = player.Input;
    }
}
