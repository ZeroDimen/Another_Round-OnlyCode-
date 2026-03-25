using UnityEngine;
using static Constants;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float rotationSpeed; // 총알의 회전 속도
    [SerializeField] private float lifetime = 0.3f; // 총알의 수명(초)

    [SerializeField] private GameObject hitEffectPrefab; // 총알이 적과 충돌할 때 생성될 이펙트 프리팹

    private float fixedRotationSign; // 총알의 회전 방향

    private Vector3 pivotPoint = new Vector3(0, 0, 0); // 원형 맵의 중심점(하드 코딩)

    private void OnDisable()
    {
        // 풀에서 다시 사용될 때 이전에 사용했던 회전 값이 남아있지 않도록 초기화
        fixedRotationSign = 0f;

        // 추가적으로, 총알 오브젝트의 로컬 회전값도 초기화하는 것이 좋습니다.
        transform.localRotation = Quaternion.identity;
    }

    // 원형 맵을 기준으로 회전 이동
    private void Update()
    {
        float currentRotationSpeed = rotationSpeed; // 현재 회전 속도

        Vector3 rotationAxis = Vector3.up * fixedRotationSign; // 회전 축 설정(음수면 오른쪽, 양수면 왼쪽)
        transform.RotateAround(pivotPoint, rotationAxis, currentRotationSpeed * Time.deltaTime);
    }

    // 발사 메소드
    public void Shoot(float rotationSign)
    {
        float currentLifetime = lifetime;

        fixedRotationSign = rotationSign; // 회전 방향 설정
        SoundManager.Instance.PlaySFX(SoundManager.SFX.BulletFire); // 총알 발사 사운드 재생
        Invoke("DestroyBullet", currentLifetime); // 수명 후에 총알 제거 예약
    }

    // 총알 제거 메소드
    public void DestroyBullet()
    {
        CancelInvoke("DestroyBullet");

        if (gameObject.activeInHierarchy)
        {
            PoolManager.Instance.Release(this.gameObject);
        }
    }

    // 총알 충돌 처리
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("Boss"))
        {
            //이펙트 오브젝트를 풀에서 가져옵니다.
            SoundManager.Instance.PlaySFX(SoundManager.SFX.EnemyHit); // 적 피격 사운드 재생
            GameObject hitEffect = PoolManager.Instance.Get(hitEffectPrefab);
            hitEffect.transform.position = transform.position;
            hitEffect.transform.rotation = transform.rotation;
            
            // 적과 충돌 시 총알 제거
            DestroyBullet();
        }
    }
}
