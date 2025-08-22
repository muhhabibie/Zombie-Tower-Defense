using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMelee : MonoBehaviour
{
    [Header("Basic Attack")]
    public float attackRange = 2f;
    public int damage = 25;
    public float attackRate = 1f;
    private float nextAttackTime = 0f;

    [Header("Skill Attack")]
    public float skillRange = 2.5f;
    public int skillDamage = 60;
    public float skillCooldown = 5f;
    private float nextSkillTime = 0f;

    [Header("References")]
    public Animator animator;

    void Update()
    {
        // Basic Attack (Left Mouse)
        if (Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextAttackTime)
        {
            BasicAttack();
            nextAttackTime = Time.time + 1f / attackRate;
        }

        // Skill Attack (Key Q)
        if (Keyboard.current.qKey.wasPressedThisFrame && Time.time >= nextSkillTime)
        {
            SkillAttack();
            nextSkillTime = Time.time + skillCooldown;
        }
    }

    void BasicAttack()
    {
        Debug.Log("Basic Attack!");
        animator.SetTrigger("Attack");

        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy e in enemies)
        {
            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist <= attackRange)
            {
                e.TakeDamage(damage);
                Debug.Log($"{e.name} kena Basic Attack, -{damage} HP, sisa HP: {e.GetCurrentHP()}");
            }
        }
    }

    void SkillAttack()
    {
        Debug.Log("Skill Attack!");
        animator.SetTrigger("Skill");

        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy e in enemies)
        {
            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist <= skillRange)
            {
                e.TakeDamage(skillDamage);
                Debug.Log($"{e.name} kena Skill Attack, -{skillDamage} HP, sisa HP: {e.GetCurrentHP()}");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Range Basic
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Range Skill
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, skillRange);
    }
}
