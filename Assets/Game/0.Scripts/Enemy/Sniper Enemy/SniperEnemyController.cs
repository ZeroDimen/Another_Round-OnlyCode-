using UnityEngine;

// EnemyController를 상속받는다고 가정
public class SniperEnemyController : EnemyController
{
    [Header("Sniper 발사 설정")]
    [SerializeField] private GameObject bulletPrefab; // 총알 프리팹
    [SerializeField] private Transform firePoint;     // 총알 발사 위치

    // ----------------------------------------
    // 상태 변수 (SniperAttackBehavior에서 접근/수정)
    // ----------------------------------------
    private float attackTimer;
    private float retreatTimer;
    private bool isRetreating = false;

    // ----------------------------------------
    // 타이머 및 상태 접근자 (Getter/Setter)
    // ----------------------------------------

    // 공격 타이머 관리
    public float GetAttackTimer() => attackTimer;
    public void SetAttackTimer(float newTimer) => attackTimer = newTimer;

    // 후퇴 타이머 관리
    public float GetRetreatTimer() => retreatTimer;
    public void SetRetreatTimer(float newTimer) => retreatTimer = newTimer;

    // 후퇴 상태 관리
    public bool IsRetreating() => isRetreating;

    // 후퇴 시작 및 종료 메서드
    public void StartRetreat(float duration)
    {
        isRetreating = true; // 후퇴 상태 설정
        retreatTimer = duration; // 후퇴 지속 시간 설정
    }

    public void EndRetreat()
    {
        isRetreating = false;
        // 필요하다면, 후퇴 종료 시 적의 속도를 즉시 0으로 만드는 로직 추가
    }

    // ----------------------------------------
    // 행동 실행 메서드 (SniperAttackBehavior가 호출)
    // ----------------------------------------

    // 원거리 공격 실행 (EnemyController의 Shoot 메서드를 재정의)
    public void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogError("Bullet Prefab 또는 Fire Point가 SniperController에 할당되지 않았습니다.", this);
            return;
        }

        if (TargetTransform == null) return;

        // 1. 총알 생성 (PoolManager 사용)
        GameObject bulletInstance = PoolManager.Instance.Get(bulletPrefab);

        if (bulletInstance == null) return;

        // 2. 위치 설정 (총알은 firePoint에서 시작)
        bulletInstance.transform.position = firePoint.position;

        // 3. EnemyBullet 컴포넌트를 가져옵니다.
        EnemyBullet enemyBullet = bulletInstance.GetComponent<EnemyBullet>();

        // 4. ⭐️ 공전 방향 부호를 결정합니다. ⭐️
        // (적의 공전 방향 'currentOrbitDirection'을 총알의 공전 방향으로 사용)
        float orbitSign = currentOrbitDirection;

        if (enemyBullet != null)
        {
            // 5. ⭐️ Initialize 메서드를 호출하여 공전 방향 부호만 전달합니다. ⭐️
            enemyBullet.Initialize(Vector3.zero, 0f, orbitSign); // direction과 speed는 Dummy로 전달
        }
        else
        {
            Debug.LogError("총알 프리팹에 EnemyBullet 컴포넌트가 없습니다.");
        }

        // 부모 해제
        //bulletInstance.transform.SetParent(null);
    }

    // 후퇴 이동 실행 (공전 궤도의 반대 방향으로 이동)
    // EnemyController의 이동 메서드를 재정의하거나, 새로 정의된 것으로 가정
    public void MoveReverseOrbit(float distance)
    {
        
    }
}