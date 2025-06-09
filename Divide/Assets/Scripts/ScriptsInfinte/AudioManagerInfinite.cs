using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip moveClip;
    [SerializeField] private AudioClip nutrientCollectClip;
    [SerializeField] private AudioClip bombBuffCollectClip;
    [SerializeField] private AudioClip teleportClip;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f; // 2D audio
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayButtonClickSound()
    {
        if (buttonClickClip != null)
            audioSource.PlayOneShot(buttonClickClip, 0.5f);
    }

    public void PlayMoveSound()
    {
        if (moveClip != null)
            audioSource.PlayOneShot(moveClip, 0.1f);
    }

    public void PlayNutrientCollectSound()
    {
        if (nutrientCollectClip != null)
            audioSource.PlayOneShot(nutrientCollectClip, 0.5f);
    }

    public void PlayBombBuffCollectSound()
    {
        if (bombBuffCollectClip != null)
            audioSource.PlayOneShot(bombBuffCollectClip, 0.1f);
    }

    public void PlayTeleportSound()
    {
        if (teleportClip != null)
            audioSource.PlayOneShot(teleportClip, 0.1f);
    }
}