using UnityEngine;

namespace LastBastion.Player
{
    public interface IDamageable
    {
        void TakeDamage(int amount);
    }

    public class Health : MonoBehaviour, IDamageable
    {
        public int maxHealth = 100;
        public int currentHealth;
        public System.Action OnDead;
        public System.Action<int, int> OnHealthChanged;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(int amount)
        {
            currentHealth -= amount;
            if (currentHealth < 0) currentHealth = 0;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            if (currentHealth == 0)
            {
                OnDead?.Invoke();
                var player = GetComponent<PlayerController>();
                if (player != null)
                {
                    //player.OnDeath();
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }

        public void Heal(int amount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}
