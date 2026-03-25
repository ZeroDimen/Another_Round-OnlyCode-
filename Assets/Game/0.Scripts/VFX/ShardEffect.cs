using System.Collections;
using UnityEngine;
using static Constants;

[RequireComponent(typeof(Collider))]
public class ShardEffect : MonoBehaviour, ISkillEffect
{
    private enum ShardState
    {
        Searching,
        Tracking
    }

    private ShardState state = ShardState.Searching;

    private float damage;
    private Transform firePoint;
    private float detectRadius = 8f;
    private float speed = 12f;               // 최대 속도
    private float worldRadius = 25f;
    private Vector3 mapCenter = Vector3.zero;

    private Transform currentTarget;
    private bool initialized = false;
    private float hoverTime = 0.25f;
    private float lifeTime = 5f;
    private Collider col;

    private float acceleration = 10f;        // 가속도
    private float currentSpeed = 0f;         // 현재 속도

    public void Initialize(float dmg, Transform firePoint, float detectR, float spd, float worldR, Vector3 center)
    {
        this.damage = dmg;
        this.firePoint = firePoint;
        this.detectRadius = detectR;
        this.speed = spd;
        this.worldRadius = worldR;
        this.mapCenter = center;

        CommonInit();
    }

    public void InitializeEffect(float dmg, Transform firePoint)
    {
        this.damage = dmg;
        this.firePoint = firePoint;
        CommonInit();
    }

    private void CommonInit()
    {
        col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        AlignToMapCurvature();
        initialized = true;
        StartCoroutine(StateController());
    }

    private void AlignToMapCurvature()
    {
        Vector3 fromCenter = (transform.position - mapCenter).normalized;
        Vector3 tangent = new Vector3(-fromCenter.z, 0f, fromCenter.x).normalized;
        if (tangent.sqrMagnitude < 0.0001f)
            tangent = Vector3.forward;

        Quaternion rot = Quaternion.LookRotation(tangent, fromCenter);
        Vector3 euler = rot.eulerAngles;
        euler.z = 90f;
        transform.rotation = Quaternion.Euler(euler);
    }

    private IEnumerator StateController()
    {
        if (!initialized) yield break;

        yield return new WaitForSeconds(hoverTime);

        state = ShardState.Searching;
        float searchTimer = 0f;
        float maxSearchTime = 3f;

        while (true)
        {
            if (state == ShardState.Searching)
            {
                AlignToMapCurvature();

                EnemyController found = FindClosestEnemy(firePoint.position, detectRadius);
                if (found != null)
                {
                    currentTarget = found.transform;
                    state = ShardState.Tracking;
                    StartCoroutine(TrackAndMove());
                    yield break;
                }

                searchTimer += Time.deltaTime;
                if (searchTimer >= maxSearchTime)
                {
                    Destroy(gameObject);
                    yield break;
                }
            }

            yield return null;
        }
    }

    private EnemyController FindClosestEnemy(Vector3 center, float radius)
    {
        Collider[] hits = Physics.OverlapSphere(center, radius);
        EnemyController closest = null;
        float minDist = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            EnemyController enemy = hit.GetComponent<EnemyController>();
            if (enemy == null) continue;

            float d = Vector3.Distance(center, enemy.transform.position);
            if (d < minDist)
            {
                minDist = d;
                closest = enemy;
            }
        }

        return closest;
    }

    private IEnumerator TrackAndMove()
    {
        state = ShardState.Tracking;
        float elapsed = 0f;
        float currentSpeed = 0f; // 가속용

        while (gameObject != null && elapsed < lifeTime)
        {
            elapsed += Time.deltaTime;

            if (currentTarget == null)
            {
                // 타겟 잃음 -> 탐색 상태로 복귀
                StartCoroutine(StateController());
                yield break;
            }

            EnemyController check = currentTarget.GetComponent<EnemyController>();
            if (check == null || check.State == EEnemyState.Dead)
            {
                // 적이 죽거나 사라짐 -> 탐색 상태 복귀
                currentTarget = null;
                StartCoroutine(StateController());
                yield break;
            }

            // 곡면 법선
            Vector3 normal = (transform.position - mapCenter).normalized;

            // 타겟 방향
            Vector3 toTarget = (currentTarget.position - transform.position);
            float distToTarget = toTarget.magnitude;

            // 곡면 투영 이동 방향
            Vector3 moveDir = Vector3.ProjectOnPlane(toTarget.normalized, normal).normalized;
            if (moveDir.sqrMagnitude < 0.0001f)
                moveDir = Vector3.Cross(Vector3.up, normal).normalized;

            // 가속 처리: 부드럽게 최고 속도에 도달
            currentSpeed = Mathf.MoveTowards(currentSpeed, speed, Time.deltaTime * speed * 2f);

            // 이동
            Vector3 newPos = transform.position + moveDir * currentSpeed * Time.deltaTime;

            // 높이 보정
            float targetY = currentTarget.position.y;
            newPos.y = Mathf.Lerp(transform.position.y, targetY, 5f * Time.deltaTime);

            // 곡면 반지름 유지
            Vector3 fromCenter = newPos - mapCenter;
            Vector2 flat = new Vector2(fromCenter.x, fromCenter.z);
            if (flat.sqrMagnitude > 0.000001f)
            {
                Vector2 flatDir = flat.normalized * worldRadius;
                newPos.x = flatDir.x;
                newPos.z = flatDir.y;
            }

            transform.position = newPos;

            // 회전: 이동 방향으로 바라보기 (forward가 이동 방향)
            if (moveDir.sqrMagnitude > 0.0001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(moveDir, normal);
                Vector3 euler = lookRot.eulerAngles;
                euler.z = 90f; // 2D 시점 유지
                transform.rotation = Quaternion.Euler(euler);
            }

            // 충돌 판정
            if (distToTarget <= 0.6f)
            {
                EnemyController ec = currentTarget.GetComponent<EnemyController>();
                if (ec != null)
                    ec.SetHit(((int)damage));

                Destroy(gameObject);
                yield break;
            }

            yield return null;
        }

        // 추적 중 시간 초과 시 소멸
        if (gameObject != null)
            Destroy(gameObject);
    }


    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other == null) return;
    //
    //     EnemyController ec = other.GetComponentInParent<EnemyController>();
    //     if (ec != null)
    //     {
    //         ec.SetHit(((int)damage));
    //         Destroy(gameObject);
    //     }
    // }
}
