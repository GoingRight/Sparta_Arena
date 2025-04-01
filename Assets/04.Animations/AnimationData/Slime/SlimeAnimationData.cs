using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SlimeAnimationData
{
    [SerializeField] private string moveParameterName = "IsMove";
    [SerializeField] private string attackParameterName = "IsAttack";
    [SerializeField] private string damageParameterName = "IsDamaged";
    [SerializeField] private string replicateParameterName = "IsReplicate";

    public int MoveParameterHash {  get; private set; }
    public int AttackParameterHash { get; private set; }
    public int DamageParameterHash { get; private set; }
    public int ReplicateParameterHash { get; private set;}

    public void Initialize()
    {
        MoveParameterHash = Animator.StringToHash(moveParameterName);
        AttackParameterHash = Animator.StringToHash(attackParameterName);
        DamageParameterHash = Animator.StringToHash(damageParameterName);
        ReplicateParameterHash = Animator.StringToHash(replicateParameterName);
    }
}
