using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillInventory : MonoBehaviour, ISkillContainer
{
    // 플레이어의 현재 스킬 슬롯에 대한 "원조" 데이터를 가지고 있다
    // PlayerSkillManager와 Cooldown_UI는 Inventory의 참조를 가지고 있다
    // 현재 스킬 슬롯에 스킬을 추가하고 빼는 기능이 있다 
    // 임시: 스킬 Instance생성 및 추가를 Start함수에서 한다

    // TODO: 스킬 해금 기록, 영구 재화, 소켓 관련 기능 추가

    [SerializeField] private SkillDeck Deck;
    [SerializeField] private PlayerSkillManager Manager;
    [SerializeField] private Cooldown_UI CooldownUI;
    [SerializeField] private PlayerStatus Status;

    public Skill[] Skills { get; private set; }
    public event Action<Skill, int> OnSkillAdded;
    public event Action<int> OnSkillRemoved;

    private void Start()
    {
        // 초기화 함수 - 이 함수 위에서 작업하면 NullReference뜸 
        InitializeSkillInventory();

        
        AddSkillToSlot(SpawnSkillWithId(1), 1);
        AddSkillToSlot(SpawnSkillWithId(2), 2);
        AddSkillToSlot(SpawnSkillWithId(4), 3);
        AddSkillToSlot(SpawnSkillWithId(96), 4);
    }

    private void InitializeSkillInventory()
    {
        Skills = new Skill[Constants.SKILLSLOT_COUNT]; // 4

        // 참조 생성 - 이제 Cooldown_UI에서 Event를 부름으로 참조 생성을 전부 먼저 해야함
        Manager.InitializeFromInventory(this);
        CooldownUI.InitializeFromInventory(this);
    }

    // 랜덤한 스킬 instance 생성
    public Skill SpawnRandomSkillInstance()
    {
        List<SkillData> selectedSkillData = Deck.GetRandomSkillData(1);
        
        Skill skill = SkillMaker.CreateSkill(selectedSkillData[0]);
        skill.InitializeWithContext(new SkillContext(Manager, Status, 0f));
        return skill;
    }

    // 지정된 ID의 스킬 instance 생성
    public Skill SpawnSkillWithId(int id)
    {
        SkillData data = Deck.GetSkillDataById(id);
        Skill skill = SkillMaker.CreateSkill(data);
        skill.InitializeWithContext(new SkillContext(Manager, Status, 0f));
        return skill;
    }

    /// <summary>
    /// 스킬 instance를 플레이어의 스킬슬롯에 추가
    /// </summary>
    /// <param name="skill">SkillMaker를 통해서 만든 Skill instance</param>
    /// <param name="slot">스킬슬롯 1번 ~ 4번</param>
    public void AddSkillToSlot(Skill skill, int slot) // 1234번 슬롯
    {
        if (slot > Skills.Length || slot <= 0)
        {
            Debug.LogWarning($"{slot}번 슬롯은 존재하지 않습니다!");
            return;
        }

        int slot_index = slot - 1; // 슬롯 번호 -> Array의 index (0123) 으로 변형

        Skills[slot_index] = skill; // 참조 추가
        OnSkillAdded.Invoke(skill, slot); // 이벤트 발생
    }

    public Skill RemoveSkillFromSlot(int slot) // 1234번 슬롯
    {
        if (slot > Skills.Length || slot <= 0)
        {
            Debug.LogWarning($"{slot}번 슬롯은 존재하지 않습니다!");
            return null;
        }

        int slot_index = slot - 1; // 슬롯 번호 -> Array의 index (0123) 으로 변형

        if (Skills[slot_index] == null)
        {
            Debug.LogWarning("$해당 슬롯에 스킬이 없습니다!");
            return null;
        }

        Skill skill = Skills[slot_index]; // 참조 복사
        Skills[slot_index] = null; // 참조 제거
        OnSkillRemoved.Invoke(slot); // 이벤트 발생
        return skill; // 스킬 반환
    }
}
