using UnityEngine;
using UnityEngine.InputSystem;

public enum LookState { RIGHT, LEFT };

public class PlayerMovement : MonoBehaviour
{
    // ref
    [SerializeField] private GameObject player_pivot;
    [SerializeField] private MeshRenderer HitboxVisible;
    [SerializeField] private BackgroundMovement background;
    [SerializeField] private Animator _animator;
    [SerializeField] private PlayerActions PlayerActions;

    // var
    private Vector3 move_input;
    [SerializeField] private float player_x_speed;
    private float player_y_speed;
    [SerializeField] private float flying_speed_leftright = 30f;
    [SerializeField] private float flying_speed_updown = 10f;
    public LookState PlayerFace { get; private set; }
    public bool LockDirection_Button { get; private set; }
    public bool LockDirection_Skill { get; private set; }
    public bool MovementDisabled_LeftRight { get; private set; }

    private float PLAYER_HEIGHT = 1.7f;

    private float BASE_SPEED_LEFTRIGHT = 30f;
    private float BASE_SPEED_UPDOWN = 10f;

    private float FOCUSMODE_SPEED_LEFTRIGHT = 15f;
    private float FOCUSMODE_SPEED_UPDOWN = 5f;

    private float PLAYAREA_Y_LOWERBOUND = 5f;
    private float PLAYAREA_Y_UPPERBOUND = 20f;

    private float PLAYERBOUND_Y_LOWER;
    private float PLAYERBOUND_Y_UPPER;


    private void Awake()
    {
        PLAYERBOUND_Y_LOWER = PLAYAREA_Y_LOWERBOUND + (PLAYER_HEIGHT * 0.5f);
        PLAYERBOUND_Y_UPPER = PLAYAREA_Y_UPPERBOUND - (PLAYER_HEIGHT * 0.5f);
    }

    private void FixedUpdate()
    {
        FlyingMove();
        UpdateAnimations();
        background.Parallax(player_x_speed);
    }

    private void FlyingMove()
    {
        // leftright
        // update speed
        if (MovementDisabled_LeftRight) move_input.x = 0;
        player_x_speed = -move_input.x * flying_speed_leftright;

        if (player_x_speed != 0)
        {
            // move player
            player_pivot.transform.Rotate(0, player_x_speed * Time.fixedDeltaTime, 0);
        }

        // updown
        // update speed
        player_y_speed = move_input.y * flying_speed_updown;
        // move player + clamp y
        Vector3 pos = transform.position;
        pos += Vector3.up * flying_speed_updown * move_input.y * Time.fixedDeltaTime;
        pos.y = Mathf.Clamp(pos.y, PLAYERBOUND_Y_LOWER, PLAYERBOUND_Y_UPPER);
        transform.position = pos;
        // TODO: bottom is fine for hard clamping (it's ground), top may need smoother movement 
    }

    private void UpdateAnimations()
    {
        if (move_input.x == 0 || LockDirection_Button || LockDirection_Skill) return;

        if ((move_input.x > 0) && (PlayerFace == LookState.LEFT))
        {
            PlayerFace = LookState.RIGHT;
            PlayerActions.FireRight();
            _animator.SetTrigger("TurnLeftToRight");
        }
        else if ((move_input.x < 0) && (PlayerFace == LookState.RIGHT))
        {
            PlayerFace = LookState.LEFT;
            PlayerActions.FireLeft();
            _animator.SetTrigger("TurnRightToLeft");
        }
    }

    public void OnFocusMode(InputValue value)
    {
        if (value.isPressed)
        {
            GameManager.Instance.SetGameState(Constants.EGameState.FocusMode);
            flying_speed_leftright = FOCUSMODE_SPEED_LEFTRIGHT;
            flying_speed_updown = FOCUSMODE_SPEED_UPDOWN;
            HitboxVisible.enabled = true;
        }
        else
        {
            GameManager.Instance.SetGameState(Constants.EGameState.Play);
            flying_speed_leftright = BASE_SPEED_LEFTRIGHT;
            flying_speed_updown = BASE_SPEED_UPDOWN;
            HitboxVisible.enabled = false;
        }
    }

    public void OnMove(InputValue value)
    {
        move_input = value.Get<Vector2>();
    }


    public void OnHoldDirection(InputValue value)
    {
        if (value.isPressed)
        {
            LockDirection_Button = true;
        }
        else
        {
            LockDirection_Button = false;
        }
    }

    public void SetLockDirection_Skill(bool toggle)
    {
        LockDirection_Skill = toggle;
    }

    public void StopMovement_LeftRight(bool toggle)
    {
        MovementDisabled_LeftRight = toggle;
    }
}
