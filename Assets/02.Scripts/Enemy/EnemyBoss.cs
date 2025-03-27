using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;

public abstract class EnemyBoss : EnemyBase
{
    protected int bossPhase;

    protected override void Attack()
    {
        throw new System.NotImplementedException();
    }

    protected override void FindPlayer()
    {
        throw new System.NotImplementedException();
    }

    protected override void Move()
    {
        throw new System.NotImplementedException();
    }
}
