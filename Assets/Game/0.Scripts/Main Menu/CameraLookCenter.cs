using UnityEngine;

public class CameraLookCenter : MonoBehaviour
{
    public Transform target;

    void LateUpdate()
    {
        if (target != null)
            transform.LookAt(target.position);
    }
}
