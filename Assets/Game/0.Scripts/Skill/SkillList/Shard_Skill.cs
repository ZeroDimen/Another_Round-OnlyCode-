using System.Collections;
using UnityEngine;

/// <summary>
/// 샤드 스킬
/// - 샤드 생성, 데미지, 쿨타임만 관리
/// - 실제 움직임/추적/충돌은 ShardEffect에 위임
/// </summary>
public class Shard_Skill : Skill
{
    public Shard_Skill(SkillData data) : base(data) { }

    private int shardCount = 7;
    private float spawnRadius = 3f;
    private const float WORLD_RADIUS = 25f; // 맵 반지름 (지름 50 기준)
    private Vector3 mapCenter = Vector3.zero;

    public override void OnKeyDown()
    {
        if (!IsReady())
        {
            float remaining = Data.CooldownTime - (Time.time - time_last_use);
            Debug.Log($"Shard_Skill 쿨타임: {remaining:0.00}초");
            return;
        }

        StartCooldown();

        // 기본 데미지는 Data나 하드코딩으로 조정
        float skillDamage = 5f;

        // 여러 샤드 생성
        SpawnShards(Context.Manager, skillDamage);

        SoundManager.Instance.PlaySFX(SoundManager.SFX.Shard); // sound
    }

    public override void OnKeyUp() { }

    // 샤드들을 플레이어 주변에 생성
    private void SpawnShards(PlayerSkillManager manager, float damage)
    {
        if (Data.Prefab == null)
        {
            Debug.LogWarning("Shard_Skill: 프리팹이 설정되지 않았습니다!");
            return;
        }

        Transform firePoint = manager.FirePointCenter;

        // 플레이어의 곡면 기반 위치
        float playerX2D = Circle2DConverter.X_3Dto2D(firePoint.position);
        Vector3 playerCurvedPos = Circle2DConverter.X_2Dto3D(playerX2D, WORLD_RADIUS, firePoint.position.y);

        // 플레이어 위치에서 맵 바깥쪽/접선 방향 계산
        Vector3 outwardFromMapAtPlayer = (playerCurvedPos - mapCenter).normalized;
        Vector3 tangentDir = new Vector3(-outwardFromMapAtPlayer.z, 0f, outwardFromMapAtPlayer.x).normalized;

        // FirePoint.forward에서 피치(상하 기울기) 계산 (X축 회전에 사용 가능)
        Vector3 fpForward = firePoint.forward.normalized;
        float forwardFlatMag = Mathf.Sqrt(fpForward.x * fpForward.x + fpForward.z * fpForward.z);
        float fpPitch = Mathf.Atan2(fpForward.y, forwardFlatMag) * Mathf.Rad2Deg;

        for (int i = 0; i < shardCount; i++)
        {
            float angleDeg = (360f / shardCount) * i;
            float rad = angleDeg * Mathf.Deg2Rad;

            float offsetY = Mathf.Sin(rad) * spawnRadius;
            float offsetTangent = Mathf.Cos(rad) * spawnRadius;

            // 플레이어 곡면 위치 기준으로 샤드 생성 위치 계산
            Vector3 spawnPos = playerCurvedPos
                + tangentDir * offsetTangent
                + Vector3.up * (offsetY + 1f);

            // 플레이어 기준 바깥쪽을 전방으로 사용
            Vector3 outwardFromPlayer = (spawnPos - firePoint.position).normalized;
            if (outwardFromPlayer.sqrMagnitude < 0.0001f) outwardFromPlayer = firePoint.forward;

            // 곡면 법선(맵 중심 -> spawnPos)을 up으로 사용
            Vector3 normalFromMap = (spawnPos - mapCenter).normalized;

            // 기본 회전: forward = outwardFromPlayer, up = normalFromMap
            Quaternion baseRot = Quaternion.LookRotation(outwardFromPlayer, normalFromMap);

            // 최종 보정: X = FirePoint.forward 기반 pitch, Z = 90 고정
            Vector3 euler = baseRot.eulerAngles;
            euler.x = fpPitch;
            euler.z = 90f;
            Quaternion spawnRot = Quaternion.Euler(euler);

            // 샤드 생성
            GameObject shard = GameObject.Instantiate(Data.Prefab, spawnPos, spawnRot);
            shard.transform.SetParent(null);

            // ShardEffect 초기화
            ShardEffect se = shard.GetComponent<ShardEffect>();
            if (se == null)
            {
                Debug.LogWarning("Shard prefab에 ShardEffect 컴포넌트가 없습니다.");
                continue;
            }

            se.Initialize(damage, firePoint, 10f, 12f, WORLD_RADIUS, mapCenter);
            
            // 자동 제거 (Data.ActiveTime 사용)
            Context.Manager.StartCoroutine(DestroyAfterActiveTime(shard, Data.ActiveTime));
        }
    }

    private IEnumerator DestroyAfterActiveTime(GameObject shard, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (shard != null) GameObject.Destroy(shard);
    }
}
