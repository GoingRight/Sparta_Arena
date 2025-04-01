using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerAnimationData
{
    [SerializeField] private string groundParameterName = "@Ground";
    [SerializeField] private string idleParameterName = "Idle";
    [SerializeField] private string walkParameterName = "Walk";
    [SerializeField] private string runParameterName = "Run";
    [SerializeField] private string airParameterName = "@Air";
    [SerializeField] private string jumpParameterName = "Jump";
    [SerializeField] private string fallParameterName = "Fall";
    [SerializeField] private string meleeAttackParameterName = "@MeleeAttack";
    [SerializeField] private string comboAttackParameterName = "ComboAttack";
    [SerializeField] private string rangeAttackParameterName = "@RangeAttack";

    public int GroundParameterHash { get; private set; }
    public int IdleParameterHash { get; private set; }
    public int WalkParameterHash { get; private set; }
    public int RunParameterHash { get; private set; }
    public int AirParameterHash { get; private set; }
    public int JumpParameterHash { get; private set; }
    public int FallParameterHash { get; private set; }
    public int MeleeAttackParameterHash { get; private set; }
    public int ComboAttackParameterHash { get; private set; }
    public int RangeAttackParameterHash { get; private set; }

    public void Initialize()
    {
        GroundParameterHash = Animator.StringToHash(groundParameterName);
        IdleParameterHash = Animator.StringToHash(idleParameterName);
        WalkParameterHash = Animator.StringToHash(walkParameterName);
        RunParameterHash = Animator.StringToHash(runParameterName);
        AirParameterHash = Animator.StringToHash(airParameterName);
        JumpParameterHash = Animator.StringToHash(jumpParameterName);
        FallParameterHash = Animator.StringToHash(fallParameterName);
        MeleeAttackParameterHash = Animator.StringToHash(meleeAttackParameterName);
        ComboAttackParameterHash = Animator.StringToHash(comboAttackParameterName);
        RangeAttackParameterHash = Animator.StringToHash(rangeAttackParameterName);

        Animator anim = new Animator();
        //anim.SetBool(GroundParameterHash, true);
    }
}
