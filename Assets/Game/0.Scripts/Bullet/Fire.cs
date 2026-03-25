using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
//using UnityEngine.Pool;

public class Fire : MonoBehaviour
{
    [Header("총알 설정")]
    [SerializeField] private GameObject bulletPrefab; // 총알 프리팹
    [SerializeField] private GameObject firePoint; // 총알 발사 위치 (기준점)
    [SerializeField] private float firePointOffsetZ = 0.5f; // 좌우 대칭을 위한 Z축 오프셋 (인스펙터에서 설정)

    private PlayerActions playerActions; // 플레이어 액션 스크립트 참조
    private Transform player; // 플레이어 Transform 참조
    private float currentRotationSign = 1f; // 연속 발사 시 총알의 회전 방향

    private Coroutine fireCoroutine = null;
    //private IObjectPool<Bullet> _managedPool;

    private void Awake()
    {
        //_managedPool = new ObjectPool<Bullet>(CreateBullet, OnGetBullet, OnReleaseBullet, OnDestroyBullet, maxSize: 30);

        // 1. Player Transform을 먼저 찾습니다.
        player = GameObject.FindWithTag("Player")?.transform;

        // 2. Player Transform이 있으면, 거기서 PlayerActions 컴포넌트를 가져옵니다.
        if (player != null)
        {
            playerActions = player.GetComponent<PlayerActions>();
        }

        if (player == null || playerActions == null)
        {
            Debug.LogError("Player 또는 PlayerActions 스크립트를 찾을 수 없습니다! 태그와 컴포넌트 할당을 확인하세요.");
        }
    }

    // 단발 공격 로직 (PlayerActions에서 호출됨)
    public void BasicAttackFire(FireState state)
    {
        if (playerActions == null) return;

        //var bullet = _managedPool.Get();

        //PoolManager를 통해 총알 GameObject를 가져옵니다.
        GameObject newBulletObj = PoolManager.Instance.Get(bulletPrefab);

        //Bullet 컴포넌트를 가져옵니다.
        Bullet bullet = newBulletObj.GetComponent<Bullet>();
        if (bullet == null) return;

        // 1. firePoint의 월드 위치에서 시작합니다.
        Vector3 spawnPosition = firePoint.transform.position;
        float rotationSign;

        // 2. FireState에 따라 Z축 오프셋과 회전 부호를 결정합니다.
        if (state == FireState.RIGHT)
        {
            rotationSign = -1f; // 오른쪽 부호
            spawnPosition.z += firePointOffsetZ; // 오른쪽 오프셋 적용
        }
        else // FireState.LEFT
        {
            rotationSign = 1f; // 왼쪽 부호
            spawnPosition.z -= firePointOffsetZ; // 왼쪽 오프셋 적용
        }

        // 3. 위치 및 방향 적용
        bullet.transform.position = spawnPosition;
        bullet.Shoot(rotationSign);
    }

    //// 풀매니저로 통합하기 전 오브젝트 풀링
    // ----------------------------------------
    // Object Pool 관련 메소드
    // ----------------------------------------

    //private Bullet CreateBullet()
    //{
    //    // 총알이 이 스크립트의 GameObject를 부모로 가지도록 생성 (선택 사항)
    //    Bullet bullet = Instantiate(bulletPrefab, transform).GetComponent<Bullet>();

    //    // 발사 지점(firePoint)을 부모로 설정하는 것은 Instantiate(prefab, parent) 형태를 쓸 때만 의미가 있습니다.
    //    // 현재는 World Space에 생성하고 있으므로, 위의 코드를 사용합니다.

    //    bullet.SetManagedPool(_managedPool);
    //    return bullet;
    //}

    //private void OnGetBullet(Bullet bullet)
    //{
    //    bullet.gameObject.SetActive(true);
    //}

    //private void OnReleaseBullet(Bullet bullet)
    //{
    //    bullet.gameObject.SetActive(false);
    //}

    //private void OnDestroyBullet(Bullet bullet)
    //{
    //    Destroy(bullet.gameObject);
    //}
}