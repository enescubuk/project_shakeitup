using UnityEngine;
using UnityEngine.Audio;
using DG.Tweening;

public class SoundManager : MonoBehaviour
{
    public AudioMixer audioMixer;

    public static SoundManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        OpenBass();
        CloseDrums();
        CloseMusic();
    }

    public void UpdateSound(float value, float drumThreshold, float musicThreshold)
    {
        if (value >= drumThreshold)
            OpenDrums();
        else
            CloseDrums();

        if (value >= musicThreshold)
            OpenMusic();
        else
            CloseMusic();
    }

    public void OpenBass()
    {
        SmoothSetFloat("bass", 0);
    }

    public void CloseBass()
    {
        SmoothSetFloat("bass", -80);
    }

    public void OpenDrums()
    {
        SmoothSetFloat("drums", 0);
    }

    public void CloseDrums()
    {
        SmoothSetFloat("drums", -80);
    }

    public void OpenMusic()
    {
        SmoothSetFloat("music", 0);
    }

    public void CloseMusic()
    {
        SmoothSetFloat("music", -80);
    }

    private void SmoothSetFloat(string parameter, float targetValue, float duration = 1f)
    {
        float currentValue;
        audioMixer.GetFloat(parameter, out currentValue);
        DOTween.To(() => currentValue, x => audioMixer.SetFloat(parameter, x), targetValue, duration).SetEase(Ease.OutQuad);
    }
}
