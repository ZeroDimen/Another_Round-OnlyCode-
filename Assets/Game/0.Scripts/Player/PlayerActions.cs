using UnityEngine;

public enum FireState { RIGHT, LEFT, HOLD }

public class PlayerActions : MonoBehaviour
{
    // assignments
    [SerializeField] private Fire _fire_script;
    [SerializeField] private GameObject AttackIndicator;
    [SerializeField] private PlayerMovement PlayerMovement;
    [SerializeField] private GameObject FirePosition;

    // var
    [SerializeField] private int ATTACKS_PER_SECOND = 8;
    private float fire_timer;
    private float FIRE_INCREMENT;

    [SerializeField] public FireState FireState { get; private set; }

    private void Update()
    {
        UpdateAttacksPerSecond();

        // 상태에 따른 공격 발사
        switch (FireState)
        {
            case FireState.RIGHT:
                BasicAttackTimer(FireState.RIGHT);
                break;
            case FireState.LEFT:
                BasicAttackTimer(FireState.LEFT);
                break;
            case FireState.HOLD:
                fire_timer = 0f;
                break;
        }
    }

    private void BasicAttackTimer(FireState state)
    {
        fire_timer += Time.deltaTime;

        if (fire_timer > FIRE_INCREMENT)
        {
            _fire_script.BasicAttackFire(state);
            fire_timer = 0f;
        }
    }

    private void UpdateAttacksPerSecond()
    {
        FIRE_INCREMENT = (float)(1f / ATTACKS_PER_SECOND); // 공격 속도 업데이트
    }

    public void FireLeft()
    {
        FireState = FireState.LEFT;
        IndicatorLeft();
        FirePositionLeft();
    }

    public void FireRight()
    {
        FireState = FireState.RIGHT;
        IndicatorRight();
        FirePositionRight();
    }

    public void FireHold(bool toggle)
    {
        if (toggle) // true
        {
            FireState = FireState.HOLD;
        }
        else // false
        {
            if (PlayerMovement.PlayerFace == LookState.LEFT) FireState = FireState.LEFT;
            else FireState = FireState.RIGHT;
        }
    }

    private void IndicatorLeft()
    {
        AttackIndicator.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        AttackIndicator.transform.localPosition = new Vector3(0f, -1f, 0.25f);
    }

    private void IndicatorRight()
    {
        AttackIndicator.transform.localEulerAngles = new Vector3(90f, 180f, 0f);
        AttackIndicator.transform.localPosition = new Vector3(0f, -1f, -0.25f);
    }

    private void FirePositionLeft()
    {
        FirePosition.transform.localRotation = Quaternion.Euler(-180f, 0f, 0f);
    }

    private void FirePositionRight()
    {
        FirePosition.transform.localRotation = Quaternion.Euler(-0f, 0f, 0f);
    }
}
