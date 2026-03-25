using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

// 각 웨이브의 설정을 정의하는 클래스
[System.Serializable]
public class WaveData
{
    public string waveName = "Wave";
    public float waveDuration = 60f;      // 웨이브 지속 시간 (1분)
    public int killsToAdvance = 50;       // 다음 웨이브로 넘어가기 위한 킬 수
    public float spawnRate = 1f;        // 해당 웨이브의 스폰 속도
    public GameObject[] enemiesToSpawn;   // 해당 웨이브에서 스폰할 일반 몬스터 프리팹 배열
    public GameObject bossPrefab;         // 해당 웨이브가 보스 웨이브일 경우 스폰할 보스 프리팹 (없으면 null)
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("웨이브 설정")]
    [SerializeField] private WaveData[] waves; // 인스펙터에서 웨이브 순서대로 설정

    [Header("참조")]
    [SerializeField] private EnemySpawner enemySpawner; // EnemySpawner 스크립트 참조

    [SerializeField] private TextMeshProUGUI waveNameAndKill; // 웨이브 이름과 킬 수 표시 텍스트
    [SerializeField] private TextMeshProUGUI waveTime; // 웨이브 이름과 킬 수 표시 텍스트

    [SerializeField] private SkillManager rewardPanel; // 웨이브 보상 패널 참조

    [SerializeField] private GameUIManager gameUIManager;

    [SerializeField] private GameObject bossObject;

    // --- 내부 상태 변수 ---
    private int currentWaveIndex = -1;
    private float waveTimer;
    private int enemiesKilledThisWave; // 현재 웨이브에서 죽인 적 수
    private bool isWaveActive = false;
    private bool bossSpawned = false;

    public int enemiesKilledTotal;    // 전체 웨이브에서 죽인 적 수

    public Dictionary<string, int> waveKillCounts = new Dictionary<string, int>(); // 웨이브별 킬 수 기록용

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (enemySpawner == null)
        {
            enemySpawner = FindFirstObjectByType<EnemySpawner>();
        }

        if (rewardPanel == null)
        {
            rewardPanel = FindFirstObjectByType<SkillManager>();
        }

        if (gameUIManager == null)
        {
            gameUIManager = FindFirstObjectByType<GameUIManager>();
        }


        if (bossObject != null)
        {
            bossObject.SetActive(false);
        }

        else
        {
            Debug.LogWarning("씬에 배치된 보스 오브젝트가 LevelManager에 할당되지 않았습니다. (보스 웨이브가 없다면 정상)");
        }
    }

    private void Start()
    {
        // 게임 시작 시 첫 웨이브 시작
        StartNextWave();
    }

    private void Update()
    {
        if (!isWaveActive) return;

        // 웨이브 타이머 갱신
        waveTimer -= Time.deltaTime;
        waveTime.text = $"Next Wave : {Mathf.CeilToInt(waveTimer)}";

        // OnWaveTimerChanged?.Invoke(waveTimer);

        // 시간이 다 되면 다음 웨이브로
        if (waveTimer <= 0f)
        {
            GameManager.Instance.SetGameState(Constants.EGameState.Pause);
            gameUIManager.ShowRewardPanel(); // 웨이브 보상 패널 호출
            SoundManager.Instance.PlaySFX(SoundManager.SFX.RewardScreen); // 웨이브 클리어 사운드 재생
            Debug.Log($"시간 종료! {waves[currentWaveIndex].waveName} 클리어.");
            StartNextWave();
        }
    }

    // 적이 죽었을 때 호출될 함수 (Enemy 스크립트에서 호출해야 함)
    public void EnemyKilled(string prefabName)
    {
        // 현재 웨이브가 활성화되어 있지 않거나, 보스가 이미 스폰된 상태라면 무시
        if (!isWaveActive || bossSpawned)
        {
            return;
        }

        WaveData currentWave = waves[currentWaveIndex];

        enemiesKilledThisWave++;
        enemiesKilledTotal++;
        waveNameAndKill.text = $"{currentWave.waveName} - {enemiesKilledThisWave}/{currentWave.killsToAdvance}";


        // OnKillCountChanged?.Invoke(enemiesKilledThisWave);

        // 웨이브별 킬 수 기록
        if (!string.IsNullOrEmpty(prefabName))
        {
            if (waveKillCounts.ContainsKey(prefabName))
            {
                waveKillCounts[prefabName]++;
            }
            else
            {
                // 혹시 모를 예외 (웨이브 목록엔 없었지만 스폰된 경우)
                waveKillCounts.Add(prefabName, 1);
            }

            // 테스트용 로그
            Debug.Log($"[{prefabName}] 처치! (총 {waveKillCounts[prefabName]}마리)");
        }

        // 만약 보스 웨이브가 아니고, 킬 수를 충족했다면
        if (currentWave.bossPrefab == null && enemiesKilledThisWave >= currentWave.killsToAdvance)
        {
            GameManager.Instance.SetGameState(Constants.EGameState.Pause);
            gameUIManager.ShowRewardPanel(); // 웨이브 보상 패널 호출
            SoundManager.Instance.PlaySFX(SoundManager.SFX.RewardScreen); // 웨이브 클리어 사운드 재생
            Debug.Log($"킬 수 충족! {currentWave.waveName} 클리어.");
            StartNextWave();
        }
    }

    // 보스가 죽었을 때 호출될 함수 (Boss 스크립트에서 호출해야 함)
    public void BossKilled()
    {
        if (!isWaveActive || !bossSpawned) return;

        WaveData currentWave = waves[currentWaveIndex];
        enemiesKilledThisWave++;
        enemiesKilledTotal++;
        gameUIManager.BossHPBar(null); // 보스 HP 바 UI 비활성화
        waveNameAndKill.text = $"{currentWave.waveName} - {enemiesKilledThisWave}/{currentWave.killsToAdvance}";

        Debug.Log("보스 처치! 다음 웨이브로 넘어갑니다.");
        StartNextWave();
    }


    // 다음 웨이브를 준비하고 시작
    private void StartNextWave()
    {
        isWaveActive = false;
        enemySpawner.StopSpawning(); // 현재 진행 중인 스폰 중지

        currentWaveIndex++;

        // 모든 웨이브를 클리어한 경우
        if (currentWaveIndex >= waves.Length)
        {
            GameManager.Instance.SetGameState(Constants.EGameState.Clear);
            Debug.Log("모든 웨이브 클리어! 게임 승리!");
            // (게임 승리 로직)
            return;
        }

        // 새 웨이브 시작
        StartWave(waves[currentWaveIndex]);
    }

    // 특정 웨이브 시작
    private void StartWave(WaveData wave)
    {
        Debug.Log($"--- {wave.waveName} 시작! ---");
        waveTimer = wave.waveDuration;
        enemiesKilledThisWave = 0;
        isWaveActive = true;
        bossSpawned = false;

        // UI 업데이트
        if (waveNameAndKill != null)
        {
            waveNameAndKill.text = $"{wave.waveName} - {enemiesKilledThisWave}/{wave.killsToAdvance}";
        }

        // 새 웨이브가 시작될 때마다 딕셔너리를 초기화
        waveKillCounts.Clear();
        // 웨이브에 스폰될 적 목록을 딕셔너리에 미리 0으로 추가
        if (wave.enemiesToSpawn != null)
        {
            foreach (var prefab in wave.enemiesToSpawn)
            {
                if (!waveKillCounts.ContainsKey(prefab.name))
                {
                    waveKillCounts.Add(prefab.name, 0);
                }
            }
        }
        // 1. 보스 웨이브인 경우
        if (wave.bossPrefab != null)
        {
            Debug.Log("보스 웨이브! 보스를 1회 스폰합니다.");
            //enemySpawner.SpawnSpecificEnemy(wave.bossPrefab); // 보스 스포너 호출
            gameUIManager.BossHPBar(wave.bossPrefab); // 보스 HP 바 UI 활성화

            if (bossObject != null)
            {
                // (추가) 씬에 있는 보스를 활성화
                bossObject.SetActive(true);

                // (수정) BossHPBar에 프리팹 대신 씬에 있는 '인스턴스'를 넘겨줌
                gameUIManager.BossHPBar(bossObject);
            }
            else
            {
                Debug.LogError("보스 웨이브가 시작되었으나 'inSceneBossObject'가 할당되지 않았습니다!");
            }
            bossSpawned = true;
            // 일반 몹 스폰은 StopSpawning()으로 이미 중지된 상태
        }
        // 2. 일반 몹 웨이브인 경우
        else
        {
            if (wave.enemiesToSpawn.Length > 0)
            {
                // EnemySpawner에게 이번 웨이브에 스폰할 몹과 스폰 속도를 알려주고 스폰 시작
                enemySpawner.StartSpawning(wave.enemiesToSpawn, wave.spawnRate);
            }
            else
            {
                Debug.LogWarning($"{wave.waveName}에 스폰할 적이 설정되지 않았습니다!");
            }
        }
    }

    // 외부(예: UI)에서 특정 프리팹의 킬 수를 확인할 수 있는 함수
    public int GetKillCountForPrefab(string prefabName)
    {
        int count = 0;
        waveKillCounts.TryGetValue(prefabName, out count);
        return count;
    }
}