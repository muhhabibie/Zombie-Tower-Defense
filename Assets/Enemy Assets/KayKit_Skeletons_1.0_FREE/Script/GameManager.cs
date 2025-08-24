using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LastBastion.Core
{
    public enum GamePhase
    {
        Preparation,
        Wave,
        Upgrade,
        Defeat
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        public Transform bastion;
        public LastBastion.Player.PlayerController player;
        public LastBastion.UI.HUDController hud;

        [Header("Waves")]
        public int currentWave = 0;
        public GamePhase phase = GamePhase.Preparation;
        public float timeBetweenWaves = 5f;

        [Header("Difficulty")]
        public AnimationCurve enemyCountByWave = AnimationCurve.Linear(1, 5, 20, 40);
        public AnimationCurve enemyHpMultiplierByWave = AnimationCurve.Linear(1, 1, 20, 3);

        public event Action<int> OnWaveStarted;
        public event Action<int> OnWaveCleared;
        public event Action OnDefeat;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Hook UI values
            if (player != null && player.health != null)
            {
                player.health.OnHealthChanged += (cur, max) => hud?.SetPlayerHp(cur, max);
                hud?.SetPlayerHp(player.health.currentHealth, player.health.maxHealth);
            }
            var bastionHealth = bastion != null ? bastion.GetComponent<LastBastion.Structures.BastionHealth>() : null;
            if (bastionHealth != null)
            {
                hud?.SetBastionHp(bastionHealth.currentHealth, bastionHealth.maxHealth);
            }

            OnWaveStarted += (w) => hud?.SetWave(w);

            EnterPreparation();
        }

        private void EnterPreparation()
        {
            phase = GamePhase.Preparation;
            hud?.SetPhase("Prep");
            StartCoroutine(BeginNextWaveRoutine());
        }

        private IEnumerator BeginNextWaveRoutine()
        {
            yield return new WaitForSeconds(timeBetweenWaves);
            // Placeholder untuk logika wave, karena spawner dihapus
            currentWave++;
            OnWaveStarted?.Invoke(currentWave);
            EnterUpgradePhase();
        }

        private void EnterUpgradePhase()
        {
            if (phase == GamePhase.Defeat) return;
            phase = GamePhase.Upgrade;
            hud?.SetPhase("Upgrade");
            // Fase upgrade sekarang kosong (tidak ada pilihan upgrade)
            StartCoroutine(WaitThenPrep());
        }

        private IEnumerator WaitThenPrep()
        {
            yield return new WaitForSeconds(2f);
            EnterPreparation();
        }

        public void TriggerDefeat()
        {
            if (phase == GamePhase.Defeat) return;
            phase = GamePhase.Defeat;
            hud?.SetPhase("Defeat");
            OnDefeat?.Invoke();

            if (GameUIOver.Instance != null)
            {
                GameUIOver.Instance.GameOver();
            }
        }

    }
}