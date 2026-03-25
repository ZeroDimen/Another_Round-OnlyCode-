using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamepadSettingsElement : SettingsElement
{
    [Header("UI References")]
    [SerializeField] private TMP_Text value;
    [SerializeField] private Image icon;

    [Header("Icon Sets")]
    [SerializeField] private GamepadIcons XBox_Icons;
    [SerializeField] private GamepadIcons DualShock_Icons;
    [SerializeField] private GamepadIcons Switch_Icons;

    private const string XBox = "XBox";
    private const string DualShock = "DualShock";
    private const string Switch = "Switch";

    private void OnEnable()
    {
        UpdateDisplay();
    }

    public override void UpdateDisplay()
    {
        if (value != null)
        {
            switch (SettingsManager.Instance.GamepadType)
            {
                case GamepadType.XBox:
                    value.text = XBox;
                    break;
                case GamepadType.DualShock:
                    value.text = DualShock;
                    break;
                case GamepadType.Switch:
                    value.text = Switch;
                    break;
                default:
                    value.text = "TEST";
                    break;
            }
        }

        if (icon != null)
        {
            bool south_confirm = SettingsManager.Instance.ConfirmType == ConfirmType.South;
            var gamepad_setting = SettingsManager.Instance.GamepadType;

            switch (gamepad_setting)
            {
                case GamepadType.XBox:
                    icon.sprite = south_confirm ? XBox_Icons.button_south : XBox_Icons.button_east;
                    break;
                case GamepadType.DualShock:
                    icon.sprite = south_confirm ? DualShock_Icons.button_south : DualShock_Icons.button_east;
                    break;
                case GamepadType.Switch:
                    icon.sprite = south_confirm ? Switch_Icons.button_south : Switch_Icons.button_east;
                    break;
            }
        }
    }

    public override void CycleValue(int direction)
    {
        var current = SettingsManager.Instance.GamepadType;
        int length = System.Enum.GetValues(typeof(GamepadType)).Length;
        int next = ((int)current + direction + length) % length;

        SettingsManager.Instance.SetGamepadType((GamepadType)next);

        UpdateDisplay();
    }
}
