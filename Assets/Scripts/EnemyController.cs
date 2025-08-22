 using UnityEngine;

namespace LastBastion.Enemies
{
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyController : MonoBehaviour, LastBastion.Player.IDamageable
    {
        public float moveSpeed = 2.5f;
        public int contactDamage = 10;
        public int maxHealth = 30;
        public float attackInterval = 1.0f;

        private int currentHealth;
        private Rigidbody rb;
        private Transform bastion;
        private float nextAttackTime;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            currentHealth = maxHealth;

            // Kunci rotasi biar enemy tidak miring
            rb.freezeRotation = true;
        }

        public void Initialize(Transform bastionTarget, float hpMultiplier)
        {
            bastion = bastionTarget;
            currentHealth = Mathf.RoundToInt(maxHealth * hpMultiplier);
        }

        private void FixedUpdate()
        {
            if (bastion == null) return;

            // Arahkan ke bastion
            Vector3 dir = (bastion.position - rb.position).normalized;

            // Hanya gerak di XZ, Y dibuat 0
            dir.y = 0f;

            // Gerakkan musuh tanpa memutar akibat physics
            rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);

            // Atur rotasi supaya selalu menghadap bastion
            if (dir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(dir);
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision.transform == bastion && Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + attackInterval;
                var bastionHealth = collision.gameObject.GetComponent<LastBastion.Structures.BastionHealth>();
                if (bastionHealth != null)
                {
                    bastionHealth.TakeDamage(contactDamage);
                }
            }
        }

        public void TakeDamage(int amount)
        {
            currentHealth -= amount;
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Destroy(gameObject);
            // Kalau mau kasih tahu EnemySpawner, panggil:
            // LastBastion.Enemies.EnemySpawner.Instance.NotifyEnemyKilled(this);
        }
    }
}
