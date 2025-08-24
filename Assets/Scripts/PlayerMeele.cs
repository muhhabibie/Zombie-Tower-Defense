// PlayerMelee.cs (tambahkan field & gunakan multiplier)
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMelee : MonoBehaviour
{
    [Header("Basic Attack")]
    public float attackRange = 2f;
    public int damage = 25;
    public float attackRate = 1f;     // serangan per detik
    private float nextAttackTime = 0f;

    [Header("Skill Attack")]
    public float skillRange = 2.5f;
    public int skillDamage = 60;
    public float skillCooldown = 5f;  // detik
    private float nextSkillTime = 0f;

    [Header("References")]
    public Animator animator;

    // ⬇️ NEW
    private DebuffReceiver debuff;

    void Awake()
    {
        debuff = GetComponent<DebuffReceiver>();
    }

    void Update()
    {
        float slowMul = debuff ? debuff.AttackSpeedMul : 1f; // 0..1

        // Basic Attack (Left Mouse)
        if (Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextAttackTime)
        {
            BasicAttack();

            float effectiveRate = attackRate * Mathf.Max(0.0001f, slowMul); // rate turun kalau slow < 1
            nextAttackTime = Time.time + 1f / effectiveRate;
        }

        // Skill Attack (Key Q) -> cooldown membesar saat di-slow
        if (Keyboard.current.qKey.wasPressedThisFrame && Time.time >= nextSkillTime)
        {
            SkillAttack();

            float slowFactor = 1f / Mathf.Max(0.0001f, slowMul); // slow 0.2 => cooldown x5
            nextSkillTime = Time.time + skillCooldown * slowFactor;
        }
    }

    void BasicAttack()
    {
        animator.SetTrigger("Attack");
        var enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (var e in enemies)
        {
            if (Vector3.Distance(transform.position, e.transform.position) <= attackRange)
            {
                int finalDamage = Mathf.RoundToInt(damage * BuffManager.PlayerDamageMul); // buff global
                e.TakeDamage(finalDamage);
            }
        }
    }

    void SkillAttack()
    {
        animator.SetTrigger("Skill");
        var enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (var e in enemies)
        {
            if (Vector3.Distance(transform.position, e.transform.position) <= skillRange)
            {
                int finalDamage = Mathf.RoundToInt(skillDamage * BuffManager.PlayerDamageMul);
                e.TakeDamage(finalDamage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.blue; Gizmos.DrawWireSphere(transform.position, skillRange);
    }
}
