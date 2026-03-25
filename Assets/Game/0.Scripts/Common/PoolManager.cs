using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    // 딕셔너리: <프리팹(GameObject), 해당 프리팹의 GameObject 풀>
    private Dictionary<GameObject, IObjectPool<GameObject>> pools = new Dictionary<GameObject, IObjectPool<GameObject>>();

    // 딕셔너리: <현재 활성화된 오브젝트 인스턴스, 해당 오브젝트의 원본 프리팹>
    // 이 딕셔너리가 Poolable 컴포넌트의 역할을 대체하여 반환 시 어느 풀로 돌아가야 할지 알려줍니다.
    private Dictionary<GameObject, GameObject> instanceToPrefabMap = new Dictionary<GameObject, GameObject>();


    [Header("사전 생성 풀 설정")]
    [Tooltip("미리 생성할 풀과 초기 크기를 설정합니다.")]
    [SerializeField] private PoolSetup[] poolSetups; // 맨 밑에 클래스 참조


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        //DontDestroyOnLoad(gameObject);

        InitializePools();
    }

    // ----------------------------------------------------
    // 풀 생성 로직
    // ----------------------------------------------------

    // 사전 설정된 풀들을 초기화하는 메서드 (인스펙터에서 설정)
    private void InitializePools()
    {
        if (poolSetups == null) return;
        foreach (var setup in poolSetups)
        {
            if (setup.prefab != null && !pools.ContainsKey(setup.prefab))
            {
                CreatePool(setup.prefab, setup.initialSize, setup.maxSize);
            }
        }
    }

    // 프리팹 기반으로 오브젝트 풀을 생성하는 메서드
    private IObjectPool<GameObject> CreatePool(GameObject prefab, int initialSize = 10, int maxSize = 30)
    {
        if (pools.ContainsKey(prefab)) return pools[prefab];

        IObjectPool<GameObject> pool = new ObjectPool<GameObject>(
            // Create
            () =>
            {
                GameObject newObj = Instantiate(prefab, transform);
                // ⭐️ 생성 시점에 매핑 정보를 추가합니다.
                instanceToPrefabMap.Add(newObj, prefab);
                return newObj;
            },
            // Get (풀에서 꺼낼 때)
            (obj) => { obj.gameObject.SetActive(true); },
            // Release (풀로 돌려보낼 때)
            (obj) => { obj.gameObject.SetActive(false); },
            // Destroy
            (obj) => {
                instanceToPrefabMap.Remove(obj); // ⭐️ 파괴 시 매핑 정보도 제거합니다.
                Destroy(obj);
            },
            collectionCheck: true,
            defaultCapacity: initialSize,
            maxSize: maxSize
        );

        pools.Add(prefab, pool);

        // 초기 크기만큼 미리 생성합니다.
        for (int i = 0; i < initialSize; i++)
        {
            pool.Release(pool.Get());
        }

        return pool;
    }

    // ----------------------------------------------------
    // 외부 호출 메서드 (Get & Release)
    // ----------------------------------------------------

    /// <summary>
    /// 오브젝트를 풀에서 꺼내 사용합니다. (풀이 없으면 생성)
    /// </summary>
    public GameObject Get(GameObject prefab)
    {
        if (prefab == null) return null;

        // 풀이 없으면 생성
        if (!pools.TryGetValue(prefab, out IObjectPool<GameObject> pool))
        {
            pool = CreatePool(prefab);
        }

        return pool.Get();
    }

    /// <summary>
    /// 오브젝트를 풀로 돌려보냅니다. (GameObject 인스턴스 기반)
    /// </summary>
    public void Release(GameObject obj)
    {
        if (obj == null) return;

        // 1. 인스턴스 맵에서 원본 프리팹을 찾습니다.
        if (instanceToPrefabMap.TryGetValue(obj, out GameObject originalPrefab))
        {
            // 2. 원본 프리팹을 키로 사용하여 해당 풀을 찾습니다.
            if (pools.TryGetValue(originalPrefab, out IObjectPool<GameObject> pool))
            {
                pool.Release(obj);
            }
            else
            {
                // 원본 프리팹은 있는데 풀이 없을 경우 (에러 상황)
                Destroy(obj);
            }
        }
        else
        {
            // 맵에 없는 오브젝트 (풀링으로 생성되지 않은 오브젝트)
            Destroy(obj);
        }
    }
}

// Inspector 설정을 위한 직렬화 가능한 클래스
[System.Serializable]
public class PoolSetup
{
    public GameObject prefab;
    public int initialSize = 10; // 초기 크기
    public int maxSize = 30; // 최대 크기
}