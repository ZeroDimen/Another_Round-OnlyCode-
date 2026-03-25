using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerInputState { GAMEPLAY, UI }

public class PlayerStatus : MonoBehaviour
{
    public float HP { get; private set; }
    public float AttackDamage { get; private set; }
    private float PLAYER_MAXHP = 100f;
    private float PLAYER_ATTACK_DAMAGE = 10f;
    public float BaseMaxHp => PLAYER_MAXHP;
    public float BaseAttack => PLAYER_ATTACK_DAMAGE;

    [SerializeField] private TextMeshProUGUI hp_display;
    [SerializeField] private PlayerMovement PlayerMovement;
    [SerializeField] private PlayerInput PlayerInput;

    public bool GAMEPLAY_INPUT_ENABLED => PlayerInputState == PlayerInputState.GAMEPLAY;
    public bool UI_INPUT_ENABLED => PlayerInputState == PlayerInputState.UI;
    public PlayerInputState PlayerInputState { get; private set; } = PlayerInputState.GAMEPLAY;
    public event Action<PlayerInputState> OnPlayerInputStateChanged;

    public void Start()
    {
        InitializePlayer();
    }

    private void Update()
    {
        hp_display.text = $"HP - {HP}";
    }

    private void InitializePlayer()
    {
        // PlayerStatusData가 있으면 그 값을 사용
        if (PlayerStatusData.Instance != null)
        {
            PLAYER_MAXHP = PlayerStatusData.Instance.GetHP();
            PLAYER_ATTACK_DAMAGE = PlayerStatusData.Instance.GetAttack();
            Debug.Log($"[PlayerStatus] PlayerStatusData에서 스탯 불러옴: HP={PLAYER_MAXHP}, Attack={PLAYER_ATTACK_DAMAGE}");
        }

        HP = PLAYER_MAXHP;
        AttackDamage = PLAYER_ATTACK_DAMAGE;
    }

    public float GetHp()
    {
        return HP;
    }

    public void TakeDamage(float damage)
    {
        HP -= damage;

        if (HP <= 0) Death();
    }

    public void SetAttackDamage(float newDamage)
    {
        PLAYER_ATTACK_DAMAGE = newDamage;
        AttackDamage = newDamage;
    }


    public void RestoreFullHp(float maxHp)
    {
        HP = maxHp;
    }


    public void Death()
    {
        GameManager.Instance.SetGameState(Constants.EGameState.GameOver);
        Debug.Log($"플레이어 사망!");
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.gameObject.CompareTag("Enemy"))
    //    {
    //        var enemyAttackPower = other.gameObject.GetComponent<EnemyController>().enemyStatus.attackPower;
    //        Debug.Log($"적의 공격력: {enemyAttackPower}로 피격!");
    //        TakeDamage(enemyAttackPower);
    //    }

    //    // TODO: Move bullet collision here
    //}

    public void EnemyCollision(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            var enemyAttackPower = other.gameObject.GetComponent<EnemyController>().enemyStatus.attackPower;
            Debug.Log($"적의 공격력: {enemyAttackPower}로 피격!");
            TakeDamage(enemyAttackPower);
        }
        else if(other.gameObject.CompareTag("EnemyBullet"))
        {
            var enemyBulletAttackPower = other.gameObject.GetComponent<EnemyBullet>().enemyStatus.attackPower;
            Debug.Log($"적의 총알 공격력: {enemyBulletAttackPower}로 피격!");
            TakeDamage(enemyBulletAttackPower);
        }
        else if(other.gameObject.CompareTag("Boss"))
        {
            var bossAttackPower = other.gameObject.GetComponent<BossStatus>().AttackPower;
            Debug.Log($"보스의 공격력: {bossAttackPower}로 피격!");
            TakeDamage(bossAttackPower);
        }

        // TODO: Move bullet collision here
    }

    /// <summary>
    /// 메뉴 인풋으로 변경
    /// </summary>
    public void SwitchToUI()
    {
        if (PlayerInputState == PlayerInputState.UI)
        {
            Debug.LogWarning("이미 InputActions이 UI로설정되있습니다!");
        }
        
        PlayerInputState = PlayerInputState.UI;
        PlayerInput.SwitchCurrentActionMap("UI");
        OnPlayerInputStateChanged.Invoke(PlayerInputState.UI);
    }

    /// <summary>
    /// 게임 인풋으로 변경
    /// </summary>
    public void SwitchToGameplay()
    {
        if (PlayerInputState == PlayerInputState.GAMEPLAY)
        {
            Debug.LogWarning("이미 InputActions이 Gameplay로 설정되있습니다!");
        }

        PlayerInputState = PlayerInputState.GAMEPLAY;
        PlayerInput.SwitchCurrentActionMap("Gameplay");
        OnPlayerInputStateChanged.Invoke(PlayerInputState.GAMEPLAY);
    }

    /// <summary>
    /// 스킬들이 플레이어 방향 고정에 사용하는 Wrapper 
    /// </summary>
    public void LockDirection(bool toggle)
    {
        PlayerMovement.SetLockDirection_Skill(toggle);
    }

    /// <summary>
    /// 플레이어 좌우이동을 끄는 함수
    /// </summary>
    public void StopMovement_LeftRight(bool toggle)
    {
        PlayerMovement.StopMovement_LeftRight(toggle);
    }
}
