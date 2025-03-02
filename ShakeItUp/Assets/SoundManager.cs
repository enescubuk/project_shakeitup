using UnityEngine;
using UnityEngine.Audio;
using DG.Tweening;
using TMPro;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public AudioMixer audioMixer;
    public static SoundManager Instance;
    public TMP_Text scoreText;

    private float scoreCounter;
    private int currentStage = 1; // Başlangıçta Stage 1

    private Color normalColor = Color.white;
    private Color stage2Color = new Color(1f, 0.85f, 0.4f); // Açık sarı
    private Color stage3Color = Color.red;

    private Coroutine shakeRoutine; // Shake için coroutine

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        CloseMusic(); // Önce müziği kapat
        CloseDrums(); // Sonra davulları kapat
        currentStage = 1; // Stage'i elle 1 olarak ayarla
        UpdateScoreVisual(); // Başlangıçtaki rengi beyaz olarak ayarla
    }

    public void UpdateSound(float value, float drumThreshold, float musicThreshold)
    {
        if (value >= musicThreshold)
        {
            OpenMusic();
        }
        else
        {
            CloseMusic();
        }

        if (value >= drumThreshold)
        {
            OpenDrums();
        }
        else
        {
            CloseDrums();
        }
    }

    public void OpenBass() // Stage 1 (her zaman açık)
    {
        SmoothSetFloat("bass", 0);
        currentStage = 1;
        UpdateScoreVisual();
    }

    public void OpenDrums() // Stage 2
    {
        SmoothSetFloat("drums", 0);
        currentStage = 2;
        UpdateScoreVisual();
    }

    public void CloseDrums()
    {
        SmoothSetFloat("drums", -80);
        currentStage = 1; // Drums kapanırsa Stage 1'e düşüyoruz
        UpdateScoreVisual();
    }

    public void OpenMusic() // Stage 3
    {
        SmoothSetFloat("music", 0);
        currentStage = 3;
        UpdateScoreVisual();
    }

    public void CloseMusic()
    {
        SmoothSetFloat("music", -80);

        // Eğer zaten Stage 1'deysek, Stage 2'ye zorlamayalım
        if (currentStage != 1)
        {
            currentStage = 2;
        }
        
        UpdateScoreVisual();
    }

    private void SmoothSetFloat(string parameter, float targetValue, float duration = 1f)
    {
        float currentValue;
        audioMixer.GetFloat(parameter, out currentValue);
        DOTween.To(() => currentValue, x => audioMixer.SetFloat(parameter, x), targetValue, duration).SetEase(Ease.OutQuad);
    }

    internal void Score(float speed, int streak)
    {
        float newScore = 10 * speed * streak;

        if (newScore > scoreCounter)
        {
            AnimateScoreIncrease();
        }

        scoreCounter = newScore;
        scoreText.text = scoreCounter.ToString("N0"); // Noktalı sayı formatı
    }

    private void AnimateScoreIncrease()
    {

        // Her stage için farklı animasyonlar
        if (currentStage == 1)
        {
            scoreText.transform.DOShakeScale(0.3f, 0.2f, 8, 90); // Hafif titreme
        }
        else if (currentStage == 2)
        {
            scoreText.transform.DOShakeScale(0.4f, 0.3f, 10, 90);
            scoreText.transform.DOScale(1.2f, 0.2f).SetLoops(2, LoopType.Yoyo); // Hafif büyüyüp küçülme
        }
        else if (currentStage == 3)
        {
            scoreText.transform.DOShakeScale(0.5f, 0.4f, 12, 90);
            scoreText.transform.DOShakeRotation(0.5f, 10f, 10, 90); // Daha agresif titreme
            scoreText.transform.DOScale(1.3f, 0.2f).SetLoops(2, LoopType.Yoyo);
        }
    }

    private void UpdateScoreVisual()
{
    Debug.Log("Current Stage: " + currentStage);

    Color targetColor = normalColor;

    if (shakeRoutine != null)
    {
        StopCoroutine(shakeRoutine);
        shakeRoutine = null;
    }

    // Önceki tüm shake ve scale animasyonlarını temizle
    scoreText.rectTransform.DOKill(true);
    scoreText.transform.DOKill(true);

    if (currentStage == 1)
    {
        targetColor = normalColor;
        scoreText.transform.localScale = Vector3.one;
    }
    else if (currentStage == 2)
    {
        targetColor = stage2Color;
        scoreText.transform.DOScale(1.1f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        shakeRoutine = StartCoroutine(ShakeEffect(5f, 0.5f, 10)); // Daha görünür parametreler
    }
    else if (currentStage == 3)
    {
        targetColor = stage3Color;
        scoreText.transform.DOScale(1.4f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        shakeRoutine = StartCoroutine(ShakeEffect(10f, 0.3f, 15)); // Daha güçlü shake
    }

    scoreText.DOColor(targetColor, 0.3f);
}

    private IEnumerator ShakeEffect(float strength, float interval, int vibrato)
{
    while (true)
    {
        // RectTransform üzerinde shake uygula
        scoreText.rectTransform.DOShakeAnchorPos(0.3f, strength, vibrato, 90, false, true);
        yield return new WaitForSeconds(interval);
    }
}
}
