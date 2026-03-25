using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class InputGuideSwitcher : MonoBehaviour
{
    [Header("UI Images")]
    public GameObject keyboardGuide;
    public GameObject controllerGuide;

    private string currentDevice = "Keyboard";

    private void Start()
    {
        // 기본적으로 키보드 이미지 활성화
        keyboardGuide.SetActive(true);
        controllerGuide.SetActive(false);

        // 모든 입력 이벤트 감지 시작
        InputSystem.onEvent += OnInputEvent;
    }

    private void OnDestroy()
    {
        // 오브젝트가 파괴될 때 이벤트 해제 (메모리 누수 방지)
        InputSystem.onEvent -= OnInputEvent;
    }

    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        if (device == null) return;

        // 장치 종류에 따라 전환
        if (device is Gamepad)
        {
            SwitchToController();
        }
        else if (device is Keyboard || device is Mouse)
        {
            SwitchToKeyboard();
        }
    }

    private void SwitchToKeyboard()
    {
        if (currentDevice == "Keyboard") return;

        currentDevice = "Keyboard";
        keyboardGuide.SetActive(true);
        controllerGuide.SetActive(false);
        Debug.Log("[InputGuideSwitcher] Switched to Keyboard input.");
    }

    private void SwitchToController()
    {
        if (currentDevice == "Controller") return;

        currentDevice = "Controller";
        keyboardGuide.SetActive(false);
        controllerGuide.SetActive(true);
        Debug.Log("[InputGuideSwitcher] Switched to Controller input.");
    }
}
