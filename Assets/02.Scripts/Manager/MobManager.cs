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

    private float underAttackTimer = 0f;
    private const float underAttackDuration = 3f;

    protected override void OnSetup()
    {
        AllMobs = new RxList<RxModel>();
        GroupStatus = new MobGroupStatusModel();
        GroupStatus.Setup(this);
    }

    protected override void OnInit()
    {
        RxTimer.Every(0.1f, this, RecalculateGroupStatus);
        Formation = GetComponent<MobFormationPositioner>();
        Formation.Setup(this);
    }

    public void Register(RxModel mobModel)
    {
        AllMobs.Add(mobModel);
        Formation?.RefreshMobControllers();
    }

    public void NotifyUnderAttack()
    {
        underAttackTimer = underAttackDuration;
    }

    private void RecalculateGroupStatus()
    {
        if (underAttackTimer > 0f)
            underAttackTimer -= Time.deltaTime;

        var models = AllMobs.Value;
        int count = models.Count;

        if (count == 0)
        {
            GroupStatus.UpdateStatus(1f, 0, false); // 기본 상태
            return;
        }

        // MobModel들이 IMobHealthReadable을 구현했다고 가정
        float totalRatio = models
            .OfType<IMobHealthReadable>()
            .Sum(m => m.HealthRatio);

        GroupStatus.UpdateStatus(
            totalRatio / count,
            count,
            underAttackTimer > 0f
        );
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

    public void UpdateStatus(float avgHealth, int allyCount, bool underAttack)
    {
        AvgHealth.SetValue(avgHealth, this);
        AllyCount.SetValue(allyCount, this);
        UnderAttack.SetValue(underAttack, this);
    }
}