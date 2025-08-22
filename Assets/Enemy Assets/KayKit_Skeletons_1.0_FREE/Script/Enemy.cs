using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
public class Enemy : MonoBehaviour
{
    private EnemyStats stats;
    private int currentHP;

    void Awake()
    {
        stats = GetComponent<EnemyStats>();
        currentHP = stats.baseHP;
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Bisa tambahkan animasi mati kalau ada animator
        // Animator anim = GetComponent<Animator>();
        // if (anim != null) anim.SetTrigger("Die");

        Destroy(gameObject); // musuh dihancurkan
    }

    public int GetCurrentHP()
    {
        return currentHP;
    }
}
