using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class InkProjectile : MonoBehaviour
{
    private float lifeTime = 5f;
    private float damage = 10f;
    private bool hasHit = false;

    private void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Projectile");

        int playerLayer = LayerMask.NameToLayer("Player");
        int projectileLayer = LayerMask.NameToLayer("Projectile");

        if (playerLayer != -1 && projectileLayer != -1)
        {
            Physics.IgnoreLayerCollision(projectileLayer, playerLayer, true);
        }
    }

    public void Init(float life, float dmg)
    {
        lifeTime = life;
        damage = dmg;
        hasHit = false;
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;

        Transform hitTransform = collision.transform;
        if (hitTransform == null) return;
        if (hitTransform.CompareTag("Player")) return;

        if (hitTransform.CompareTag("Enemy"))
        {
            IDamageable damageable = hitTransform.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
        }

        hasHit = true;
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        if (other == null) return;
        if (other.CompareTag("Player")) return;

        if (other.CompareTag("Enemy"))
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }

            hasHit = true;
            Destroy(gameObject);
        }
    }
}