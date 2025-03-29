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
    [SerializeField] private string attackParameterName = "@Attack";
    [SerializeField] private string comboAttackParameterName = "ComboAttack";

    public int GroundParameterHash { get; private set; }
    public int IdleParameterHash { get; private set; }
    public int WalkParameterHash { get; private set; }
    public int RunParameterHash { get; private set; }
    public int airParameterHash { get; private set; }
    public int jumpParameterHash { get; private set; }
    public int fallParameterHash { get; private set; }
    public int attackParameterHash { get; private set; }
    public int comboAttackParameterHash { get; private set; }

    public void Initialize()
    {
        GroundParameterHash = Animator.StringToHash(groundParameterName);
        IdleParameterHash = Animator.StringToHash(idleParameterName);
        WalkParameterHash = Animator.StringToHash(walkParameterName);
        RunParameterHash = Animator.StringToHash(runParameterName);
        airParameterHash = Animator.StringToHash(airParameterName);
        jumpParameterHash = Animator.StringToHash(jumpParameterName);
        fallParameterHash = Animator.StringToHash(fallParameterName);
        attackParameterHash = Animator.StringToHash(attackParameterName);
        comboAttackParameterHash = Animator.StringToHash(comboAttackParameterName);

        Animator anim = new Animator();
        //anim.SetBool(GroundParameterHash, true);
    }
}
