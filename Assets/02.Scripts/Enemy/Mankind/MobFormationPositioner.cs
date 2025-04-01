using UnityEngine;
using System.Collections.Generic;
using Akasha;

public class MobFormationPositioner : Goldbug
{
    public float baseRadius = 3f;
    public float spacingPerMob = 0.6f;

    private MobManager manager;
    private List<AndroidMobController> mobControllers = new();
    private Transform player => manager?.player?.transform;

    public void Setup(MobManager mobManager)
    {
        manager = mobManager;

        RxBinder.BindEach(manager.AllMobs, onAdd: _ => RefreshMobControllers(), context: this);

        RefreshMobControllers(); // 초기 1회 보장
    }

    public void RefreshMobControllers()
    {
        mobControllers.Clear();
        //if (manager.AllMobs.Value.Count <= 0) return;
        foreach (var mobModel in manager.AllMobs.Value)
        {
            if (mobModel.ReactiveOwner is AndroidMobEntity entity)
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
        int count = mobControllers.Count;
        float radius = baseRadius + spacingPerMob * count / (2 * Mathf.PI);

        for (int i = 0; i < count; i++)
        {
            var mob = mobControllers[i];
            float angle = (2 * Mathf.PI * i) / count;

            Vector3 targetOffset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            Vector3 targetPosition = playerPos + targetOffset;
            Vector3 direction = (targetPosition - mob.transform.position);

            Vector3 moveDir = direction.normalized * Mathf.Clamp(direction.magnitude, 0f, 1f);

            mob.Move(moveDir);
        }
    }
}