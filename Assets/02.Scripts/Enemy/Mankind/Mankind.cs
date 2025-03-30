using UnityEngine;

public enum StrategyType
{
    Idle,
    Attack,
    Defend
}

public enum FormationRole
{
    Fighter,
    Supporter
}

public abstract class Mankind : MonoBehaviour
{
    public MobManager Manager { get; set; }

    public abstract FormationRole Role { get; }

    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public StrategyType strategyState = StrategyType.Idle;

    public virtual float GetHealthRatio() => Mathf.Clamp01(currentHealth / maxHealth);

    protected virtual void Start()
    {
        MobManager.Instance?.RegisterMob(this);
    }

    public virtual void ApplyDamage(float amount)
    {
        if (strategyState == StrategyType.Defend)
            amount *= 0.5f;

        currentHealth = Mathf.Max(0, currentHealth - amount);

        Manager?.NotifyUnderAttack();
    }

    public abstract void TickUpdate();
    public abstract void EvaluateStrategy();
}