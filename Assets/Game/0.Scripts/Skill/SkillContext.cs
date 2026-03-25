using UnityEngine;

public class SkillContext
{
    public PlayerSkillManager Manager { get; private set; }
    public PlayerStatus Status { get; private set; }
    public float Damage { get; private set; }

    public SkillContext(PlayerSkillManager manager, PlayerStatus status, float damage)
    {
        Manager = manager;
        Status = status;
        Damage = damage;
    }
}
