using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class FormationController : MonoBehaviour
{
    public float radius = 5f;
    public float spacingAngle = 15f;
    public float maxArcAngle = 180f;
    public float separationDistance = 1.5f;

    private MobManager manager;

    private void Start()
    {
        manager = MobManager.Instance;
    }

    private void LateUpdate()
    {
        if (manager == null || manager.allMobs.Count == 0) return;

        Vector3 center = manager.PlayerPosition;
        Vector3 forward = Vector3.forward; // 나중엔 플레이어 forward 사용

        // 역할에 따라 분리
        var fighters = manager.allMobs.Where(m => m.Role == FormationRole.Fighter).ToList();
        var supporters = manager.allMobs.Where(m => m.Role == FormationRole.Supporter).ToList();

        PlaceInArc(fighters, center, forward, radius);
        PlaceInArc(supporters, center, -forward, radius + 1.5f); // 약간 뒤쪽
    }

    private void PlaceInArc(List<Mankind> list, Vector3 center, Vector3 direction, float arcRadius)
    {
        int count = list.Count;
        if (count == 0) return;

        float totalAngle = Mathf.Min(maxArcAngle, spacingAngle * (count - 1));
        float startAngle = -totalAngle / 2f;

        for (int i = 0; i < count; i++)
        {
            Mankind mob = list[i];
            float angle = startAngle + spacingAngle * i;

            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);
            Vector3 offset = rot * direction.normalized * arcRadius;
            Vector3 targetPos = center + offset;

            // 단순 Separation (중복 시 밀어냄)
            foreach (var other in list)
            {
                if (other == mob) continue;
                Vector3 diff = targetPos - other.transform.position;
                if (diff.magnitude < separationDistance)
                    targetPos += diff.normalized * (separationDistance - diff.magnitude);
            }

            mob.transform.position = Vector3.Lerp(mob.transform.position, targetPos, 0.2f);
        }
    }
}