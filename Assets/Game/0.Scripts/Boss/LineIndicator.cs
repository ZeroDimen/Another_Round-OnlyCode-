using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LineIndicator : MonoBehaviour
{
    private LineRenderer line_renderer;

    private void Awake()
    {
        line_renderer = GetComponent<LineRenderer>();
        line_renderer.loop = false;
        line_renderer.positionCount = 0;
        line_renderer.useWorldSpace = true;
    }

    public void ShowArc(Vector3 startPos, Vector3 endPos, float radius, int segments = 32)
    { 
        float startX = Circle2DConverter.X_3Dto2D(startPos);
        float endX = Circle2DConverter.X_3Dto2D(endPos);
        float deltaX = Circle2DConverter.X_2DDistance(startX, endX);
        float startY = startPos.y;
        float endY = endPos.y;

        line_renderer.positionCount = segments + 1;

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            // Lerp in faux 2D coordinates
            float x2D = Mathf.Lerp(startX, startX + deltaX, t);
            float y = Mathf.Lerp(startY, endY, t);

            // Convert back to world space along the cylinder
            Vector3 worldPos = Circle2DConverter.X_2Dto3D(x2D, radius, y);
            line_renderer.SetPosition(i, worldPos);
        }
    }

    public void Hide()
    {
        line_renderer.positionCount = 0;
    }
}
