using DamageNumbersPro;
using UnityEngine;
using UnityEngine.UI;

public class BossStatus : MonoBehaviour
{
    [SerializeField] private DamageNumber bulletDamagePopup;
    [SerializeField] private DamageNumber skillDamagePopup;
    
    [Header("Boss Status")]
    private float maxHp;
    [SerializeField] private float hp;
    [SerializeField] private float attackPower;
    public float HP { get => hp; private set => hp = value; }
    public float AttackPower { get => attackPower; protected set => attackPower = value; }

    private PlayerStatus playerStatus; // 플레이어 상태 참조
    private BossMovement bossState; // 보스 상태 참조
    private Animator bossAnimator; // 보스 애니메이터 참조
    private Image bossHPBar; // 보스 HP 바 UI 참조 

    private void Awake()
    {
        bossState = GetComponent<BossMovement>();
        bossAnimator = bossState.GetComponent<Animator>();
    }

    void Start()
    {
        maxHp = hp;
        if (playerStatus == null)
        {
            playerStatus = GameObject.FindWithTag("Player").GetComponent<PlayerStatus>();
        }

        // 보스 HP 바 UI 찾기
        if (bossHPBar == null)
        {
            GameObject bossHPBarObj = GameObject.Find("[Image] HP Bar");
            if (bossHPBarObj != null)
            {
                bossHPBar = bossHPBarObj.GetComponent<Image>();
                if (bossHPBar != null)
                {
                    bossHPBar.fillAmount = 1f; // 초기화
                }
                else
                {
                    Debug.LogWarning("BossHPBar 오브젝트에 Image 컴포넌트가 없습니다.");
                }
            }
            else
            {
                Debug.LogWarning("BossHPBar 오브젝트를 찾을 수 없습니다.");
            }
        }

        bossState = GetComponent<BossMovement>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Bullet"))
        {
            if (bossAnimator.GetBool("RollIdle") == true)
            {
                return; // 무적 상태에서는 데미지 무시
            }
            else
            {
                var playerAttackPower = playerStatus.AttackDamage;
                Debug.Log($"플레이어 공격! {name}이 {playerAttackPower}의 데미지를 입음.");
                SetHit((int)playerAttackPower);
                Debug.Log(transform.position);
                Debug.Log(playerAttackPower);
                bulletDamagePopup.Spawn(transform.position + (Vector3.up * 4), playerAttackPower);
            }

        }

        if (other.CompareTag("Skill"))
        {
            SkillCollider SkillCollider = other.gameObject.GetComponentInParent<SkillCollider>();
            if (SkillCollider == null) Debug.Log($"{name}이 {other.gameObject.name}의 SkillCollider를 찾을 수 없습니다!");
            else
            {
                if (bossAnimator.GetBool("RollIdle") == true)
                {
                    return; // 무적 상태에서는 데미지 무시
                }
                Debug.Log($"Skill hit! {name} took {SkillCollider.Damage} damage");
                SetHit((int)SkillCollider.Damage);
                skillDamagePopup.Spawn(transform.position + (Vector3.up * 4), SkillCollider.Damage);
            }
        }
        else if (other.CompareTag("Ground"))
        {
            bossState.isGround = true;
        }  
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            bossState.isGround = false;
        }
    }

    private void SetHit(int damage)
    {
        HP -= damage;

        if (HP <= 0)
        {
            HP = 0;
        }

        if (bossHPBar != null)
        {
            bossHPBar.fillAmount = HP / maxHp;
        }

        if (HP == 0)
        {
            Debug.Log($"{name}이 파괴되었습니다!");
            LevelManager.Instance.BossKilled();
            Destroy(gameObject);
        }
    }
}
