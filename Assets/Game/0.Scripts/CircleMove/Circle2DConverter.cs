using UnityEngine;

public static class Circle2DConverter
{
    public const float DEGREES_PER_UNIT = 10f;

    public static float X_3Dto2D(Vector3 pos)
    {
        Vector3 flat = new Vector3(pos.x, 0f, pos.z); // Y제외
        float angle = Mathf.Atan2(flat.x, flat.z) * Mathf.Rad2Deg; // 라디안 -> 각도값 변환
        float x = angle / DEGREES_PER_UNIT;
        return x;
    }

    public static Vector3 X_2Dto3D(float x_2D, float radius, float y)
    {
        float angle = x_2D * DEGREES_PER_UNIT;
        float radian = angle * Mathf.Deg2Rad;
        float x = Mathf.Sin(radian) * radius;
        float z = Mathf.Cos(radian) * radius;
        return new Vector3(x, y, z);
    }

    public static float X_2DDistance(float a, float b)
    {
        float degree_delta = Mathf.DeltaAngle(a * DEGREES_PER_UNIT, b * DEGREES_PER_UNIT);
        return degree_delta / DEGREES_PER_UNIT;
    }
}
