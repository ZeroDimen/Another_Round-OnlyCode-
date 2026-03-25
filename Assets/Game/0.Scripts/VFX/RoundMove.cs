using System;
using UnityEngine;

public class RoundMove : MonoBehaviour
{
    private Vector3 pivotPoint = new Vector3(0, 2, 0); // 원형 맵의 중심점(하드 코딩)
    
    [Tooltip("초당 회전하는 속도")]
    public float rotationSpeed = 50f;

    private int isRight;

    private void Start()
    {
        if (gameObject.transform.rotation.x != 0)
        {
            isRight = 1;
        }
        else
        {
            isRight = -1;
        }
    }

    // 원형 맵을 기준으로 회전 이동
    private void Update()
    {
        Vector3 rotationAxis = Vector3.up * isRight; // 회전 축 설정(음수면 오른쪽, 양수면 왼쪽)
        transform.RotateAround(pivotPoint, rotationAxis, rotationSpeed * Time.deltaTime);
    }
}
