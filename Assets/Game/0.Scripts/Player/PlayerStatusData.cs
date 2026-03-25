using UnityEngine;

public class PlayerStatusData : MonoBehaviour
{
    public static PlayerStatusData Instance { get; private set; }

    // 기본 스탯
    private const float DEFAULT_HP = 100f;
    private const float DEFAULT_ATTACK = 10f;

    // 현재(업그레이드 적용된) 수치
    public float CurrentHP { get; private set; }
    public float CurrentAttack { get; private set; }

    // 업그레이드 레벨 (저장됨)
    private int hpLevel;
    private int attackLevel;

    // 최대 레벨 제한
    private const int MAX_LEVEL = 10;

    // 업그레이드당 증가량
    private const int HP_PER_LEVEL = 2;
    private const int ATTACK_PER_LEVEL = 2;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData(); // 실행 시 PlayerPrefs에서 불러오기
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 저장된 값 불러오기
    private void LoadData()
    {
        hpLevel = PlayerPrefs.GetInt("HPLevel", 0);
        attackLevel = PlayerPrefs.GetInt("AttackLevel", 0);
        RecalculateStats();
    }

    // 현재 수치 계산
    private void RecalculateStats()
    {
        CurrentHP = DEFAULT_HP + HP_PER_LEVEL * hpLevel;
        CurrentAttack = DEFAULT_ATTACK + ATTACK_PER_LEVEL * attackLevel;
    }

    // 현재 상태 저장
    private void SaveData()
    {
        PlayerPrefs.SetInt("HPLevel", hpLevel);
        PlayerPrefs.SetInt("AttackLevel", attackLevel);
        PlayerPrefs.Save();
    }

    // 외부 호출용
    public void UpgradeHP()
    {
        if (hpLevel < MAX_LEVEL)
        {
            hpLevel++;
            SaveData();
            RecalculateStats();
        }
        else
        {
            Debug.Log("[PlayerStatusData] HP는 이미 최대 레벨입니다!");
        }
    }

    public void UpgradeAttack()
    {
        if (attackLevel < MAX_LEVEL)
        {
            attackLevel++;
            SaveData();
            RecalculateStats();
        }
        else
        {
            Debug.Log("[PlayerStatusData] 공격력은 이미 최대 레벨입니다!");
        }
    }

    // 초기화 (기본값으로 복구)
    public void ResetUpgrades()
    {
        hpLevel = 0;
        attackLevel = 0;
        SaveData();
        RecalculateStats();
    }

    // PlayerStatus가 쓸 Getter
    public float GetHP() => CurrentHP;
    public float GetAttack() => CurrentAttack;

    // Base 값 Getter (참고용)
    public float GetDefaultHP() => DEFAULT_HP;
    public float GetDefaultAttack() => DEFAULT_ATTACK;

    // UpgradeUIManager가 쓸 Lv 값
    public int GetHPLevel() => hpLevel;
    public int GetAttackLevel() => attackLevel;

}
