using System.Collections;
using UnityEngine;

public class Beam_Skill : Skill
{
    public Beam_Skill(SkillData data) : base(data)
    {
    }
    
    private bool isCharging = false;
    private GameObject attackEffect;
    private ISkillEffect effect;
    private Transform fire_pos;
    private const float SKILL_BASEDAMAGE = 10;
    private float chargeTime = 1;
    
    public override void OnKeyDown()
    {
        if (IsReady())
        {
            // 쿨타임 완료 확인이 됬으니, 실행
            Debug.Log("Beam_Skill 사용"); // 메세지 출력

            StartCooldown(); // 쿨타임 시작

            SpawnEffect(SKILL_BASEDAMAGE); // 이펙트 생성

            SoundManager.Instance.PlaySFX(SoundManager.SFX.BeamCharge); // sound
        }
        else
        {
            // 쿨타임일 경우, 잔여 쿨타임을 계산
            float remaining_cooldown = Data.CooldownTime - (Time.time - time_last_use);
            Debug.Log($"Beam_Skill 쿨타임: {remaining_cooldown:0.00}초");
        }
    }

    public override void OnKeyUp()
    {
        isCharging = false;
        SoundManager.Instance.PlaySFX(SoundManager.SFX.BeamFire); // sound
    }
    
    private void SpawnEffect(float damage)
    {
        chargeTime = 1;
        isCharging = true;
        if (Data.Prefab == null)
        {
            Debug.LogWarning("Beam_Skill 이펙트 prefab이 없습니다!");
            return;
        }

        // prefab을 만듬
        GameObject vfx = GameObject.Instantiate(Data.Prefab, Context.Manager.FirePointFront);
        attackEffect = vfx.transform.GetChild(8).gameObject; // 스킬 사용 이펙트 오브젝트
        
        effect = vfx.GetComponent<ISkillEffect>();
        
        float skilldamage = damage;
        Transform fire_pos = Context.Manager.FirePointFront;
        effect.InitializeEffect(damage, fire_pos);

        // destroy 코루틴 시작
        Context.Manager.StartCoroutine(DestroyAfterActiveTime(vfx));
    }

    private IEnumerator DestroyAfterActiveTime(GameObject vfx)
    {
        Context.Status.LockDirection(true);

        yield return new WaitForSeconds(1f);
        
        while (isCharging)
        {
            yield return new WaitForSeconds(0.5f);
            chargeTime += 0.5f;
            if (chargeTime > 10f)
            {
                isCharging = false; // 10초동안 KeyUp 호출 없을시 자동으로 발사
                SoundManager.Instance.PlaySFX(SoundManager.SFX.BeamFire); // sound
            }
        }
        
        effect.InitializeEffect(SKILL_BASEDAMAGE * chargeTime, fire_pos);
        attackEffect.SetActive(true);
        
        yield return new WaitForSeconds(0.3f);
        Context.Status.LockDirection(false);
        GameObject.Destroy(vfx);
        attackEffect.SetActive(false);
    }
}