using UnityEngine;
using UnityEngine.InputSystem;

public static class ConfirmInputSwapper
{
    public static void ApplyRebind(InputActionAsset asset, ConfirmType confirm_type)
    {
        Debug.Log($"rebinding {asset.name}");
        var uiMap = asset.FindActionMap("UI", true);
        var confirm = uiMap.FindAction("Confirm");
        var cancel = uiMap.FindAction("Cancel");

        if (confirm == null || cancel == null)
        {
            Debug.LogWarning($"{asset.name}의 Confirm혹은 Cancel버튼을 찾지 못하였습니다!");
            return;
        }

        bool south_confirm = confirm_type == ConfirmType.South;
        confirm.ApplyBindingOverride(0, south_confirm ? "<Gamepad>/buttonSouth" : "<Gamepad>/buttonEast");
        cancel.ApplyBindingOverride(0, south_confirm ? "<Gamepad>/buttonEast" : "<Gamepad>/buttonSouth");
    }
}
