using UnityEngine;
using System.Collections;

public class CatapultTower : MonoBehaviour
{
    [Header("Tower Settings")]
    public float range = 5f;
    public float fireRate = 1f;
    public GameObject projectilePrefab;
    public Transform[] firePoints;
    public float turnSpeed = 5f;

    [Header("Catapult Arm")]
    public Transform towerBase;
    public Transform catapultArm;
    public float armRotateSpeed = 100f;
    public float armMaxAngle = 130f;

    [Header("Audio")]
    public AudioSource audioSource;     // komponen audio
    public AudioClip shootSFX;          // suara tembakan

    private float fireCooldown = 0f;
    private GameObject currentTarget;
    private bool isAnimating = false;

    void Update()
    {
        fireCooldown -= Time.deltaTime;

        if (currentTarget == null || !IsTargetValid(currentTarget))
            currentTarget = GetNewTarget();

        if (currentTarget != null)
        {
            RotateTower(currentTarget.transform);

            if (fireCooldown <= 0f && !isAnimating)
            {
                StartCoroutine(RotateAndShoot());
                fireCooldown = 1f / fireRate;
            }
        }
        else
        {
            if (!isAnimating && catapultArm.localEulerAngles.x != 0f)
                catapultArm.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    bool IsTargetValid(GameObject target)
    {
        if (target == null) return false;
        float dist = Vector3.Distance(transform.position, target.transform.position);
        return dist <= range;
    }

    GameObject GetNewTarget()
    {
        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        GameObject bestTarget = null;
        float shortestDist = Mathf.Infinity;

        foreach (Enemy e in enemies)
        {
            Vector3 targetPos = e.transform.position + new Vector3(0f, 1f, 0f);
            float dist = Vector3.Distance(transform.position, targetPos);
            if (dist <= range && dist < shortestDist)
            {
                shortestDist = dist;
                bestTarget = e.gameObject;
            }
        }
        return bestTarget;
    }

    void RotateTower(Transform target)
    {
        if (towerBase == null) towerBase = transform;

        Vector3 dir = target.position - towerBase.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            towerBase.rotation = Quaternion.Lerp(towerBase.rotation, lookRot, Time.deltaTime * turnSpeed);
        }
    }

    IEnumerator RotateAndShoot()
    {
        isAnimating = true;

        float elapsedUp = 0f;
        float durationUp = armMaxAngle / armRotateSpeed;
        while (elapsedUp < durationUp)
        {
            float t = elapsedUp / durationUp;
            float angle = Mathf.Lerp(0f, armMaxAngle, t);
            catapultArm.localRotation = Quaternion.Euler(angle, 0f, 0f);
            elapsedUp += Time.deltaTime;
            yield return null;
        }
        catapultArm.localRotation = Quaternion.Euler(armMaxAngle, 0f, 0f);

        // Tembak + mainkan suara
        Shoot(currentTarget);

        float elapsedDown = 0f;
        float durationDown = 0.5f;
        while (elapsedDown < durationDown)
        {
            float t = elapsedDown / durationDown;
            float angle = Mathf.Lerp(armMaxAngle, 0f, t);
            catapultArm.localRotation = Quaternion.Euler(angle, 0f, 0f);
            elapsedDown += Time.deltaTime;
            yield return null;
        }

        catapultArm.localRotation = Quaternion.Euler(0f, 0f, 0f);
        isAnimating = false;
    }

    void Shoot(GameObject target)
    {
        if (projectilePrefab == null || firePoints.Length == 0 || target == null) return;

        foreach (Transform fp in firePoints)
        {
            GameObject proj = Instantiate(projectilePrefab, fp.position, Quaternion.identity);
            Projectile p = proj.GetComponent<Projectile>();
            if (p != null)
                p.SetTarget(target.transform, new Vector3(0f, 1f, 0f));
        }

        // Play sound effect sekali tiap tembakan
        if (audioSource != null && shootSFX != null)
            audioSource.PlayOneShot(shootSFX);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
