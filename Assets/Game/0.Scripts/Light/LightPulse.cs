using System;
using UnityEngine;

public class LightPulse : MonoBehaviour
{
     
    public float pulseSpeed = 1.0f; 
    public float maxRed = 1.0f;  
    public float minRed = 0.6f;  
    private Light _light;

    private void Start()
    {
        _light =  GetComponent<Light>();
    }

    private void Update()
    {
        float sinValue = Mathf.Sin(Time.time * pulseSpeed);
        float currentRedValue = ((sinValue + 1.0f) / 2.0f) * (maxRed - minRed) + minRed;
        Color objectColor = _light.color;
        
        objectColor.r = currentRedValue;
        _light.color = objectColor;
    }
}
