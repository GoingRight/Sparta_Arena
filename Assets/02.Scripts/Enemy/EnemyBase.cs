using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyBase : Character
{
    protected override void TakeDamage(float damage)
    {
        throw new System.NotImplementedException();
    }

    protected abstract void Move();
    protected abstract void FindPlayer();
    protected abstract void Attack();
}
