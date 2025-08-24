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
    }

    public ChoiceWidget[] choices = new ChoiceWidget[3];

    Action<BuffType> onPicked;
    List<BuffType> current;

    void Awake()
    {
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
            if (active && choices[i].label != null)
                choices[i].label.text = PrettyName(options[i]);
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
