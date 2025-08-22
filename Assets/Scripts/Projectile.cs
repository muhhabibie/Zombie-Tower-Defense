using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 20;

    private Transform target;
    private Vector3 targetOffset = new Vector3(0f, 1f, 0f); // offset sasaran

    public void SetTarget(Transform newTarget, Vector3 offset)
    {
        target = newTarget;
        targetOffset = offset;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Posisi target + offset (biar nembak badan/kepala, bukan kaki)
        Vector3 targetPos = target.position + targetOffset;
        Vector3 dir = targetPos - transform.position;
        float step = speed * Time.deltaTime;

        // 🎯 Rotasi projectile agar selalu menghadap target
        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }

        if (dir.magnitude <= step)
        {
            HitTarget();
        }
        else
        {
            transform.Translate(dir.normalized * step, Space.World);
        }
    }

    void HitTarget()
    {
        Enemy e = target.GetComponent<Enemy>();
        if (e != null)
        {
            e.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}
