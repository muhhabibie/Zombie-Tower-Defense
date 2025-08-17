using UnityEngine;

public class IsoCameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset; // Biarkan diatur di Inspector
    public float smoothSpeed = 0.125f; // Kecepatan smooth follow
    public Vector3 rotation = new Vector3(45f, 45f, 0f); // Sudut isometrik

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("Target untuk IsoCameraFollow belum diatur!", gameObject);
        }
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // Smooth follow
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;

            // Atur rotasi kamera untuk sudut isometrik
            transform.rotation = Quaternion.Euler(rotation);
        }
    }
}