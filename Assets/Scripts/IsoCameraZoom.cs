using UnityEngine;

[RequireComponent(typeof(Camera))]
public class IsoCameraZoom : MonoBehaviour
{
    public Camera cam;
    public float zoomSpeed = 5f;
    public float minZoom = 5f;

    private float maxZoom; // batas zoom-out = posisi awal kamera

    private void Start()
    {
        if (cam == null) cam = Camera.main;

        // Simpan orthographicSize awal sebagai maxZoom
        maxZoom = cam.orthographicSize;
    }

    void Update()
    {
        // Scroll wheel (Input System)
        float scroll = UnityEngine.InputSystem.Mouse.current.scroll.ReadValue().y;

        // Update ukuran kamera
        cam.orthographicSize -= scroll * zoomSpeed * Time.deltaTime;

        // Clamp biar tidak lebih kecil/lebih besar dari batas
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
    }
}
