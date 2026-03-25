using DamageNumbersPro;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Constants;

public class EnemyController : MonoBehaviour
{
    public EEnemyState State { get; private set; }
    public Transform TargetTransform { get; private set; } // 플레이어(타겟) Transform

    [HideInInspector]
    public string prefabName; // 적 프리팹 이름

    [SerializeField] private DamageNumber bulletDamagePopup;
    [SerializeField] private DamageNumber skillDamagePopup;
    
    [Header("--- 적 속성 ---")]
    public EnemyStatus enemyStatus;

    [Header("--- 탐지 설정 ---")] //(적 마다 설정 필요)
    [Tooltip("이 거리 안에 들어와야 'Search'에서 'Attack' 상태로 변경됩니다.")]
    [SerializeField] private float patrolDetectionDistance = 15f; // 탐지 거리 (원거리 적 기준)
    [SerializeField] private float verticalDetectionLimit = 5f; // y축 범위 지정;(원거리 적 기준)

    [Header("--- 공격 행동 (전략 패턴) ---")]
    [Tooltip("이 적이 Attack 상태일 때 사용할 '행동 에셋'을 연결하세요.")]
    public EnemyAttackBehavior attackBehavior;

    [Header("--- 궤도 및 이동 설정 (Search 상태) ---")]
    [Tooltip("궤도를 따라 회전하는 속도")]
    [SerializeField] private float orbitMoveSpeed = 9f;

    [Header("--- 플레이어 추적 설정 ---")]

    // 맵의 중심점 (플레이어/스포너와 동일한 중심을 사용)
    private Vector3 pivotPoint = new Vector3(0, 0, 0);

    // 상하 이동 제한 영역 (플레이어 스크립트의 상수와 동일)
    private const float PLAYAREA_Y_LOWERBOUND = 5f;
    private const float PLAYAREA_Y_UPPERBOUND = 20f;
    private float enemyBoundYLower;
    private float enemyBoundYUpper;

    [Header("--- 플레이어 이동 영역 설정 ---")]
    [Tooltip("플레이어 이동 영역의 중심점 (XZ 평면)")]
    [SerializeField] protected Vector3 playerPlayAreaCenter = new Vector3(0, 0, 0); // XZ 평면 중심(pivotPoint와 동일)
    [Tooltip("플레이어 이동 영역의 반지름")]
    [SerializeField] private float playerPlayAreaRadius = 25f; // 파란 원의 반지름 (예시 값)

    [Header("--- 피격 플래시 설정 ---")]
    private Renderer enemyRenderer;
    private Color originalColor;
    private Color originalEmissionColor;
    private const float FLASH_DURATION = 0.05f;
    private Coroutine flashCoroutine = null;

    // 내부 상태 변수
    private Vector3 directionToPlayer;
    protected float currentOrbitDirection; // 현재 궤도 회전 방향 (Start에서 고정됨)

    public GameObject DeathEffectPrefab; // 적 사망 이펙트 프리팹

    // 외부에서 플레이어 이동 영역 정보를 가져올 수 있도록 속성(Property) 추가
    public Vector3 PlayerPlayAreaCenter => playerPlayAreaCenter;
    public float PlayerPlayAreaRadius => playerPlayAreaRadius;

    private Dictionary<EEnemyState, ICharacterState> states;

    private GameObject Player; // 플레이어 참조 저장
    private PlayerStatus PlayerStatus; // 참조 저장

    private void Awake()
    {
        // 1. 공격 행동이 할당되지 않았으면 경고
        if (attackBehavior == null)
        {
            Debug.LogError(gameObject.name + "에 'Attack Behavior'가 할당되지 않았습니다!");
            enabled = false;
            return;
        }

        // 2. 공격 행동(SO)을 복제하여 이 인스턴스만 사용하도록 함
        attackBehavior = Instantiate(attackBehavior);

        // 3. 상태 머신 초기화
        InitializeStateMachine();

        // 4. Y축 경계선 계산
        float ENEMY_HEIGHT = 1.7f;
        enemyBoundYLower = PLAYAREA_Y_LOWERBOUND + (ENEMY_HEIGHT * 0.5f);
        enemyBoundYUpper = PLAYAREA_Y_UPPERBOUND - (ENEMY_HEIGHT * 0.5f);

        enemyRenderer = GetComponentInChildren<Renderer>();
        if (enemyRenderer != null)
        {
            Material mat = enemyRenderer.material;

            // 1. 원래 기본 색상 저장
            originalColor = mat.color;

            // 2. 원래 Emission 색상 및 상태 저장
            // Emission이 활성화되어 있는지 확인
            if (mat.IsKeywordEnabled("_EMISSION"))
            {
                originalEmissionColor = mat.GetColor("_EmissionColor");
            }
            else
            {
                // Emission이 꺼져있다면 검은색으로 간주
                originalEmissionColor = Color.black;
            }
        }
    }

    private void OnEnable()
    {
        // 1.진행 중인 색상 깜빡임 코루틴 강제 중지
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        // 2.머테리얼 색상을 원래 색상으로 강제 복구
        if (enemyRenderer != null && originalColor != Color.clear)
        {
            Material mat = enemyRenderer.material;

            // Base Color 복구
            mat.color = originalColor;

            // Emission 사용 시 복구 로직
            if (mat.IsKeywordEnabled("_EMISSION"))
            {
                mat.SetColor("_EmissionColor", originalEmissionColor);
                if (originalEmissionColor == Color.black)
                {
                    mat.DisableKeyword("_EMISSION");
                }
            }
        }

        enemyStatus.hp = enemyStatus.maxHp; // 체력 초기화
        State = EEnemyState.Search;
        // 활성화 시 초기 상태 설정
        SetState(EEnemyState.Search);
    }

    private void Start()
    {
        // 1. 스폰 즉시 플레이어를 찾습니다.
        Player = GameObject.FindWithTag("Player");
        if (Player != null)
        {
            TargetTransform = Player.transform;

            // 2. PlayerStatus 참조 찾기
            PlayerStatus = Player.GetComponent<PlayerStatus>();
            if (PlayerStatus == null) Debug.Log($"{name}이 PlayerStatus를 찾을 수 없습니다!");

            currentOrbitDirection = Random.Range(0, 2) == 0 ? 1f : -1f; // 50% 확률로 시계/반시계 방향 설정

            // 3. Search 상태로 진입 (공전 시작)
            SetState(EEnemyState.Search);
        }
        else
        {
            Debug.LogError("플레이어 오브젝트를 찾을 수 없습니다! 'Player' 태그를 확인하세요.");
            SetState(EEnemyState.None);
            enabled = false;
        }
    }

    private void InitializeStateMachine()
    {
        var enemyStateSearch = new EnemyStateSearch(this);
        var enemyStateAttack = new EnemyStateAttack(this);
        var enemyStateDamaged = new EnemyStateDamaged(this);
        var enemyStateDead = new EnemyStateDead(this);

        states = new Dictionary<EEnemyState, ICharacterState>
    {
      { EEnemyState.Search, enemyStateSearch },
      { EEnemyState.Attack, enemyStateAttack },
      { EEnemyState.Damaged, enemyStateDamaged },
      { EEnemyState.Dead, enemyStateDead }
    };
    }

    private void Update()
    {
        // 게임 일시정지 처리
        if (GameManager.Instance.GameState == EGameState.Pause)
        {
            SetState(EEnemyState.None);
        }
        else
        {
            if (State == EEnemyState.None)
            {
                SetState(EEnemyState.Search);
            }
        }

        if (TargetTransform == null)
        {
            SetState(EEnemyState.None);
            return;
        }

        // 1. 플레이어 방향 계산
        CalculateDirectionVector();

        // 2. 현재 상태의 Update 실행
        if (State != EEnemyState.None && State != EEnemyState.Dead)
        {
            states[State].OnUpdate();
        }
    }

    public void SetState(EEnemyState state)
    {
        if (State == EEnemyState.Dead && state != EEnemyState.None) return;
        if (State == state) return;

        if (State != EEnemyState.None && states.ContainsKey(State))
        {
            states[State].OnExit();
        }

        State = state;

        if (State != EEnemyState.None && states.ContainsKey(State))
        {
            states[State].OnEnter();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Bullet"))
        {
            var playerAttackPower = PlayerStatus.AttackDamage;
            Debug.Log($"플레이어 공격! {name}이 {playerAttackPower}의 데미지를 입음.");
            bulletDamagePopup.Spawn(transform.position + Vector3.up, playerAttackPower);
            SetHit((int)playerAttackPower);
        }

        if (other.CompareTag("Skill"))
        {
            SkillCollider SkillCollider = other.gameObject.GetComponentInParent<SkillCollider>();
            if (SkillCollider == null) Debug.Log($"{name}이 {other.gameObject.name}의 SkillCollider를 찾을 수 없습니다!");
            else
            {
                Debug.Log($"Skill hit! {name} took {SkillCollider.Damage} damage");
                skillDamagePopup.Spawn(transform.position + Vector3.up, SkillCollider.Damage);
                SetHit((int)SkillCollider.Damage);
            }
        }
    }

    // 파티클 시스템에서 제공하는 충돌 확인
    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Bullet"))
        {
            Debug.Log("OnParticleCollision");
            SetHit(100);
        }
    }

    public void SetHit(int damage)
    {
        if (State == EEnemyState.Dead) return;

        // TODO : 체력 감소 구현
        enemyStatus.hp -= damage;

        if (enemyStatus.hp <= 0)
        {
            SetState(EEnemyState.Dead);
        }
        else
        {
            FlashRedOnDamage();
            SetState(EEnemyState.Damaged);
        }
    }

    /// <summary>
    /// 데미지를 입었을 때 일시적으로 빨간색으로 깜빡이는 코루틴을 시작합니다.
    /// </summary>
    public void FlashRedOnDamage()
    {
        if (enemyRenderer == null) return;

        // 이전에 실행 중인 코루틴이 있다면 중지
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        // 새 코루틴 시작
        flashCoroutine = StartCoroutine(FlashRedCoroutine());
    }

    // 붉은색으로 깜빡였다가 원래 색상으로 돌아오는 코루틴 로직 (내용 변경 없음)
    private IEnumerator FlashRedCoroutine()
    {
        Material mat = enemyRenderer.material;

        // 1.Emission 활성화 및 강한 빨간색 설정 (Unlit처럼 보이게 함)
        mat.EnableKeyword("_EMISSION");
        // Color.red에 * 연산을 하면 HDR(High Dynamic Range) 효과로 더욱 강하게 빛날 수 있습니다.
        mat.SetColor("_EmissionColor", Color.red * 5f); // 5f: 빛의 강도 (임의의 강한 값)

        // 2. 지정된 시간만큼 대기
        yield return new WaitForSeconds(FLASH_DURATION);

        // 3. 원래 Emission 색상으로 복원
        // Emission이 원래 꺼져있었다면 (originalEmissionColor가 black) SetColor만으로 충분합니다.
        mat.SetColor("_EmissionColor", originalEmissionColor);

        // 원래 Emission이 꺼져있었다면 (originalEmissionColor == Color.black), 
        // Emission 키워드를 끄는 것이 메모리/성능상 더 좋습니다.
        if (originalEmissionColor == Color.black)
        {
            mat.DisableKeyword("_EMISSION");
        }

        flashCoroutine = null;
    }

    // ======================================================================
    //     상태(State) 클래스에서 호출할 공용 함수들
    // ======================================================================

    // [Search 상태일 때] 고정 공전 이동만 수행합니다.
    public void PerformOrbitMovement()
    {
        if (TargetTransform == null) return;

        // 1. 고정 방향 공전
        HandleFixedOrbitMovement();
    }

    /// <summary>
    /// [공용] 플레이어가 '전방 반구' (patrolDetectionDistance) 내에 있는지 확인합니다.
    /// </summary>
    public bool CheckAttackRange()
    {
        if (TargetTransform == null) return false;

        // 1. (거리) Y축 거리가 제한 범위 내에 있는지 먼저 확인
        float yDistance = Mathf.Abs(directionToPlayer.y);
        if (yDistance > verticalDetectionLimit)
        {
            return false; // Y축 거리가 너무 멀면 즉시 탐지 실패
        }

        // 2. (거리) XZ 평면 거리를 계산하여 탐지 반경 안에 있는지 확인
        // Vector3에서 Y축을 0으로 만들어 XZ 평면 투영 거리를 구합니다.
        Vector3 directionXZ = directionToPlayer;
        directionXZ.y = 0;

        float sqrDistanceXZ = directionXZ.sqrMagnitude;
        float sqrDetectionRange = patrolDetectionDistance * patrolDetectionDistance;

        if (sqrDistanceXZ <= sqrDetectionRange)
        {
            // 3. (방향) 플레이어가 적의 '정면' 180도 안에 있는가? (기존 로직 유지)
            if (Vector3.Dot(transform.forward, directionToPlayer) > 0f)
            {
                return true;
            }
        }

        return false;
    }


    // ======================================================================
    //     이동/회전 함수들
    // ======================================================================

    /// <summary>
    /// [공통] 플레이어를 향하는 방향 벡터를 계산합니다. (Update에서 매번 호출)
    /// </summary>
    private void CalculateDirectionVector()
    {
        directionToPlayer = TargetTransform.position - transform.position;
    }

    /// <summary>
    /// [Search] 'currentOrbitDirection'에 설정된 고정 방향으로 공전합니다.
    /// </summary>
    private void HandleFixedOrbitMovement()
    {
        // 1. 공전 이동 수행

        float currentSpeed = orbitMoveSpeed;

        Vector3 rotationAxis = Vector3.up * currentOrbitDirection;
        transform.RotateAround(pivotPoint, rotationAxis, currentSpeed * Time.deltaTime);

        // 2. 공전 방향을 바라보도록 회전 업데이트
        Vector3 currentDirectionFromPivot = transform.position - pivotPoint;

        // Vector3.Cross 결과에 -1을 곱하거나 (부호 반전)
        // 혹은 currentOrbitDirection을 곱하는 대신, 그 반대 부호를 곱합니다.
        // ❗ 수정된 부분: (부호 반전)
        float correctedDirection = -currentOrbitDirection;

        Vector3 orbitTangentDirection = Vector3.Cross(currentDirectionFromPivot, Vector3.up).normalized * correctedDirection;

        // 이 접선 방향으로 적을 회전시킵니다.
        Quaternion targetRotation = Quaternion.LookRotation(orbitTangentDirection);
        transform.rotation = targetRotation;
    }

    // --- 기즈모 ---
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 1. 공격 범위 기즈모 (반구 모양)
        float radius = patrolDetectionDistance;

        //// 1-1. 수평 반원 (XZ 평면, 적의 앞쪽)
        //// 첫 번째 수평 반원
        //Handles.color = Color.yellow;
        //Handles.DrawWireArc(
        //    transform.position,    // 중심
        //    Vector3.up,            // 회전 축 (Y축)
        //    transform.forward,     // 시작 방향
        //    90f,                   // 정면 기준 +90도
        //    radius                 // 반지름
        //);

        // 두 번째 수평 반원
        Handles.color = Color.green; //
        Handles.DrawWireArc(
            transform.position,
            Vector3.up,
            transform.forward,
            -90f,                  // 정면 기준 -90도
            radius
        );

        //// 1-2. 수직 반원 (적의 정면)
        //Handles.color = Color.magenta; //
        //Handles.DrawWireArc(
        //    transform.position,
        //    transform.right,       // 회전 축 (적의 오른쪽)
        //    transform.forward,
        //    180f,                  // 정면 기준 180도 (위/아래)
        //    radius
        //);

        // 3. 플레이어 이동 영역 (파란색 구)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(playerPlayAreaCenter, playerPlayAreaRadius);

        // 4. 상태 텍스트 표시
        if (Application.isPlaying)
        {
            string stateText = $"State: {State}";
            Handles.color = Color.white; // 텍스트는 다시 흰색으로 설정
            Handles.Label(transform.position + Vector3.up * 2f, stateText);
        }
    }
#endif
}