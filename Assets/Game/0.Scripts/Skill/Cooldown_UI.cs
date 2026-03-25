using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Cooldown_UI : MonoBehaviour, ISkillContainer
{
    public Skill[] Skills { get; private set; } // 스킬 데이터 구조
    [SerializeField] private Image[] SlotBorder = new Image[4];
    [SerializeField] private Image[] Cooldowns = new Image[4]; // 슬롯[커버] 이미지
    [SerializeField] private Image[] Icons = new Image[4];
    [SerializeField] private float flash_duration = 0.5f;
    private bool[] cooldownSFXplayed = new bool[4];

    private void Start()
    {
        for (int i = 0; i < cooldownSFXplayed.Length; i++)
        {
            cooldownSFXplayed[i] = true;
        }
    }

    private void Update()
    {
        UpdateCooldowns();
    }

    /// <summary>
    /// 매 프레임마다 Skill내부에서 쿨타임을 가져온다
    /// </summary>
    private void UpdateCooldowns()
    {
        for (int i = 0; i < Constants.SKILLSLOT_COUNT; i++)
        {
            // 해당 슬롯에 스킬이 있다면
            if (Skills[i] != null)
            {
                // 스킬 진행도를 불러옴
                Cooldowns[i].fillAmount = Skills[i].CurrentCooldownRatio();

                if (Skills[i].IsReady() && !cooldownSFXplayed[i])
                {
                    StartCoroutine(FlashReadyRoutine(SlotBorder[i]));
                    SoundManager.Instance.PlaySFX(SoundManager.SFX.CooldownDone);
                    cooldownSFXplayed[i] = true;
                }
                else if (Skills[i].CurrentCooldownRatio() > 0 && cooldownSFXplayed[i])
                {
                    cooldownSFXplayed[i] = false;
                }
            }
        }
    }

    private void UpdateIcon_SkillAdded(Skill skill, int slot)
    {
        UpdateIcon(skill, slot);
    }

    private void UpdateIcon_SkillRemoved(int slot)
    {
        UpdateIcon(null, slot);
    }

    private void UpdateIcon(Skill skill, int slot)
    {
        int slot_index = slot - 1; // 슬롯 번호 -> Array의 index (0123) 으로 변형
        
        if (skill == null)
        {
            Icons[slot_index].sprite = null;
        }
        else
        {
            Icons[slot_index].sprite = skill.Data.Icon;
        }
    }

    public void InitializeFromInventory(PlayerSkillInventory inventory)
    {
        Skills = inventory.Skills;

        // 이벤트 Listener 추가
        inventory.OnSkillAdded += UpdateIcon_SkillAdded;
        inventory.OnSkillRemoved += UpdateIcon_SkillRemoved;
    }

    private IEnumerator FlashReadyRoutine(Image image)
    {
        yield return image.DOColor(Color.blue, flash_duration * 0.5f);
        yield return image.DOColor(Color.white, flash_duration * 0.5f);
    }
}
