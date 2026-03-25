using UnityEngine;

public class MenuRotatorController : MonoBehaviour
{
    [Header("패널 회전 관련")]
    public Transform[] panels;
    public float rotationSpeed = 3f;
    public float rotationStep = 60f;

    private int currentPanel = 0;
    private float targetAngle = 0f;

    private void Start()
    {
        if (panels.Length == 0) return;

        foreach (var panel in panels)
        {
            if (panel != null)
            {
                Vector3 scale = panel.localScale;
                scale.x *= -1;
                panel.localScale = scale;
            }
        }

        FocusPanel(0, true);
    }

    private void Update()
    {
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.Euler(0, targetAngle, 0),
            Time.deltaTime * rotationSpeed
        );
    }

    public void FocusPanel(int panelIndex, bool instant = false)
    {
        if (panels.Length == 0) return;

        currentPanel = (panelIndex + panels.Length) % panels.Length;

        //  180도 빼서 뒤집힘 문제 해결
        targetAngle = -currentPanel * rotationStep;

        if (instant)
            transform.rotation = Quaternion.Euler(0, targetAngle, 0);
    }
}
