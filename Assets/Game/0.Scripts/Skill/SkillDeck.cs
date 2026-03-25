using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillDeck", menuName = "Scriptable Objects/Skill Deck")]
public class SkillDeck : ScriptableObject
{
    [SerializeField] private SkillData[] _AllSkills;
    public SkillData[] AllSkills => _AllSkills;

    // 스킬덱에서 랜덤으로 스킬 num개 반환
    public List<SkillData> GetRandomSkillData(int num = 1)
    {
        // 스킬덱이 없을 경우
        if (_AllSkills == null || _AllSkills.Length == 0)
        {
            Debug.LogWarning("스킬덱에 스킬이 없습니다!");
            return new List<SkillData>(); // 빈 리스트 반환
        }
        List<SkillData> selectedSkills = new List<SkillData>();
        List<SkillData> tempSkills = new List<SkillData>(_AllSkills);
        
        int countToPick = Mathf.Min(num, tempSkills.Count);
        
        for (int i = 0; i < countToPick; i++)
        {
            int randomIndex = Random.Range(0, tempSkills.Count);
            
            SkillData randomSkill = tempSkills[randomIndex];
            selectedSkills.Add(randomSkill);
            
            tempSkills.RemoveAt(randomIndex);
        }
        
        if (num > _AllSkills.Length)
        {
            Debug.LogWarning($"요청한 스킬이 {_AllSkills.Length}개보다 많음)");
        }
        
        return selectedSkills;
    }
 
    public SkillData GetSkillDataById(int id)
    {
        // 스킬덱이 없을 경우
        if (_AllSkills == null || _AllSkills.Length == 0)
        {
            Debug.LogWarning("스킬덱에 스킬이 없습니다!");
            return null;
        }

        // 스킬 검색; Array 방식
        SkillData search_result = null;
        foreach (var skill in _AllSkills)
        {
            if (id == skill.ID)
            {
                search_result = skill;
                Debug.Log($"SkillID {id}를 가진 스킬을 찾았습니다. 스킬명: {search_result.Name}");
                return search_result;
            }
        }

        if (search_result == null)
        {
            Debug.LogWarning($"해당 SkillID({id})를 가진 스킬이 없습니다!");
        }
        return null;
    }
}
