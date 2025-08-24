using System.Collections;
using System.Collections.Generic;
using TMPro; // Pastikan namespace ini ada
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SkillQUse : MonoBehaviour
{
    [Header("Pengaturan Cooldown")]
    [Tooltip("Isi durasi cooldown skill di sini. HARUS SAMA dengan cooldown di Player.")]
    public float skillCooldown = 5f;
    private float nextSkillTime = 0f;

    [Header("Referensi Visual")]
    public Sprite defaultSprite;
    public Sprite selectedSprite;

    [Header("Referensi Komponen")]
    public Image skillIconImage; // Hanya satu image sekarang
    public TextMeshProUGUI cooldownText;
    public List<Button> skillButton;
    void Start()
    {
        // Pastikan UI dalam keadaan siap
        if (skillIconImage != null) skillIconImage.sprite = defaultSprite;
        if (cooldownText != null) cooldownText.gameObject.SetActive(false);
        SetButtonsInteractable(true);
    }

    void Update()
    {
        if (PauseMenu.GameIsPaused)
        {
            // Jika game dijeda, buat semua tombol TIDAK BISA DIKLIK
            SetButtonsInteractable(false);
            return; // Keluar agar input keyboard juga tidak berfungsi
        }
        else
        {
            // Jika game tidak dijeda, buat semua tombol BISA DIKLIK KEMBALI
            SetButtonsInteractable(true);
        }
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

    void SetButtonsInteractable(bool isInteractable)
    {
        // Loop melalui setiap tombol di dalam daftar
        foreach (Button btn in skillButton)
        {
            // Atur status interactable-nya
            btn.interactable = isInteractable;
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