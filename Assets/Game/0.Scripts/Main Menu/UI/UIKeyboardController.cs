using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[System.Serializable]
public class PanelButtons
{
    public Selectable[] selectables;
}

public class UIKeyboardController : MonoBehaviour
{
    [Header("References")]
    public MenuRotatorController rotator;
    public List<PanelButtons> panels = new List<PanelButtons>();

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;

    [Header("Input Actions")]
    public InputActionAsset inputActions;

    private InputAction moveUp;
    private InputAction moveDown;
    private InputAction moveLeft;
    private InputAction moveRight;
    private InputAction confirm;
    private InputAction cancel;
    private InputAction screenLeft;
    private InputAction screenRight;

    private int currentPanel = 0;
    private int currentSelectable = 0;
    private bool isAdjusting = false;

    private float lastMoveLeftRight = 0f;
        private float lastMoveUpDown = 0f;
        private float moveCooldown = 0.15f;

    private void OnMoveUp(InputAction.CallbackContext ctx)
    {
        if (Time.unscaledTime - lastMoveUpDown < moveCooldown) return;

        lastMoveUpDown = Time.unscaledTime;
        MoveVertical(-1);
    }
    private void OnMoveDown(InputAction.CallbackContext ctx)
    {
        if (Time.unscaledTime - lastMoveUpDown < moveCooldown) return;

        lastMoveUpDown = Time.unscaledTime;
        MoveVertical(1);
    }
    private void OnMoveLeft(InputAction.CallbackContext ctx)
    {
        if (Time.unscaledTime - lastMoveLeftRight < moveCooldown) return;

        lastMoveLeftRight = Time.unscaledTime;
        OnLeft();
    }
    private void OnMoveRight(InputAction.CallbackContext ctx)
    {
        if (Time.unscaledTime - lastMoveLeftRight < moveCooldown) return;

        lastMoveLeftRight = Time.unscaledTime;
        OnRight();
    }
    private void OnConfirmPerformed(InputAction.CallbackContext ctx) => OnConfirm();
    private void OnCancelPerformed(InputAction.CallbackContext ctx) => OnCancel();
    private void OnScreenLeftPerformed(InputAction.CallbackContext ctx) => OnScreenLeft();
    private void OnScreenRightPerformed(InputAction.CallbackContext ctx) => OnScreenRight();

    private void Awake()
    {
        var uiMap = inputActions.FindActionMap("UI");

        moveUp = uiMap.FindAction("MoveUp");
        moveDown = uiMap.FindAction("MoveDown");
        moveLeft = uiMap.FindAction("MoveLeft");
        moveRight = uiMap.FindAction("MoveRight");
        confirm = uiMap.FindAction("Confirm");
        cancel = uiMap.FindAction("Cancel");
        screenLeft = uiMap.FindAction("ScreenLeft");
        screenRight = uiMap.FindAction("ScreenRight");

        moveUp.performed += OnMoveUp;
        moveDown.performed += OnMoveDown;
        moveLeft.performed += OnMoveLeft;
        moveRight.performed += OnMoveRight;
        confirm.performed += OnConfirmPerformed;
        cancel.performed += OnCancelPerformed;
        screenLeft.performed += OnScreenLeftPerformed;
        screenRight.performed += OnScreenRightPerformed;
    }

    private void OnEnable()
    {
        moveUp.Enable();
        moveDown.Enable();
        moveLeft.Enable();
        moveRight.Enable();
        confirm.Enable();
        cancel.Enable();
        screenLeft.Enable();
        screenRight.Enable();
    }

    private void OnDisable()
    {
        moveUp.Disable();
        moveDown.Disable();
        moveLeft.Disable();
        moveRight.Disable();
        confirm.Disable();
        cancel.Disable();
        screenLeft.Disable();
        screenRight.Disable();

        moveUp.performed -= OnMoveUp;
        moveDown.performed -= OnMoveDown;
        moveLeft.performed -= OnMoveLeft;
        moveRight.performed -= OnMoveRight;
        confirm.performed -= OnConfirmPerformed;
        cancel.performed -= OnCancelPerformed;
        screenLeft.performed -= OnScreenLeftPerformed;
        screenRight.performed -= OnScreenRightPerformed;
    }

    private void Start()
    {
        AutoSetupPanels();
        HighlightCurrentSelectable();
        rotator.FocusPanel(currentPanel, true);
    }

    // 자동 패널 등록 (Hierarchy 순서)
    private void AutoSetupPanels()
    {
        panels.Clear();

        Canvas[] canvases = FindObjectsOfType<Canvas>(true);
        System.Array.Sort(canvases, (a, b) =>
            a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

        foreach (var canvas in canvases)
        {
            Transform[] children = canvas.GetComponentsInChildren<Transform>(true);
            List<Selectable> found = new List<Selectable>();
            HashSet<Selectable> added = new HashSet<Selectable>();

            foreach (Transform child in children)
            {
                if (child.CompareTag("Select"))
                {
                    var sel = child.GetComponent<Selectable>();
                    if (sel != null && !added.Contains(sel))
                    {
                        found.Add(sel);
                        added.Add(sel);
                    }
                }
            }

            if (found.Count > 0)
            {
                PanelButtons panel = new PanelButtons();
                panel.selectables = found.ToArray();
                panels.Add(panel);
            }
        }
    }

    private void OnLeft()
    {
        var     selectable = panels[currentPanel].selectables[currentSelectable];
        if (selectable == null) return;

        var setting = selectable.GetComponent<SettingsElement>();
        if (setting != null)
        {
            setting.CycleValue(-1);
            SoundManager.Instance.PlaySFX(SoundManager.SFX.MenuMove);
            return;
        }

        if (isAdjusting)
            AdjustValue(-0.05f);
        else
        {
            SoundManager.Instance.PlaySFX(SoundManager.SFX.MenuMove);
            MoveHorizontal(1); // A or Left Stick 
        }
    }

    private void OnRight()
    {
        var selectable = panels[currentPanel].selectables[currentSelectable];
        if (selectable == null) return;

        var setting = selectable.GetComponent<SettingsElement>();
        if (setting != null)
        {
            setting.CycleValue(1);
            SoundManager.Instance.PlaySFX(SoundManager.SFX.MenuMove);
            return;
        }

        if (isAdjusting)
            AdjustValue(0.05f);
        else
        {
            SoundManager.Instance.PlaySFX(SoundManager.SFX.MenuMove);
            MoveHorizontal(-1); // D or Left Stick 
        }
    }

    private void OnConfirm()
    {
        var selectable = panels[currentPanel].selectables[currentSelectable];
        if (selectable == null) return;

        if (selectable is Scrollbar || selectable is Slider)
        {
            isAdjusting = true;
            Debug.Log("Scroll mode ON");
        }
        else if (selectable is Button button)
        {
            button.onClick.Invoke();
        }
    }

    private void OnCancel()
    {
        if (isAdjusting)
        {
            isAdjusting = false;
            Debug.Log("Scroll mode OFF");
        }
    }

    private void OnScreenLeft()
    {
        currentPanel = (currentPanel + 1 + panels.Count) % panels.Count;
        rotator.FocusPanel(currentPanel);

        currentSelectable = 0;
        HighlightCurrentSelectable();
    }

    private void OnScreenRight()
    {
        currentPanel = (currentPanel + -1 + panels.Count) % panels.Count;
        rotator.FocusPanel(currentPanel);

        currentSelectable = 0;
        HighlightCurrentSelectable();
    }

    private void MoveVertical(int dir)
    {
        var selectables = panels[currentPanel].selectables;
        if (selectables.Length == 0) return;

        currentSelectable = (currentSelectable + dir + selectables.Length) % selectables.Length;
        HighlightCurrentSelectable();
    }

    private void MoveHorizontal(int dir)
    {
        var selectables = panels[currentPanel].selectables;
        if (selectables.Length == 0) return;

        int internalDir = -dir;
        currentSelectable += internalDir;

        if (currentSelectable >= 0 && currentSelectable < selectables.Length)
        {
            HighlightCurrentSelectable();
            return;
        }

        currentPanel = (currentPanel + dir + panels.Count) % panels.Count;
        currentSelectable = 0;

        rotator.FocusPanel(currentPanel);
        HighlightCurrentSelectable();
    }

    private void AdjustValue(float delta)
    {
        var selectable = panels[currentPanel].selectables[currentSelectable];

        if (selectable is Scrollbar scrollbar)
        {
            scrollbar.value = Mathf.Clamp01(scrollbar.value + delta);
            SoundManager.Instance.PlaySFX(SoundManager.SFX.MenuMove);
        }
        else if (selectable is Slider slider)
            slider.value = Mathf.Clamp01(slider.value + delta);
    }

    private void HighlightCurrentSelectable()
    {
        for (int p = 0; p < panels.Count; p++)
        {
            foreach (var sel in panels[p].selectables)
            {
                if (sel == null) continue;

                var btn = sel.GetComponent<Button>();
                if (btn != null)
                {
                    var colors = btn.colors;
                    colors.normalColor = normalColor;
                    btn.colors = colors;
                }

                var outline = sel.GetComponent<Outline>();
                if (outline == null)
                    outline = sel.gameObject.AddComponent<Outline>();

                outline.effectDistance = new Vector2(3f, 3f);

                if (p == currentPanel && sel == panels[currentPanel].selectables[currentSelectable])
                    outline.effectColor = selectedColor;
                else
                    outline.effectColor = new Color(0, 0, 0, 0);
            }
        }
    }
}
