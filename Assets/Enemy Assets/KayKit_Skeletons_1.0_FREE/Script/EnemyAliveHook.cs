// EnemyAliveHook.cs
using UnityEngine;

public class EnemyAliveHook : MonoBehaviour
{
    void OnEnable()  { EnemySpawner.AliveEnemies++; }
    void OnDisable() { EnemySpawner.AliveEnemies--; }
}
