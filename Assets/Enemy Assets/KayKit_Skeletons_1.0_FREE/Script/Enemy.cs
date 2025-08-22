using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
public class Enemy : MonoBehaviour, IDamageable
{
    private EnemyStats stats;
    private int currentHP;

    [Header("Coin Drop")]
    public GameObject coinPrefab;  // prefab koin
    public int coinsToDrop = 1;    // jumlah koin yang dijatuhkan

    void Awake()
    {
        stats = GetComponent<EnemyStats>();
        currentHP = stats.baseHP;
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        DropCoins();
        Destroy(gameObject);
    }

    private void DropCoins()
    {
        if (coinPrefab == null) return;

        for (int i = 0; i < coinsToDrop; i++)
        {
            // spawn koin sedikit acak posisi agar tidak tumpuk
            Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 0.5f, Random.Range(-0.5f, 0.5f));
            Instantiate(coinPrefab, spawnPos, Quaternion.identity);
        }
    }

    public int GetCurrentHP()
    {
        return currentHP;
    }
}
