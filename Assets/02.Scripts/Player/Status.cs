using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class Status
{
    [SerializeField] private float attack;
    public float Attack { get { return attack; } set { attack = value; } }

    [SerializeField] private float defence;
    public float Defence { get { return defence; } set { defence = value; } }

    [SerializeField] private float maxHP;
    public float MaxHP { get { return maxHP; } set { maxHP = value; } }

    [SerializeField] private float currentHP;
    public float CurrentHP { get { return currentHP; } set { currentHP = value; } }

    [SerializeField] private float speed;
    public float Speed { get { return speed; } set { speed = value; } }
}
