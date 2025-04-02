using System;
using UnityEngine;

public class Player : Character
{
    [field: Header("FSM")]
    [field: SerializeField] public PlayerAnimationData AnimationData { get; private set; }
    [field: SerializeField] public PlayerSO Data { get; private set; }

    protected internal PlayerStateMachine stateMachine;

    public Action detectTakeDamage;

    public Animator Animator { get; private set; }
    public PlayerController Input { get; private set; }
    public Rigidbody RigidBody { get; private set; }

    public float RunSpeed { get; private set; } = 7f;

    private void Awake()
    {
        AnimationData.Initialize();
        Animator = GetComponentInChildren<Animator>();
        Input = GetComponent<PlayerController>();
        RigidBody = GetComponent<Rigidbody>();

        stateMachine = new PlayerStateMachine(this);
    }

    private void Start()
    {
        stateMachine.ChangeState(stateMachine.IdleState);
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

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Enemy"))
        {
            Character collisionOBJ = collision.gameObject.GetComponent<Character>();
            float damage = collisionOBJ.stat.Attack;
            TakeDamage(damage);
        }
    }
}
