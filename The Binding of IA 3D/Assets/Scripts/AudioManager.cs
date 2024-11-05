using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioClip backgroundMusicClip; // Clip de la música de fondo
    public AudioSource backgroundMusicSource; // AudioSource para la música de fondo


    void Start()
    {
        PlayBackgroundMusic();
    }
    void Awake()
    {
        // Configurar el patrón Singleton para asegurar que solo haya un AudioManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Crear y configurar el AudioSource para la música de fondo
        backgroundMusicSource = gameObject.AddComponent<AudioSource>();
        backgroundMusicSource.clip = backgroundMusicClip;
        backgroundMusicSource.loop = true; // Activar loop para la música de fondo
        backgroundMusicSource.playOnAwake = false; // No reproducir automáticamente
    }

    public void PlayBackgroundMusic()
    {
        if (backgroundMusicSource.isPlaying) return; // Evitar que se reproduzca varias veces
        backgroundMusicSource.Play();
    }

    public void StopBackgroundMusic()
    {
        if (backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Stop();
        }
    }

    public void SetBackgroundVolume(float volume)
    {
        backgroundMusicSource.volume = volume;
    }
}
