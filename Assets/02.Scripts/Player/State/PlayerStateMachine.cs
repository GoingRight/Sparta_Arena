using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : StateMachine
{
    public Player player { get; }
    public PlayerController playerController { get; }
    public float RotationDamping { get; private set; }
    public float JumpForce { get; private set; }
    public Transform MainCamTransform { get; private set; }
    public PlayerStateMachine(Player player)
    {
        this.player = player;

        MainCamTransform = Camera.main.transform;
        RotationDamping = player.Data.GroundData.BaseRotationDamping;
    }
}
