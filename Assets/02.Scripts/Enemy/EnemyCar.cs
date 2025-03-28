using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCar : EnemyBoss
{
    private Vector3 curPlayerPosition;
    private Player player;
    private Rigidbody _rb;
    private Coroutine dashCoroutine;
    private Light[] lights;

    private void Awake()
    {
        bossPhase = 1;
        _rb = GetComponent<Rigidbody>();
        lights = GetComponentsInChildren<Light>();
        foreach(Light light in lights)
        {
            light.intensity = 0;
        }
    }

    private void Start()
    {
        player = GameManager.instance.player;
        FindPlayer();
        dashCoroutine = StartCoroutine(DashCo());
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
        if(stat.CurrentHP/stat.MaxHP < 0.4f)
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
            light.intensity = 1;
        }
        if(Quaternion.Angle(player.transform.rotation,
            Quaternion.LookRotation(transform.position - curPlayerPosition))< 60f)//플레이어가 차를 바라보는 방향에서 60도 
        {
            //눈부심 UI켜기
        }
        yield return new WaitForSeconds(0.1f);
        foreach(var light in lights)
        {
            light.intensity = 0;
        }
    }

    private IEnumerator DashCo()
    {
        StartCoroutine(RotateCo());
        yield return new WaitForSeconds(2); //플레이어가 피할 시간을 줌
        if(bossPhase == 2)
        {
            Attack();
        }
        _rb.AddForce(Vector3.forward * stat.Speed, ForceMode.Impulse);
        yield return new WaitForSeconds(2); //공격후 잠시 멈춰있음
        dashCoroutine = StartCoroutine(DashCo());
    }

    private IEnumerator RotateCo()
    {
        Vector3 curPosition = transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(curPlayerPosition - curPosition);
        while(transform.rotation == targetRotation)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 20 * Time.deltaTime); // 1초에 20도씩 회전
            targetRotation = Quaternion.LookRotation(curPlayerPosition - curPosition);
            yield return null;
        }
    }


}
