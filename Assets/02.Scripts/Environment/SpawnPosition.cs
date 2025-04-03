using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPosition : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        WaveManager.Instance.spawnPosition = this.transform;
    }


}
