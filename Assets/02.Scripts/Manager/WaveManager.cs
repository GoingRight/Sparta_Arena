using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    static WaveManager instance;
    public static WaveManager Instance { get { return instance; } private set { instance = value; } }

    public List<WaveData> waves = new List<WaveData>();
    private List<WaveData> mobWaves = new List<WaveData>();
    private List<WaveData> bossWaves = new List<WaveData>();

    public int waveLength;
    public int monsterCount;
    private int[] randomMonsterType;

    [HideInInspector] public int curWaveIndex;
    public Transform spawnPosition;

    private void Awake()
    {
        if (instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        Initialize();
    }


    public void Initialize()
    {
        for(int i = 0; i < waves.Count; i++)
        {
            if (waves[i].isBossWave)
            {
                bossWaves.Add(waves[i]);
            }
            else
            {
                mobWaves.Add(waves[i]);
            }
        }

        waveLength = Mathf.Clamp(waveLength, 0, mobWaves.Count);
        randomMonsterType = new int[waveLength];
    }

    public void WaveStart()
    {
        if(curWaveIndex <= waveLength)
        {
            SpawnMob(randomMonsterType[curWaveIndex-1]);
        }
        else
        {
            SpawnBoss();
        }
    }

    public void ResetWave()
    {
        List<int> list = new List<int>();
        for (int i = 0; i < mobWaves.Count; i++)
        {
            list.Add(i);
        }

        for (int i = 0; i < waveLength; i++)
        {
            int randomValue = Random.Range(0, list.Count);
            randomMonsterType[i] = list[randomValue];
            list.RemoveAt(randomValue);
        }
        curWaveIndex = 5;
    }

    public void OnSceneLoded()
    {
        ResetWave();
        Invoke("WaveStart", 2f);
    }

    public void SubtractMonsterCount()
    {
        WaveManager.Instance.monsterCount--;
        if (monsterCount <= 0)
        { 
            curWaveIndex++;
            Invoke("WaveStart", 2f);
        }
    }

    public void SpawnMob(int waveIndex)
    {
        if (mobWaves[waveIndex] == null) return;
        for(int i = 0; i < mobWaves[waveIndex].monsterCount; i++)
        {
            Instantiate(mobWaves[waveIndex].monsterPrefab, (spawnPosition.transform.position+Vector3.forward*2*i), Quaternion.identity, spawnPosition);
            monsterCount++;
        }
    }

    public void SpawnBoss()
    {
        int randomValue = Random.Range(0, bossWaves.Count);
        Instantiate(bossWaves[randomValue].monsterPrefab, spawnPosition.transform.position, Quaternion.identity, spawnPosition);
        monsterCount++;
    }

    //public IEnumerator SpawnWaveCo()
    //{
    //    for(int i = 0; i<waveLength; i++)
    //    {
    //        SpawnMob(randomMonsterType[i]);
    //        yield return new WaitUntil(() =>spawnPosition.childCount == 0); // 몬스터를 모두 처치할 때까지 대기
    //        yield return new WaitForSeconds(3); //Wave 처리하고 3초 정도 대기 시간
    //        curWaveIndex++;
    //    }
    //    SpawnBoss();
    //}
}
