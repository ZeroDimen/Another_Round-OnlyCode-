using UnityEngine;

// 카메라의 화면비를 조절하여 비율이 디스플레이에서도 의도된 화면을 보여주는 스크립트
public class CameraResolution : MonoBehaviour
{
    private void Awake()
    {
        Camera camera = GetComponent<Camera>();
        Rect rect = camera.rect;

        // 16 / 9 비율을 벗어날 경우 여백을 제거
        // (현제 디스플레이 가로 / 세로) /  (고정하고 싶은 가로 / 세로)
        float scaleheight = ((float)Screen.width / Screen.height) / ((float)16 / 9);
        float scalewidth = 1f / scaleheight;


        if (scaleheight < 1) //위 아래 공간이 남는 경우
        {
            rect.height = scaleheight;
            rect.y = (1f - scaleheight) / 2f;
        }
        else //좌우 공간이 남는 경우
        {
            rect.width = scalewidth;
            rect.x = (1f - scalewidth) / 2f;
        }

        camera.rect = rect;
    }
}