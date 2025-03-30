using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public struct MobGroupStatus
{
    public float AvgHealth;
    public int AllyCount;
    public bool UnderAttack;
}

public class MobManager : MonoBehaviour
{
    public static MobManager Instance { get; private set; }

    public PlayerController player;
    public Vector3 PlayerPosition => player != null ? player.transform.position : Vector3.zero;

    public List<Mankind> allMobs = new();
    public MobGroupStatus CurrentStatus { get; private set; }

    private float underAttackTimer = 0f;
    private const float underAttackDuration = 3f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (player == null)
            player = FindObjectOfType<PlayerController>();
    }

    public void RegisterMob(Mankind mob)
    {
        if (!allMobs.Contains(mob))
        {
            allMobs.Add(mob);
            mob.Manager = this;
        }
    }

    public void NotifyUnderAttack()
    {
        underAttackTimer = underAttackDuration;
    }

    private void FixedUpdate()
    {
        if (underAttackTimer > 0f)
            underAttackTimer -= Time.fixedDeltaTime;

        RecalculateStatus();
    }

    private void RecalculateStatus()
    {
        int count = allMobs.Count;
        if (count == 0) return;

        float totalHealth = allMobs.Sum(m => m.GetHealthRatio());

        CurrentStatus = new MobGroupStatus
        {
            AvgHealth = totalHealth / count,
            AllyCount = count,
            UnderAttack = underAttackTimer > 0f
        };

        foreach (var mob in allMobs)
        {
            if (mob is AndroidMob android)
            {
                android.ReceiveGroupStatus(CurrentStatus);
            }
        }
    }
}