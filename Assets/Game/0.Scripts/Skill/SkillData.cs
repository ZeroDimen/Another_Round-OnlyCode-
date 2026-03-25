using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Scriptable Objects/SkillData")]
public class SkillData : ScriptableObject
{
    [SerializeField] private int _ID; // index
    [SerializeField] private string _Name; // 이름
    [SerializeField] private string _Description; // 설명
    [SerializeField] private float _CooldownTime; // 쿨타임
    [SerializeField] private float _ActiveTime; // 지속시간
    [SerializeField] private Sprite _Icon; // 이미지
    [SerializeField] private GameObject _Prefab; // 파티클 오브젝트

    // public int ID {get => _ID; set => _ID = value} 
    // Setter 함수들이 필요없으니 제거함
    // ScriptableObject 클래스는 직접 변경하는것보다 복사해서 쓰는게 더 깔끔함
    public int ID => _ID;
    public string Name => _Name;
    public string Description => _Description;
    public float CooldownTime => _CooldownTime;
    public float ActiveTime => _ActiveTime;
    public Sprite Icon => _Icon;
    public GameObject Prefab => _Prefab;
}
