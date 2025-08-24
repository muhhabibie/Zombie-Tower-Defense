using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using TMPro; // Pastikan namespace ini ada

public class SkillQUse : MonoBehaviour
{
    [Header("Pengaturan Cooldown")]
    [Tooltip("Isi durasi cooldown skill di sini. HARUS SAMA dengan cooldown di Player.")]
    public float skillCooldown = 5f;
    private float nextSkillTime = 0f;

    [Header("Referensi Visual")]
    public Sprite defaultSprite;
    public Sprite selectedSprite;
    public Image skillIconImage; // Hanya satu image sekarang
    public TextMeshProUGUI cooldownText;

    void Start()
    {
        // Pastikan UI dalam keadaan siap
        if (skillIconImage != null) skillIconImage.sprite = defaultSprite;
        if (cooldownText != null) cooldownText.gameObject.SetActive(false);
    }

    void Update()
    {
        // Cek input keyboard untuk tombol 'Q'
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            // Cek apakah skill sudah siap digunakan (tidak sedang cooldown)
            if (Time.time >= nextSkillTime)
            {
                // Jika ya, mulai cooldown
                nextSkillTime = Time.time + skillCooldown;
                StartCoroutine(CooldownRoutine(skillCooldown));
            }
        }
    }

    private IEnumerator CooldownRoutine(float duration)
    {
        // 1. Ubah sprite & tampilkan teks
        skillIconImage.sprite = selectedSprite;
        cooldownText.gameObject.SetActive(true);

        float timer = duration;
        while (timer > 0)
        {
            // Update teks hitung mundur
            cooldownText.text = Mathf.Ceil(timer).ToString();
            timer -= Time.deltaTime;
            yield return null; // Tunggu frame berikutnya
        }

        // 2. Kembalikan ke status siap
        cooldownText.gameObject.SetActive(false);
        skillIconImage.sprite = defaultSprite;
    }
}