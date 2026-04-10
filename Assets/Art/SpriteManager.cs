using System.Collections.Generic;
using UnityEngine;

public class SpriteManager : MonoBehaviour
{
    public static SpriteManager Instance { get; private set; }

    [SerializeField] SpriteRenderer _playerPortrait;
    [SerializeField] SpriteRenderer _npcPortrait;

    // Map mood tag strings → MoodID
    static readonly Dictionary<string, MoodID> MoodMap = new() {
        { "neutral",      MoodID.Neutral     },
        { "goofy",        MoodID.Goofy       },
        { "eepy",         MoodID.Eepy        },
        { "happy",        MoodID.Happy       },
        { "angry",        MoodID.Angry       },
        { "princess-ani", MoodID.PrincessAni },
        { "straight",     MoodID.StraightFace},
        { "talking",      MoodID.Talking     },
    };

    // Map npc tag strings → CharacterID
    static readonly Dictionary<string, CharacterID> NpcMap = new() {
        { "slushy",    CharacterID.Slushy   },
        { "dhruv",     CharacterID.Dhruv    },
        { "nischala",  CharacterID.Nischala },
        { "jabin",     CharacterID.Jabin    },
    };

    CharacterID _currentNPC = CharacterID.Slushy;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Pre-generate and display default sprites
        RefreshPlayerPortrait(MoodID.Neutral);
        RefreshNPCPortrait(MoodID.Neutral);
    }

    void OnEnable()
    {
        if (InkManager.Instance == null) return;
        InkManager.Instance.OnMoodTag += HandleMoodTag;
        InkManager.Instance.OnNpcTag  += HandleNpcTag;
    }

    void OnDisable()
    {
        if (InkManager.Instance == null) return;
        InkManager.Instance.OnMoodTag -= HandleMoodTag;
        InkManager.Instance.OnNpcTag  -= HandleNpcTag;
    }

    void HandleNpcTag(string npcTag)
    {
        if (NpcMap.TryGetValue(npcTag.ToLower(), out var id))
            _currentNPC = id;
    }

    void HandleMoodTag(string moodTag)
    {
        if (!MoodMap.TryGetValue(moodTag.ToLower(), out var mood)) return;

        // Player portrait: only PrincessAni overrides the default neutral look
        MoodID playerMood = mood == MoodID.PrincessAni ? MoodID.PrincessAni : mood;
        RefreshPlayerPortrait(playerMood);
        RefreshNPCPortrait(mood);
    }

    void RefreshPlayerPortrait(MoodID mood)
    {
        if (_playerPortrait == null) return;
        _playerPortrait.sprite = PixelArtLibrary.Build(CharacterID.Monkey, mood);
    }

    void RefreshNPCPortrait(MoodID mood)
    {
        if (_npcPortrait == null) return;
        _npcPortrait.sprite = PixelArtLibrary.Build(_currentNPC, mood);
    }

    // Public API for direct calls (e.g. from WorldMapController)
    public void SetNPC(CharacterID id)
    {
        _currentNPC = id;
        RefreshNPCPortrait(MoodID.Neutral);
    }

    public void SetPlayerMood(MoodID mood) => RefreshPlayerPortrait(mood);
}
