using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Akasha;

public class MobFormationPositioner : MonoBehaviour
{
    public float spacing = 1.2f;
    public float stopDistance = 0.2f;
    public float moveDamp = 10f;
    public float maxSpeed = 3f;
    public float minSpeed = 0.5f;
    public float speedCurveExponent = 2f;
    public float formationRadius = 6f;

    private MobManager manager;
    private List<AndroidMobController> mobControllers = new();
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
            if (mobModel.Owner is AndroidMobEntity entity)
            {
                var controller = entity.GetComponent<AndroidMobController>();
                if (controller != null)
                    mobControllers.Add(controller);
            }
        }
    }

    private void Update()
    {
        if (player == null || mobControllers.Count == 0) return;

        Vector3 playerPos = player.position;
        Vector3 playerForward = player.forward;
        Vector3 playerRight = Vector3.Cross(Vector3.up, playerForward);

        int count = mobControllers.Count;
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            var mob = mobControllers[i];

            float angleDeg = angleStep * i;
            float angleRad = angleDeg * Mathf.Deg2Rad;
            Vector3 formationOffset = new Vector3(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad)) * formationRadius;
            Vector3 targetPos = playerPos + formationOffset;

            bool isFleeing = Vector3.Distance(mob.transform.position, playerPos) < formationRadius - 1f;

            // 기본 Boids 요소 계산
            Vector3 separation = Vector3.zero;
            Vector3 alignment = Vector3.zero;
            Vector3 cohesion = Vector3.zero;
            int neighborCount = 0;

            foreach (var other in mobControllers)
            {
                if (other == mob) continue;
                Vector3 diff = mob.transform.position - other.transform.position;
                float dist = diff.magnitude;

                if (dist < 3f)
                {
                    // fleeing 애가 끼어있다면 나도 살짝 밀려나는 힘 반영
                    if (isFleeing && dist < 2f)
                    {
                        separation += (mob.transform.position - other.transform.position).normalized * (2f - dist);
                    }
                    {
                        separation += diff.normalized / Mathf.Max(dist, 0.1f);

                        // 플레이어를 피하려는 몹은 진형 유지 몹에게도 밀어내는 효과
                        if (Vector3.Distance(other.transform.position, playerPos) < formationRadius - 1f)
                        {
                            separation += (mob.transform.position - other.transform.position).normalized * (1.5f - dist);
                        }
                        alignment += other.MoveDirection;
                        cohesion += other.transform.position;
                        neighborCount++;
                    }
                }

                if (neighborCount > 0)
                {
                    alignment = (alignment / neighborCount).normalized;
                    cohesion = ((cohesion / neighborCount) - mob.transform.position).normalized;
                }

                Vector3 toTarget = targetPos - mob.transform.position;
                float distance = toTarget.magnitude;

                float t = Mathf.Pow(Mathf.InverseLerp(0f, 10f, distance), speedCurveExponent);
                float speed = Mathf.Lerp(minSpeed, maxSpeed, t);

                Vector3 fleeFromPlayer = Vector3.zero;
                float fleeDistance = formationRadius + 2f;
                float distToPlayer = Vector3.Distance(mob.transform.position, playerPos);
                if (distToPlayer < fleeDistance)
                {
                    Vector3 fleeDir = (mob.transform.position - playerPos).normalized;
                    Vector3 lateralDir = Vector3.Cross(Vector3.up, playerForward);
                    float lateralInfluence = Vector3.Dot(fleeDir, lateralDir);
                    Vector3 adjustedFleeDir = lateralDir * Mathf.Sign(lateralInfluence);
                    fleeFromPlayer = Vector3.Lerp(fleeDir, adjustedFleeDir, 0.8f) * Mathf.Clamp01((fleeDistance - distToPlayer) / fleeDistance);
                }

                Vector3 moveDir = (toTarget.normalized * 2f + separation * 1f + alignment * 0.5f + cohesion * 0.5f).normalized * speed;

                if (distToPlayer < formationRadius - 1f)
                {
                    moveDir = Vector3.Lerp(moveDir, fleeFromPlayer.normalized * speed, 1f);
                }

                if (distance < stopDistance)
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
}