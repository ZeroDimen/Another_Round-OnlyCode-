using UnityEngine;

public interface ICircleMover
{
    Transform Transform { get; }
    Vector3 Pivot { get; }
    float Speed_LeftRight { get; }
    float Speed_UpDown { get; }
}
