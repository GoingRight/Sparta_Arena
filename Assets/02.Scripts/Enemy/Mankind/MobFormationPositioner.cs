using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Akasha;

public class MobFormationPositioner : MonoBehaviour
{
    public float spacing = 1.2f;
    public float stopDistance = 0.6f;
    public float moveDamp = 5f;
    public float maxSpeed = 3f;
    public float minSpeed = 0.5f;
    public float speedCurveExponent = 2f;
    public float formationRadius = 6f;
    public float ringSpacing = 1.5f;
    public float idealSpacing = 1.8f;
    public float radiusBoost = 2f;
    public float wanderStrength = 0.5f;
    public float flankOffsetAngle = 45f; // ⬅️ 새 변수: 우회 각도

    private MobManager manager;
    private List<IMobController> mobControllers = new();
    private Transform player => manager?.player?.transform;

    public void Setup(MobManager mobManager)
    {
        manager = mobManager;
        RefreshMobControllers();
    }

    public void RefreshMobControllers()
    {
        mobControllers.Clear();
        if (manager == null || manager.AllMobs == null) return;

        foreach (var mobModel in manager.AllMobs.Value)
        {
            if (mobModel.ReactiveOwner is BaseEntity entity)
            {
                var controller = entity.GetComponent<IMobController>();
                if (controller != null)
                    mobControllers.Add(controller);
            }
        }
    }

    private void Update()
    {
        if (player == null || mobControllers.Count == 0) return;

        StrategyType strategy = manager.GroupStatus.GroupStrategy.Value;

        float strategyRadius = formationRadius;
        float strategyFleeStrength = 0f;
        float strategyAggression = 1f;

        switch (strategy)
        {
            case StrategyType.Retreat:
                strategyRadius = 9f;
                strategyFleeStrength = 1f;
                strategyAggression = 0f;
                break;
            case StrategyType.Attack:
                strategyRadius = 4.5f;
                strategyFleeStrength = 0f;
                strategyAggression = 1.5f;
                break;
            case StrategyType.Hold:
            default:
                strategyRadius = 6f;
                strategyFleeStrength = 0.5f;
                strategyAggression = 1f;
                break;
        }

        Vector3 playerPos = player.position;
        Vector3 playerForward = player.forward;

        int count = mobControllers.Count;
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            var mob = mobControllers[i];

            int ringIndex = i / Mathf.Max(1, count / 2);
            float radiusOffset = ringIndex * ringSpacing;
            float radius = strategyRadius + radiusOffset;

            float angleDeg = angleStep * i;
            float angleRad = angleDeg * Mathf.Deg2Rad;
            Vector3 baseOffset = new Vector3(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad));

            float flankSign = (i % 2 == 0) ? 1f : -1f;
            Vector3 flankOffset = Quaternion.AngleAxis(flankOffsetAngle * flankSign, Vector3.up) * (mob.transform.position - playerPos).normalized;

            Vector3 formationOffset = (baseOffset + flankOffset).normalized * radius;
            Vector3 targetPos = playerPos + formationOffset;

            bool isFleeing = Vector3.Distance(mob.transform.position, playerPos) < radius - 1f;

            Vector3 separation = Vector3.zero;
            Vector3 alignment = Vector3.zero;
            Vector3 cohesion = Vector3.zero;
            int neighborCount = 0;

            float avgSpacing = 0f;
            foreach (var other in mobControllers)
            {
                if (other == mob) continue;
                Vector3 diff = mob.transform.position - other.transform.position;
                float dist = diff.magnitude;

                if (dist < 3f)
                {
                    if (isFleeing && dist < 2f)
                        separation += diff.normalized * (2f - dist);

                    separation += diff.normalized / Mathf.Max(dist, 0.1f);

                    if (Vector3.Distance(other.transform.position, playerPos) < radius - 1f)
                        separation += diff.normalized * (1.5f - dist);

                    alignment += other.MoveDirection;
                    cohesion += other.transform.position;
                    neighborCount++;
                    avgSpacing += dist;
                }
            }

            if (neighborCount > 0)
            {
                alignment = (alignment / neighborCount).normalized;
                cohesion = ((cohesion / neighborCount) - mob.transform.position).normalized;
                avgSpacing /= neighborCount;
            }

            float spacingDelta = Mathf.Clamp01((idealSpacing - avgSpacing) / idealSpacing);
            float dynamicRadius = radius + spacingDelta * radiusBoost;

            Vector3 toTarget = (playerPos + formationOffset.normalized * dynamicRadius) - mob.transform.position;
            float distance = toTarget.magnitude;

            float t = Mathf.Pow(Mathf.InverseLerp(0f, 10f, distance), speedCurveExponent);
            float speed = Mathf.Lerp(minSpeed, maxSpeed, t);

            Vector3 fleeFromPlayer = Vector3.zero;
            float fleeDistance = dynamicRadius + 2f;
            float distToPlayer = Vector3.Distance(mob.transform.position, playerPos);
            if (distToPlayer < fleeDistance)
            {
                Vector3 fleeDir = (mob.transform.position - playerPos).normalized;
                Vector3 lateralDir = Vector3.Cross(Vector3.up, playerForward);
                float lateralInfluence = Vector3.Dot(fleeDir, lateralDir);
                Vector3 adjustedFleeDir = lateralDir * Mathf.Sign(lateralInfluence);
                fleeFromPlayer = Vector3.Lerp(fleeDir, adjustedFleeDir, 0.8f) * Mathf.Clamp01((fleeDistance - distToPlayer) / fleeDistance);
            }

            Vector3 escapeVector = (-alignment).normalized * wanderStrength;

            Vector3 moveDir = (toTarget.normalized * 2f + separation * 1f + alignment * 0.5f + cohesion * 0.5f + escapeVector).normalized * speed;

            if (distToPlayer < dynamicRadius - 1f)
            {
                moveDir = Vector3.Lerp(moveDir, fleeFromPlayer.normalized * speed, 1f);
            }

            if (distance < stopDistance || moveDir.magnitude < 0.1f)
            {
                moveDir = Vector3.zero;
            }
            else
            {
                moveDir = Vector3.Lerp(mob.MoveDirection, moveDir, Time.deltaTime * moveDamp);
            }

            mob.Move(moveDir);
        }
    }
}