using UnityEngine;

public class BackgroundMovement : MonoBehaviour
{
    [SerializeField] private float BACKGROUND_PARALLAX_SPEED = 0.5f;

    public void Parallax(float player_speed)
    {
        transform.Rotate(0, -player_speed * (1f - BACKGROUND_PARALLAX_SPEED) * Time.fixedDeltaTime, 0);
    }
}
