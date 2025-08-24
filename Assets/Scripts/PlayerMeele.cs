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
    public AudioSource audioSource;          // sumber audio
    public AudioClip basicAttackSfx;         // suara basic attack
    public AudioClip skillAttackSfx;         // suara skill

    // ⬇️ NEW
    private DebuffReceiver debuff;

    void Awake()
    {
        debuff = GetComponent<DebuffReceiver>();

        // kalau belum ada, auto-add AudioSource
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        float slowMul = debuff ? debuff.AttackSpeedMul : 1f; // 0..1

        // Basic Attack (Left Mouse)
        if (Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextAttackTime)
        {
            BasicAttack();

            float effectiveRate = attackRate * Mathf.Max(0.0001f, slowMul);
            nextAttackTime = Time.time + 1f / effectiveRate;
        }

        // Skill Attack (Key Q)
        if (Keyboard.current.qKey.wasPressedThisFrame && Time.time >= nextSkillTime)
        {
            SkillAttack();

            float slowFactor = 1f / Mathf.Max(0.0001f, slowMul);
            nextSkillTime = Time.time + skillCooldown * slowFactor;
        }
    }

    void BasicAttack()
    {
        animator.SetTrigger("Attack");

        // 🔊 play sound
        if (basicAttackSfx != null)
            audioSource.PlayOneShot(basicAttackSfx);

        var enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (var e in enemies)
        {
            if (Vector3.Distance(transform.position, e.transform.position) <= attackRange)
            {
                int finalDamage = Mathf.RoundToInt(damage * BuffManager.PlayerDamageMul);
                e.TakeDamage(finalDamage);
            }
        }
    }

    void SkillAttack()
    {
        animator.SetTrigger("Skill");

        // 🔊 play sound
        if (skillAttackSfx != null)
            audioSource.PlayOneShot(skillAttackSfx);

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
