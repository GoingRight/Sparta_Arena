using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyChicken : EnemyBoss
{
    #region States
    private enum EnemyState
    {
        Idle,
        Moving,
        Chasing,
        Attacking
    }
    #endregion

    #region Inspector Variables
    [Header("감지 범위")]
    [SerializeField] private float detectRange = 3f; // 플레이어 감지 범위
    [SerializeField] private float attackRange = 1f; // 공격 범위

    [Header("이동")]
    [SerializeField] private float rotateSpeed = 5f; // 회전 속도
    [SerializeField] private float chaseSpeed = 1.5f; // 추적 속도
    [SerializeField] private float minMoveDistance = 1f; // 최소 이동 범위
    [SerializeField] private float maxMoveDistance = 7f; // 최대 이동 범위
    [SerializeField] private float minIdleTime = 2f; // 최소 Idle 시간
    [SerializeField] private float maxIdleTime = 5f; // 최대 Idle 시간

    [Header("공격")]
    [SerializeField] private float attackCooldown = 2f; // 공격 쿨타임
    [SerializeField] private float attackCheckInterval = 0.1f; // 공격 확인 시간

    [Header("2페이즈")]
    [SerializeField] private float phase2Speed = 1.5f; // 2페이즈 속도 배수
    [SerializeField] private float phase2Damage = 1.5f; // 2페이즈 공격 배수
    #endregion

    #region Private Variables
    private Transform player;
    private ChickenAnimationController animController;
    private Vector3 targetPosition;
    private float baseSpeed; // 기본 속도
    private float moveTimer = 0f; // 이동 쿨타임 초기화
    private float nextMoveTime = 0f; // 이동 시간
    private float attackTimer = 0f; // 공격 쿨타임 초기화
    private float nextAttackTime = 0f; // 공격 시간

    private EnemyState currentState = EnemyState.Idle; // 상태 Idle 기본값
    private bool canAttack = true; // 공격 가능 여부
    private bool isDetected = false; // 플레이어 감지 여부
    private bool isAttackRange = false; // 공격 가능 여부
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        InitializeComponents();
        SetInitialState();
    }

    private void Update()
    {
        if (player == null) return;

        UpdateTimers();
        FindPlayer();
        Move();
        CheckAndEnterPhase2();
    }
    #endregion

    #region Initialization
    private void InitializeComponents() // 컴포넌트 초기화
    {
        animController = GetComponent<ChickenAnimationController>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        baseSpeed = stat.Speed;
    }

    private void SetInitialState() // 상태 초기화
    {
        SetNextMoveTime();
        RandomDestination();
    }
    #endregion

    #region Update Logic
    private void UpdateTimers() // 공격 & 이동 타이머
    {
        if (!canAttack) // 공격 쿨다운 업데이트
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                canAttack = true;
                attackTimer = 0f;
                CheckUpdateState();
            }
        }

        nextAttackTime += Time.deltaTime; // 공격 범위 체크 타이머 업데이트
        if (nextAttackTime >= attackCheckInterval)
        {
            nextAttackTime = 0f;
            CheckAttackRange();
        }

        if (currentState != EnemyState.Attacking && currentState != EnemyState.Chasing) // 이동 타이머 업데이트
        {
            moveTimer += Time.deltaTime;
        }
    }

    private void CheckAttackRange() // 공격 가능 여부
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        isAttackRange = distanceToPlayer <= attackRange;

        if (!isAttackRange && currentState == EnemyState.Attacking) // 공격 범위 벗어났을 때 상태 전환
        {
            currentState = EnemyState.Idle;
            animController.OnAttackAnimationComplete();
            CheckUpdateState();
        }
    }

    private void CheckUpdateState() // 거리 기반 상태 결정
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange && canAttack) // 공격 범위 체크
        {
            Attack();
            return;
        }

        if (distanceToPlayer <= detectRange) // 추적 범위 체크
        {
            EnterChaseState();
            return;
        }

        ResetRoaming();
    }
    #endregion

    #region Movement
    private void ChasePlayer() // 플레이어 추적
    {
        if (currentState == EnemyState.Attacking) return; // 공격 중이면 종료

        Vector3 direction = (player.position - transform.position).normalized; // 플레이어 향해 이동 및 회전
        direction.y = 0f;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(direction),
            rotateSpeed * Time.deltaTime
        );

        float distanceToPlayer = Vector3.Distance(transform.position, player.position); // 거리 계산
        if (distanceToPlayer > attackRange) // 바라보는 방향으로 이동
        {
            transform.position += transform.forward * (stat.Speed * chaseSpeed) * Time.deltaTime;
        }
        else if (canAttack) // 공격
        {
            Attack();
        }
    }

    private void HandleRoaming() // 배회 중 상태 전환
    {
        if (currentState == EnemyState.Attacking || currentState == EnemyState.Chasing) return; // 공격 or 추적 중 종료

        if (currentState == EnemyState.Idle && moveTimer >= nextMoveTime) // 이동 시작
        {
            StartMoveState();
        }
        else if (currentState == EnemyState.Moving) // 목표 지점을 향한 이동
        {
            MoveTowardsTarget();
        }
    }

    private void MoveTowardsTarget() // 목표 지점 이동
    {
        Vector3 directionTarget = (targetPosition - transform.position).normalized; // 방향 설정
        directionTarget.y = 0f;

        float distanceTarget = Vector3.Distance(transform.position, targetPosition); // 목표 거리 설정

        if (distanceTarget < 0.5f) // 도달 확인
        {
            StartIdleState();
            return;
        }

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(directionTarget),
            rotateSpeed * Time.deltaTime
        );

        transform.position += transform.forward * stat.Speed * Time.deltaTime; // 이동
    }
    #endregion

    #region State Management
    private void EnterChaseState() // 추적 상태
    {
        if (currentState == EnemyState.Chasing) return;

        currentState = EnemyState.Chasing;
        animController.SetMoving(false);
        animController.SetRunning(true);
    }

    private void StartIdleState() // 정지 상태
    {
        if (currentState == EnemyState.Idle) return;

        currentState = EnemyState.Idle;
        animController.SetMoving(false);
        SetNextMoveTime();
    }

    private void StartMoveState() // 이동 상태
    {
        if (currentState == EnemyState.Moving) return;

        currentState = EnemyState.Moving;
        animController.SetMoving(true);
        RandomDestination();
    }

    private void ResetRoaming() // 배회 상태
    {
        if (currentState == EnemyState.Idle) return;

        currentState = EnemyState.Idle;
        stat.Speed = baseSpeed;
        animController.SetRunning(false);
        animController.SetMoving(false);
        SetNextMoveTime();
    }
    #endregion

    #region Combat
    protected override void Attack() // 공격 로직
    {
        if (currentState == EnemyState.Attacking || !canAttack) return; // 중복 공격 방지

        float distanceToPlayer = Vector3.Distance(transform.position, player.position); // 공격 거리 계산
        if (distanceToPlayer > attackRange) return; // 거리 초과 시 종료

        Vector3 direction = (player.position - transform.position).normalized; // 방향 계산
        direction.y = 0f;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(direction),
            rotateSpeed * Time.deltaTime
        );

        currentState = EnemyState.Attacking;
        canAttack = false;
        attackTimer = 0f;

        animController.SetMoving(false);
        animController.SetRunning(false);
        animController.SetAttacking(true);
        animController.TriggerAttack();
    }

    private void CheckAndEnterPhase2() // 페이즈 전환 여부
    {
        if (bossPhase == 1 && stat.CurrentHP <= stat.MaxHP * 0.5f)
        {
            EnterPhase2();
        }
    }

    private void EnterPhase2() // 페이즈 전환
    {
        bossPhase = 2;
        stat.Speed *= phase2Speed;
        stat.Attack *= phase2Damage;
        baseSpeed = stat.Speed;
    }
    #endregion

    #region Utility
    private void RandomDestination() // 목적지 설정
    {
        float randomDistance = Random.Range(minMoveDistance, maxMoveDistance); // 거리 설정
        float randomAngle = Random.Range(0f, 360f); // 방향 설정

        Vector3 randomDirection = Quaternion.Euler(0f, randomAngle, 0f) * Vector3.forward; // 방향 계산
        targetPosition = transform.position + randomDirection * randomDistance; // 목적지 설정
    }

    private void SetNextMoveTime() // 이동 시간 초기화
    {
        moveTimer = 0f;
        nextMoveTime = Random.Range(minIdleTime, maxIdleTime);
    }
    #endregion

    #region Animation Events
    public void OnAttackAnimationComplete()
    {
        currentState = EnemyState.Idle;
        animController.OnAttackAnimationComplete();

        CheckUpdateState(); // 상태 체크 수행
    }
    #endregion

    #region Interface Implementation
    protected override void FindPlayer() // 플레이어 감지 로직
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool wasDetected = isDetected;
        isDetected = distanceToPlayer <= detectRange;

        if (currentState == EnemyState.Attacking) return; // 공격 중이면 종료

        if (currentState != EnemyState.Attacking) // 공격 상태 아닐 시 플레이어 감지 상태 변경
        {
            if (isDetected && !wasDetected) // 감지 됐을 시 추적
            {
                EnterChaseState();
            }
            else if (!isDetected && wasDetected) // 아닐 시 배회
            {
                ResetRoaming();
            }
        }
    }

    protected override void Move() // 이동 로직
    {
        if (currentState == EnemyState.Attacking) return;

        if (isDetected)
        {
            ChasePlayer();
        }
        else
        {
            HandleRoaming();
        }
    }
    #endregion
}