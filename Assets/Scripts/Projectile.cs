using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Base Stats")]
    public float speed = 10f;
    public int damage = 20;

    [Header("Debuff Area Probe")]
    public float auraProbeRadius = 0.1f;      // radius kecil utk cek aura di posisi peluru
    public LayerMask auraMask = ~0;           // biarkan ~0 (Everything) atau batasi ke layer Enemy

    private Transform target;
    private Vector3 targetOffset = new Vector3(0f, 1f, 0f);

    // multiplier dari Tower saat ditembak (mis. tower-debuff)
    private float extraDamageMul = 1f;
    private float extraSpeedMul  = 1f;

    // cache untuk OverlapSphere tanpa GC
    static readonly Collider[] hits = new Collider[8];

    public void SetTarget(Transform newTarget, Vector3 offset)
    {
        target = newTarget;
        targetOffset = offset;
    }

    public void SetDamageMultiplier(float mul) => extraDamageMul = Mathf.Max(0f, mul);
    public void SetSpeedMultiplier (float mul) => extraSpeedMul  = Mathf.Max(0f, mul);

    void Update()
    {
        if (target == null) { Destroy(gameObject); return; }

        // === Cek apakah peluru berada DI DALAM aura debuff musuh ===
        bool inAura = IsInsideDebuffAura();

        // speed efektif: base * buff global * (mul dari tower) * (mul dari aura)
        float auraSpeedMul = inAura ? 0.2f : 1f;   // -80% speed
        Vector3 targetPos = target.position + targetOffset;
        Vector3 dir = targetPos - transform.position;
        float step = speed * BuffManager.BulletSpeedMul * extraSpeedMul * auraSpeedMul * Time.deltaTime;

        if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);

        if (dir.magnitude <= step)
        {
            HitTarget(inAura); // kirim status inAura saat impact utk damage
        }
        else
        {
            transform.Translate(dir.normalized * step, Space.World);
        }
    }

    void HitTarget(bool inAuraNow)
    {
        // damage efektif: base * buff global * (mul dari tower) * (mul dari aura saat impact)
        float auraDmgMul = inAuraNow ? 0.5f : 1f;  // -50% damage
        int finalDamage = Mathf.RoundToInt(damage * BuffManager.TurretDamageMul * extraDamageMul * auraDmgMul);

        var e = target.GetComponent<Enemy>();
        if (e != null) e.TakeDamage(finalDamage);

        Destroy(gameObject);
    }

    bool IsInsideDebuffAura()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, auraProbeRadius, hits, auraMask, QueryTriggerInteraction.Collide);
        for (int i = 0; i < count; i++)
        {
            if (hits[i] == null) continue;
            // cukup ada EnemyDebuffAura di parent object collider
            if (hits[i].GetComponentInParent<EnemyDebuffAura>() != null)
                return true;
        }
        return false;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, auraProbeRadius);
    }
#endif
}
