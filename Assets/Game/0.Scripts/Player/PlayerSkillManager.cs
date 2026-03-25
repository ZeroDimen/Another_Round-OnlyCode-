using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSkillManager : MonoBehaviour, ISkillContainer
{
    public Skill[] Skills { get; private set; }

    // 발사 지점 (추가 가능)
    // 본인의 참조를 넘기니, manager.[지점 이름]으로 호출 가능
    [SerializeField] private Transform _FirePointCenter;
    [SerializeField] private Transform _FirePointFront;
    [SerializeField] private Transform _FirePointBelow;
    [SerializeField] private Transform _FirePointFloor;
    public Transform FirePointCenter => _FirePointCenter;
    public Transform FirePointFront => _FirePointFront;
    public Transform FirePointBelow => _FirePointBelow;
    public Transform FirePointFloor => _FirePointFloor;


    public void InitializeFromInventory(PlayerSkillInventory inventory)
    {
        Skills = inventory.Skills;
    }

    public void OnSkillInput(InputValue value, int index)
    {
        // 공통 null 체크
        if (Skills[index] == null) // 스킬 슬롯이 비어있으면
        {
            Debug.Log($"{index + 1}번 슬롯에 스킬이 없습니다!");
            return;
        }

        if (value.isPressed) // KeyDown
        {
            Debug.Log($"스킬 {index + 1} KeyDown");
            Skills[index].OnKeyDown();
        }
        else // KeyUp
        {
            Debug.Log($"스킬 {index + 1} KeyUp");
            Skills[index].OnKeyUp();
        }
    }

    public void OnSkill1(InputValue value) => OnSkillInput(value, 0);
    public void OnSkill2(InputValue value) => OnSkillInput(value, 1);
    public void OnSkill3(InputValue value) => OnSkillInput(value, 2);
    public void OnSkill4(InputValue value) => OnSkillInput(value, 3);
}
