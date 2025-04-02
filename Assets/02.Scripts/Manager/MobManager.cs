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
        // Rx 기반 변화 감지로 집단 상태 계산
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
            return;
        }

        float totalRatio = models.OfType<IMobHealthReadable>().Sum(m => m.HealthRatio);
        GroupStatus.UpdateStatus(totalRatio / count, count, GroupStatus.UnderAttack.Value, this);
    }
}

public class MobGroupStatusModel : RxModel
{
    public RxVar<float> AvgHealth { get; private set; }
    public RxVar<int> AllyCount { get; private set; }
    public RxVar<bool> UnderAttack { get; private set; }

    public void Setup(object owner)
    {
        SetReactiveOwner(owner);

        AvgHealth = new RxVar<float>(1f, this);
        AllyCount = new RxVar<int>(0, this);
        UnderAttack = new RxVar<bool>(false, this);
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