using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class EnemyDebuffAura : MonoBehaviour
{
    public float radius = 4f;
    [Range(0f,1f)] public float slowMultiplier = 0.2f; // 80% slow
    public LayerMask affectedLayers = ~0; // default: semuanya (pastikan Default dicentang)

    private readonly HashSet<DebuffReceiver> inside = new();
    SphereCollider col;

    void Awake()
    {
        col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = radius;

        // pastikan ada rigidbody supaya OnTrigger* jalan
        if (!TryGetComponent<Rigidbody>(out var rb))
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    void OnValidate()
    {
        var c = GetComponent<SphereCollider>();
        if (c) { c.isTrigger = true; c.radius = radius; }
    }

    void OnTriggerEnter(Collider other)
    {
        var r = other.GetComponentInParent<DebuffReceiver>();
        if (r == null) return;
        if (affectedLayers.value != 0 &&
            (affectedLayers.value & (1 << other.gameObject.layer)) == 0) return;
        Debug.Log(r);
        if (inside.Add(r)) r.ApplySlow(this, slowMultiplier);
    }

    void OnTriggerExit(Collider other)
    {
        var r = other.GetComponentInParent<DebuffReceiver>();
        if (r != null && inside.Remove(r)) r.RemoveSlow(this);
    }

    // Fallback: scan manual (kalau ingin, bisa dimatikan)
    void FixedUpdate()
    {
        var hits = Physics.OverlapSphere(transform.position, radius, affectedLayers, QueryTriggerInteraction.Ignore);
        var curr = new HashSet<DebuffReceiver>();
        foreach (var h in hits)
        {
            var r = h.GetComponentInParent<DebuffReceiver>();
            if (!r) continue;
            curr.Add(r);
            if (inside.Add(r)) r.ApplySlow(this, slowMultiplier);
        }
        // remove yg keluar
        foreach (var r in new List<DebuffReceiver>(inside))
            if (!curr.Contains(r)) { r.RemoveSlow(this); inside.Remove(r); }
    }

    void OnDisable()
    {
        foreach (var r in inside) if (r) r.RemoveSlow(this);
        inside.Clear();
    }
}
