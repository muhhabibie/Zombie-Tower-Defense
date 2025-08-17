using System;
using System.Collections;
using UnityEngine;

namespace LastBastion.Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        public EnemyController enemyPrefab;
        public Transform mapPlane; // drag objek Plane ke sini di Inspector
        public LayerMask groundLayer; // Layer untuk ground detection
        public int maxEnemyCount = 20; // Batas maksimum musuh
        public Vector2 spawnDelayRange = new Vector2(0.1f, 0.5f); // Rentang delay spawn

        public int ActiveEnemyCount { get; private set; }

        private Vector3 mapCenter;
        private float halfWidth;
        private float halfLength;

        private void Start()
        {
            if (mapPlane == null)
            {
                Debug.LogError("Map Plane belum di-assign di EnemySpawner!");
                return;
            }

            MeshRenderer rend = mapPlane.GetComponent<MeshRenderer>();
            if (rend != null)
            {
                Bounds bounds = rend.bounds;
                mapCenter = bounds.center;
                halfWidth = bounds.size.x / 2f;
                halfLength = bounds.size.z / 2f;
            }
            else
            {
                Debug.LogError("Map Plane tidak punya MeshRenderer!");
            }
        }

        public void SpawnWave(int count, float hpMultiplier, Action onFinishedSpawning)
        {
            StartCoroutine(SpawnRoutine(count, hpMultiplier, onFinishedSpawning));
        }

        private IEnumerator SpawnRoutine(int count, float hpMultiplier, Action onFinishedSpawning)
        {
            if (LastBastion.Core.GameManager.Instance == null || LastBastion.Core.GameManager.Instance.bastion == null)
            {
                Debug.LogError("GameManager atau bastion belum diinisialisasi!");
                yield break;
            }

            if (ActiveEnemyCount + count > maxEnemyCount)
            {
                count = maxEnemyCount - ActiveEnemyCount;
                if (count <= 0) yield break;
            }

            ActiveEnemyCount += count;
            for (int i = 0; i < count; i++)
            {
                float randX = UnityEngine.Random.Range(mapCenter.x - halfWidth, mapCenter.x + halfWidth);
                float randZ = UnityEngine.Random.Range(mapCenter.z - halfLength, mapCenter.z + halfLength);
                Vector3 spawnPos = new Vector3(randX, mapCenter.y + 10f, randZ); // Mulai dari atas

                // Raycast untuk menyesuaikan ketinggian
                RaycastHit hit;
                if (Physics.Raycast(spawnPos, Vector3.down, out hit, 20f, groundLayer))
                {
                    spawnPos.y = hit.point.y;
                }

                var enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                enemy.Initialize(LastBastion.Core.GameManager.Instance.bastion, hpMultiplier);

                yield return new WaitForSeconds(UnityEngine.Random.Range(spawnDelayRange.x, spawnDelayRange.y));
            }
            onFinishedSpawning?.Invoke();
        }

        public void NotifyEnemyKilled(EnemyController e)
        {
            ActiveEnemyCount = Mathf.Max(0, ActiveEnemyCount - 1);
        }
    }
}