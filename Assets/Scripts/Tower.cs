using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Tower Settings")]
    public float range = 5f;                // jangkauan serangan
    public float fireRate = 1f;             // peluru per detik
    public GameObject projectilePrefab;     // prefab peluru
    public Transform[] firePoints;          // 🔥 Bisa 2 atau lebih firepoint
    public float turnSpeed = 5f;            // kecepatan rotasi

    private float fireCooldown = 0f;        // timer cooldown
    private GameObject currentTarget;       // target musuh saat ini

    void Update()
    {
        fireCooldown -= Time.deltaTime;

        // Cek kondisi target
        if (currentTarget == null || !IsTargetValid(currentTarget))
        {
            currentTarget = GetNewTarget();
        }

        // 🔄 Putar tower kalau ada target
        if (currentTarget != null)
        {
            RotateTower(currentTarget.transform);

            if (fireCooldown <= 0f)
            {
                Shoot(currentTarget);
                fireCooldown = 1f / fireRate; // reset cooldown
            }
        }
    }

    // 🔍 Cek apakah target masih valid
    bool IsTargetValid(GameObject target)
    {
        if (target == null) return false;

        float dist = Vector3.Distance(transform.position, target.transform.position);
        return dist <= range;
    }

    // 🎯 Ambil musuh baru kalau target lama mati / keluar range
    GameObject GetNewTarget()
    {
        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        GameObject bestTarget = null;
        float shortestDist = Mathf.Infinity;

        foreach (Enemy e in enemies)
        {
            Vector3 targetPos = e.transform.position + new Vector3(0f, 1f, 0f); // offset Y
            float dist = Vector3.Distance(transform.position, targetPos);
            if (dist <= range)
            {
                if (dist < shortestDist)
                {
                    shortestDist = dist;
                    bestTarget = e.gameObject;
                }
            }
        }

        return bestTarget;
    }

    void RotateTower(Transform target)
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0f; // biar rotasi hanya di sumbu Y (tidak miring ke atas/bawah)

        Quaternion lookRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, Time.deltaTime * turnSpeed);
    }

    void Shoot(GameObject target)
    {
        if (projectilePrefab == null || firePoints.Length == 0)
        {
            Debug.LogWarning("⚠️ ProjectilePrefab atau FirePoints belum di-assign!");
            return;
        }

        foreach (Transform fp in firePoints)
        {
            GameObject proj = Instantiate(projectilePrefab, fp.position, Quaternion.identity);
            Projectile p = proj.GetComponent<Projectile>();
            if (p != null)
            {
                // kasih target + offset 1 unit ke atas
                p.SetTarget(target.transform, new Vector3(0f, 1f, 0f));
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
