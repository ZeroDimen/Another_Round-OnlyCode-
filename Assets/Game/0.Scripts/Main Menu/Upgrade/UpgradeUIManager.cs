using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;


public class UpgradeUIManager : MonoBehaviour
{
    private UpgradeManager upgradeManager;

    private void Start()
    {
        StartCoroutine(DelayedBind());
    }

    private IEnumerator DelayedBind()
    {
        yield return new WaitForSeconds(0.1f); // 씬 로드 후 UI 안정화 시간
        upgradeManager = FindObjectOfType<UpgradeManager>();
        if (upgradeManager == null)
        {
            Debug.LogError("UpgradeManager를 찾을 수 없음");
            yield break;
        }

        BindAllButtons();
    }

    public void BindAllButtons()
    {
        BindButton("[Button] UpgradeHP", upgradeManager.UpgradeHp);
        BindButton("[Button] UpgradeAttack", upgradeManager.UpgradeAttack);
        BindButton("[Button] GameStart", upgradeManager.OnStartGame);
        BindButton("[Button] Reset", upgradeManager.ResetUpgrades);

        Debug.Log("UpgradeUIManager: 버튼 자동 연결 완료");
        UpdateUpgradeTexts();
    }

    private void BindButton(string buttonName, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObj = GameObject.Find(buttonName);
        if (buttonObj == null)
        {
            Debug.LogWarning($"{buttonName} 버튼을 찾을 수 없음.");
            return;
        }

        Button button = buttonObj.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning($"{buttonName} 오브젝트에 Button 컴포넌트가 없음.");
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
        Debug.Log($"{buttonName} 연결 완료");
    }

    // 최대 레벨 설정 (원하는 값으로 바꿔도 됨)
    private const int MAX_LEVEL = 10;

    // 업그레이드 UI 텍스트 갱신 함수
    public void UpdateUpgradeTexts()
    {
        if (PlayerStatusData.Instance == null) return;

        int hpLv = PlayerStatusData.Instance.GetHPLevel();
        int atkLv = PlayerStatusData.Instance.GetAttackLevel();

        UpdateText("[Text] UpgradeHPLevel", hpLv, MAX_LEVEL);
        UpdateText("[Text] UpgradeAttackLevel", atkLv, MAX_LEVEL);
    }

    private void UpdateText(string textName, int current, int max)
    {
        GameObject textObj = GameObject.Find(textName);
        if (textObj == null) return;

        Text uiText = textObj.GetComponent<Text>();
        if (uiText != null)
        {
            uiText.text = $"LV. {current} / LV. {max}";
            return;
        }

        TextMeshProUGUI tmpText = textObj.GetComponent<TextMeshProUGUI>();
        if (tmpText != null)
        {
            tmpText.text = $"LV. {current} / LV. {max}";
        }
    }

}
