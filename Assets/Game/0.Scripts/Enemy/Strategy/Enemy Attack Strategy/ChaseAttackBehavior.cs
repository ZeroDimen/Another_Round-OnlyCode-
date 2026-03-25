using UnityEngine;
using static Constants; // EGameState 사용을 위해 필요

[CreateAssetMenu(fileName = "ChaseAttackBehavior", menuName = "Scriptable Objects/ChaseAttackBehavior")]
public class ChaseAttackBehavior : EnemyAttackBehavior
{
    [Header("Chase Settings")]
    [Tooltip("Z축 접근 속도 (현재 사용하지 않음)")]
    [SerializeField] private float approachSpeedZ = 5f;
    [Tooltip("Y축 추적 속도")]
    [SerializeField] private float chaseSpeedY = 7f;

    [Header("Closeness Settings")]
    [Tooltip("이 거리 이내로 가까워지면 XZ 공전을 멈추고 Y축 직접 추격으로 전환합니다.")]
    [SerializeField] private float closeEnoughDistance;

    public override void OnUpdate(EnemyController enemyController)
    {
        if (enemyController.TargetTransform == null) return;

        float adjustedDeltaTime = Time.deltaTime;

        Transform enemyTransform = enemyController.transform;
        Transform targetTransform = enemyController.TargetTransform;

        Vector3 currentPos = enemyTransform.position;
        Vector3 directionToTarget = targetTransform.position - currentPos;
        float distanceToTarget = directionToTarget.magnitude;

        // 2. 가까워졌는지 확인 (XZ 공전을 멈추고 Y축 추적으로 전환)
        if (distanceToTarget <= closeEnoughDistance)
        {
            // XZ 이동은 하지 않고, Y축만 플레이어를 향해 직선 이동합니다.
            float yMovement = directionToTarget.normalized.y * chaseSpeedY * adjustedDeltaTime;
            Vector3 movement = new Vector3(0, yMovement, 0);
            enemyTransform.position += movement;
        }
        else // 3. 멀리 있다면, XZ 공전과 Y축 추적을 수행
        {
            // XZ 공전 이동 수행 (궤도 유지)
            // PerformOrbitMovement는 EnemyController에서 처리한다고 가정
            enemyController.PerformOrbitMovement();
            Vector3 orbitPos = enemyTransform.position;

            // Y축 추적 (플레이어의 높이를 따라감)
            float newY = Mathf.MoveTowards(
                orbitPos.y,
                targetTransform.position.y,
                chaseSpeedY * adjustedDeltaTime // 속도 보정 적용
            );

            // Z축은 공전 이동으로 이미 업데이트된 현재 위치를 유지합니다. (Z축 접근 로직 제거)
            float newZ = orbitPos.z;

            Vector3 finalPos = new Vector3(orbitPos.x, newY, newZ);

            // 영역 제약 적용: XZ 공전 궤도를 벗어나는 것을 방지합니다.
            enemyTransform.position = EnforceHardPlayAreaConstraint(enemyController, finalPos);
        }

        // 4. 회전 로직: 상하좌우(3D)로 플레이어를 즉시 쳐다봅니다.
        RotateTowardsPlayerInstantly(enemyTransform, targetTransform.position - enemyTransform.position);
    }

    /// <summary>
    /// 플레이어를 즉시 바라보도록 적의 몸체를 회전시킵니다.
    /// </summary>
    private void RotateTowardsPlayerInstantly(Transform enemyTransform, Vector3 directionToTarget)
    {
        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            enemyTransform.rotation = targetRotation;
        }
    }

    /// <summary>
    /// 적의 위치가 플레이 영역(원형 궤도)을 벗어나지 않도록 강제로 제약합니다.
    /// </summary>
    private Vector3 EnforceHardPlayAreaConstraint(EnemyController controller, Vector3 nextPos)
    {
        Vector3 areaCenter = controller.PlayerPlayAreaCenter;
        float radius = controller.PlayerPlayAreaRadius;

        Vector3 vectorToNextXZ = nextPos - areaCenter;
        vectorToNextXZ.y = 0; // XZ 평면의 벡터만 계산

        float distanceXZ = vectorToNextXZ.magnitude;

        if (distanceXZ > radius)
        {
            // 반지름 범위 밖으로 나갔다면, 벡터를 반지름으로 제한
            Vector3 clampedVectorXZ = vectorToNextXZ.normalized * radius;

            return new Vector3(
                areaCenter.x + clampedVectorXZ.x,
                nextPos.y, // Y축은 그대로 유지
                areaCenter.z + clampedVectorXZ.z
            );
        }

        return nextPos;
    }
}