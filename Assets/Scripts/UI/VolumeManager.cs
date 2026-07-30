using UnityEngine;
using UnityEngine.UI;

public class VolumeManager : MonoBehaviour
{
    public Slider volumeSlider;

    void Start()
    {
        volumeSlider.value = AudioListener.volume;
    }

    public void ChangeVolume()
    {
        AudioListener.volume = volumeSlider.value;
    }
}