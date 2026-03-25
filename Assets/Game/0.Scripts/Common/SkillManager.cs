using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[System.Serializable]
public class SkillChoiceData
{
    public int skillId;
    public GameObject skillPanel;
    public Outline skillOutline;
    public Image skillImage;
    public TextMeshProUGUI skillName;
    public TextMeshProUGUI skillDescription;
    
}

public class SkillManager : MonoBehaviour
{
    [SerializeField] private GameObject penel;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerSkillInventory playerSkillInventory;
    
    [SerializeField] private SkillDeck Deck;
    [SerializeField] private SkillChoiceData[] skillChoice;
    
    [SerializeField] private GameObject skillTempObj;
    [SerializeField] private Image skillTempImage;
    [SerializeField] private GameObject exitButton;
    
    [SerializeField] private GameObject[] skillButtons;
    [SerializeField] private GameUIManager gameUIManager;

    private Transform penelTransfrom;
    private CanvasGroup penelCanvasGroup;

    private Vector3 tempInitPos;
    private int skillTempId;
    private int currentPanel = 0;
    private Outline exitButtonOutline;
    
    private void Awake()
    {
        Init();

        if (gameUIManager == null) gameUIManager = FindFirstObjectByType<GameUIManager>();
    }

    private void RewardEnable()
    {
        skillChoice[0].skillOutline.enabled = true;
        
        if (playerInput != null)
        {
            playerInput.actions["NavigateLeft"].performed += OnNavigateLeft;
            playerInput.actions["NavigateRight"].performed += OnNavigateRight;
            playerInput.actions["Confirm"].performed += OnSkillSelect;
        }
    }

    private void RewardDisable()
    {
        foreach (var outline in skillChoice)
        {
            outline.skillOutline.enabled = false;
        }
        exitButtonOutline.enabled = false;
        
        if (playerInput != null)
        {
            playerInput.actions["NavigateLeft"].performed -= OnNavigateLeft;
            playerInput.actions["NavigateRight"].performed -= OnNavigateRight;
            playerInput.actions["Confirm"].performed -= OnSkillSelect;
        }
    }

    public void OnReward()
    {
        GameManager.Instance.SetGameState(Constants.EGameState.Menu);
        SoundManager.Instance.bgmAudioSource.volume *= 0.5f;
        Time.timeScale = 0;
        int penelIndex = 0;
        currentPanel = 0;

        List<SkillData> selectedSkills = Deck.GetRandomSkillData(skillChoice.Length);
        
        foreach (var skill in selectedSkills)
        {
            skillChoice[penelIndex].skillId = skill.ID;
            skillChoice[penelIndex].skillImage.sprite = skill.Icon;
            skillChoice[penelIndex].skillName.text = skill.Name;
            skillChoice[penelIndex].skillDescription.text = skill.Description;
            penelIndex++;
        }
        
        
        penel.SetActive(true);

        for (int i = 0; i < skillChoice.Length; i++)
        {
            skillChoice[i].skillPanel.transform.DOScale(1f, 0f).SetEase(Ease.OutBack).SetUpdate(UpdateType.Normal, true);
            skillChoice[i].skillOutline.enabled = false;
        }
        
        
        penelCanvasGroup.DOFade(1, 0.3f).SetEase(Ease.Linear).SetUpdate(UpdateType.Normal, true);
        penelTransfrom.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(UpdateType.Normal, true)
            .OnComplete(() =>
                {
                    RewardEnable();
                }
            );
    }

    public void OffReward()
    {
        RewardDisable();
        penelCanvasGroup.DOFade(0, 0.3f).SetEase(Ease.Linear).SetUpdate(UpdateType.Normal, true);
        penelTransfrom.DOScale(0f, 0.3f).SetEase(Ease.InBack).SetUpdate(UpdateType.Normal,true)
            .OnComplete(() =>
                {
                    skillTempObj.SetActive(false);
                    exitButton.SetActive(false);
                    penel.SetActive(false);
                    skillTempObj.transform.localPosition = tempInitPos;
                }
            );
        
        Time.timeScale = 1;
        SoundManager.Instance.bgmAudioSource.volume *= 2f;
        GameManager.Instance.SetGameState(Constants.EGameState.Play);
        gameUIManager.CloseRewardPanel();
    }

    private void OnNavigateLeft(InputAction.CallbackContext context)
    {
        currentPanel--;
        
        if (!skillTempObj.activeSelf)
        {
            foreach (var skill in skillChoice)
            {
                skill.skillOutline.enabled = false;
            }
            
            if (currentPanel < 0)
            {
                currentPanel =  skillChoice.Length - 1;
            }
        
            skillChoice[currentPanel].skillOutline.enabled = true;
        }
        else
        {
            if (currentPanel < 0)
            {
                currentPanel =  skillButtons.Length - 1;
            }

            if (currentPanel == skillButtons.Length)
            {
                exitButtonOutline.enabled = true;
            }
            else
            {
                Vector3 tempInitPos = skillTempObj.transform.localPosition;
                tempInitPos.x = skillButtons[currentPanel].transform.localPosition.x;
                Debug.Log(skillButtons[currentPanel].transform.localPosition.x);
                
                skillTempObj.transform.localPosition = tempInitPos;
                exitButtonOutline.enabled = false;
            }
        }
    }
    
    private void OnNavigateRight(InputAction.CallbackContext context)
    {
        currentPanel++;
        
        if (!skillTempObj.activeSelf)
        {
            foreach (var skill in skillChoice)
            {
                skill.skillOutline.enabled = false;
            }
            
            if (currentPanel >= skillChoice.Length)
            {
                currentPanel = 0;
            }
            skillChoice[currentPanel].skillOutline.enabled = true;
        }
        else
        {
            if (currentPanel >= skillButtons.Length + 1)
            {
                currentPanel = 0;
            }

            if (currentPanel == skillButtons.Length)
            {
                exitButtonOutline.enabled = true;
            }
            else
            {
                Vector3 tempInitPos = skillTempObj.transform.localPosition;
                tempInitPos.x = skillButtons[currentPanel].transform.localPosition.x;
                Debug.Log(skillButtons[currentPanel].transform.localPosition.x);
                
                skillTempObj.transform.localPosition = tempInitPos;
                exitButtonOutline.enabled = false;
            }
        }      
    }

    private void OnSkillSelect(InputAction.CallbackContext context)
    {
        if (!skillTempObj.activeSelf)
        {
            skillTempImage.sprite =  skillChoice[currentPanel].skillImage.sprite;
            skillTempId = skillChoice[currentPanel].skillId;
            
            foreach (var choice in skillChoice)
            {
                choice.skillPanel.transform.DOScale(0f, 0.3f).SetEase(Ease.InBack).SetUpdate(UpdateType.Normal, true);
            }
            
            currentPanel = 0;
            Vector3 tempInitPos = skillTempObj.transform.position;
            tempInitPos.x = skillButtons[currentPanel].transform.position.x;
            skillTempObj.transform.position = tempInitPos;
            
            skillTempObj.SetActive(true);
            exitButton.SetActive(true);
        }
        else
        {
            if (currentPanel == skillButtons.Length)
            {
                Debug.Log(skillButtons.Length);
                Debug.Log(currentPanel);
                OffReward();
            }
            else
            {
                Sprite temp_Sprite = skillTempImage.sprite;
                int temp_Id = skillTempId;
            
                skillTempImage.sprite = skillButtons[currentPanel].transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite;
                skillTempId = playerSkillInventory.Skills[currentPanel].Data.ID;
            
                skillButtons[currentPanel].transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = temp_Sprite;
                playerSkillInventory.AddSkillToSlot(playerSkillInventory.SpawnSkillWithId(temp_Id), currentPanel + 1);
            }
            
        }
    }

    private void Init()
    {
        penelTransfrom =  penel.GetComponent<Transform>();
        penelCanvasGroup = penel.GetComponent<CanvasGroup>();
        exitButtonOutline = exitButton.GetComponent<Outline>();
        tempInitPos = skillTempObj.transform.localPosition;
    }
}
