using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public AudioClip attackSound;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // contoh tombol serangan
        {
            // logika serangan player
            Debug.Log("Player Attack!");

            // mainkan suara serangan
            AudioManager.instance.PlaySFX(attackSound);
        }
    }
}
