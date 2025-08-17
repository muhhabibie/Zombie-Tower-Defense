using UnityEngine;

namespace LastBastion.Structures
{
    public class BastionHealth : MonoBehaviour
    {
        public int maxHealth = 300;
        public int currentHealth;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(int amount)
        {
            currentHealth -= amount;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                LastBastion.Core.GameManager.Instance.TriggerDefeat();
            }
            LastBastion.Core.GameManager.Instance.hud?.SetBastionHp(currentHealth, maxHealth);
        }

        public void Heal(int amount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            LastBastion.Core.GameManager.Instance.hud?.SetBastionHp(currentHealth, maxHealth);
        }
    }
}
