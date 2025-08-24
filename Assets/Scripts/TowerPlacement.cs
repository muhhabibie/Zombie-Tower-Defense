using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class TowerPlacement : MonoBehaviour
{
    [Header("Tower Settings")]
    public GameObject[] towerPrefabs;
    public int[] towerCosts;
    public float placementRange = 5f; // jarak maksimal dari player
    public LayerMask groundMask;

    private int selected = -1;
    private Transform player;
    private LastBastion.Player.PlayerController playerController;
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
                playerController = p.GetComponent<LastBastion.Player.PlayerController>();
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

    public void SelectTower(int towerIndex)
    {
        // Cek apakah index valid sesuai dengan jumlah tower prefab Anda
        if (towerIndex >= 0 && towerIndex < towerPrefabs.Length)
        {
            selected = towerIndex;
            Debug.Log($"✅ Tower {towerIndex + 1} dipilih melalui tombol UI.");
        }
        else
        {
            Debug.LogWarning($"⚠️ Indeks tower {towerIndex} tidak valid!");
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
            if (dist > placementRange * BuffManager.PlacementRangeMul)
            {
                Debug.LogWarning("❌ Too far from player. Distance: " + dist);
                return;
            }

            // 1. Ambil harga tower yang dipilih
            int cost = towerCosts[selected];

            // 2. Cek apakah pemain punya cukup koin
            if (playerController.coinCount < cost)
            {
                Debug.LogWarning($"❌ Koin tidak cukup! Butuh {cost}, hanya punya {playerController.coinCount}");
                return; // Batalkan penempatan
            }

            // 3. Jika koin cukup, kurangi koin pemain
            playerController.SpendCoins(cost); // Kita akan buat fungsi ini di PlayerController

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
