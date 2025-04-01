using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float damage;
    private float lifetime = 5f; // 투사체 생존 시간
    private bool isInitialized = false;
    private bool hasHit = false;

    private void Start()
    {
        // 일정 시간 후 자동으로 파괴
        Destroy(gameObject, lifetime);
    }

    public void Initialize(float projectileDamage)
    {
        damage = projectileDamage;
        isInitialized = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isInitialized || hasHit) return;

        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                // player.TakeDamage(damage);
                hasHit = true;
                Destroy(gameObject);
            }
        }
    }

    private void OnDrawGizmos()
    {
        // 디버그용 기즈모 (투사체의 충돌 범위 표시)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
} 