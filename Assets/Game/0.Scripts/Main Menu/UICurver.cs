using UnityEngine;

public class UICurver : MonoBehaviour
{
    [SerializeField] private float radius = 3000f; // 곡률 반경
    [SerializeField] private float angleOffset = 0f; // 시작 각도
    [SerializeField] private bool applyOnStart = true;

    private RectTransform[] uiElements;

    private void Start()
    {
        if (applyOnStart)
            ApplyCurvature();
    }

    public void ApplyCurvature()
    {
        uiElements = GetComponentsInChildren<RectTransform>(true);

        foreach (RectTransform rect in uiElements)
        {
            if (rect == transform as RectTransform) continue;

            // x 위치에 따라 회전 각도 계산
            float x = rect.localPosition.x;
            float angle = (x / radius) * Mathf.Rad2Deg;

            // 새로운 위치 계산
            float rad = Mathf.Deg2Rad * (angle + angleOffset);
            float newX = Mathf.Sin(rad) * radius;
            float newZ = (1 - Mathf.Cos(rad)) * radius;

            rect.localPosition = new Vector3(newX, rect.localPosition.y, -newZ);

        }
    }
}
