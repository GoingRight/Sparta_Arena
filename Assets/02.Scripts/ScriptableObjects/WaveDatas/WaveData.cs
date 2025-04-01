using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new WaveData", menuName ="new Wave")]
public class WaveData : ScriptableObject
{
    public GameObject monsterPrefab;
    public int monsterCount;
    public bool isBossWave;
}
