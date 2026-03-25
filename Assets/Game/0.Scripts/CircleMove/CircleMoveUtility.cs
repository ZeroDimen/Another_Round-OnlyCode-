using UnityEngine;

public static class CircleMoveUtility
{  
    public static void MoveLeftRight(Transform transform, Vector3 pivot, float input, float speed)
    {
        if (Mathf.Approximately(input, 0f)) return; // 인풋이 0에 근접할 경우, 실행정지 - Approximately를 쓰는 이유는 float의 정확도가 떨어지는 것을 보완하기 위함

        transform.RotateAround(pivot, Vector3.up, input * speed * Time.fixedDeltaTime);
    }

    public static void MoveUpDown(Transform transform, float input, float speed, float bound_low = 5f, float bound_high = 20f)
    {
        if (Mathf.Approximately(input, 0f)) return; // 인풋이 0에 근접할 경우, 실행정지 - Approximately를 쓰는 이유는 float의 정확도가 떨어지는 것을 보완하기 위함

        Vector3 pos = transform.position;
        pos += Vector3.up * input * speed * Time.fixedDeltaTime;
        pos.y = Mathf.Clamp(pos.y, bound_low, bound_high);
        transform.position = pos;
    }
}
