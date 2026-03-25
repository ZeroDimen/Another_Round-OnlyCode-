using System.Collections;
using UnityEngine;

public class AutoReleaseEffect : MonoBehaviour
{
    private ParticleSystem ps;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void OnEnable()
    {
        // 파티클 시스템 재생
        if (ps != null)
        {
            ps.Play();
            // 파티클 재생 시간만큼 기다린 후 풀로 반환하는 코루틴 시작
            StartCoroutine(ReleaseAfterDuration(ps.main.duration + ps.main.startLifetime.constantMax));
        }
    }

    private IEnumerator ReleaseAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);

        // 풀로 반환
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Release(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
