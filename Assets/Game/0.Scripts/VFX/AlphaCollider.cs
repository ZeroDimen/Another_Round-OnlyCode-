using UnityEngine;

public class AlphaCollider : MonoBehaviour
{
    [SerializeField] private Collider Collider;
    [SerializeField] private ParticleSystem[]  Particles;
    [SerializeField] private GameObject parents;
    public int Maxcount = 1;
    private float count;
    private void Start()
    {
        parents.SetActive(true);
        count = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyBullet"))
        {
            other.gameObject.SetActive(false);
            count++;
            
            float alpha = 1 - count / Maxcount;
            Debug.Log(alpha);
            
            foreach (var particle in Particles)
            {
                var currentColor = particle.colorOverLifetime;
                var particleColor = particle.main.startColor;
                
                currentColor.color = new ParticleSystem.MinMaxGradient(new Color(particleColor.color.r, particleColor.color.g, particleColor.color.b, alpha));
            }

            if (alpha <= 0)
            {
                parents.SetActive(false);
            }
        }
    }

    private void OnDisable()
    {
        count = 0;
    }
}
