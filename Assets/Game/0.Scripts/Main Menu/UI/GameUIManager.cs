using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.EventSystems;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    private Dictionary<string, Button> gameButtons = new Dictionary<string, Button>();
    public bool isPaused = false;

    // 상태 출력용 타이머
    private float logInterval = 1f; // 로그 출력 간격 (초)
    private float nextLogTime = 0f;

    public InputActionAsset inputActions;
    private InputAction pauseAction;
    private PlayerStatus PlayerStatus;

    [Header("팝업창 UI 할당")]
    [SerializeField] private GameObject GameOverUINavigator;
    [SerializeField] private GameObject PauseUINavigator;
    [SerializeField] private GameObject SkillRewardUINavigator;
    [SerializeField] private SkillManager SkillManager;

    [Header("UI")]
    [SerializeField] private PauseUIController pauseMenuUI; // 일시정지 메뉴 UI

    [Header("볼륨UI")]
    [SerializeField] private Scrollbar masterVolumn;
    [SerializeField] private Scrollbar bgmVolumn;  
    [SerializeField] private Scrollbar sfxVolumn;

    [Header("보스체력바 UI")]
    [SerializeField] private GameObject HPBar;

    [Header("게임오버 또는 클리어")]
    [SerializeField] private GameObject gameoverUI;
    [SerializeField] private Button retryButton; // 포커스를 설정할 Close 버튼
    [SerializeField] private TextMeshProUGUI gameoverTotalKill; // 총 킬 수 텍스트
    [SerializeField] private TextMeshProUGUI gameoverAndClear; // 클리어 텍스트 또는 게임오버 텍스트

    private void Awake()
    {
        if (inputActions != null)
        {
            var uiMap = inputActions.FindActionMap("UI");
            if (uiMap != null)
            {
                pauseAction = uiMap.FindAction("Pause");
                if (pauseAction != null)
                    pauseAction.performed += ctx => Pause();
            }
        }
    }

    private void OnEnable()
    {
        pauseAction?.Enable();

        if (SoundManager.Instance != null && SoundManager.Instance.mainMixer != null)
        {
            // ⭐ 1. AudioMixer에서 데시벨(dB) 값 가져오기 ⭐
            if (SoundManager.Instance.mainMixer.GetFloat("MasterVolume", out float masterDB))
            {
                // 2. dB 값을 정규화된 선형 볼륨(0.0 ~ 1.0)으로 변환 후 할당
                masterVolumn.value = SoundManager.Instance.DecibelToVolume(masterDB);
            }

            if (SoundManager.Instance.mainMixer.GetFloat("BGMVolume", out float bgmDB))
            {
                bgmVolumn.value = SoundManager.Instance.DecibelToVolume(bgmDB);
            }

            if (SoundManager.Instance.mainMixer.GetFloat("SFXVolume", out float sfxDB))
            {
                sfxVolumn.value = SoundManager.Instance.DecibelToVolume(sfxDB);
            }
        }

        // 게임 재시작시, Menu Navigator오브젝트 전부 비활성화
        GameOverUINavigator.SetActive(false);
        PauseUINavigator.SetActive(false);
        SkillRewardUINavigator.SetActive(false);
    }

    public void BossHPBar(GameObject boss)
    {
        if(boss != null)
        {
            HPBar.SetActive(true);
        }
        else
        {
            HPBar.SetActive(false);
        }
    }

    private void OnDisable()
    {
        pauseAction?.Disable();
    }

    private void Start()
    {
        FindAndBindButtons();
        PlayerStatus = FindObjectOfType<PlayerStatus>();
        StartCoroutine(DelayResetHp());

        if (pauseMenuUI != null)
        {
            Debug.Log("PauseMenuUI를 찾았습니다!");
        }

        if (gameoverUI != null)
        {
            gameoverUI.SetActive(false);
        }

        masterVolumn.onValueChanged.AddListener(SoundManager.Instance.SetMasterVolume);
        bgmVolumn.onValueChanged.AddListener(SoundManager.Instance.SetBGMVolume);
        sfxVolumn.onValueChanged.AddListener(SoundManager.Instance.SetSFXVolume);
    }

    private void Update()
    {
        // 1초마다 현재 체력 로그 출력
        if (Time.time >= nextLogTime)
        {
            nextLogTime = Time.time + logInterval;

            if (PlayerStatus != null)
            {
                float currentHp = PlayerStatus.HP;
                float maxHp = 0f;

                if (PlayerStatusData.Instance != null)
                    maxHp = PlayerStatusData.Instance.GetHP();

                Debug.Log($"[현재 체력] {currentHp} / {maxHp}");
            }
        }
    }

    // 플레이어 체력을 최대치로 회복 (한 프레임 지연)
    private IEnumerator DelayResetHp()
    {
        yield return null;

        ResetPlayerHpToMax();
    }

    // 플레이어 체력을 최대치로 회복
    private void ResetPlayerHpToMax()
    {
        if (PlayerStatus != null)
        {
            float maxHp = 0f;

            if (PlayerStatusData.Instance != null)
                maxHp = PlayerStatusData.Instance.GetHP();

            PlayerStatus.RestoreFullHp(maxHp);
            Debug.Log($"[GameUIManager] 플레이어 체력을 최대치({maxHp})로 회복했습니다.");
        }
        else
        {
            Debug.LogWarning("[GameUIManager] PlayerStatus가 연결되어 있지 않습니다!");
        }
    }


    // [Button] 으로 시작하는 버튼 자동 탐색
    private void FindAndBindButtons()
    {
        Button[] allButtons = FindObjectsOfType<Button>(true);

        foreach (Button btn in allButtons)
        {
            string btnName = btn.gameObject.name;

            if (!btnName.StartsWith("[Button]"))
                continue;

            string actionName = btnName.Replace("[Button]", "").Trim();

            if (!gameButtons.ContainsKey(actionName))
                gameButtons.Add(actionName, btn);

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnButtonClicked(actionName));

            Debug.Log($"[GameUIManager] '{btnName}' 버튼 연결 완료 → {actionName}");
        }

        Debug.Log($"[GameUIManager] 총 {gameButtons.Count}개 버튼 연결 완료");
    }

    private void OnButtonClicked(string actionName)
    {
        switch (actionName)
        {
            case "Exit":
                Exit();
                break;
            case "Resume":
                Resume();
                break;
            case "Retry":
                Retry();
                break;
            case "Pause":
                Pause();
                break;
            default:
                Debug.LogWarning($"[GameUIManager] 알 수 없는 버튼 이름: {actionName}");
                break;
        }
    }

    public void ShowRewardPanel()
    {
        SkillRewardUINavigator.SetActive(true);
        SkillManager.OnReward();
    }

    public void CloseRewardPanel()
    {
        SkillRewardUINavigator.SetActive(false);
    }

    public void Exit()
    {

        if (UpgradeManager.Instance != null)
        {
            Time.timeScale = 1f;
            UpgradeManager.Instance.GoToMainMenuScene();
        }
        else
        {
            Debug.LogWarning("[GameUIManager] UpgradeManager 인스턴스가 없습니다.");
        }
    }

    public void Resume()
    {
        if (!isPaused)
        {
            Debug.Log("[GameUIManager] 게임이 이미 실행 중입니다.");
            return;
        }

        ClearFocus();

        pauseMenuUI.HidePauseMenu();
        PauseUINavigator.SetActive(false);
        PlayerStatus.SwitchToGameplay();
        Time.timeScale = 1f;
        isPaused = false;
        Debug.Log("[GameUIManager] 게임 재개됨");
    }
    
    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        Pause();
    }

    public void Pause()
    {
        if (!isPaused)
        {
            isPaused = true;
            Time.timeScale = 0f;
            Debug.Log("[GameUIManager] 게임 일시정지됨");
            PauseUINavigator.SetActive(true);
            pauseMenuUI.ShowPauseMenu();
            PlayerStatus.SwitchToUI();
        }
        else
        {
            PlayerStatus.SwitchToGameplay();
            Resume(); // Resume 함수를 호출하여 중복 코드 제거
        }
    }

    public void GameOverOn()
    {
        if(GameManager.Instance.GameState == Constants.EGameState.Clear)
        {
            gameoverAndClear.text = "Game Clear!";
        }
        else
        {
            gameoverAndClear.text = "Game Over";
        }
        Time.timeScale = 0f;
        GameOverUINavigator.SetActive(true);
        gameoverUI.SetActive(true);
        gameoverTotalKill.text = $"Total Kill: {LevelManager.Instance.enemiesKilledTotal}";
        // SetFocusOnButton(retryButton);
    }

    private void SetFocusOnButton(Button focusButton)
    {
        if (focusButton != null)
        {
            // EventSystem이 현재 선택된 오브젝트를 Close 버튼으로 설정합니다.
            EventSystem.current.SetSelectedGameObject(focusButton.gameObject);

            // 버튼의 Select() 함수를 명시적으로 호출합니다.
            focusButton.Select();
        }
    }

    private void ClearFocus()
    {
        // 현재 선택된 오브젝트를 null로 설정하여 포커스를 해제합니다.
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
