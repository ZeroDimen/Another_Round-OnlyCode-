using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameOverUIController : MonoBehaviour
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

        if (Elements.Count != 2) Debug.Log($"GameOver메뉴에 할당된 버튼이 2개가 아닙니다!");
    }

    private void OnEnable()
    {
        if (Input == null) return;

        Input.actions["NavigateLeft"].performed += OnNavigateLeft;
        Input.actions["NavigateRight"].performed += OnNavigateRight;
        Input.actions["Confirm"].performed += OnConfirm;

        HighlightCurrent();
    }

    private void OnDisable()
    {
        if (Input == null) return;


        Input.actions["NavigateLeft"].performed -= OnNavigateLeft;
        Input.actions["NavigateRight"].performed -= OnNavigateRight;
        Input.actions["Confirm"].performed -= OnConfirm;
    }


    public void OnNavigateLeft(InputAction.CallbackContext ctx)
    {
        // 중복인풋방지
        if (!MenuActive() || Time.unscaledTime - LastInputTime < InputRepeatDelay) return;
        LastInputTime = Time.unscaledTime;

        current_index--;
        if (current_index < 0) current_index = Elements.Count - 1;
        SoundManager.Instance.PlaySFX(SoundManager.SFX.MenuMove);
        HighlightCurrent();
    }

    public void OnNavigateRight(InputAction.CallbackContext ctx)
    {
        // 중복인풋방지
        if (!MenuActive() || Time.unscaledTime - LastInputTime < InputRepeatDelay) return;
        LastInputTime = Time.unscaledTime;

        current_index++;
        if (current_index > Elements.Count - 1) current_index = 0;
        SoundManager.Instance.PlaySFX(SoundManager.SFX.MenuMove);
        HighlightCurrent();
    }

    public void OnConfirm(InputAction.CallbackContext ctx)
    {
        Debug.Log("MenuInput: Confirm");
        if (current_index == 0)
        {
            GameUI.Exit();
        }
        else if (current_index == 1)
        {
            GameUI.Retry();
        }
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

    private bool MenuActive()
    {
        return Status.UI_INPUT_ENABLED;
    }
}
