using UnityEngine;

public static class SkillMaker
{
    public static Skill CreateSkill(SkillData data)
    {
        if (data == null)
        {
            Debug.LogWarning($"CreateSkill: 스킬 데이터가 null입니다!");
            return null;
        }

        switch (data.ID)
        {
            case 0:
                return new Laser_Skill(data);
            case 1:
                return new Tornado_Skill(data);
            case 2:
                return new Beam_Skill(data);
            case 3:
                return new Spray_Skill(data);
            case 4:
                return new Shield_Skill(data);
            case 96:
                return new Shard_Skill(data);
            case 97:
                return new Blackhole_Skill(data);
            case 98:
                return new Wave_Skill(data);
            default:
                Debug.LogWarning($"해당 스킬ID({data.ID})에 맞는 스킬 데이터가 없습니다.");
                return null;

        }
    }
}
