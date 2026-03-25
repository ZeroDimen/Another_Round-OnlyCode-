using UnityEngine;
using System.Collections;

public class Blackhole_Skill : Skill
{
    public Blackhole_Skill(SkillData data) : base(data)
    {
    }

    public override void OnKeyDown()
    {
        if (IsReady())
        {
            float SKILL_BASEDAMAGE = 5f; // 기본 피해량
            StartCooldown(); // 쿨타임 시작
            SpawnEffect(SKILL_BASEDAMAGE); // 블랙홀 생성
            SoundManager.Instance.PlaySFX(SoundManager.SFX.Blackhole); // sound
        }
        else
        {
            float remaining = Data.CooldownTime - (Time.time - time_last_use);
            Debug.Log($"Blackhole_Skill 쿨타임: {remaining:0.00}초");
        }
    }

    public override void OnKeyUp()
    {
    }

    private void SpawnEffect(float damage)
    {
        if (Data.Prefab == null)
        {
            return;
        }

        // 플레이어의 바라보는 방향
        Transform firePoint = Context.Manager.FirePointFront;

        // (1) firePoint 기준으로 Z축 방향으로 일정 수치만큼 앞 위치 계산
        Vector3 offsetPos = firePoint.position + firePoint.forward * 5f;

        // 현재 위치의 원상 좌표 구하기
        float currentX_2D = Circle2DConverter.X_3Dto2D(offsetPos);


        // 현재 반지름 계산
        float radius = new Vector2(firePoint.position.x, firePoint.position.z).magnitude;

        // 곡면 위 위치 계산
        Vector3 spawnPos = Circle2DConverter.X_2Dto3D(currentX_2D, radius, firePoint.position.y);

        // 맵 중심(0, y, 0)을 기준으로, 소환 지점이 바깥쪽을 바라보도록 방향 계산
        Vector3 radialDir = (spawnPos - new Vector3(0f, spawnPos.y, 0f)).normalized;

        // 블랙홀이 플레이어의 진행 방향을 바라보게 회전
        // (맵 곡률에 따라 Y축이 자연스럽게 회전되도록)
        Quaternion baseRot = Quaternion.LookRotation(radialDir, Vector3.up);

        Quaternion spawnRot = baseRot * Quaternion.Euler(0f, 90f, 90f);

        // 블랙홀 생성
        GameObject vfx = GameObject.Instantiate(Data.Prefab, spawnPos, spawnRot);
        vfx.transform.SetParent(null);

        // 블랙홀을 플레이어 하이라키 밖으로 두기 (플레이어 따라가지 않게)
        vfx.transform.SetParent(null);

        // 이펙트 초기화
        ISkillEffect effect = vfx.GetComponent<ISkillEffect>();
        if (effect != null)
        {
            effect.InitializeEffect(damage, firePoint);
        }

        // 지속시간이 끝나면 제거
        Context.Manager.StartCoroutine(DestroyAfterActiveTime(vfx));
    }

    private IEnumerator DestroyAfterActiveTime(GameObject vfx)
    {
        yield return new WaitForSeconds(Data.ActiveTime);
        GameObject.Destroy(vfx);
    }
}
