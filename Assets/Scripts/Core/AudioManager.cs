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
        Debug.Log($"<color=cyan>Âm lượng Nhạc:</color> {volume * 100:F0}%");
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource) sfxSource.volume = volume;
        PlayerPrefs.SetFloat(SFX_KEY, volume);
        Debug.Log($"<color=yellow>Âm lượng Hiệu ứng:</color> {volume * 100:F0}%");
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

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource == null || clip == null) return;
        
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
        Debug.Log($"<color=green>Đã kích hoạt phát nhạc:</color> {clip.name}");
    }

    public void StopMusic()
    {
        if (musicSource) musicSource.Stop();
    }

    public void SetMusicMute(bool mute)
    {
        if (musicSource) musicSource.mute = mute;
    }

    public void PlaySFXAtPoint(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, volume * GetSFXVolume());
    }
}
