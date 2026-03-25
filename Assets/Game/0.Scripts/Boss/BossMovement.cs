using System.Collections;
using UnityEngine;

public enum EBossState { Roll, Jump, Idle, RollIdle, Charge, Turn, Move }

[RequireComponent(typeof(Rigidbody))]
public class BossMovement : MonoBehaviour, ICircleMover
{
    [Header("Movement Variables")]
    [SerializeField] private float speed_leftright = 200f;
    [SerializeField] private float speed_updown = 20f;
    private Vector3 pivot = Vector3.zero;
    private bool gravity_enabled;

    [Header("Components")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private LineIndicator li;
    [SerializeField] private Animator bossAnimator;

    // Interface Variables
    public Transform Transform => transform;
    public Vector3 Pivot => pivot;
    public float Speed_LeftRight { get => speed_leftright; private set => speed_leftright = value; }
    public float Speed_UpDown { get => speed_updown; private set => speed_updown = value; }

    // test
    public EBossState State;
    float timer = 0f;
    bool is_jumping;
    public bool isGround = true;
    Coroutine jump_routine;
    Coroutine charge_routine;
    Coroutine transform_routine;
    Coroutine turn_routine;
    GameObject Player;

    private void Awake()
    {
        if (bossAnimator == null)
        {
            bossAnimator = GetComponent<Animator>();
            bossAnimator.SetBool("Idle", true);
        }
    }

    private void Start()
    {
        gravity_enabled = true;
        State = EBossState.RollIdle;

        Player = GameObject.FindWithTag("Player");
        if (Player == null) Debug.Log("Boss: Could not find player!");
    }

    private void FixedUpdate()
    {
        // check gravity toggle
        if (gravity_enabled) rb.useGravity = true;
        else rb.useGravity = false;

        // run state timer
        timer += Time.fixedDeltaTime;
        if (timer > 3f)
        {
            timer = 0f;
            int random = Random.Range(1, 101);
            if (random <= 35)
            {
                State = EBossState.Idle;
            }

            else if (random <= 40)
            {
                State = EBossState.RollIdle;
            }

            else if (random <= 60)
            {
                // Idle일 때 Charge로 전환 시 RollTrans 거치기
                if (State != EBossState.RollIdle)
                {
                    State = EBossState.RollIdle;
                }
                State = EBossState.Charge;
            }
            else if (random <= 80)
            {
                if (isGround == true) return;
                State = EBossState.Jump;
            }

            else if (random <= 90)
            {
                if (State != EBossState.RollIdle)
                {
                    bossAnimator.SetBool("RollIdle", true);
                    bossAnimator.SetBool("Idle", false);
                    State = EBossState.Roll;
                }

            }

            else
            {
                State = EBossState.Turn;
            }

            timer = 0f;
        }

        // do state actions
        switch (State)
        {
            case EBossState.Idle:
                if (transform_routine == null)
                {
                    transform_routine = StartCoroutine(BossRollToIdleTrans());
                }
                break;
            case EBossState.Jump:
                JumpAround(1f, 12f);
                break;
            case EBossState.Roll:
                RollAround(-1f);
                break;
            case EBossState.RollIdle:
                if (transform_routine == null)
                {
                    rb.useGravity = true;
                    transform_routine = StartCoroutine(BossIdleToRollTrans());
                }
                break;
            case EBossState.Charge:
                rb.useGravity = false;
                Charge();
                break;
            case EBossState.Turn:
                if (turn_routine == null)
                {
                    turn_routine = StartCoroutine(BossTurn());
                }
                break;
        }
    }

    public IEnumerator BossRollToIdleTrans()
    {
        bossAnimator.SetBool("Idle", true);
        bossAnimator.SetBool("RollIdle", false);

        yield return new WaitForSeconds(2f);

        transform_routine = null;
    }

    public IEnumerator BossIdleToRollTrans()
    {

        bossAnimator.SetBool("Idle", false);
        bossAnimator.SetBool("RollIdle", true);

        yield return new WaitForSeconds(2f);

        transform_routine = null;
    }

    public IEnumerator BossTurn()
    {


        // 2. 턴 애니메이션 시작
        bossAnimator.SetTrigger("Turn"); // 이 Trigger는 애니메이션 재생을 시작

        // 3. 애니메이션 시간만큼 대기
        yield return new WaitForSeconds(2f);

        // 5. 상태 정리
        transform_routine = null;
        State = EBossState.Idle;
    }

    public void RollAround(float input)
    {
        CircleMoveUtility.MoveLeftRight(Transform, Pivot, input, Speed_LeftRight);
    }

    public void JumpAround(float input, float jump_height)
    {
        // how?
        RollAround(input);

        if (jump_routine == null)
        {
            Debug.Log("Start Jump Routine");
            jump_routine = StartCoroutine(JumpRoutine(jump_height));
        }
    }

    private void Charge()
    {
        if (charge_routine == null)
        {
            Debug.Log("Start Charge Routine");
            charge_routine = StartCoroutine(ChargeRoutine());
        }
    }

    private IEnumerator JumpRoutine(float jump_height)
    {
        if (is_jumping) yield break;

        is_jumping = true;

        //rb.linearVelocity = new Vector3(0f, jump_height, 0f);
        bossAnimator.SetBool("Jump", true);
        rb.AddForce(Vector3.up * jump_height, ForceMode.Impulse);

        bossAnimator.SetBool("Jump", false);
        //yield return new WaitUntil(IsGrounded);
        yield return new WaitForSeconds(3f);
        //Debug.Log("JumpRoutine: IsGrounded");

        is_jumping = false;
        jump_routine = null;
        State = EBossState.Idle;
    }

    private bool IsGrounded()
    {
        return Mathf.Approximately(Transform.position.y, 6.5f);
    }

    private IEnumerator ChargeRoutine(float wait_time = 1f, float charge_time = 0.5f, float player_add_distance = 5f)
    {
        bossAnimator.SetBool("RollIdle", true);
        bossAnimator.SetBool("Idle", false);
        // 플레이어 위치 저장
        if (Player == null) yield break;
        var target = Player.transform.position;

        // 돌진 방향 계산
        var self2D_X = Circle2DConverter.X_3Dto2D(transform.position);
        var target2D_X = Circle2DConverter.X_3Dto2D(target);
        var deltaX = Circle2DConverter.X_2DDistance(self2D_X, target2D_X);
        var deltaY = target.y - transform.position.y;
        Vector2 deltaXY = new Vector2(deltaX, deltaY);
        Vector2 direction = deltaXY.normalized;


        if (deltaX > 0)
        {
            // deltaX가 양수: 플레이어가 보스 기준으로 오른쪽(시계 방향)에 있음
            // 오른쪽으로 굴러가는 애니메이션 Bool을 켬
            bossAnimator.SetBool("RightRoll", false);
            bossAnimator.SetBool("LeftRoll", true);
        }
        else if (deltaX < 0)
        {
            // deltaX가 음수: 플레이어가 보스 기준으로 왼쪽(시계 반대 방향)에 있음
            // 왼쪽으로 굴러가는 애니메이션 Bool을 켬
            bossAnimator.SetBool("RightRoll", true);
            bossAnimator.SetBool("LeftRoll", false);
        }

        // 돌진 목적지 계산
        float charge_distance = deltaXY.magnitude + player_add_distance; // player plus 2
        var self2D = new Vector2(self2D_X, transform.position.y);
        var destination2D = self2D + direction * charge_distance;

        // clamp
        destination2D.y = Mathf.Clamp(destination2D.y, Constants.PLAYAREA_Y_MIN, Constants.PLAYAREA_Y_MAX);

        // 경고 표시 킴
        var destination3D = Circle2DConverter.X_2Dto3D(destination2D.x, Constants.PLAYAREA_RADIUS, destination2D.y);
        li.ShowArc(transform.position, destination3D, Constants.PLAYAREA_RADIUS);

        // wait_time초 대기
        yield return new WaitForSeconds(wait_time);

        // 경고 표시 끔
        li.Hide();

        // 돌진
        float timer = 0f;
        while (timer < charge_time)
        {
            timer += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(timer / charge_time);
            // 2D 벡터로 거리계산
            Vector2 current2D = Vector2.Lerp(self2D, destination2D, t);

            // 2D 위치를 3D로 환산
            Vector3 new_position = Circle2DConverter.X_2Dto3D(current2D.x, Constants.PLAYAREA_RADIUS, current2D.y);
            transform.position = new_position;

            yield return new WaitForFixedUpdate();
        }

        bossAnimator.SetBool("RightRoll", false);
        bossAnimator.SetBool("LeftRoll", false);

        charge_routine = null;
    }
}
