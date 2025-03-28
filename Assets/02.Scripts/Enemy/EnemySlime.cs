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
    private Vector3 targetDir;

    private bool isMove = false;
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
        animator = GetComponentInChildren<Animator>();
        _rb = GetComponentInChildren<Rigidbody>();
        SlimeAnimationData = new SlimeAnimationData();
        SlimeAnimationData.Initialize();
        lastAtkTime = 0;
    }

    private void Start()
    {
        player = GameManager.instance.player;
        IsMove = true;
        InvokeRepeating("CallDuplicate", 5f, duplicateRate);
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
        Vector3 distance = curPlayerPosition - transform.position;
        distance.y = 0;
        targetDir = distance.normalized;
        lastAtkTime += Time.deltaTime;
        if (distance.magnitude <= atkDistance)
        {
            IsMove = false;
            if(lastAtkTime >= 1.5f)
            {
                Attack();
            }
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
        float originY = _rb.velocity.y;
        if (animator.GetBool(SlimeAnimationData.MoveParameterHash))
        {
            _rb.velocity = new Vector3(targetDir.x*stat.Speed, originY, targetDir.z * stat.Speed);
        }
        else
        {
            _rb.velocity = new Vector3(0,originY, 0);
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

            transform.rotation = Quaternion.LookRotation(targetDir);
        }
        else
        {
            return;
        }

    }

    private void CallDuplicate()
    {
        animator.SetTrigger(SlimeAnimationData.ReplicateParameterHash);
        IsMove = false;
    }

    public void Duplicate()
    {
        EnemySlime replica = Instantiate(this.gameObject).GetComponent<EnemySlime>();
        replica.transform.position = transform.position + Vector3.up;
        replica.stat = this.stat;
    }
}
