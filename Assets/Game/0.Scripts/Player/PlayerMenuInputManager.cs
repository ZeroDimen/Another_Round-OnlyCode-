using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerMenuInputManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStatus Status;
    [SerializeField] private GameUIManager GameUI;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI InputMode;
    [SerializeField] private TextMeshProUGUI GameplayControls;
    [SerializeField] private TextMeshProUGUI MenuControls;

    private void Start()
    {
        Status.OnPlayerInputStateChanged += UpdateButtonGuide;
    }

    private void UpdateButtonGuide(PlayerInputState state)
    {
        bool inMenu = state == PlayerInputState.UI;
        InputMode.text = inMenu ? "INPUT MODE: MENU" : "INPUT MODE: GAMEPLAY";
        GameplayControls.enabled = !inMenu;
        MenuControls.enabled = inMenu;
    }
}
