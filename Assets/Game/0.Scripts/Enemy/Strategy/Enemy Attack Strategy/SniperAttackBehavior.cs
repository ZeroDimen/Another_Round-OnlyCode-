using UnityEngine;
using static Constants;

[CreateAssetMenu(fileName = "SniperAttackBehavior", menuName = "Scriptable Objects/SniperAttackBehavior")]
public class SniperAttackBehavior : EnemyAttackBehavior
{
    [Header("Sniper 공격 설정")]
    [Tooltip("초당 발사 횟수")]
    [SerializeField] private float attacksPerSecond = 1f;

    [Header("첫 공격 설정")]
    [Tooltip("Attack 상태 진입 후 첫 공격까지의 지연 시간 (초)")]
    [SerializeField] private float initialAttackDelay = 0.2f; // 첫 공격 지연 시간

    [Header("후퇴 설정")]
    [Tooltip("후퇴 이동 속도")]
    [SerializeField] private float retreatSpeed = 5f;
    [Tooltip("후퇴 이동 지속 시간")]
    [SerializeField] private float retreatDuration = 0.3f;

    private float fireRate; // 발사 간격 (1 / attacksPerSecond)

    private void OnEnable()
    {
        // 발사 간격 계산
        fireRate = 1f / attacksPerSecond;
    }

    public override void OnEnter(EnemyController enemyController)
    {
        SniperEnemyController sniperController = enemyController as SniperEnemyController;
        if (sniperController != null)
        {
            // 첫 공격 지연 시간(initialAttackDelay)을 반영하여 타이머를 설정합니다.
            // (fireRate - initialAttackDelay) 값을 채워둠으로써, OnUpdate에서 initialAttackDelay만큼만 더하면 발사됩니다.
            float initialTimerValue = fireRate - initialAttackDelay;

            // 안전 장치: 타이머 음수 방지
            if (initialTimerValue < 0f) initialTimerValue = 0f;

            sniperController.SetAttackTimer(initialTimerValue);
            sniperController.EndRetreat(); // 후퇴 상태 초기화
        }
    }

    public override void OnUpdate(EnemyController enemyController)
    {
        SniperEnemyController sniperController = enemyController as SniperEnemyController;

        float adjustedDeltaTime = Time.deltaTime;

        // 형변환 실패 시 종료
        if (sniperController == null)
        {
            return;
        }

        // ----------------------------------------------------
        // 후퇴 이동 및 타이머 관리
        // ----------------------------------------------------
        if (sniperController.IsRetreating())
        {
            // 후퇴 이동 실행 (공전 궤도의 반대 방향으로 이동)
            float moveDistance = retreatSpeed * Time.deltaTime;
            sniperController.MoveReverseOrbit(moveDistance);

            // 후퇴 타이머 감소 및 종료 확인
            float currentRetreatTimer = sniperController.GetRetreatTimer();
            currentRetreatTimer -= adjustedDeltaTime;
            sniperController.SetRetreatTimer(currentRetreatTimer);

            if (currentRetreatTimer <= 0)
            {
                sniperController.EndRetreat();
            }
        }

        // ----------------------------------------------------
        // 공격 타이머 및 발사 로직
        // ----------------------------------------------------

        float currentAttackTimer = sniperController.GetAttackTimer();
        currentAttackTimer += adjustedDeltaTime;

        // 발사 조건: 타이머 충족 및 후퇴 중이 아닐 때
        if (currentAttackTimer >= fireRate && !sniperController.IsRetreating())
        {
            // 1. 공격 발사 명령
            sniperController.Shoot();

            // 2. 발사 후 즉시 후퇴 패턴 시작(미구현)
            sniperController.StartRetreat(retreatDuration);

            // 3. 타이머 초기화 (다음 발사는 fireRate 간격으로 나갑니다.)
            currentAttackTimer = 0f;
        }

        // 업데이트된 타이머 값을 저장
        sniperController.SetAttackTimer(currentAttackTimer);
    }
}