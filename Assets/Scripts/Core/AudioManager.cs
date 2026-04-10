using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class SoundEntry
    {
        public string    Key;
        public AudioClip Clip;
    }

    [SerializeField] List<SoundEntry> _sounds;
    [SerializeField] AudioSource      _musicSource;
    [SerializeField] AudioSource      _sfxSource;

    // Keys that play as looping background music rather than one-shots
    static readonly HashSet<string> LoopKeys = new() {
        "rain", "metro", "music_lofi", "music_cnd",
        "music_library", "music_gallery", "music_stars", "music_rain"
    };

    readonly Dictionary<string, AudioClip> _map = new();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (var entry in _sounds)
            if (!string.IsNullOrEmpty(entry.Key) && entry.Clip != null)
                _map[entry.Key] = entry.Clip;
    }

    void OnEnable()
    {
        if (InkManager.Instance != null)
            InkManager.Instance.OnSfxTag += HandleSfxTag;
    }

    void OnDisable()
    {
        if (InkManager.Instance != null)
            InkManager.Instance.OnSfxTag -= HandleSfxTag;
    }

    void HandleSfxTag(string key)
    {
        if (!_map.TryGetValue(key, out var clip)) return;

        if (LoopKeys.Contains(key))
        {
            if (_musicSource.clip == clip && _musicSource.isPlaying) return;
            _musicSource.clip = clip;
            _musicSource.loop = true;
            _musicSource.Play();
        }
        else
        {
            _sfxSource.PlayOneShot(clip);
        }
    }

    public void StopMusic() => _musicSource.Stop();

    public void PlaySfx(string key)
    {
        if (_map.TryGetValue(key, out var clip))
            _sfxSource.PlayOneShot(clip);
    }
}
