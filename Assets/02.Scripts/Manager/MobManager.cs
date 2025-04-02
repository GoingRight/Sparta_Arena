using Akasha;
using UnityEngine;
using System.Linq;

public class MobManager : Manager<MobManager>
{
    protected override bool IsPersistent => false;

    public GameObject player;
    public Vector3 PlayerPosition => player != null ? player.transform.position : Vector3.zero;

    public RxList<RxModel> AllMobs { get; private set; }
    public MobGroupStatusModel GroupStatus { get; private set; }
    public MobFormationPositioner Formation { get; private set; }

    private const float underAttackDuration = 3f;
    private float underAttackTimer = 0f;

    private void Update()
    {
        RxQueue.ExecuteAll();
    }

    protected override void OnSetup()
    {
        if (isInitialized) return;

        AllMobs = new RxList<RxModel>();
        GroupStatus = new MobGroupStatusModel();
        GroupStatus.Setup(this);

        Formation = GetComponent<MobFormationPositioner>();
        Formation.Setup(this);
    }

    protected override void OnInit()
    {
        if (isInitialized) return;

        RxBinder.Bind(AllMobs, _ => RecalculateGroupStatus(), this);
        RxBinder.Bind(GroupStatus.AvgHealth, _ => RecalculateGroupStatus(), this);
        RxBinder.Bind(GroupStatus.UnderAttack, _ => RecalculateGroupStatus(), this);
    }

    public void Register(RxModel mobModel)
    {
        AllMobs.Add(mobModel);
        Formation.RefreshMobControllers();
    }

    public void NotifyUnderAttack()
    {
        underAttackTimer = underAttackDuration;
        GroupStatus.UpdateUnderAttack(true, this);
    }

    private void RecalculateGroupStatus()
    {
        if (underAttackTimer > 0f)
        {
            underAttackTimer -= Time.deltaTime;
            if (underAttackTimer <= 0f)
                GroupStatus.UpdateUnderAttack(false, this);
        }

        var models = AllMobs.Value;
        int count = models.Count;

        if (count == 0)
        {
            GroupStatus.UpdateStatus(1f, 0, false, this);
            GroupStatus.GroupStrategy.SetValue(StrategyType.Hold, this);
            return;
        }

        float totalRatio = models.OfType<IMobHealthReadable>().Sum(m => m.HealthRatio);
        float avgHealth = totalRatio / count;
        bool underAttack = GroupStatus.UnderAttack.Value;

        GroupStatus.UpdateStatus(avgHealth, count, underAttack, this);
        GroupStatus.GroupStrategy.SetValue(DetermineStrategy(avgHealth, count, underAttack), this);
    }

    private StrategyType DetermineStrategy(float avgHealth, int allyCount, bool underAttack)
    {
        if (avgHealth < 0.3f)
            return StrategyType.Retreat;

        if (underAttack)
            return StrategyType.Attack;

        return StrategyType.Hold;
    }
    public void ApplyHealToAllAllies(float amount)
    {
        foreach (var model in AllMobs.Value.OfType<IMobHealthReadable>())
        {
            if (model is AndroidMobModel mob && mob.IsAlive.Value)
            {
                float healed = Mathf.Min(100f, mob.CurrentHealth.Value + amount);
                mob.CurrentHealth.SetValue(healed, this);

                Debug.Log($"[MobManager] 힐 적용됨: {mob.ReactiveOwner?.GetType().Name} → +{amount}");
            }
        }
    }
    public void ApplyBuffToAllAllies()
    {
        Debug.Log("[MobManager] 버프 적용 (예: 공격력 증가)");
        // TODO: RxVar로 상태 버프 관리 시 확장
    }
    public void ApplyDebuffToPlayer(float duration)
    {
        Debug.Log($"[MobManager] 플레이어에게 {duration}초간 디버프 적용");

        // 예시: PlayerController.Instance?.ApplySlow(duration);
    }
}
public class MobGroupStatusModel : RxModel
{
    public RxVar<float> AvgHealth { get; private set; }
    public RxVar<int> AllyCount { get; private set; }
    public RxVar<bool> UnderAttack { get; private set; }
    public RxVar<StrategyType> GroupStrategy { get; private set; }

    public void Setup(object owner)
    {
        SetReactiveOwner(owner);

        AvgHealth = new RxVar<float>(1f, this);
        AllyCount = new RxVar<int>(0, this);
        UnderAttack = new RxVar<bool>(false, this);
        GroupStrategy = new RxVar<StrategyType>(StrategyType.Hold, this);
    }

    public void UpdateStatus(float avgHealth, int allyCount, bool underAttack, object caller)
    {
        AvgHealth.SetValue(avgHealth, caller);
        AllyCount.SetValue(allyCount, caller);
        UnderAttack.SetValue(underAttack, caller);
    }

    public void UpdateUnderAttack(bool underAttack, object caller)
    {
        if (UnderAttack.Value != underAttack)
            UnderAttack.SetValue(underAttack, caller);
    }
}