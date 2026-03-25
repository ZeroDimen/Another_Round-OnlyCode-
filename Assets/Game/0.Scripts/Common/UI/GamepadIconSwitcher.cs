using UnityEngine;
using UnityEngine.UI;

public class GamepadIconSwitcher : MonoBehaviour
{
    [Header("Icon Sets")]
    [SerializeField] private GamepadIcons XBox_Icons;
    [SerializeField] private GamepadIcons DualShock_Icons;
    [SerializeField] private GamepadIcons Switch_Icons;

    [Header("UI Elements")]
    [SerializeField] private Image left_stick;
    [SerializeField] private Image left_bumper;
    [SerializeField] private Image right_bumper;
    [SerializeField] private Image confirm_button;
    [SerializeField] private Image cancel_button;
    [SerializeField] private Image button_north;
    [SerializeField] private Image button_east;
    [SerializeField] private Image button_south;
    [SerializeField] private Image button_west;
    [SerializeField] private Image start_button;

    private void OnEnable()
    {
        SettingsManager.Instance.OnGamepadTypeChanged += UpdateIcons;
        SettingsManager.Instance.OnConfirmTypeChanged += UpdateConfirmButtons;
        UpdateIcons(SettingsManager.Instance.GamepadType);
    }

    private void OnDisable()
    {
        SettingsManager.Instance.OnGamepadTypeChanged -= UpdateIcons;
        SettingsManager.Instance.OnConfirmTypeChanged -= UpdateConfirmButtons;
    }

    private void UpdateIcons(GamepadType type)
    {
        var iconset = GetIconsByType(type);
        if (iconset == null)
        {
            Debug.Log("GamepadIconSwitcher: GetIconsByType - 오류가 있었습니다.");
            return;
        }
        var confirm_south = SettingsManager.Instance.ConfirmType == ConfirmType.South;

        if (left_stick != null) left_stick.sprite = iconset.left_stick;
        if (left_bumper != null) left_bumper.sprite = iconset.left_bumper;
        if (right_bumper != null) right_bumper.sprite = iconset.right_bumper;
        if (confirm_button != null) confirm_button.sprite = confirm_south ? iconset.button_south : iconset.button_east;
        if (cancel_button != null) cancel_button.sprite = confirm_south ? iconset.button_east : iconset.button_south;
        if (button_north != null) button_north.sprite = iconset.north_filled;
        if (button_east != null) button_east.sprite = iconset.east_filled;
        if (button_south != null) button_south.sprite = iconset.south_filled;
        if (button_west != null) button_west.sprite = iconset.west_filled;
        if (start_button != null) start_button.sprite = iconset.start_button;
    }

    private void UpdateConfirmButtons(ConfirmType type)
    {
        var iconset = GetIconsByType(SettingsManager.Instance.GamepadType);
        if (iconset == null)
        {
            Debug.Log("GamepadIconSwitcher: GetIconsByType - 오류가 있었습니다.");
            return;
        }
        var confirm_south = type == ConfirmType.South;

        if (confirm_button != null) confirm_button.sprite = confirm_south ? iconset.button_south : iconset.button_east;
        if (cancel_button != null) cancel_button.sprite = confirm_south ? iconset.button_east : iconset.button_south;
    }

    private GamepadIcons GetIconsByType(GamepadType type)
    {
        switch (type)
        {
            case GamepadType.XBox:
                return XBox_Icons;
            case GamepadType.DualShock:
                return DualShock_Icons;
            case GamepadType.Switch:
                return Switch_Icons;
        }
        return null;
    }
}
