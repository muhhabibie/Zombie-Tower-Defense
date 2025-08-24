using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro; // untuk UI text (TextMeshPro)

public class TowerPlacement : MonoBehaviour
{
    [Header("Tower Settings")]
    public GameObject[] towerPrefabs;
    public int[] towerCosts;
    public float placementRange = 5f;
    public LayerMask groundMask;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip placeTowerSfx;
    public AudioClip notEnoughCoinsSfx;  // 🔊 dipakai juga untuk gagal jarak

    [Header("UI Feedback")]
    public TextMeshProUGUI warningText;  // 🔤 text untuk notif
    public float warningDuration = 2f;   // lama munculnya text

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
        // cari player dulu
        while (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                player = p.transform;
                playerController = p.GetComponent<LastBastion.Player.PlayerController>();
                Debug.Log("✅ Player ditemukan di: " + player.position);
            }
            yield return null;
        }

        if (towerPrefabs.Length == 0)
        {
            Debug.LogWarning("⚠️ Tower prefabs belum diisi di Inspector.");
        }

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (warningText != null)
            warningText.gameObject.SetActive(false); // sembunyikan awal
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
            if (!hit.collider.CompareTag("SnowTile")) return;

            SnowTile tile = hit.collider.GetComponent<SnowTile>();
            if (tile == null || tile.isOccupied) return;

            // 🔹 cek jarak
            float dist = Vector3.Distance(player.position, hit.point);
            if (dist > placementRange * BuffManager.PlacementRangeMul)
            {
                Debug.LogWarning($"❌ Terlalu jauh! Distance: {dist}");

                if (notEnoughCoinsSfx != null && audioSource != null)
                    audioSource.PlayOneShot(notEnoughCoinsSfx);

                if (warningText != null)
                    StartCoroutine(ShowWarning("Terlalu jauh untuk menempatkan tower!"));

                return;
            }

            int cost = towerCosts[selected];

            // 🔹 cek koin
            if (playerController.coinCount < cost)
            {
                Debug.LogWarning($"❌ Koin tidak cukup! Butuh {cost}, hanya punya {playerController.coinCount}");

                if (notEnoughCoinsSfx != null && audioSource != null)
                    audioSource.PlayOneShot(notEnoughCoinsSfx);

                if (warningText != null)
                    StartCoroutine(ShowWarning("Koin tidak cukup!"));

                return;
            }

            // ✅ jika cukup koin dan jarak oke → tempatkan tower
            playerController.SpendCoins(cost);

            Vector3 spawnPos = hit.collider.bounds.center;
            spawnPos.y += hit.collider.bounds.extents.y;

            GameObject towerPrefab = towerPrefabs[selected];
            Collider towerCol = towerPrefab.GetComponent<Collider>();
            spawnPos.y += towerCol ? towerCol.bounds.extents.y : towerPrefab.transform.localScale.y / 2f;

            Instantiate(towerPrefab, spawnPos, Quaternion.identity);
            tile.isOccupied = true;
            Debug.Log("🏰 Tower placed at " + spawnPos);

            if (placeTowerSfx != null && audioSource != null)
                audioSource.PlayOneShot(placeTowerSfx);
        }
    }

    IEnumerator ShowWarning(string msg)
    {
        warningText.text = msg;
        warningText.gameObject.SetActive(true);
        yield return new WaitForSeconds(warningDuration);
        warningText.gameObject.SetActive(false);
    }
}
    