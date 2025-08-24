using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static BuffManager;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject normalEnemyPrefab;
    public GameObject fastEnemyPrefab;
    public GameObject tankEnemyPrefab;
    public GameObject debuffEnemyPrefab;

    [Header("Spawner Settings")]
    public MapGenerator3 mapGenerator;
    public float spawnInterval = 1.5f;
    public float waveInterval = 5f;
    public int enemiesPerWave = 5;
    public int wavesPerRound = 3;

    [Header("Countdown UI (optional)")]
    public TextMeshProUGUI countdownTMP;   // boleh inactive di awal
    public int nextRoundCountdown = 15;

    [Header("Buff Choice UI")]
    public BuffChoiceUI buffChoiceUI;      // drag panel BuffChoiceUI (inactive default)

    [Header("Enemy Tag (fallback)")]
    public string enemyTag = "Enemy";

    public static int AliveEnemies = 0;

    int currentWave = 0;
    int currentRound = 1;

    void Start()
    {
        if (countdownTMP != null) countdownTMP.gameObject.SetActive(false);
        StartCoroutine(SpawnRounds());
    }

    IEnumerator SpawnRounds()
    {
        while (true)
        {
            // Update enemy scaling (GLOBAL)
            CurrentEnemyHpMul    = 1f + (currentRound - 1) * 0.10f;
            CurrentEnemySpeedMul = 1f + (currentRound - 1) * 0.05f;
            Debug.Log($"=== Start Round {currentRound} === [HPx{CurrentEnemyHpMul:F2}, SPDx{CurrentEnemySpeedMul:F2}]");

            currentWave = 0;

            // 1) Spawn semua wave ronde ini
            for (int w = 0; w < wavesPerRound; w++)
            {
                currentWave++;
                Debug.Log($"-- Wave {currentWave} (Round {currentRound}) --");
                yield return StartCoroutine(SpawnWave());
                yield return new WaitForSeconds(waveInterval);
            }

            // 2) Tunggu semua musuh habis
            yield return StartCoroutine(WaitUntilEnemiesCleared());

            // 3) Fase pemilihan buff
            yield return StartCoroutine(HandleBuffSelection());

            // 4) Countdown sebelum ronde berikutnya
            yield return StartCoroutine(RoundCountdown(nextRoundCountdown));

            // 5) Naik ronde & scaling jumlah musuh
            currentRound++;
            enemiesPerWave += 2;
        }
    }

    IEnumerator SpawnWave()
    {
        for (int i = 0; i < enemiesPerWave; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        if (mapGenerator == null || mapGenerator.waypoints == null || mapGenerator.waypoints.Count == 0)
        {
            Debug.LogWarning("MapGenerator/waypoints belum diisi.");
            return;
        }

        Vector3 spawnPos = mapGenerator.waypoints[0];
        var prefab = ChooseEnemyType();
        var enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

        if (enemy.GetComponent<EnemyAliveHook>() == null)
            enemy.AddComponent<EnemyAliveHook>();

        var movement = enemy.GetComponent<EnemyMovement>();
        var stats    = enemy.GetComponent<EnemyStats>();

        if (movement != null && stats != null)
        {
            movement.SetPath(mapGenerator.waypoints);

            // gunakan GLOBAL scaling
            stats.maxHP = Mathf.RoundToInt(stats.baseHP * CurrentEnemyHpMul);
            stats.speed = stats.baseSpeed * CurrentEnemySpeedMul;
            movement.speed = stats.speed;
        }
    }

    GameObject ChooseEnemyType()
    {
        if (currentRound <= 5) return normalEnemyPrefab;
        if (currentRound <= 8) return Random.Range(0, 100) < 70 ? normalEnemyPrefab : fastEnemyPrefab;
        if (currentRound <= 12)
        {
            int r = Random.Range(0, 100);
            if (r < 50) return normalEnemyPrefab;
            if (r < 80) return fastEnemyPrefab;
            return tankEnemyPrefab;
        }
        else
        {
            int r = Random.Range(0, 100);
            if (r < 40) return normalEnemyPrefab;
            if (r < 65) return fastEnemyPrefab;
            if (r < 85) return tankEnemyPrefab;
            return debuffEnemyPrefab;
        }
    }

    IEnumerator WaitUntilEnemiesCleared()
    {
        while (true)
        {
            int byHook = AliveEnemies;
            int byTag  = SafeCountByTag(enemyTag);
            if (byHook == 0 && byTag == 0) break;
            yield return null;
        }
        Debug.Log("✔ Semua musuh habis.");
    }

    int SafeCountByTag(string tagName)
    {
        if (string.IsNullOrEmpty(tagName)) return 0;
        try { return GameObject.FindGameObjectsWithTag(tagName).Length; }
        catch { return 0; }
    }

    IEnumerator HandleBuffSelection()
    {
        if (buffChoiceUI == null) yield break;

        // siapkan 3 opsi unik acak
        var all = new List<BuffManager.BuffType>
        {
            BuffManager.BuffType.PlayerDamage,
            BuffManager.BuffType.TurretDamage,
            BuffManager.BuffType.TowerAttackRange,
            BuffManager.BuffType.PlacementRange,
            BuffManager.BuffType.BulletSpeed
        };
        for (int i = 0; i < all.Count; i++) // shuffle
        {
            int r = Random.Range(i, all.Count);
            (all[i], all[r]) = (all[r], all[i]);
        }
        var offer = all.GetRange(0, 3);

        bool picked = false;
        BuffManager.BuffType pickedType = offer[0];
        buffChoiceUI.gameObject.SetActive(true);
        buffChoiceUI.Show(offer, (t) =>
        {
            pickedType = t;
            picked = true;
        });

        while (!picked) yield return null;

        // Terapkan buff (angka bisa kamu atur)
        float val = pickedType switch
        {
            BuffManager.BuffType.PlayerDamage     => 1.20f,
            BuffManager.BuffType.TurretDamage     => 1.20f,
            BuffManager.BuffType.TowerAttackRange => 1.15f,
            BuffManager.BuffType.PlacementRange   => 1.25f,
            BuffManager.BuffType.BulletSpeed      => 1.25f,
            _ => 1.0f
        };
        BuffManager.ApplyBuff(pickedType, val);
        buffChoiceUI.gameObject.SetActive(false);

        // (opsional) refresh tower yang sudah ada kalau kamu punya method khusus
        // var towers = FindObjectsByType<Tower>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        // foreach (var t in towers) t.RecalculateFromBuffs();
    }

    IEnumerator RoundCountdown(int seconds)
    {
        if (countdownTMP == null || seconds <= 0) yield break;

        countdownTMP.gameObject.SetActive(true);
        for (int t = seconds; t > 0; t--)
        {
            countdownTMP.text = $"Next Round in {t}";
            yield return new WaitForSeconds(1f);
        }
        countdownTMP.text = "";
        countdownTMP.gameObject.SetActive(false);
    }
}
