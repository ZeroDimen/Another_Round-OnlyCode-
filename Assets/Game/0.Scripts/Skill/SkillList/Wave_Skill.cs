using System.Collections;
using UnityEngine;

public class Wave_Skill : Skill
{
    public Wave_Skill(SkillData data) : base(data)
    {
    }

    public override void OnKeyDown()
    {
        if (IsReady())
        {
            float SKILL_BASEDAMAGE = 98f;
            // 쿨타임 완료 확인이 됬으니, 실행
            Debug.Log("Wave_Skill 사용"); // 메세지 출력

            StartCooldown(); // 쿨타임 시작

            SpawnEffect(SKILL_BASEDAMAGE); // 이펙트 생성

            SoundManager.Instance.PlaySFX(SoundManager.SFX.Wave); // sound
        }
        else
        {
            // 쿨타임일 경우, 잔여 쿨타임을 계산
            float remaining_cooldown = Data.CooldownTime - (Time.time - time_last_use);
            Debug.Log($"Wave_Skill 쿨타임: {remaining_cooldown:0.00}초");
        }
    }

    public override void OnKeyUp()
    {
        
    }
    
    private void SpawnEffect(float damage)
    {
        if (Data.Prefab == null)
        {
            Debug.LogWarning("Wave_Skill 이펙트 prefab이 없습니다!");
            return;
        }

        // prefab을 만듬
        GameObject vfx = GameObject.Instantiate(Data.Prefab, Context.Manager.FirePointCenter.position, Context.Manager.FirePointCenter.rotation);
        ISkillEffect effect = vfx.GetComponent<ISkillEffect>();
        float skilldamage = damage;
        Transform fire_pos = Context.Manager.FirePointCenter;
        effect.InitializeEffect(damage, fire_pos);

        // destroy 코루틴 시작
        Context.Manager.StartCoroutine(DestroyAfterActiveTime(vfx));
    }

    private IEnumerator DestroyAfterActiveTime(GameObject vfx)
    {
        yield return new WaitForSeconds(Data.ActiveTime);
        GameObject.Destroy(vfx);
    }
}