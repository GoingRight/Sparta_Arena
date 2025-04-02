using UnityEngine;

public class ChickenAnimationController : MonoBehaviour
{
    private Animator animator;
    private readonly int IsMoving = Animator.StringToHash("IsMoving");
    private readonly int IsRunning = Animator.StringToHash("IsRunning");
    private readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    private readonly int AttackTrigger = Animator.StringToHash("Attack");
    private readonly int HitTrigger = Animator.StringToHash("Hit");
    private readonly int IsDead = Animator.StringToHash("IsDead");

    private bool isMoving;
    private bool isRunning;
    private bool isAttacking;
    private bool isDead;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetMoving(bool value)
    {
        if (isMoving != value)
        {
            isMoving = value;
            animator.SetBool(IsMoving, value);
        }
    }

    public void SetRunning(bool value)
    {
        if (isRunning != value)
        {
            isRunning = value;
            animator.SetBool(IsRunning, value);
        }
    }

    public void SetAttacking(bool value)
    {
        if (isAttacking != value)
        {
            isAttacking = value;
            animator.SetBool(IsAttacking, value);
        }
    }

    public void TriggerAttack()
    {
        animator.SetTrigger(AttackTrigger);
    }

    public void TriggerHit()
    {
        animator.SetTrigger(HitTrigger);
    }

    public void SetDead(bool value)
    {
        if (isDead != value)
        {
            isDead = value;
            animator.SetBool(IsDead, value);
        }
    }

    public void OnAttackAnimationComplete()
    {
        SetAttacking(false);
        SetRunning(false);
        SetMoving(false);
    }

    public void OnDeathAnimationComplete()
    {
        // 죽음 애니메이션이 완료된 후 처리할 로직
        Destroy(gameObject);
    }

    public void ResetAllStates()
    {
        SetAttacking(false);
        SetRunning(false);
        SetMoving(false);
    }
} 