using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Player player;
    private EnemyBoss boss;
    public EnemyBoss Boss
    {
        get { return boss; }
        set
        {
            boss = value;
            UIManager.Instance.mainUI.bossNameTxt.text = Boss.bossName;
            UIManager.Instance.mainUI.SetBossHPBar();
            UIManager.Instance.mainUI.bossInfo.SetActive(true);
        }
    }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (player == null)
            player = FindFirstObjectByType<Player>();
    }

    public void Victory()
    {
        //승리화면 띄우기
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0;
        UIManager.Instance.mainUI.victoryUI.gameObject.SetActive(true);
    }
}
