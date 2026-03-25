using UnityEngine;

public class EnemyStateDead : EnemyState, ICharacterState
{
    public EnemyStateDead(EnemyController enemyController) : base(enemyController)
    {
    }

    public void OnEnter()
    {

        //죽음 이펙트 실행
        if (enemyController.DeathEffectPrefab != null && PoolManager.Instance != null)
        {
            // A. 이펙트 오브젝트를 풀에서 가져옵니다.
            SoundManager.Instance.PlaySFX(SoundManager.SFX.EnemyDie); // 몬스터 죽음 사운드 재생
            GameObject effectObj = PoolManager.Instance.Get(enemyController.DeathEffectPrefab);

            // B. 이펙트를 현재 몬스터의 위치와 회전으로 설정합니다.
            effectObj.transform.position = enemyController.transform.position;
            effectObj.transform.rotation = enemyController.transform.rotation;

            // C. 이펙트 재생 및 일정 시간 후 풀로 자동 반환하는 로직이 
            //    이펙트 프리팹의 스크립트(예: AutoReleaseEffect)에 구현되어 있어야 합니다.
        }
        // (예시: 만약 보스 태그가 있다면)
        // if (enemyController.CompareTag("Boss"))
        // {
        //     LevelManager.Instance?.BossKilled();
        // }
        // else
        // {
        //     // 일반 몹이므로, 저장된 prefabName과 함께 킬 카운트 보고
        //     LevelManager.Instance?.EnemyKilled(enemyController.prefabName);
        // }

        // (보스와 스크립트를 공유하지 않는다면 아래 한 줄만 있으면 됩니다)
        LevelManager.Instance?.EnemyKilled(enemyController.prefabName);

        // PoolManager를 통해 적 오브젝트를 반환합니다.
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Release(enemyController.gameObject);
        }
        else
        {
            // 안전장치: PoolManager가 없으면 최종적으로 파괴합니다.
            Debug.LogWarning("PoolManager 인스턴스를 찾을 수 없습니다. 적을 즉시 파괴합니다.");
            GameObject.Destroy(enemyController.gameObject);
        }
    }

    public void OnExit()
    {
        
    }

    public void OnUpdate()
    {
        
    }
}
