using UnityEngine;

public class LightRotator : MonoBehaviour
{
    public float oscillationSpeed = 1.0f; 
    public float oscillationRange = 30.0f; 
    private float initialYRotation; 

    void Start()
    {
        initialYRotation = transform.localEulerAngles.y;
    }

    void Update()
    {
        float rotationOffset = Mathf.Sin(Time.time * oscillationSpeed) * oscillationRange;
        float targetYRotation = initialYRotation + rotationOffset;
        
        transform.localRotation = Quaternion.Euler(transform.localEulerAngles.x, targetYRotation, transform.localEulerAngles.z);
    }
}