using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static BuffManager;

public class BuffChoiceUI : MonoBehaviour
{
    [Serializable] public class ChoiceWidget
    {
        public Button button;
        public TextMeshProUGUI label;
        public Image backgroundImage;
    }

    [System.Serializable]
    public class BuffVisual
    {
        public BuffType buffType;
        public Sprite backgroundSprite;
    }

    public ChoiceWidget[] choices = new ChoiceWidget[3];

    Action<BuffType> onPicked;
    List<BuffType> current;
    public List<BuffVisual> buffVisuals;
    private Dictionary<BuffType, Sprite> buffVisualsDictionary;
    void Awake()
    {
        buffVisualsDictionary = new Dictionary<BuffType, Sprite>();
        foreach (var visual in buffVisuals)
        {
            if (!buffVisualsDictionary.ContainsKey(visual.buffType))
            {
                buffVisualsDictionary.Add(visual.buffType, visual.backgroundSprite);
            }
        }
        for (int i = 0; i < choices.Length; i++)
        {
            int idx = i;
            if (choices[i]?.button != null)
                choices[i].button.onClick.AddListener(() => Pick(idx));
        }
        gameObject.SetActive(false);
    }

    public void Show(List<BuffType> options, Action<BuffType> onPicked)
    {
        current = options;
        this.onPicked = onPicked;

        for (int i = 0; i < choices.Length; i++)
        {
            bool active = i < options.Count;
            choices[i].button.gameObject.SetActive(active);
            if (active)
            {
                BuffType currentBuff = options[i];

                // Set Teks (seperti sebelumnya)
                if (choices[i].label != null)
                    choices[i].label.text = PrettyName(currentBuff);

                // --- LOGIKA BARU: Set Background ---
                if (choices[i].backgroundImage != null && buffVisualsDictionary.ContainsKey(currentBuff))
                {
                    // Ambil sprite dari dictionary dan terapkan ke background
                    choices[i].backgroundImage.sprite = buffVisualsDictionary[currentBuff];
                }
            }
        }
        gameObject.SetActive(true);
    }

    void Pick(int idx)
    {
        if (current == null || idx < 0 || idx >= current.Count) return;
        var picked = current[idx];
        gameObject.SetActive(false);
        onPicked?.Invoke(picked);
    }

    string PrettyName(BuffType t) => t switch
    {
        BuffType.PlayerDamage     => "Player Damage +20%",
        BuffType.TurretDamage     => "Turret Damage +20%",
        BuffType.TowerAttackRange => "Tower Range +15%",
        BuffType.PlacementRange   => "Placement Range +25%",
        BuffType.BulletSpeed      => "Bullet Speed +25%",
        _ => t.ToString()
    };
}
