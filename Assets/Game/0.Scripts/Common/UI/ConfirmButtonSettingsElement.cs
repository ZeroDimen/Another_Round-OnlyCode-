using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmButtonSettingsElement : SettingsElement
{
    [Header("UI References")]
    [SerializeField] private TMP_Text value;
    [SerializeField] private Image icon;

    [Header("Icon Sets")]
    [SerializeField] private GamepadIcons XBox_Icons;
    [SerializeField] private GamepadIcons DualShock_Icons;
    [SerializeField] private GamepadIcons Switch_Icons;

    private void OnEnable()
    {
        UpdateDisplay();
    }

    public override void UpdateDisplay()
    {
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

        var current = SettingsManager.Instance.ConfirmType;
        int length = System.Enum.GetValues(typeof(ConfirmType)).Length;
        int next = ((int)current + direction + length) % length;

        SettingsManager.Instance.SetConfirmType((ConfirmType)next);

        UpdateDisplay();
    }
}
