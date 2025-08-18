using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

            for (int w = 0; w < wavesPerRound; w++)
            {
                currentWave++;
                Debug.Log($"-- Wave {currentWave} (Round {currentRound}) --");

                yield return StartCoroutine(SpawnWave());
                yield return new WaitForSeconds(waveInterval);
            }

            currentRound++;
            enemiesPerWave += 2; // scaling jumlah musuh tiap ronde
            yield return new WaitForSeconds(10f); // jeda antar ronde
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
        if (mapGenerator == null || mapGenerator.waypoints == null || mapGenerator.waypoints.Count == 0) return;

        // ambil posisi spawn = waypoint pertama
        Vector3 spawnPos = mapGenerator.waypoints[0];

        // pilih prefab musuh berdasarkan ronde
        GameObject prefabToSpawn = ChooseEnemyType();

        GameObject enemy = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

        // set path ke EnemyMovement
        EnemyMovement movement = enemy.GetComponent<EnemyMovement>();
        EnemyStats stats = enemy.GetComponent<EnemyStats>();

        if (movement != null && stats != null)
        {
            movement.SetPath(mapGenerator.waypoints);

            // scaling stat
            float hpMultiplier = 1f + (currentRound - 1) * 0.1f;     // +10% HP per ronde
            float speedMultiplier = 1f + (currentRound - 1) * 0.05f; // +5% speed per ronde

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
}
