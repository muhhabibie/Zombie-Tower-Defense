using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private List<Vector3> waypoints;
    private int currentWaypointIndex = 0;

    public float speed = 2f;
    private Animator animator;
    private bool isSpawning = true;

    // Lama animasi spawn
    public float spawnAnimationDuration = 1.3f;

    // Offset Y saat spawn
    public float spawnYOffset = 2f;

    private Rigidbody rb;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true; // biar tidak gangguan physics
        }
    }

    public void SetPath(List<Vector3> path)
    {
        waypoints = new List<Vector3>(path);
        currentWaypointIndex = 0;

        if (waypoints.Count > 0)
        {
            // posisi spawn dengan offset Y
            Vector3 spawnPos = waypoints[0];
            spawnPos.y = 0.58f + spawnYOffset;
            transform.position = spawnPos;
        }

        // Play spawn animation
        if (animator != null)
        {
            animator.Play("Spawn_Ground");
        }

        // Start coroutine untuk tunggu animasi selesai
        StartCoroutine(WaitForSpawnAnimation());
    }

    private IEnumerator WaitForSpawnAnimation()
    {
        yield return new WaitForSeconds(spawnAnimationDuration);
        OnSpawnAnimationFinished();
    }

    public void OnSpawnAnimationFinished()
    {
        isSpawning = false;

        if (animator != null)
        {
            animator.Play("Walking_D_Skeletons");
        }
    }

    void Update()
    {
        if (isSpawning) return;
        if (waypoints == null || waypoints.Count == 0) return;
        if (currentWaypointIndex >= waypoints.Count) return;

        // target waypoint (pakai XZ dari waypoint, Y absolute 0.58)
        Vector3 target = waypoints[currentWaypointIndex];
        target.y = 0.58f;

        // arah movement
        Vector3 direction = (target - transform.position).normalized;
        Vector3 move = new Vector3(direction.x, 0, direction.z);

        if (move.sqrMagnitude > 0.001f)
        {
            // update posisi
            transform.position += move * speed * Time.deltaTime;

            // rotasi supaya selalu menghadap arah gerakan
            Quaternion lookRot = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 10f * Time.deltaTime);
        }

        // paksa Y selalu 0.58
        Vector3 pos = transform.position;
        pos.y = 0.58f;
        transform.position = pos;

        // cek apakah sudah sampai waypoint
        if (Vector3.Distance(
                new Vector3(transform.position.x, 0, transform.position.z),
                new Vector3(waypoints[currentWaypointIndex].x, 0, waypoints[currentWaypointIndex].z)) < 0.2f)
        {
            currentWaypointIndex++;

            // jika sudah di waypoint terakhir -> hancurkan musuh
            if (currentWaypointIndex >= waypoints.Count)
            {
                Destroy(gameObject);
            }
        }
    }
}
