using System.Collections;
using UnityEngine;

public class Laser_Skill : Skill
{
    public Laser_Skill(SkillData data) : base(data)
    {
    }

    public override void OnKeyDown()
    {
        if (IsReady()) // 스킬이 쿨타임이 아니면
        {
            float SKILL_BASEDAMAGE = 0f;

            Debug.Log("Laser_Skill 사용"); // 메세지 출력

            StartCooldown(); // 쿨타임 시작

            SpawnEffect(SKILL_BASEDAMAGE); // 이펙트 생성

            SoundManager.Instance.PlaySFX(SoundManager.SFX.Laser); // sound
        }
        else
        {
            // 쿨타임일 경우, 잔여 쿨타임을 계산
            float remaining_cooldown = Data.CooldownTime - (Time.time - time_last_use);
            Debug.Log($"Laser_Skill 쿨타임: {remaining_cooldown:0.00}초");
        }
    }

    public override void OnKeyUp()
    {
    }

    private void SpawnEffect(float damage)
    {
        if (Data.Prefab == null)
        {
            Debug.LogWarning("ExampleSkill에 이펙트 prefab이 없습니다!");
            return;
        }

        // prefab을 만듬
        GameObject vfx = GameObject.Instantiate(Data.Prefab, Context.Manager.FirePointFront);
        ISkillEffect effect = vfx.GetComponent<ISkillEffect>();        
        float skilldamage = damage;
        Transform fire_pos = Context.Manager.FirePointFront;
        effect.InitializeEffect(damage, fire_pos);

        // destroy 코루틴 시작
        Context.Manager.StartCoroutine(DestroyAfterActiveTime(vfx));
    }

    private IEnumerator DestroyAfterActiveTime(GameObject vfx)
    {
        Context.Status.LockDirection(true);
        yield return new WaitForSeconds(Data.ActiveTime);
        Context.Status.LockDirection(false);
        GameObject.Destroy(vfx);
    }
}
