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

    public PlayerStateMachine(Player player)
    {
        this.Player = player;

        MainCamTransform = Camera.main.transform;

        IdleState = new PlayerIdleState(this);

        RotationDamping = player.Data.GroundData.BaseRotationDamping;
    }
}
