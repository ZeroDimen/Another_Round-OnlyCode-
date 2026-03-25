using UnityEngine.InputSystem.XR;
using static Constants;

public class EnemyStateAttack : EnemyState, ICharacterState
{
    public EnemyStateAttack(EnemyController enemyController) : base(enemyController)
    {
    }

    // 공격 상태 진입: 연결된 '행동'의 OnEnter 호출
    public void OnEnter()
    {
        if (enemyController.attackBehavior != null)
        {
            enemyController.attackBehavior.OnEnter(enemyController);
        }
    }

    // 공격 상태 종료: 연결된 '행동'의 OnExit 호출
    public void OnExit()
    {
        if (enemyController.attackBehavior != null)
        {
            enemyController.attackBehavior.OnExit(enemyController);
        }
    }

    // 매 프레임 실행
    public void OnUpdate()
    {
        // 1. (공통) 플레이어가 공격 범위를 벗어났는지 확인
        if (!enemyController.CheckAttackRange())
        {
            // 2. 벗어났다면 다시 Search 상태 (공전 시작)
            enemyController.SetState(EEnemyState.Search);
            return;
        }

        // 3. (핵심) 범위 내라면, 연결된 '행동'의 OnUpdate를 실행
        if (enemyController.attackBehavior != null)
        {
            enemyController.attackBehavior.OnUpdate(enemyController);
        }
    }
}