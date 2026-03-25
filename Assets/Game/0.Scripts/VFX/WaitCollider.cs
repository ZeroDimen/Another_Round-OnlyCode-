using System.Collections;
using UnityEngine;

public class WaitCollider : MonoBehaviour
{
    public float waitDuration = 1.0f; // 에너지를 모으는 시간
    public float ActiveDuration = 0.5f; // 레이저가 활성화되는 시간 

    [SerializeField]
    private Collider collider; // 레이저 오브젝트에 붙어있는 콜라이더를 저장할 변수

    void Awake()
    {
        if (collider == null)
        {
            enabled = false; // 콜라이더가 없으면 스크립트를 비활성화
            return;
        }
        
        collider.enabled = false;
    }

    void OnEnable()
    {
        StartCoroutine(WaitSequence());
    }

    IEnumerator WaitSequence()
    {
        yield return new WaitForSeconds(waitDuration);
        collider.enabled = true;
        Debug.Log("Beam 콜라이더 생성");
        
        yield return new WaitForSeconds(ActiveDuration);
        collider.enabled = false;
        
    }

    void OnDisable()
    {
        StopAllCoroutines();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }
}