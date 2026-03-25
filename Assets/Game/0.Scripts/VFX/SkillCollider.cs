using UnityEngine;

public class SkillCollider : MonoBehaviour, ISkillEffect
{
    private float damage;
    public float Damage { get => damage; private set => value = damage; }

    public void InitializeEffect(float damage, Transform firePosition)
    {
        this.damage =  damage;
        Debug.Log($"{name}이 Damage: {this.damage}로 초기화되었습니다.");
    }
}
