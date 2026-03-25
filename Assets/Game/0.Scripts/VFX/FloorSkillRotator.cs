using UnityEngine;

public class FloorSkillRotator : MonoBehaviour
{
    // Floor에 생성되는 스킬은 뒤집었을 경우 Floor 반대쪽 면에 생성되기 때문에 바꿔줄 스크립트가 필요
    void Start()
    {
        if (gameObject.transform.rotation.x != 0)
        {
            gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }
}
