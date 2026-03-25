using System.Collections;
using UnityEngine;

public class BlackholeEffect : MonoBehaviour, ISkillEffect
{
    private float damage;
    private float pullRadius = 6f;     // 흡입 범위
    private float damageRadius = 4f;    // 폭발 및 지속 데미지 범위
    private float pullSpeed = 5f;       // 초당 끌어당기는 속도
    private float damageInterval = 1f;  // 데미지 주는 간격
    private float activeTime = 4f;
    private float shakeIntensity = 0.5f;  // 중심 근처 흔들림 세기
    private float explosionDamage = 100f;

    [Header("폭발 이펙트")]
    public GameObject explosionPrefab;

    [Header("Explosion Settings")]
    public float explosionDuration = 1.5f; // 폭발 유지 시간 동안도 끌어당김 유지

    private float nextDamageTime = 0f;

    public void InitializeEffect(float skillDamage, Transform firePos)
    {
        damage = skillDamage;
        StartCoroutine(ActivateBlackhole());
    }

    private IEnumerator ActivateBlackhole()
    {
        float elapsed = 0f;

        // 블랙홀 유지 시간 동안 적 끌어당기기
        while (elapsed < activeTime)
        {
            AttractAndDamageEnemies();
            elapsed += Time.deltaTime;
            yield return null;
        }

        // === 폭발 발생 및 폭발 중에도 끌어당김 유지 ===
        yield return StartCoroutine(TriggerExplosion());
        Destroy(gameObject);
    }

    private void AttractAndDamageEnemies()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pullRadius);

        // 이번 프레임에 데미지를 줄지 여부 판단
        bool shouldDealDamage = Time.time >= nextDamageTime;

        foreach (Collider hit in hits)
        {
            EnemyController enemy = hit.GetComponent<EnemyController>();
            if (enemy == null) continue;

            Vector3 dir = (transform.position - enemy.transform.position).normalized;
            float dist = Vector3.Distance(transform.position, enemy.transform.position);

            // 거리 기반 속도 감소 (중심 가까울수록 천천히)
            float t = Mathf.Clamp01(1f - (dist / pullRadius));
            float moveSpeed = pullSpeed * (0.3f + t);

            // 부드럽게 이동 (teleport 방지)
            Vector3 targetPos = Vector3.Lerp(enemy.transform.position, transform.position, moveSpeed * Time.deltaTime);

            // 중심 근처에서 살짝 흔들림
            if (dist < pullRadius * 0.4f)
            {
                Vector3 shake = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f)
                ) * shakeIntensity * Time.deltaTime;
                targetPos += shake;
            }

            enemy.transform.position = targetPos;

            // 일정 간격으로 데미지
            if (shouldDealDamage && dist <= damageRadius)
                enemy.SetHit((int)damage);
        }

        if (shouldDealDamage)
            nextDamageTime = Time.time + damageInterval;
    }

    private IEnumerator TriggerExplosion()
    {
        float nextDamageTime = 0f;

        // 폭발 이펙트 생성
        if (explosionPrefab != null)
        {
            Quaternion explosionRot = transform.rotation;
            GameObject explosion = Instantiate(explosionPrefab, transform.position, explosionRot);
            Destroy(explosion, explosionDuration + 0.5f);
        }

        float preExplosionTime = 0.5f;
        float wait = 0f;
        while (wait < preExplosionTime)
        {
            AttractAndDamageEnemies();
            wait += Time.deltaTime;
            yield return null;
        }

        // 폭발 지속 시간 동안 계속 적 끌어당기기 유지
        float elapsed = 0f;
        float duration = 5.5f;

        while (elapsed < duration)
        {
            AttractAndDamageEnemies();
            elapsed += Time.deltaTime;

            // 3초가 끝나기 직전(마지막 루프)에 폭발 데미지를 한 번만 적용
            if (elapsed + Time.deltaTime >= duration)
            {
                Collider[] hits = Physics.OverlapSphere(transform.position, damageRadius);
                foreach (Collider hit in hits)
                {
                    EnemyController enemy = hit.GetComponent<EnemyController>();
                    if (enemy != null)
                    {
                        int finalDamage = (int)explosionDamage;
                        enemy.SetHit(finalDamage);
                    }
                }
            }
        }

        // 종료 직전에 잠깐 대기
        yield return new WaitForSeconds(0.05f);
    }

    private void OnDrawGizmosSelected()
    {
        // 흡입 범위 (노랑)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pullRadius);

        // 지속 및 폭발 범위 (빨강)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
