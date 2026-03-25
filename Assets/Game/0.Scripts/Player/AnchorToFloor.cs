using UnityEngine;

public class AnchorToFloor : MonoBehaviour
{
    private Vector3 worldPosition;

    void Update()
    {
        worldPosition = new Vector3(transform.position.x, 5f, transform.position.z);
        transform.position = worldPosition;
    }
}
