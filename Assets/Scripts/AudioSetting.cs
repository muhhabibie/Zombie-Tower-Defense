using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSetting: MonoBehaviour
{
    public AudioMixer audioMixer; // buat AudioMixer di Unity (Edit → Audio → Audio Mixer)
    public Slider volumeSlider;

    void Start()
    {
        float volume;
        audioMixer.GetFloat("MasterVolume", out volume);
        volumeSlider.value = Mathf.Pow(10, volume / 20f); // convert dB ke linear
    }

    public void SetVolume(float value)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20); // convert linear ke dB
    }
}
