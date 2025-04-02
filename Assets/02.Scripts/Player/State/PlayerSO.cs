using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[Serializable]
public class PlayerGroundData
{
    [field: SerializeField][field: Range(0f, 25f)] public float BaseRotationDamping { get; private set; } = 1f;
}

[Serializable]
public class PlayerAirData
{
    [field: Header("JumpData")]
    [field: SerializeField][field: Range(100f, 2000f)] public float JumpForce { get; private set; } = 500f;
}

[Serializable]
public class PlayerAttackData
{
    [field: SerializeField] public List<AttackInfoData> AttackInfoDatas { get; private set; }
    public int GetAttackInfoCount() { return AttackInfoDatas.Count; }
    public AttackInfoData AttackInfoData(int index) { return AttackInfoDatas[index]; }
}

[Serializable]
public class AttackInfoData
{
    [field: SerializeField] public string AttackName { get; private set; }
    [field: SerializeField] public int ComboStateIndex { get; private set; }
    [field: SerializeField][field: Range(0f, 1f)] public float ComboTransitionTime { get; private set; }
    [field: SerializeField][field: Range(0f, 1f)] public float ForceTranstionTime { get; private set; }
    [field: SerializeField][field: Range(-10f, 10f)] public float Force { get; private set; }
    [field: SerializeField] public int Damage;
    [field: SerializeField][field: Range(0f, 1f)] public float Dealing_Start_TransitionTime { get; private set; }
    [field: SerializeField][field: Range(0f, 1f)] public float Dealing_End_TransitionTime { get; private set; }
}

[CreateAssetMenu(fileName = "PlayerSO", menuName = "Characters/Player", order = 1)]
public class PlayerSO : ScriptableObject
{
    [field: SerializeField] public PlayerGroundData GroundData { get; private set; }
    [field: SerializeField] public PlayerAirData AirData { get; private set; }
    [field: SerializeField] public PlayerAttackData AttackData { get; private set; }

}