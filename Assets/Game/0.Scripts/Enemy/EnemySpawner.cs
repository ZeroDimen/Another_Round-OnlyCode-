using UnityEngine;
using System.Collections;

// TODO : 적을 스폰할 때 오브젝트 풀링 기법 적용하기 (완료됨)
public class EnemySpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    // [SerializeField] private GameObject[] enemyPrefab; // LevelManager가 관리하므로 제거
    // [SerializeField] private float spawnRate = 0.5f;  // LevelManager가 관리하므로 제거
    // private float timer = 0f;                       // LevelManager가 관리하므로 제거

    [Header("맵 및 플레이어 설정")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float mapRadius = 25f;
    [SerializeField] private Vector3 pivotPoint = new Vector3(0, 0, 0);

    [Header("거리 제한")]
    [SerializeField] private float minSpawnDistance = 20f;
    [SerializeField] private float maxSpawnDistance = 50f;

    [Header("높이 제한")]
    [SerializeField] private float spawnHeightMin = 7f;
    [SerializeField] private float spawnHeightMax = 19f;

    // === 내부 변수 ===
    private Coroutine _spawnCoroutine;
    private GameObject[] _currentEnemyPrefabs; // 현재 웨이브에서 스폰할 프리팹
    private float _currentSpawnRate;           // 현재 웨이브의 스폰 속도

    private void Start()
    {
        // 씬 시작 시 플레이어 Transform 할당
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        // 스폰은 LevelManager가 시작시킴
        // if (playerTransform != null)
        // {
        //     StartSpawning();
        // }
    }

    // LevelManager가 일반 몹 스폰을 시작시킬 때 호출
    public void StartSpawning(GameObject[] prefabsToSpawn, float spawnRate)
    {
        if (playerTransform == null)
        {
            Debug.LogError("플레이어 Transform이 설정되지 않았습니다. 스폰을 시작할 수 없습니다.");
            return;
        }

        if (prefabsToSpawn == null || prefabsToSpawn.Length == 0)
        {
            Debug.LogError("스폰할 적 프리팹이 없습니다.");
            return;
        }

        _currentEnemyPrefabs = prefabsToSpawn;
        _currentSpawnRate = spawnRate;

        // 이미 실행 중인 코루틴이 있다면 중지하고 새로 시작
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
        }
        _spawnCoroutine = StartCoroutine(SpawnCoroutine());
    }

    // LevelManager가 스폰을 중지시킬 때 호출
    public void StopSpawning()
    {
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }
    }

    // Update() 메서드 제거 (LevelManager가 시간 관리)

    // 일반 몹 스폰 코루틴
    private IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            // 현재 웨이브의 몬스터 중 랜덤 스폰
            SpawnEnemy(_currentEnemyPrefabs);
            yield return new WaitForSeconds(_currentSpawnRate);

            // 난이도 조절 로직 (spawnRate -= 0.1f) 제거
            // 웨이브별로 spawnRate가 고정되므로, 난이도 조절은 LevelManager의 WaveData에서 설정
        }
    }

    // 조건에 맞는 적 스폰 위치를 찾고 적을 생성 (일반 몹용)
    private void SpawnEnemy(GameObject[] prefabsToUse)
    {
        Vector3 spawnPosition;
        if (FindValidSpawnPosition(out spawnPosition))
        {
            // 적 프리팹 배열에서 무작위로 선택
            GameObject selectedPrefab = prefabsToUse[Random.Range(0, prefabsToUse.Length)];

            // PoolManager를 사용하여 적을 풀에서 가져옴
            GameObject newEnemy = PoolManager.Instance.Get(selectedPrefab);

            if (newEnemy != null)
            {
                SetupEnemy(newEnemy, spawnPosition, selectedPrefab); // 선택된 프리팹 전달
            }
            else
            {
                Debug.LogWarning($"{selectedPrefab.name} 풀에서 오브젝트를 가져오지 못했습니다.");
            }
        }
        else
        {
            Debug.LogWarning("적절한 스폰 위치를 찾지 못했습니다.");
        }
    }

    // LevelManager가 보스 등 특정 적을 1회 스폰할 때 호출
    public void SpawnSpecificEnemy(GameObject prefabToSpawn)
    {
        Vector3 spawnPosition;
        if (FindValidSpawnPosition(out spawnPosition))
        {
            GameObject newEnemy = PoolManager.Instance.Get(prefabToSpawn);
            if (newEnemy != null)
            {
                SetupEnemy(newEnemy, spawnPosition, prefabToSpawn);
                Debug.Log($"{prefabToSpawn.name} 스폰 완료.");
            }
            else
            {
                Debug.LogWarning($"{prefabToSpawn.name} 풀에서 오브젝트를 가져오지 못했습니다.");
            }
        }
        else
        {
            // 보스 스폰 실패 시 강력한 경고 또는 재시도 로직 필요
            Debug.LogError($"[중요] {prefabToSpawn.name} 스폰 위치 찾기 실패!");
        }
    }


    // 스폰 위치를 찾는 로직 (재사용을 위해 별도 함수로 분리)
    private bool FindValidSpawnPosition(out Vector3 spawnPosition)
    {
        spawnPosition = Vector3.zero;

        // 유효한 스폰 위치를 찾을 때까지 최대 50번 시도
        for (int i = 0; i < 50; i++)
        {
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float spawnX = Mathf.Cos(randomAngle) * mapRadius;
            float spawnZ = Mathf.Sin(randomAngle) * mapRadius;
            float randomY = Random.Range(spawnHeightMin, spawnHeightMax);

            Vector3 potentialPos = new Vector3(pivotPoint.x + spawnX, randomY, pivotPoint.z + spawnZ);

            // 플레이어와의 XZ 평면 거리 계산
            Vector3 playerPosXZ = playerTransform.position;
            playerPosXZ.y = potentialPos.y;

            float distanceToPlayer = Vector3.Distance(potentialPos, playerPosXZ);

            if (distanceToPlayer >= minSpawnDistance && distanceToPlayer <= maxSpawnDistance)
            {
                spawnPosition = potentialPos;
                return true; // 유효한 위치 찾음
            }
        }

        return false; // 유효한 위치 찾기 실패
    }

    // 스폰된 적의 위치, 회전, 부모를 설정
    private void SetupEnemy(GameObject newEnemy, Vector3 spawnPosition, GameObject originalPrefab)
    {
        newEnemy.transform.position = spawnPosition;
        newEnemy.transform.rotation = Quaternion.identity;
        newEnemy.transform.SetParent(transform); // 계층 구조 관리를 위해 유지
        // newEnemy.SetActive(true); // PoolManager Get에서 해준다면 필요 없음

        // 적 인스턴스에 자신의 프리팹 이름을 저장시킵니다.
        EnemyController controller = newEnemy.GetComponent<EnemyController>();
        if (controller != null)
        {
            controller.prefabName = originalPrefab.name;
        }
        else
        {
            Debug.LogWarning($"{originalPrefab.name}에서 EnemyController를 찾을 수 없습니다!");
        }
    }

#if UNITY_EDITOR
    // --- 기즈모 (수정 없음) ---
    private void OnDrawGizmos()
    {
        if (playerTransform != null)
        {
            Vector3 playerPosOnPlane = playerTransform.position;
            playerPosOnPlane.y = pivotPoint.y;

            Gizmos.color = Color.red;
            DrawCircle(playerPosOnPlane, minSpawnDistance);

            Gizmos.color = Color.yellow;
            DrawCircle(playerPosOnPlane, maxSpawnDistance);

            Gizmos.color = Color.blue;
            DrawCircle(pivotPoint, mapRadius);
        }
    }


    private void DrawCircle(Vector3 center, float radius)
    {
        UnityEditor.Handles.color = Gizmos.color;
        UnityEditor.Handles.DrawWireDisc(center, Vector3.up, radius);
    }
#endif
}