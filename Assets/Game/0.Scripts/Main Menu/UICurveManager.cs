using UnityEngine;

public class UICurveManager : MonoBehaviour
{
    public static UICurveManager Instance { get; private set; }
    public float Radius = 3000f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ApplyToAll()
    {
        foreach (UICurver curver in FindObjectsOfType<UICurver>())
        {
            curver.ApplyCurvature();
        }
    }
}
