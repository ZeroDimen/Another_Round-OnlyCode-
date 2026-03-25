using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GamepadType { XBox, DualShock, Switch }
public enum ConfirmType { East, South }

public class SettingsManager : Singleton<SettingsManager>
{
    [Header("Game Settings")]
    [SerializeField] private int _FPS = 60;
    [SerializeField] private GamepadType _GamepadType = GamepadType.XBox;
    [SerializeField] private ConfirmType _ConfirmType = ConfirmType.South;

    [Header("Input Action Assets")]
    [SerializeField] private InputActionAsset MainMenuInput;
    [SerializeField] private InputActionAsset GameplayInput;

    private Coroutine rebinding;

    // 외부 호출용
    public int FPS { get => _FPS; private set => _FPS = value; }
    public GamepadType GamepadType { get => _GamepadType; private set => _GamepadType = value; }
    public ConfirmType ConfirmType { get => _ConfirmType; private set => _ConfirmType = value; }
    public InputActionAsset MainMenu { get => MainMenuInput; }
    public InputActionAsset Gameplay { get => GameplayInput; }
    

    // const처리
    private const string PREF_GAMEPAD = "GamepadType";
    private const string PREF_CONFIRM = "ConfirmType";

    // update events
    public event Action<GamepadType> OnGamepadTypeChanged;
    public event Action<ConfirmType> OnConfirmTypeChanged;

    protected override void Awake()
    {
        Debug.Log($"[SettingsManager] Awake on {gameObject.scene.name}");

        base.Awake();
        AutoAssignInputs();
        LoadSettings();
    }

    private void OnDestroy()
    {
        Debug.Log($"[SettingsManager] Destroyed on {gameObject.scene.name}");
        Debug.Log(Environment.StackTrace);
    }

    private void Start()
    { 
        UnityEngine.Application.targetFrameRate = FPS;
    }

    private void AutoAssignInputs()
    {
        if (MainMenuInput == null)
            MainMenuInput = Resources.Load<InputActionAsset>("MainMenuInput");
        if (GameplayInput == null)
            GameplayInput = Resources.Load<InputActionAsset>("GameplayInput");

        if (MainMenuInput == null || GameplayInput == null)
            Debug.LogWarning("SettingsManager: InputActions 로딩 실패!");
    }

    private void LoadSettings()
    {
        _GamepadType = (GamepadType)PlayerPrefs.GetInt(PREF_GAMEPAD, 0); // 0 = xbox, 1 = dualshock, 2 = switch
        _ConfirmType = (ConfirmType)PlayerPrefs.GetInt(PREF_CONFIRM, 0); // 0 = east, 1 = south

        if (MainMenuInput != null && GameplayInput != null)
        {
            ConfirmInputSwapper.ApplyRebind(MainMenuInput, _ConfirmType);
            ConfirmInputSwapper.ApplyRebind(GameplayInput, _ConfirmType);
        }
        else
        {
            Debug.Log("SettingsManager: MainMenuInput과 GameplayInput이 할당되어 있지 않습니다!");
        }
    }

    public void SetGamepadType(GamepadType type)
    {
        GamepadType = type;
        PlayerPrefs.SetInt(PREF_GAMEPAD, (int)type);
        PlayerPrefs.Save();

        // invoke switch buttonmap events
        OnGamepadTypeChanged?.Invoke(type);
        Debug.Log($"SetGamepadType: {type}");
    }

    public void SetConfirmType(ConfirmType type)
    {
        ConfirmType = type;
        PlayerPrefs.SetInt(PREF_CONFIRM, (int)type);
        PlayerPrefs.Save();


        if (MainMenuInput != null && GameplayInput != null)
        {
            if (rebinding == null)
            rebinding = StartCoroutine(SetConfirmTypeTimeDelay(type));
        }
        else
        {
            Debug.Log("SettingsManager: MainMenuInput과 GameplayInput이 할당되어 있지 않습니다!");
        }

        // invoke switch buttonmap events
        OnConfirmTypeChanged?.Invoke(type);
        Debug.Log($"SetConfirmType: {type}");
    }

    private IEnumerator SetConfirmTypeTimeDelay(ConfirmType type)
    {
        // 이것을 안하면 리바인딩 시 바로 다음 인풋이 나감
        yield return null; // 1프레임 딜레이
        yield return null; yield return null; yield return null; yield return null; yield return null;


        ConfirmInputSwapper.ApplyRebind(MainMenuInput, type);
        ConfirmInputSwapper.ApplyRebind(GameplayInput, type);

        rebinding = null;
    }

    protected override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
    }

    protected override void OnSceneUnloaded(Scene scene)
    {
    }
}
