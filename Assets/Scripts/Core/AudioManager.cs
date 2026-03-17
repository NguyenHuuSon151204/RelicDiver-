using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("--- Audio Sources ---")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("--- Settings Keys ---")]
    private const string MUSIC_KEY = "MusicVolume";
    private const string SFX_KEY = "SFXVolume";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ âm thanh xuyên suốt các màn
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadVolume();
    }

    public void SetMusicVolume(float volume)
    {
        if (musicSource) musicSource.volume = volume;
        PlayerPrefs.SetFloat(MUSIC_KEY, volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource) sfxSource.volume = volume;
        PlayerPrefs.SetFloat(SFX_KEY, volume);
    }

    public float GetMusicVolume() => PlayerPrefs.GetFloat(MUSIC_KEY, 0.5f);
    public float GetSFXVolume() => PlayerPrefs.GetFloat(SFX_KEY, 0.5f);

    private void LoadVolume()
    {
        float music = PlayerPrefs.GetFloat(MUSIC_KEY, 0.5f);
        float sfx = PlayerPrefs.GetFloat(SFX_KEY, 0.5f);

        if (musicSource) musicSource.volume = music;
        if (sfxSource) sfxSource.volume = sfx;
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}
