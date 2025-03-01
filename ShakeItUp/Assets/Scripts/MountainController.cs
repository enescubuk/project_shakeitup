using UnityEngine;
using DG.Tweening;

public class MountainController : MonoBehaviour
{
    public bool isActive = true;
    public int characterSpeedPhase = 0;
    public float popoutDuration = 0.5f;
    
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            UpgradeMountain();
        }
    }
    public void UpgradeMountain()
    {
        if (!isActive) return;

        float scaleMultiplier = 1f + (characterSpeedPhase * 0.2f);
        float heightMultiplier = 1f + (characterSpeedPhase * 0.2f);

        Vector3 newScale = new Vector3(
            originalScale.x * scaleMultiplier,
            originalScale.y * heightMultiplier,
            originalScale.z * scaleMultiplier
        );

        transform.DOScale(newScale, popoutDuration).SetEase(Ease.OutBack);
    }
}
