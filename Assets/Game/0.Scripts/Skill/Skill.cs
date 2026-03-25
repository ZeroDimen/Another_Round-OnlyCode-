using UnityEngine;

public abstract class Skill
{
    public SkillData Data { get; private set; }
    public SkillContext Context { get; private set; }

    // SkillData에서 정보를 불러오는 Constructor
    public Skill(SkillData data)
    {
        Data = data;
    }

    // SkillContext에서 정보를 불러오는 Initialize 함수
    public void InitializeWithContext(SkillContext context)
    {
        Context = context;
    }

    // 마지막 사용 시간 [디폴트 -100초]
    protected float time_last_use = -100f;

    // KeyDown혹은 KeyUp에서 호출     
    public bool IsReady()
    {
        // 현재 시간 >= 마지막 사용 시간 + 쿨타임
        return Time.time >= time_last_use + Data.CooldownTime;
    }

    // KeyDown혹은 KeyUp에서 호출
    public virtual void StartCooldown()
    {
        time_last_use = Time.time;
    }

    // 스킬 쿨다운 UI에서 호출
    public float CurrentCooldownRatio()
    {
        // 스킬이 쿨타임이 아니면 0
        if (IsReady()) return 0f;

        // 쿨타임일 경우, 잔여 쿨타임을 계산
        float time_elapsed = Time.time - time_last_use;
        // 잔여 쿨타임 / 총 쿨타임
        float ratio = (Data.CooldownTime - time_elapsed) / Data.CooldownTime;
        // 비율 반환
        return ratio;
    }

    // PlayerSkillManager 클래스에서 호출
    // KeyDown과 KeyUp 행동을 다르게 설정해놓음 (KeyUp은 홀드/차징/온오프 스킬 등 여러 스킬 타입을 만들 수 있음)
    // 매니저를 넘겨야 MonoBehaviour를 사용한 Coroutine을 쓸 수 있음 + Transform 참조도 받기 쉬워짐
    // 이미 InitializeWithContext()를 통해 매니저를 넘겨줌
    public abstract void OnKeyDown();
    public abstract void OnKeyUp();
}
