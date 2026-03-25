using UnityEngine;

public class PlayerHitBox : MonoBehaviour
{
    [SerializeField] private PlayerStatus playerStatus;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("EnemyBullet") || other.CompareTag("Boss"))
        {
            playerStatus.EnemyCollision(other);
        }
    }
}
