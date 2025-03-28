using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyChicken : EnemyBoss
{
    [Header("Detection Settings")]
    [SerializeField] private float detectRange = 5f; // 인식 범위
    [SerializeField] private float attackRange = 1f; // 공격 범위
    
    [Header("Attack Settings")]
    [SerializeField] private float rushSpeed = 3f; // 달리기 속도
    [SerializeField] private float rushDuration = 0.5f; // 지속 시간
    // [SerializeField] private GameObject projectilePrefab; // 알 프리팹
    // [SerializeField] private float projectileSpeed = 8f; // 투사체 속도
    // [SerializeField] private float projectileDamage = 5f; // 투사체 대미지
    // [SerializeField] private float phase2AttackInterval = 2f; // Phase 2에서의 공격 간격
    
    private bool isChasing = false; // 움직임 여부
    private float randomMoveTimer = 0f; // 움직임 시간 초기화
    private float randomMoveInterval = 2f; // 움직임 시간 간격
    private Vector3 randomDirection; // 무작위 방향
    private Transform player; // 플레이어 위치
    private float phase2AttackTimer = 0f; // 공격 시간 초기화
    private bool isPhase2RushComplete = false; // 페이즈 전환

    private Animator Animator;

    private void Start()
    {
        bossPhase = 1;
        randomDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        Animator = GetComponent<Animator>();
    }

    private void Update()
    {
        Move();
        FindPlayer();
    }

    protected override void Attack()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance < attackRange)
        {
            if (bossPhase == 1)
            {
                StartCoroutine(ChickenRushAttack());
            }
            // else if (bossPhase == 2)
            // {
            //     phase2AttackTimer += Time.deltaTime;
            //     if (phase2AttackTimer >= phase2AttackInterval)
            //     {
            //         if (!isPhase2RushComplete)
            //         {
            //             StartCoroutine(ChickenRushAttack());
            //             isPhase2RushComplete = true;
            //         }
            //         else
            //         {
            //             StartCoroutine(ChickenProjectileAttack());
            //             isPhase2RushComplete = false;
            //         }
            //         phase2AttackTimer = 0f;
            //     }
            // }
        }
    }

    protected override void FindPlayer()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        isChasing = distance < detectRange;

        // 체력이 50% 이하로 떨어지면 페이즈 2로 전환
        if (stat.CurrentHP <= stat.MaxHP * 0.5f && bossPhase == 1)
        {
            bossPhase = 2;
            phase2AttackTimer = 0f;
            isPhase2RushComplete = false;
        }
    }

    protected override void Move()
    {
        if (!isChasing)
        {
            // 랜덤 이동 로직
            randomMoveTimer += Time.deltaTime;
            if (randomMoveTimer >= randomMoveInterval)
            {
                randomDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
                randomMoveTimer = 0f;
            }
            transform.Translate(randomDirection * stat.Speed * Time.deltaTime);
            
            // 이동 방향으로 오브젝트 회전
            if (randomDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(randomDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
            }
        }
        else
        {
            // 플레이어 추적 로직
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0f; // Y축 이동 제한
            transform.Translate(direction * stat.Speed * Time.deltaTime);
            
            // 플레이어 방향으로 오브젝트 회전
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
            }
        }
    }

    private IEnumerator ChickenRushAttack()
    {
        Vector3 targetPos = player.position;
        float elapsedTime = 0f;

        while (elapsedTime < rushDuration)
        {
            Vector3 direction = (targetPos - transform.position).normalized;
            direction.y = 0f; // Y축 이동 제한
            transform.position = Vector3.MoveTowards(transform.position, targetPos, rushSpeed * Time.deltaTime);
            
            // 돌진 방향으로 오브젝트 회전
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 20f * Time.deltaTime);
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator ChickenProjectileAttack()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f; // Y축 이동 제한
        
        // 투사체 발사 방향으로 오브젝트 회전
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 20f * Time.deltaTime);
        }

        // GameObject projectile = Instantiate(projectilePrefab, transform.position + direction * 1f, Quaternion.LookRotation(direction));
        // Rigidbody rb = projectile.GetComponent<Rigidbody>();
        
        // if (rb != null)
        // {
        //     rb.velocity = direction * projectileSpeed;
        // }

        yield return new WaitForSeconds(1f);
    }

    protected override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
    }
}
