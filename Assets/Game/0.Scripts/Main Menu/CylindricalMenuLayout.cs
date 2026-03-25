using UnityEngine;

public class CylindricalMenuLayout : MonoBehaviour
{
    public float radius = 10f;
    public float heightOffset = 0f;

    void Start()
    {
        ArrangePanelsInCylinder();
    }

    void ArrangePanelsInCylinder()
    {
        int count = transform.childCount;
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            Transform panel = transform.GetChild(i);
            float angle = i * angleStep * Mathf.Deg2Rad;

            Vector3 pos = new Vector3(Mathf.Sin(angle) * radius, heightOffset, Mathf.Cos(angle) * radius);
            panel.localPosition = pos;

            Vector3 lookDir = (panel.position - transform.position).normalized;
            panel.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
        }
    }
}
