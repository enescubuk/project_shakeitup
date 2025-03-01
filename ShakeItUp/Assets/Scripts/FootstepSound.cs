using System.Collections;
using UnityEngine;

public class FootstepSound : MonoBehaviour
{
    public AudioSource audioSource; // Ses kaynağı
    public AudioClip[] footstepSounds; // Adım sesleri dizisi
    public float stepInterval = 0.5f; // Adım sesleri arasındaki süre
    public float moveThreshold = 0.1f; // Hareket eşiği

    private Rigidbody rb;
    private float stepTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Rigidbody bileşenini al
    }

    void Update()
    {
        if (IsMoving())
        {
            stepTimer += Time.deltaTime;
            if (stepTimer > stepInterval)
            {
                PlayFootstep();
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    bool IsMoving()
    {
        // Karakterin hızını kontrol ederek hareket edip etmediğini anla
        return rb.linearVelocity.magnitude > moveThreshold;
    }

    void PlayFootstep()
    {
        if (footstepSounds.Length > 0)
        {
            int index = Random.Range(0, footstepSounds.Length);
            audioSource.PlayOneShot(footstepSounds[index]);
        }
    }
}
