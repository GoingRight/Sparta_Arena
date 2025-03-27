using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    [field: Header("FSM")]
    [field: SerializeField] public PlayerAnimationData animationData { get; private set; }
    [field: SerializeField] public PlayerSO Data { get; private set; }
    private PlayerStateMachine stateMachine;

    public Animator animator { get; private set; }
    public PlayerController input { get; private set; }
    public Rigidbody controller { get; private set; }


    private void Awake()
    {
        animationData.Initialize();
        animator = GetComponent<Animator>();
        input = GetComponent<PlayerController>();
        controller = GetComponent<Rigidbody>();

        stateMachine = new PlayerStateMachine(this);
    }

    private void Update()
    {
        stateMachine.HandleInput();
        stateMachine.Update();
    }

    private void FixedUpdate()
    {
        stateMachine.PhysicsUpdate();
    }
}
