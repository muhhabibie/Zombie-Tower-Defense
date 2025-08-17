using UnityEngine;
using UnityEngine.UI;

namespace LastBastion.UI
{
    public class HUDController : MonoBehaviour
    {
        public Text phaseText;
        public Slider playerHp;
        public Slider bastionHp;
        public Text waveText;

        public void SetPhase(string phase)
        {
            if (phaseText != null) phaseText.text = phase;
        }

        public void SetWave(int wave)
        {
            if (waveText != null) waveText.text = $"Wave {wave}";
        }

        public void SetPlayerHp(int current, int max)
        {
            if (playerHp == null) return;
            playerHp.maxValue = max;
            playerHp.value = current;
        }

        public void SetBastionHp(int current, int max)
        {
            if (bastionHp == null) return;
            bastionHp.maxValue = max;
            bastionHp.value = current;
        }
    }
}
