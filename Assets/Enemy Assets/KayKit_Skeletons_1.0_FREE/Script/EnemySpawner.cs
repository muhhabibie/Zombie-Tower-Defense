using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // untuk Text UI (opsional)

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject normalEnemyPrefab;
    public GameObject fastEnemyPrefab;
    public GameObject tankEnemyPrefab;
    public GameObject debuffEnemyPrefab;

    [Header("Spawner Settings")]
    public MapGenerator3 mapGenerator;
    public float spawnInterval = 1.5f; // jeda antar musuh
    public float waveInterval = 5f;    // jeda antar wave
    public int enemiesPerWave = 5;     // jumlah musuh per wave
    public int wavesPerRound = 3;      // jumlah wave per ronde

    [Header("Round Countdown (optional)")]
    public Text countdownText;         // drag-kan UI Text bila ingin tampilkan hitung mundur
    public int nextRoundCountdown = 15;

    [Header("Enemy Tag (fallback check)")]
    public string enemyTag = "Enemy";  // pastikan prefab musuh bertag ini (untuk fallback check)

    // Counter global jumlah musuh aktif (diisi oleh EnemyAliveHook)
    public static int AliveEnemies = 0;

    private int currentWave = 0;
    private int currentRound = 1;

    void Start()
    {
        StartCoroutine(SpawnRounds());
    }

    IEnumerator SpawnRounds()
    {
        while (true) // endless
        {
            Debug.Log($"=== Start Round {currentRound} ===");
            currentWave = 0;

            // 1) Spawn semua wave di ronde ini
            for (int w = 0; w < wavesPerRound; w++)
            {
                currentWave++;
                Debug.Log($"-- Wave {currentWave} (Round {currentRound}) --");

                yield return StartCoroutine(SpawnWave());
                yield return new WaitForSeconds(waveInterval);
            }

            // 2) Tunggu sampai semua musuh di ronde ini habis
            yield return StartCoroutine(WaitUntilEnemiesCleared());

            // 3) Countdown sebelum ronde berikutnya
            yield return StartCoroutine(RoundCountdown(nextRoundCountdown));

            // 4) Naikkan ronde & scaling
            currentRound++;
            enemiesPerWave += 2; // scaling jumlah musuh tiap ronde
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

        // posisi spawn = waypoint pertama
        Vector3 spawnPos = mapGenerator.waypoints[0];

        // pilih prefab musuh berdasarkan ronde
        GameObject prefabToSpawn = ChooseEnemyType();

        GameObject enemy = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

        // pastikan hook ada (kalau belum ditempel manual, bisa add otomatis)
        if (enemy.GetComponent<EnemyAliveHook>() == null)
        {
            enemy.AddComponent<EnemyAliveHook>();
        }

        // set path & scaling stat
        EnemyMovement movement = enemy.GetComponent<EnemyMovement>();
        EnemyStats stats = enemy.GetComponent<EnemyStats>();

        if (movement != null && stats != null)
        {
            movement.SetPath(mapGenerator.waypoints);

            // scaling stat
            float hpMultiplier = 1f + (currentRound - 1) * 0.10f;     // +10% HP per ronde
            float speedMultiplier = 1f + (currentRound - 1) * 0.05f;  // +5% speed per ronde

            stats.maxHP = Mathf.RoundToInt(stats.baseHP * hpMultiplier);
            stats.speed = stats.baseSpeed * speedMultiplier;

            // implementasikan speed ke movement
            movement.speed = stats.speed;
        }
    }

    GameObject ChooseEnemyType()
    {
        // Ronde 1 - 5: hanya normal
        if (currentRound >= 1 && currentRound <= 5)
        {
            return normalEnemyPrefab;
        }
        // Ronde 6 - 8: normal + fast
        else if (currentRound >= 6 && currentRound <= 8)
        {
            int rand = Random.Range(0, 100);
            return (rand < 70) ? normalEnemyPrefab : fastEnemyPrefab; // 70% normal, 30% fast
        }
        // Ronde 9 - 12: normal + fast + tank
        else if (currentRound >= 9 && currentRound <= 12)
        {
            int rand = Random.Range(0, 100);
            if (rand < 50) return normalEnemyPrefab;
            else if (rand < 80) return fastEnemyPrefab;
            else return tankEnemyPrefab;
        }
        // Ronde 13+: semua tipe musuh
        else
        {
            int rand = Random.Range(0, 100);
            if (rand < 40) return normalEnemyPrefab;       // 40% normal
            else if (rand < 65) return fastEnemyPrefab;    // 25% fast
            else if (rand < 85) return tankEnemyPrefab;    // 20% tank
            else return debuffEnemyPrefab;                 // 15% debuff
        }
    }

    IEnumerator WaitUntilEnemiesCleared()
    {
        // Tunggu sampai counter nol; fallback juga cek jumlah dengan tag (kalau ada musuh tanpa hook)
        while (true)
        {
            int aliveByHook = AliveEnemies;
            int aliveByTag = CountByTag(enemyTag);

            // dianggap habis jika keduanya nol (atau salah satunya nol & lainnya tidak bertambah)
            if (aliveByHook == 0 && aliveByTag == 0)
                break;

            yield return null;
        }

        Debug.Log("✔ Semua musuh di ronde ini sudah habis.");
    }

    int CountByTag(string tagName)
    {
        if (string.IsNullOrEmpty(tagName)) return 0;
        // Hati-hati, ini lebih mahal; hanya fallback
        return GameObject.FindGameObjectsWithTag(tagName).Length;
    }

    IEnumerator RoundCountdown(int seconds)
    {
        if (seconds <= 0) yield break;

        for (int t = seconds; t > 0; t--)
        {
            if (countdownText != null)
                countdownText.text = $"Next Round in {t}s";
            else
                Debug.Log($"Next Round in {t}s");

            yield return new WaitForSeconds(1f);
        }

        if (countdownText != null)
            countdownText.text = ""; // bersihkan teks setelah countdown
    }
}
