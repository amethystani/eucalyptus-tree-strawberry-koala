using System;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;

public class InkManager : MonoBehaviour
{
    public static InkManager Instance { get; private set; }

    [SerializeField] TextAsset _inkAsset;

    Story     _story;
    GameState _state;

    // ── Events other systems subscribe to ────────────────────────────────
    public event Action<string>       OnDialogueLine;
    public event Action<List<Choice>> OnChoicesPresented;
    public event Action<string>       OnSceneTag;      // "cnd", "library", etc.
    public event Action<string>       OnMoodTag;       // "eepy", "happy", etc.
    public event Action<string>       OnNpcTag;        // "dhruv", "slushy", etc.
    public event Action<string>       OnSfxTag;        // "rain", "metro", etc.
    public event Action               OnStealthBegin;
    public event Action<EndingType>   OnEnding;
    public event Action               OnDialogueEnd;

    public GameState State => _state;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _state = new GameState();
        _story = new Story(_inkAsset.text);
    }

    void Start() => AdvanceStory();

    public void AdvanceStory()
    {
        while (_story.canContinue)
        {
            string line = _story.Continue();
            ProcessTags(_story.currentTags);

            if (!string.IsNullOrWhiteSpace(line))
            {
                OnDialogueLine?.Invoke(line.Trim());
                // Pause after each displayable line so DialogueUI can show it.
                // Only pause if more content follows or choices are ready.
                if (_story.canContinue || _story.currentChoices.Count > 0)
                    return;
            }
        }

        if (_story.currentChoices.Count > 0)
            OnChoicesPresented?.Invoke(_story.currentChoices);
        else
            OnDialogueEnd?.Invoke();
    }

    public void ChooseOption(int index)
    {
        _story.ChooseChoiceIndex(index);
        AdvanceStory();
    }

    // Called by StealthController when stealth resolves
    public void ResumeFromStealth(int choiceIndex)
    {
        _story.ChooseChoiceIndex(choiceIndex);
        AdvanceStory();
    }

    void ProcessTags(List<string> tags)
    {
        foreach (string raw in tags)
        {
            string tag = raw.Trim();

            if      (tag.StartsWith("scene:"))       OnSceneTag?.Invoke(tag.Substring(6));
            else if (tag.StartsWith("mood:"))        OnMoodTag?.Invoke(tag.Substring(5));
            else if (tag.StartsWith("npc:"))         OnNpcTag?.Invoke(tag.Substring(4));
            else if (tag.StartsWith("sfx:"))         OnSfxTag?.Invoke(tag.Substring(4));
            else if (tag.StartsWith("goofy:"))       _state.ApplyTag(tag);
            else if (tag.StartsWith("overthinker:")) _state.ApplyTag(tag);
            else if (tag.StartsWith("lives:"))       _state.ApplyTag(tag);
            else if (tag == "stealth:begin")         OnStealthBegin?.Invoke();
            else if (tag.StartsWith("ending:"))      FireEnding(tag.Substring(7));
        }
    }

    void FireEnding(string endingName)
    {
        EndingType ending = endingName switch {
            "constellation" => EndingType.Constellation,
            "milkshake"     => EndingType.Milkshake,
            _               => EndingType.Grey
        };
        OnEnding?.Invoke(ending);
    }
}
