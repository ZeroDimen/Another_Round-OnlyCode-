using System;
using UnityEngine;
using static Constants;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public EGameState GameState { get; private set; }
    [SerializeField] public PlayerStatus PlayerStatus;
    [SerializeField] private GameUIManager gameUIManager;

    public float gameTimer;

    //protected override void Awake()
    //{
    //    base.Awake();

    //    if (PlayerStatus == null)
    //    {
    //        PlayerStatus = FindFirstObjectByType<PlayerStatus>();
    //    }

    //    SceneManager.sceneLoaded += OnSceneLoaded;
    //}

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (PlayerStatus == null)
        {
            PlayerStatus = FindFirstObjectByType<PlayerStatus>();
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void FixedUpdate()
    {
        if (GameState == EGameState.FocusMode)
        {
            Time.timeScale = 0.5f;
        }
        else
        {
            Time.timeScale = 1.0f;
        }
    }





    public void SetGameState(EGameState state)
    {
        if (GameState == state)
        {
            return;
        }
        
        switch (state)
        {
            case EGameState.Pause:
                break;
            case EGameState.Play:
                PlayerStatus.SwitchToGameplay();
                break;
            case EGameState.None:
                break;
            case EGameState.Clear:
                PlayerStatus.SwitchToUI();
                break;
            case EGameState.GameOver:
                PlayerStatus.SwitchToUI();                
                break;
            case EGameState.Menu:
                PlayerStatus.SwitchToUI();
                break;
            default:
                break;
        }
        GameState = state;
        if(state == EGameState.GameOver || state == EGameState.Clear)
        {
            gameUIManager.GameOverOn();
        }
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 게임씬에 PlayerStatus가 있다면 연결
        if (scene.name == Constants.GAMESCENE_NAME)
        {
            PlayerStatus = FindObjectOfType<PlayerStatus>();
            if (PlayerStatus != null)
            {
                Debug.Log($"[GameManager] PlayerStatus 다시 연결 완료 ({scene.name})");
            }
            else
            {
                Debug.LogWarning($"[GameManager] PlayerStatus를 찾지 못했습니다 ({scene.name})");
            }
        }
        else
        {
            // 메인 메뉴 등에서는 null 처리 가능
            PlayerStatus = null;
        }
    }
}
