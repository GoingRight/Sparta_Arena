using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyCar : EnemyBoss
{
    private Vector3 curPlayerPosition;
    private Player player;
    private Rigidbody _rb;
    private Coroutine dashCoroutine;
    private Light[] lights;
    [SerializeField]private Image flashImage;

    private void Awake()
    {
        bossPhase = 1;
        _rb = GetComponent<Rigidbody>();
        lights = GetComponentsInChildren<Light>();
        foreach (Light light in lights)
        {
            light.intensity = 0;
        }
        bossPhase = 2;
    }

    private void Start()
    {
        player = GameManager.instance.player;
        FindPlayer();
        dashCoroutine = StartCoroutine(DashCo());
        flashImage.gameObject.SetActive(false);
    }
    private void Update()
    {
        FindPlayer();
    }

    protected override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        CheckHP();
    }

    private void CheckHP()
    {
        if (stat.CurrentHP / stat.MaxHP < 0.4f)
        {
            bossPhase = 2;
        }
    }
    protected override void Move() { }

    protected override void Attack()
    {
        StartCoroutine(FlashCo());
    }

    protected override void FindPlayer()
    {
        curPlayerPosition = player.transform.position;
    }

    private IEnumerator FlashCo()
    {
        foreach (var light in lights)
        {
            light.intensity = 5;
        }
        if (Quaternion.Angle(player.transform.rotation,
            Quaternion.LookRotation(transform.position - curPlayerPosition)) < 60f)//플레이어가 차를 바라보는 방향에서 60도 
        {
            Flash();
        }
        yield return new WaitForSeconds(0.1f);
        foreach (var light in lights)
        {
            light.intensity = 0;
        }
    }

    public void Flash()
    {
        StartCoroutine(FadeAway());
    }

    private IEnumerator FadeAway()
    {
        flashImage.color = Color.white;
        flashImage.gameObject.SetActive(true);
        float a = 1;
        while(a > 0)
        {
            a -= 0.5f * Time.deltaTime;
            flashImage.color = new Color(1f, 1f, 1f, a);
            yield return null;
        }
        flashImage.gameObject.SetActive(false);
    }

    private IEnumerator DashCo()
    {
        yield return StartCoroutine(RotateCo());
        yield return new WaitForSeconds(2); //플레이어가 피할 시간을 줌
        if (bossPhase == 2)
        {
            Attack();
        }
        _rb.AddForce(transform.forward * 3500, ForceMode.Impulse);
        yield return new WaitForSeconds(2); //공격후 잠시 멈춰있음
        dashCoroutine = StartCoroutine(DashCo());
    }

    private IEnumerator RotateCo()
    {
        Vector3 distance = curPlayerPosition - transform.position;
        distance.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(distance);
   
        while (transform.rotation != targetRotation)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 50 * Time.deltaTime); // 1초에 50도씩 회전
            yield return null;
        }
    }


}
