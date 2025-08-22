using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class TowerPlacement : MonoBehaviour
{
    [Header("Tower Settings")]
    public GameObject[] towerPrefabs;
    public float placementRange = 5f; // jarak maksimal dari player
    public LayerMask groundMask;

    private int selected = -1;
    private Transform player;

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
        Debug.Log("✅ Player assigned to TowerPlacement: " + player.name);
    }

    IEnumerator Start()
    {
        // tunggu sampai player muncul (karena spawn belakangan)
        while (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                player = p.transform;
                Debug.Log("✅ Player ditemukan di: " + player.position);
            }
            yield return null;
        }

        if (towerPrefabs.Length == 0)
        {
            Debug.LogWarning("⚠️ Tower prefabs belum diisi di Inspector.");
        }
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.digit1Key.wasPressedThisFrame) { selected = 0; Debug.Log("✅ Tower 1 selected"); }
            if (kb.digit2Key.wasPressedThisFrame) { selected = 1; Debug.Log("✅ Tower 2 selected"); }
            if (kb.digit3Key.wasPressedThisFrame) { selected = 2; Debug.Log("✅ Tower 3 selected"); }
            if (kb.digit4Key.wasPressedThisFrame) { selected = 3; Debug.Log("✅ Tower 4 selected"); }
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryPlace();
        }
    }

    void TryPlace()
    {
        if (selected < 0 || selected >= towerPrefabs.Length)
        {
            Debug.LogWarning("⚠️ No tower selected!");
            return;
        }

        if (Camera.main == null)
        {
            Debug.LogError("❌ Tidak ada kamera dengan tag 'MainCamera'");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 200f, groundMask))
        {
            if (!hit.collider.CompareTag("SnowTile"))
            {
                Debug.LogWarning("❌ Not a SnowTile (hit tag: " + hit.collider.tag + ")");
                return;
            }

            SnowTile tile = hit.collider.GetComponent<SnowTile>();
            if (tile == null)
            {
                Debug.LogWarning("❌ SnowTile component tidak ditemukan!");
                return;
            }

            if (tile.isOccupied)
            {
                Debug.LogWarning("❌ Tile sudah ditempati tower!");
                return;
            }

            float dist = Vector3.Distance(player.position, hit.point);
            if (dist > placementRange)
            {
                Debug.LogWarning("❌ Too far from player. Distance: " + dist);
                return;
            }

            // Ambil posisi tile
            Vector3 spawnPos = hit.collider.bounds.center;
            spawnPos.y += hit.collider.bounds.extents.y;

            // Ambil prefab tower
            GameObject towerPrefab = towerPrefabs[selected];
            Collider towerCol = towerPrefab.GetComponent<Collider>();
            if (towerCol != null)
            {
                spawnPos.y += towerCol.bounds.extents.y; // geser setengah tinggi tower
            }
            else
            {
                spawnPos.y += towerPrefab.transform.localScale.y / 2f;
            }

            // Spawn tower
            Instantiate(towerPrefab, spawnPos, Quaternion.identity);
            tile.isOccupied = true; // tandai tile sudah dipakai
            Debug.Log("🏰 Tower placed at " + spawnPos);
        }
        else
        {
            Debug.LogWarning("❌ Raycast miss");
        }
    }
}
