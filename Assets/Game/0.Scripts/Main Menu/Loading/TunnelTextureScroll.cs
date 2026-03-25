using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class TunnelTextureScroll : MonoBehaviour
{
    public float scrollSpeed = 0.5f;
    private Renderer rend;
    private Material mat;

    void Start()
    {
        rend = GetComponent<Renderer>();
        mat = rend.material;
    }

    void Update()
    {
        float offset = Time.time * scrollSpeed;
        mat.mainTextureOffset = new Vector2(0, offset);
    }
}
