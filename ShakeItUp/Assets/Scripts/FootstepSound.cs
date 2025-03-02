using UnityEngine;

public class FootstepSound : MonoBehaviour
{
    public AudioSource footstepSource; // Ses kaynağı
    public AudioClip[] footstepSounds; // Farklı adım sesleri
    public CharacterController characterController; // Hareket için kullanılan karakter kontrolcüsü

    private void Update()
    {
        // Karakter yürüyorsa ve zeminde ise ses çal
        if (characterController.isGrounded && characterController.velocity.magnitude > 0.1f)
        {
            if (!footstepSource.isPlaying) // Aynı anda birden fazla ses çalmasını engelle
            {
                PlayFootstep();
            }
        }
    }

    void PlayFootstep()
    {
        // Rastgele bir adım sesi seç
        int index = Random.Range(0, footstepSounds.Length);
        footstepSource.clip = footstepSounds[index];
        footstepSource.Play();
    }
}
