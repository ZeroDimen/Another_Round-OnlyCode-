using System.Collections;
using UnityEngine;

public abstract class SettingsElement : MonoBehaviour
{
    public abstract void CycleValue(int direction); // -1, 1

    public abstract void UpdateDisplay();
}