using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : StateMachine
{
    public Player Player { get; }
    public PlayerController PlayerController { get; }
    public float RotationDamping { get; private set; }
    public float JumpForce { get; private set; }
    public Transform MainCamTransform { get; private set; }
    public PlayerIdleState IdleState { get; set; }
    public PlayerWalkState WalkState { get; set; }
    public PlayerRunState RunState { get; set; }

    public PlayerStateMachine(Player player)
    {
        this.Player = player;

        MainCamTransform = Camera.main.transform;

        IdleState = new PlayerIdleState(this);
        WalkState = new PlayerWalkState(this);
        RunState = new PlayerRunState(this);

        RotationDamping = player.Data.GroundData.BaseRotationDamping;

        PlayerController = player.input;
    }
}
