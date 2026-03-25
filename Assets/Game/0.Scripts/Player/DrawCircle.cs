using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder;

public class DrawCircle : MonoBehaviour
{
    public float hight = 20f;
    public int segments = 50; // 원을 이루는 선분 개수 (값이 클수록 부드러워져)
    public float radius = 1f; // 원의 반지름
    public Color lineColor = Color.blue; // 원의 색상
    public float lineWidth = 0.05f; // 원의 두께

    LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.loop = true;

        Draw();
    }

    void Draw()
    {
        lineRenderer.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float angle = ((float)i / (float)segments) * 360f * Mathf.Deg2Rad; 
            float x = Mathf.Sin(angle) * radius;
            float y = Mathf.Cos(angle) * radius;
            lineRenderer.SetPosition(i, new Vector3(x, hight, y));
        }
    }
}
