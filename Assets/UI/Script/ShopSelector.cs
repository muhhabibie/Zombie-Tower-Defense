using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class ShopSelector : MonoBehaviour
{
    [Header("Referensi Visual")]
    [Tooltip("Sprite untuk latar saat item TIDAK terpilih.")]
    public Sprite defaultSprite; // Untuk rectangle 108

    [Tooltip("Sprite untuk latar saat item SEDANG terpilih.")]
    public Sprite selectedSprite; // Untuk rectangle 107

    [Header("Daftar Item Toko")]
    [Tooltip("Masukkan komponen Image dari setiap latar item secara berurutan.")]
    public List<Image> itemBackgrounds;

    private int currentIndex = -1; // -1 berarti tidak ada yang terpilih

    void Start()
    {
        // Pastikan semua item mulai dengan latar default
        ResetAllBackgrounds();
    }

    void Update()
    {
        // Pastikan ada keyboard yang terhubung
        if (Keyboard.current == null) return;

        // Menggunakan perintah dari Input System yang baru
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            SelectItem(0); // Pilih Cannon
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            SelectItem(1); // Pilih Ballista
        }
        else if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            SelectItem(2); // Pilih Catapult
        }
        else if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            SelectItem(3); // Pilih Turret
        }
    }

    // Fungsi utama untuk memilih item
    public void SelectItem(int index)
    {
        // Jika sudah terpilih, tidak perlu melakukan apa-apa
        if (currentIndex == index) return;

        // 1. Kembalikan item yang sebelumnya terpilih ke latar default
        if (currentIndex != -1)
        {
            itemBackgrounds[currentIndex].sprite = defaultSprite;
        }

        // 2. Ubah latar item yang baru terpilih menjadi 'selected'
        itemBackgrounds[index].sprite = selectedSprite;

        // 3. Simpan index item yang baru terpilih
        currentIndex = index;

        Debug.Log(itemBackgrounds[index].gameObject.name + " dipilih.");
    }

    // Fungsi untuk memastikan semua latar kembali ke default
    private void ResetAllBackgrounds()
    {
        foreach (var bg in itemBackgrounds)
        {
            bg.sprite = defaultSprite;
        }
    }
}
