using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SkillQUse : MonoBehaviour
{
    [Header("Referensi Visual")]
    public Sprite defaultSprite;    // Sprite saat skill siap (akan menjadi pengisi)
    public Sprite selectedSprite;   // Sprite saat skill cooldown (akan menjadi latar)

    [Header("Komponen UI")]
    public Image backgroundImage;  // Komponen Image dari objek utama (Skill_Q_Icon)
    public Image fillImage;        // Komponen Image dari objek anak (Fill_Image)

    [Header("Pengaturan Cooldown")]
    [Tooltip("Isi durasi cooldown skill di sini. HARUS SAMA dengan cooldown di Player.")]
    public float skillCooldown = 5f;
    private float nextSkillTime = 0f;

    void Start()
    {
        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
            {
                Debug.Log("Tombol Q Ditekan!"); // Penanda 1: Cek input

                if (Time.time >= nextSkillTime)
                {
                    Debug.Log("Cooldown Selesai! Memulai efek visual."); // Penanda 2: Cek timer

                    nextSkillTime = Time.time + skillCooldown;
                    StartCoroutine(CooldownRoutine(skillCooldown));
                }
                else
                {
                    Debug.Log("Skill masih dalam cooldown."); // Penanda 3: Jika masih cooldown
                }
            }
        }
    }

    void Update()
    {
        // Cek input menggunakan Input System yang baru
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
        // 1. Atur keadaan cooldown
        backgroundImage.sprite = selectedSprite; // Latar berubah menjadi sprite cooldown
        fillImage.sprite = defaultSprite;       // Gambar pengisi adalah sprite 'siap'
        fillImage.fillAmount = 0;              // Mulai mengisi dari nol

        // 2. Jalankan timer visual
        float timer = 0f;
        while (timer < duration)
        {
            fillImage.fillAmount = timer / duration; // Isi dari 0 ke 1 seiring waktu
            timer += Time.deltaTime;
            yield return null;
        }

        // 3. Kembalikan ke keadaan siap
        fillImage.fillAmount = 0; // Sembunyikan lagi gambar pengisi
        backgroundImage.sprite = defaultSprite; // Latar kembali normal
    }
}