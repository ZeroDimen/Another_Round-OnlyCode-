using UnityEngine;
using static Constants;

public class EnemyBullet : MonoBehaviour
{
    [Header("총알 속성")]
    [Tooltip("총알이 자동으로 사라지는 시간 (풀로 반환될 시간)")]
    [SerializeField] private float lifetime = 5f;
    public EnemyStatus enemyStatus;

    [Header("회전 이동 설정")]
    [Tooltip("Z축을 기준으로 회전하며 이동할 때 회전하는 속도")]
    [SerializeField] private float rotationSpeed = 20f;

    [SerializeField] private GameObject enemyHitEffectPrefab; // 총알이 플레이어와 충돌할 때 생성될 이펙트 프리팹

    //이동 및 회전에 필요한 변수
    private float rotationSign; // ⭐️ 회전 방향 (1.0f 또는 -1.0f)

    private void OnEnable()
    {
        float currentLifetime = lifetime;
        Invoke("DestroyBullet", currentLifetime);
    }

    private void OnDisable()
    {
        CancelInvoke("DestroyBullet");
        
        // 풀로 돌아갈 때 이동 변수 초기화
        rotationSign = 0f; // 초기화

        // ⭐️ 풀로 돌아갈 때 회전도 초기화 (선택 사항: 씬 계층 구조를 깔끔하게 유지)
        transform.localRotation = Quaternion.identity;
    }

    private void Update()
    {
        float currentRotationSpeed = rotationSpeed;

        // ⭐️ 맵 중심점을 기준으로 Y축 회전 축을 따라 공전합니다. ⭐️
        if (rotationSign != 0f)
        {
            // Vector3.up은 Y축(수직)을 회전 축으로 사용하며, rotationSign으로 방향을 결정합니다.
            Vector3 rotationAxis = Vector3.up * rotationSign;
            transform.RotateAround(Vector3.zero, rotationAxis, currentRotationSpeed * Time.deltaTime);
        }
    }

    // ----------------------------------------
    // 외부에서 호출될 초기화 메서드 (총알 발사 직후)
    // ----------------------------------------

    /// <summary>
    /// 총알의 이동 방향, 속도, 그리고 Z축 회전 방향을 설정합니다.
    /// </summary>
    /// <param name="direction">총알이 나아갈 초기 방향</param>
    /// <param name="speed">총알의 이동 속도</param>
    /// <param name="sign">Z축 회전 방향 부호 (1.0f: 시계, -1.0f: 반시계)</param>
    public void Initialize(Vector3 direction, float speed, float sign)
    {
        rotationSign = sign; // ⭐️ 공전 방향 저장

        // RotateAround 방식은 총알 자체의 forward를 이동에 사용하지 않으므로, 
        // 맵 중심을 바라보게 회전시키는 로직은 선택 사항입니다.
        // Vector3 centerDir = (pivotPoint - transform.position).normalized;
        // transform.forward = centerDir;
    }

    // ----------------------------------------
    // 충돌 처리 및 제거 로직 (기존과 동일)
    // ----------------------------------------

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PlayerHitbox"))
        {
            //이펙트 오브젝트를 풀에서 가져옵니다.
            GameObject enemyhitEffect = PoolManager.Instance.Get(enemyHitEffectPrefab);
            enemyhitEffect.transform.position = transform.position;
            enemyhitEffect.transform.rotation = transform.rotation;
            DestroyBullet();
        }
    }

    public void DestroyBullet()
    {
        if (gameObject.activeInHierarchy)
        {
            if (PoolManager.Instance != null)
            {
                PoolManager.Instance.Release(this.gameObject);
            }
            else
            {
                Destroy(gameObject); 
            }
        }
    }
}