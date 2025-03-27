using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    public Status stat;

    protected virtual void TakeDamage(float damage)
    {
        stat.CurrentHP = Mathf.Clamp(stat.CurrentHP - damage, 0, stat.MaxHP);
    }
}
