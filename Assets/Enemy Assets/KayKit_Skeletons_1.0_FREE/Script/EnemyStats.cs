using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public int baseHP = 100;
    public float baseSpeed = 1f;

    [HideInInspector] public int maxHP;
    [HideInInspector] public float speed;

    void Start()
    {
        // Kalau lupa di-set oleh spawner
        if (maxHP == 0) maxHP = baseHP;
        if (speed == 0) speed = baseSpeed;
    }
}
