using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySlime : EnemyMob
{
    [field: Header("Animations")]
    [field: SerializeField] public SlimeAnimationData SlimeAnimationData { get; private set; }

    private Player player;
    private Vector3 curPlayerPosition;
    public float atkDistance;
    public float duplicateRate;
    private float lastAtkTime = 0;
    private Animator animator;
    private Rigidbody _rb;

    private bool isMove;
    public bool IsMove
    {
        get { return isMove; }
        set
        {
            isMove = value;
            if (isMove)
            {
                animator.SetBool(SlimeAnimationData.MoveParameterHash, true);
            }
            else
            {
                animator.SetBool(SlimeAnimationData.MoveParameterHash, false);
            }
        }
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
        SlimeAnimationData = new SlimeAnimationData();
        SlimeAnimationData.Initialize();
        lastAtkTime = 0;
    }

    private void Start()
    {
        //player = GameManager.Instance.player;
        IsMove = true;
        InvokeRepeating("CallDuplicate", 10f, duplicateRate);
    }

    private void Update()
    {
        FindPlayer();
    }

    private void FixedUpdate()
    {
        Rotate();
        Move();
    }

    protected override void FindPlayer()
    {
        curPlayerPosition = player.transform.position;
        lastAtkTime += Time.deltaTime;
        if ((curPlayerPosition - transform.position).magnitude <= atkDistance && lastAtkTime < 1.5f)
        {
            Attack();
        }
    }

    protected override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        IsMove = false;
        animator.SetTrigger(SlimeAnimationData.DamageParameterHash);
    }

    protected override void Attack()
    {
        animator.SetTrigger(SlimeAnimationData.AttackParameterHash);
        lastAtkTime = 0;
    }

    protected override void Move()
    {
        if (animator.GetBool(SlimeAnimationData.MoveParameterHash))
        {
            _rb.velocity = Vector3.forward * stat.Speed;
        }
        else
        {
            _rb.velocity = Vector3.zero;
        }
    }

    public void SetMoveOn()
    {
        IsMove = true;
    }

    private void Rotate()
    {
        if (animator.GetBool(SlimeAnimationData.MoveParameterHash))
        {
            transform.rotation = Quaternion.LookRotation(curPlayerPosition - transform.position);
        }
        else
        {
            transform.rotation = Quaternion.identity;
        }

    }

    private void CallDuplicate()
    {
        IsMove = false;
        animator.SetTrigger(SlimeAnimationData.ReplicateParameterHash);
    }

    public void Duplicate()
    {
        EnemySlime replica = Instantiate(this.gameObject).GetComponent<EnemySlime>();
        replica.transform.position = transform.position + Vector3.right;
        replica.stat = this.stat;
    }
}
