using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    [Header("Status")]
    [SerializeField] public Status stat;

    protected virtual void TakeDamage(float damage)
    {
        
    }
}
