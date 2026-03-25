using UnityEngine;

public class TunnelController : MonoBehaviour
{
    public float rotateSpeed = 50f;

    void Update()
    {
        // 터널이 제자리에서 회전만 하게
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }
}
