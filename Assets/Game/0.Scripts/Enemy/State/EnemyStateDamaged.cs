using Unity.VisualScripting;
using UnityEngine;
using static Constants;

public class EnemyStateDamaged : EnemyState, ICharacterState
{
    public EnemyStateDamaged(EnemyController enemyController) : base(enemyController)
    {
    }
    public void OnEnter()
    {
        
    }
    public void OnExit()
    {

    }
    public void OnUpdate()
    {
        // 1. Search 상태에서는 항상 공전 및 수직 추적을 수행
        enemyController.PerformOrbitMovement();

        // 2. 플레이어가 공격 범위 안에 들어왔는지 확인
        if (enemyController.CheckAttackRange())
        {
            // 3. 범위 내라면 Attack 상태로 전환
            enemyController.SetState(EEnemyState.Attack);
        }
    }
}
