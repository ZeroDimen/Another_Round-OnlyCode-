using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject PanelRoot;
    [SerializeField] private PlayerInput Input;
    [SerializeField] private PlayerStatus Status;
    [SerializeField] private GameUIManager GameUI;

    [Header("UI Elements")]
    [SerializeField] private List<Selectable> Elements = new List<Selectable>();

    [Header("Menu Settings")]
    [SerializeField] private float InputRepeatDelay = 0.15f;
    [SerializeField] private Color NormalColor = Color.white;
    [SerializeField] private Color SelectedColor = Color.orangeRed;
    private float LastInputTime = 0f;

    private int current_index = 0;
    private CanvasGroup CanvasGroup;
    private Transform PanelTransform;

    private void Awake()
    {
        PanelTransform = PanelRoot.GetComponent<Transform>();
        CanvasGroup = PanelRoot.GetComponent<CanvasGroup>();

        if (Elements.Count != 8) Debug.Log($"PauseMenu might be missing buttons!");
    }

    private void OnEnable()
    {
        if (Input == null) return;

        Input.actions["NavigateUp"].performed += OnNavigateUp;
        Input.actions["NavigateDown"].performed += OnNavigateDown;
        Input.actions["NavigateLeft"].performed += OnNavigateLeft;
        Input.actions["NavigateRight"].performed += OnNavigateRight;
        Input.actions["Confirm"].performed += OnConfirm;
        Input.actions["Cancel"].performed += OnCancel;

        HighlightCurrent();
    }

    private void OnDisable()
    {
        if (Input == null) return;

        Input.actions["NavigateUp"].performed -= OnNavigateUp;
        Input.actions["NavigateDown"].performed -= OnNavigateDown;
        Input.actions["NavigateLeft"].performed -= OnNavigateLeft;
        Input.actions["NavigateRight"].performed -= OnNavigateRight;
        Input.actions["Confirm"].performed -= OnConfirm;
        Input.actions["Cancel"].performed -= OnCancel;
    }

    public void ShowPauseMenu()
    {
        GameManager.Instance.SetGameState(Constants.EGameState.Menu);
        Time.timeScale = 0f;

        PanelRoot.SetActive(true);
        PanelTransform.DOScale(1f, 0.25f).SetEase(Ease.OutBack).SetUpdate(true);
        CanvasGroup.DOFade(1f, 0.25f).SetEase(Ease.Linear).SetUpdate(true);

        current_index = 0;
        HighlightCurrent();
    }

    public void HidePauseMenu()
    {
        Time.timeScale = 1f;
        GameManager.Instance.SetGameState(Constants.EGameState.Play);

        PanelTransform.DOScale(0f, 0.25f).SetEase(Ease.InBack).SetUpdate(true);
        CanvasGroup.DOFade(0f, 0.25f).SetEase(Ease.Linear).SetUpdate(true)
            .OnComplete(() => PanelRoot.SetActive(false));
    }



    public void OnNavigateUp(InputAction.CallbackContext ctx)
    {
        // 중복인풋방지
        if (!MenuActive() || Time.unscaledTime - LastInputTime < InputRepeatDelay) return;
        LastInputTime = Time.unscaledTime;

        if (IsLeftRightNavigatable())
        {
            current_index = 4;
            SoundManager.Instance.PlaySFX(SoundManager.SFX.MenuMove);
            HighlightCurrent();
        }
        else if (!IsLeftRightNavigatable())
        {
            current_index--;
            if (current_index < 0) current_index = Elements.Count - 1;
            SoundManager.Instance.PlaySFX(SoundManager.SFX.MenuMove);
            HighlightCurrent();
        }
    }

    public void OnNavigateDown(InputAction.CallbackContext ctx)
    {
        // 중복인풋방지
        if (!MenuActive() || Time.unscaledTime - LastInputTime < InputRepeatDelay) return;
        LastInputTime = Time.unscaledTime;

        if (IsLeftRightNavigatable())
        {
            current_index = 0; // is bottom element, loops around to top
            SoundManager.Instance.PlaySFX(SoundManager.SFX.MenuMove);
            HighlightCurrent();
        }
        else if (!IsLeftRightNavigatable())
        {
            current_index++;
            SoundManager.Instance.PlaySFX(SoundManager.SFX.MenuMove);
            HighlightCurrent();
        }
    }

    public void OnNavigateLeft(InputAction.CallbackContext ctx)
    {
        // 중복인풋방지
        if (!MenuActive() || Time.unscaledTime - LastInputTime < InputRepeatDelay) return;
        LastInputTime = Time.unscaledTime;

        if (IsScrollbar())
        {
            AdjustValue(-0.05f);
        }
        if (IsSettingsElement())
        {
            var setting = Elements[current_index].GetComponent<SettingsElement>();
            if (setting != null)
            {
                setting.CycleValue(-1);
                SoundManager.Instance.PlaySFX(SoundManager.SFX.MenuMove);
            }
        }
        else if (IsLeftRightNavigatable())
        {
            if (current_index > 5) current_index--;
            SoundManager.Instance.PlaySFX(SoundManager.SFX.MenuMove);
            HighlightCurrent();
        }
    }

    public void OnNavigateRight(InputAction.CallbackContext ctx)
    {
        // 중복인풋방지
        if (!MenuActive() || Time.unscaledTime - LastInputTime < InputRepeatDelay) return;
        LastInputTime = Time.unscaledTime;

        if (IsScrollbar())
        {
            AdjustValue(0.05f);
        }
        else if (IsSettingsElement())
        {
            var setting = Elements[current_index].GetComponent<SettingsElement>();
            if (setting != null)
            {
                setting.CycleValue(1);
                SoundManager.Instance.PlaySFX(SoundManager.SFX.MenuMove);
            }
        }
        else if (IsLeftRightNavigatable())
        {
            if (current_index < 7) current_index++;
            SoundManager.Instance.PlaySFX(SoundManager.SFX.MenuMove);
            HighlightCurrent();
        }
    }

    public void OnConfirm(InputAction.CallbackContext ctx)
    {
        Debug.Log($"PauseMenuInput: Confirm, index: {current_index}");
        if (current_index == 5)
        {
            GameUI.Exit();
        }
        else if (current_index == 6)
        {
            GameUI.Retry();
        }
        else if (current_index == 7)
        {
            GameUI.Resume();
        }
    }

    public void OnCancel(InputAction.CallbackContext ctx)
    {
        if (!MenuActive()) return;

        Debug.Log("MenuInput: Cancel");

        // TODO: close menus

        Status.SwitchToGameplay();
    }

    private void HighlightCurrent()
    {
        for (int i = 0; i < Elements.Count; i++)
        {
            var sel = Elements[i];
            if (sel == null) continue;

            var outline = sel.GetComponent<Outline>();
            if (outline == null)
                outline = sel.gameObject.AddComponent<Outline>();

            outline.effectDistance = new Vector2(3f, 3f);
            outline.effectColor = (i == current_index) ? SelectedColor : new Color(0, 0, 0, 0);

            var graphic = sel.GetComponent<Graphic>();
            if (graphic != null)
                graphic.color = (i == current_index) ? SelectedColor : NormalColor;
        }
    }

    private void AdjustValue(float delta)
    {
        Scrollbar sel = Elements[current_index].GetComponent<Scrollbar>();

        if (sel != null)
        {
            sel.value = Mathf.Clamp01(sel.value + delta);
            SoundManager.Instance.PlaySFX(SoundManager.SFX.MenuMove);
        }
    }

    private bool MenuActive()
    {
        return Status.UI_INPUT_ENABLED;
    }

    private bool IsScrollbar()
    {
        return (current_index == 0 || current_index == 1 || current_index == 2);
    }

    private bool IsSettingsElement()
    {
        return (current_index == 3 || current_index == 4);
    }

    private bool IsLeftRightNavigatable()
    {
        return (current_index == 5 || current_index == 6 || current_index == 7);
    }
}
