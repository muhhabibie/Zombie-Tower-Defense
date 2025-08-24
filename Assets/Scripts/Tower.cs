using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Tower Settings")]
    public float range = 5f;
    public float fireRate = 1f;              // shots per second
    public GameObject projectilePrefab;
    public Transform[] firePoints;
    public float turnSpeed = 5f;

    [Header("Debuff")]
    public bool slowAffectsRotation = false; // opsional

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip cannonShot;
    public AudioClip ballistaShot;
    public AudioClip catapultShot;
    public AudioClip turretShot;
    public TowerType towerType = TowerType.Cannon;

    private GameObject currentTarget;
    private DebuffReceiver debuff;
    private float fireProgress = 0f;         // model progress (stabil)

    void Awake() => debuff = GetComponent<DebuffReceiver>();

    void Update()
    {
        bool debuffed = debuff && debuff.AttackSpeedMul < 0.999f;

        float effTurnSpeed = slowAffectsRotation
            ? turnSpeed * Mathf.Clamp(debuff ? debuff.AttackSpeedMul : 1f, 0.02f, 1f)
            : turnSpeed;

        if (currentTarget == null || !IsTargetValid(currentTarget, debuffed))
            currentTarget = GetNewTarget(debuffed);

        if (currentTarget != null)
        {
            RotateTower(currentTarget.transform, effTurnSpeed);

            float effFireRate = fireRate * (debuffed ? debuff.AttackSpeedMul : 1f);

            fireProgress += effFireRate * Time.deltaTime;
            if (fireProgress >= 1f)
            {
                Shoot(currentTarget);
                fireProgress -= 1f;
            }
        }
        else
        {
            fireProgress = Mathf.Clamp01(fireProgress - Time.deltaTime);
        }
    }

    float GetEffectiveRange(bool debuffed)
    {
        float baseRange = range * BuffManager.TowerAttackRangeMul;
        return debuffed ? Mathf.Min(baseRange, 10f) : baseRange;
    }

    bool IsTargetValid(GameObject target, bool debuffed)
    {
        if (target == null) return false;
        float effRange = GetEffectiveRange(debuffed);
        float dist = Vector3.Distance(transform.position, target.transform.position);
        return dist <= effRange;
    }

    GameObject GetNewTarget(bool debuffed)
    {
        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        GameObject best = null; float shortest = Mathf.Infinity;
        float effRange = GetEffectiveRange(debuffed);

        foreach (var e in enemies)
        {
            Vector3 pos = e.transform.position + new Vector3(0f, 1f, 0f);
            float d = Vector3.Distance(transform.position, pos);
            if (d <= effRange && d < shortest) { shortest = d; best = e.gameObject; }
        }
        return best;
    }

    void RotateTower(Transform target, float effTurnSpeed)
    {
        Vector3 dir = target.position - transform.position; dir.y = 0f;
        var lookRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, Time.deltaTime * effTurnSpeed);
    }

    void Shoot(GameObject target)
    {
        if (projectilePrefab == null || firePoints.Length == 0)
        {
            Debug.LogWarning("⚠️ ProjectilePrefab/FirePoints belum di-assign!");
            return;
        }

        foreach (var fp in firePoints)
        {
            var proj = Instantiate(projectilePrefab, fp.position, Quaternion.identity);
            var p = proj.GetComponent<Projectile>();
            if (p != null) p.SetTarget(target.transform, new Vector3(0f, 1f, 0f));
        }

        PlayShootSound();
    }

    void PlayShootSound()
    {
        if (audioSource == null) return;

        AudioClip clip = null;
        switch (towerType)
        {
            case TowerType.Cannon: clip = cannonShot; break;
            case TowerType.Ballista: clip = ballistaShot; break;
            case TowerType.Catapult: clip = catapultShot; break;
            case TowerType.Turret: clip = turretShot; break;
        }

        if (clip != null)
            audioSource.PlayOneShot(clip);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}

public enum TowerType { Cannon, Ballista, Catapult, Turret }
