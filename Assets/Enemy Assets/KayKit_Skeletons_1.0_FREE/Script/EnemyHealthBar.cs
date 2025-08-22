using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Slider slider;
    public Enemy enemy; // reference ke enemy

    void Start()
    {
        if (enemy != null)
        {
            slider.maxValue = enemy.GetComponent<EnemyStats>().maxHP;
            slider.value = enemy.GetCurrentHP();
        }
    }

    void Update()
    {
        if (enemy != null && slider != null)
        {
            slider.value = enemy.GetCurrentHP();
            Debug.Log($"Slider Value: {slider.value}, Enemy HP: {enemy.GetCurrentHP()}");
        }
        if (enemy != null)
        {
            slider.value = enemy.GetCurrentHP();

            // biar health bar selalu menghadap camera
            Camera cam = Camera.main;
            if (cam != null)
            {
                transform.LookAt(transform.position + cam.transform.forward);
            }
        }
    }
}
