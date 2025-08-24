using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ShopSelector : MonoBehaviour
{
    [Header("Referensi Logika")]
    public TowerPlacement towerPlacement;

    [Header("Referensi Visual")]
    [Tooltip("Sprite untuk latar saat item TIDAK terpilih.")]
    public Sprite defaultSprite; // Untuk rectangle 108

    [Tooltip("Sprite untuk latar saat item SEDANG terpilih.")]
    public Sprite selectedSprite; // Untuk rectangle 107

    [Header("Daftar Item Toko")]
    [Tooltip("Masukkan komponen Image dari setiap latar item secara berurutan.")]
    public List<Image> itemBackgrounds;
    public List<Button> shopButtons;
    public List<TextMeshProUGUI> costTexts;

    private int currentIndex = -1; // -1 berarti tidak ada yang terpilih

    void Start()
    {
        // 1. Reset visual latar belakang
        ResetAllBackgrounds();

        // 2. Hapus seleksi dari memori
        currentIndex = -1;

        // 3. Pastikan semua tombol BISA DIKLIK saat level dimulai
        SetButtonsInteractable(true);

        UpdateShopPrices();
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

    void UpdateShopPrices()
    {
        // Pastikan jumlah teks harga cocok dengan jumlah data harga di TowerPlacement
        if (towerPlacement.towerCosts.Length != costTexts.Count)
        {
            Debug.LogError("Jumlah Teks Harga di UI tidak cocok dengan jumlah Harga Tower di skrip!");
            return;
        }

        // Loop melalui setiap elemen UI dan perbarui teksnya
        for (int i = 0; i < costTexts.Count; i++)
        {
            // Ambil harga dari TowerPlacement dan tampilkan sebagai teks
            costTexts[i].text = towerPlacement.towerCosts[i].ToString();
        }
    }

    void SetButtonsInteractable(bool isInteractable)
    {
        // Loop melalui setiap tombol di dalam daftar
        foreach (Button btn in shopButtons)
        {
            // Atur status interactable-nya
            btn.interactable = isInteractable;
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
