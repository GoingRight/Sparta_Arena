using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainSceneUI : MonoBehaviour
{
    public Image playerHPBar;
    public GameObject bossInfo;
    public TextMeshProUGUI bossNameTxt;
    public Image bossHPBar;
    public Image gameOverPanel;
    public Image victoryUI;

    public Button restartBtn;
    private void Start()
    {
        Time.timeScale = 1;
        UIManager.Instance.mainUI = this;
        WaveManager.Instance.OnSceneLoded();
    }

    public void SetPlayerHPBar()
    {
        if(GameManager.Instance.player == null)
        {
            Debug.LogWarning("플레이어 없음");
            return;
        }
        playerHPBar.fillAmount = GameManager.Instance.player.stat.CurrentHP / GameManager.Instance.player.stat.MaxHP;
    }

    public void SetBossHPBar()
    {
        if (GameManager.Instance.Boss == null)
        {
            Debug.LogWarning("보스없음");
            return;
        }
        bossHPBar.fillAmount = GameManager.Instance.Boss.stat.CurrentHP / GameManager.Instance.Boss.stat.MaxHP;
    }

    public void Restart()
    {
        SceneManager.LoadScene("MainScene");
    }
}
