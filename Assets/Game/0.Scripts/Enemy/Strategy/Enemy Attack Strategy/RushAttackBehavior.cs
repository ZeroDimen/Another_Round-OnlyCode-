using UnityEngine;
using static Constants;

[CreateAssetMenu(fileName = "RushAttackBehavior", menuName = "Scriptable Objects/RushAttackBehavior")]
public class RushAttackBehavior : EnemyAttackBehavior
{
    [Header("돌진 설정")]
    [SerializeField] private float chargeDelay; // 돌진 전 대기 시간
    [SerializeField] private float chargeSpeed; // 돌진 속도
    [SerializeField] private float chargeDuration; // 돌진 지속 시간
    [SerializeField] private float maxChargeDistance; //최대 돌진 거리 제한

    private enum RushState { None, Delaying, Charging } // 돌진 내부 상태

    private RushState currentState; // 현재 돌진 상태를 저장하는 변수
    private float stateTimer; 
    private Vector3 chargeDirection; // 돌진 방향 (고정)

    public override void OnEnter(EnemyController enemyController)
    {
        currentState = RushState.Delaying;
        stateTimer = chargeDelay;
    }

    public override void OnUpdate(EnemyController enemyController)
    {
        float adjustedDeltaTime = Time.deltaTime;

        stateTimer -= adjustedDeltaTime; // 상태 타이머 감소

        switch (currentState)
        {
            case RushState.Delaying:
                if (stateTimer <= 0f)
                {
                    StartCharge(enemyController); // 준비 시간이 끝나면 돌진 시작
                }
                break;

            case RushState.Charging:
                if (stateTimer <= 0f)
                {
                    // 돌진 시간이 끝나면 Search 상태로 복귀
                    enemyController.SetState(EEnemyState.Search);
                    return;
                }

                // 고정된 방향으로만 돌진 (XZ 평면에서)
                enemyController.transform.position += chargeDirection * chargeSpeed * adjustedDeltaTime;

                //돌진 중에도 플레이 가능 영역을 벗어나지 않도록 제한
                Vector3 currentPos = enemyController.transform.position;
                Vector2 currentPosXZ = new Vector2(currentPos.x, currentPos.z);
                Vector2 areaCenterXZ = new Vector2(enemyController.PlayerPlayAreaCenter.x, enemyController.PlayerPlayAreaCenter.z);

                if (Vector2.Distance(currentPosXZ, areaCenterXZ) > enemyController.PlayerPlayAreaRadius)
                {
                    // 영역을 벗어났다면, 중심 방향으로 위치를 보정
                    Vector2 clampedPosXZ = areaCenterXZ + (currentPosXZ - areaCenterXZ).normalized * enemyController.PlayerPlayAreaRadius;
                    enemyController.transform.position = new Vector3(clampedPosXZ.x, currentPos.y, clampedPosXZ.y);
                }
                break;
        }
    }

    /// <summary>
    /// 실제 돌진을 시작하는 함수 (Delaying -> Charging 전환 시 호출)
    /// </summary>
    private void StartCharge(EnemyController enemyController)
    {
        currentState = RushState.Charging;
        stateTimer = chargeDuration;
        
        if (enemyController.TargetTransform != null)
        {
            Vector3 targetPos = enemyController.TargetTransform.position;
            Vector3 enemyPos = enemyController.transform.position;
            Vector3 direction = targetPos - enemyPos;
            direction.y = 0; // Y축 무시 (수평 돌진)
            
            // (예외 처리) 플레이어가 바로 위에 있거나 할 경우
            if (direction.sqrMagnitude < 0.001f)
            {
                // 타겟과 위치가 같으면 그냥 적의 현재 정면으로 돌진
                chargeDirection = enemyController.transform.forward;
                chargeDirection.y = 0;
                chargeDirection.Normalize();
            }
            else
            {
                // 계산된 방향을 돌진 방향으로 저장
                chargeDirection = direction.normalized;
            }
        }
        else
        {
            // ... (타겟 없을 시 예외 처리, Y축 제거 추가) ...
            chargeDirection = enemyController.transform.forward;
            chargeDirection.y = 0;
            chargeDirection.Normalize();
        }
    }

    /// <summary>
    /// 공격 상태가 끝날 때 (Search, Damaged, Dead 등으로 전환 시)
    /// </summary>
    public override void OnExit(EnemyController enemyController)
    {
        currentState = RushState.None;
    }
}