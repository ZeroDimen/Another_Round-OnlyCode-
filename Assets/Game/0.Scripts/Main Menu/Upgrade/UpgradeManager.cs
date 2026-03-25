using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UpgradeManager : MonoBehaviour
{
    private PlayerStatus playerStatus;
    public static UpgradeManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject mainMenuUI;
    public GameObject guideUI;
    public GameObject loadingUI;
    public Transform cameraTarget;
    public float waitTime = 7f;
    public GameObject jetObject;
    public Button exitButton;
    [SerializeField] private InputActionAsset inputActions;

    private bool isLoading = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(DelayedFind());

        // 메인 메뉴 씬으로 돌아왔을 때 UI 재연결
        if (scene.name == Constants.MENUSCENE_NAME)
        {
            StartCoroutine(RebindUIManagerButtons());
        }
    }

    private IEnumerator RebindUIManagerButtons()
    {
        // 씬 전환 후 UI가 완전히 로드될 때까지 약간 대기
        yield return new WaitForSeconds(0.5f);

        FindReferences();

        UpgradeUIManager uiManager = FindObjectOfType<UpgradeUIManager>();
        if (uiManager != null)
        {
            uiManager.BindAllButtons();
        }
    }


    

    public void UpgradeHp()
    {
        if (PlayerStatusData.Instance != null)
        {
            PlayerStatusData.Instance.UpgradeHP();
            Debug.Log($"[UpgradeManager] HP 업그레이드 완료! 현재 HP: {PlayerStatusData.Instance.GetHP()}");

            FindObjectOfType<UpgradeUIManager>()?.UpdateUpgradeTexts();
        }
    }

    // 공격력 업그레이드
    public void UpgradeAttack()
    {
        if (PlayerStatusData.Instance != null)
        {
            PlayerStatusData.Instance.UpgradeAttack();
            Debug.Log($"[UpgradeManager] Attack 업그레이드 완료! 현재 Attack: {PlayerStatusData.Instance.GetAttack()}");

            FindObjectOfType<UpgradeUIManager>()?.UpdateUpgradeTexts();
        }
    }
    public void GoToGameScene()
    {
        Debug.Log("[UpgradeManager] °ÔÀÓ¾ÀÀ¸·Î ÀÌµ¿ÇÕ´Ï´Ù.");
        SceneManager.LoadScene("SoundManager");
    }

    public void ResetUpgrades()
    {
        if (PlayerStatusData.Instance != null)
        {
            PlayerStatusData.Instance.ResetUpgrades();
            Debug.Log("[UpgradeManager] 업그레이드 초기화 완료 (기본값 복원)");

            FindObjectOfType<UpgradeUIManager>()?.UpdateUpgradeTexts();
        }
    }


    public void OnStartGame()
    {
            StartCoroutine(LoadGameCoroutine());
    }

    private IEnumerator LoadGameCoroutine()
    {
        isLoading = true;

        //조작 비활성화
        if (inputActions != null)
        {
            foreach (var map in inputActions.actionMaps)
                map.Disable();

            Debug.Log("[UpgradeManager] 모든 인풋 액션 비활성화됨");
        }

        if (cameraTarget == null)
        {
            GameObject camTarget = GameObject.Find("Camera Target");
            if (camTarget != null) cameraTarget = camTarget.transform;
        }

        if (mainMenuUI == null)
        {
            GameObject menu = GameObject.Find("MainMenu UI");
            if (menu != null) mainMenuUI = menu;
        }

        if (guideUI == null)
        {
            GameObject guide = GameObject.Find("[Canvas] Guide");
            if (guide != null) guideUI = guide;
        }

        if (loadingUI == null)
        {
            GameObject loading = GameObject.Find("Loading Scene");
            if (loading != null) loadingUI = loading;
        }

        if (jetObject == null)
        {
            GameObject jet = GameObject.Find("Jet");
            if (jet != null) jetObject = jet;
        }

        Camera mainCam = Camera.main;
        if (mainCam && cameraTarget)
        {
            mainCam.transform.position = cameraTarget.position;
            mainCam.transform.rotation = cameraTarget.rotation;
        }

        if (mainMenuUI) mainMenuUI.SetActive(false);
        if (guideUI) guideUI.SetActive(false);
        if (loadingUI) loadingUI.SetActive(true);

        float totalWaitTime = 7f;
        float jetMoveStartTime = totalWaitTime - 1f;
        float elapsed = 0f;

        Vector3 startPos = jetObject.transform.position;
        Vector3 targetPos = startPos + jetObject.transform.forward * 80f;

        while (elapsed < totalWaitTime)
        {
            elapsed += Time.deltaTime;

            if (elapsed > jetMoveStartTime)
            {
                float t = Mathf.InverseLerp(jetMoveStartTime, totalWaitTime, elapsed);
                jetObject.transform.position = Vector3.Lerp(startPos, targetPos, t);
            }

            yield return null;
        }

        SceneManager.LoadScene(Constants.GAMESCENE_NAME);
    }



    public void GoToMainMenuScene()
    {
        SceneManager.LoadScene(Constants.MENUSCENE_NAME);
    }

    private System.Collections.IEnumerator DelayedFind()
    {
        yield return new WaitForSeconds(0.1f);
        FindReferences();
    }

    private void FindReferences()
    {
        if (mainMenuUI == null)
        {
            GameObject menu = GameObject.Find("MainMenu UI");
            if (menu != null)
            {
                mainMenuUI = menu;
            }
        }

        if (guideUI == null)
        {
            GameObject guide = GameObject.Find("[Canvas] Guide");
            if (guide != null)
            {
                guideUI = guide;
            }
        }

        if (loadingUI == null)
        {
            GameObject loading = GameObject.Find("Loading Scene");
            if (loading != null)
            {
                loadingUI = loading;
            }
        }

        if (cameraTarget == null)
        {
            GameObject camTarget = GameObject.Find("Camera Target");
            if (camTarget != null)
            {
                cameraTarget = camTarget.transform;
            }
        }

        if (jetObject == null)
        {
            GameObject jet = GameObject.Find("Jet");
            if (jet != null)
            {
                jetObject = jet;
            }
        }

        if (exitButton == null)
        {
            GameObject exitObj = GameObject.Find("[Button] GameExit");
            if (exitObj != null)
                exitButton = exitObj.GetComponent<Button>();
        }

        if (exitButton != null)
        {
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                var esGO = new GameObject("EventSystem");
                esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            exitButton.interactable = true;
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(OnExitGameClicked);
        }
    }

    private void OnExitGameClicked()
    {
        Debug.Log("[UpgradeManager] 게임 종료 버튼 클릭됨");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
