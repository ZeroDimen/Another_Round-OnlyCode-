using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class TunnelEmissionPulse : MonoBehaviour
{
    public Color baseColor = Color.blue;
    public Color pulseColor = new Color(1f, 0f, 1f); // 보라색
    public float pulseSpeed = 2f;

    private Renderer rend;
    private Material mat;

    void Start()
    {
        rend = GetComponent<Renderer>();
        mat = rend.material;
        mat.EnableKeyword("_EMISSION");
    }

    void Update()
    {
        // 보라색이 파란색 위에서 부드럽게 번쩍이는 효과
        float emission = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f; // 0~1 반복
        Color finalColor = Color.Lerp(baseColor, pulseColor, emission);
        mat.SetColor("_EmissionColor", finalColor * 3f); // 3배 밝게
    }
}
