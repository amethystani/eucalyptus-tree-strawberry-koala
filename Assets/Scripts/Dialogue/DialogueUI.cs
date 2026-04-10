using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Ink.Runtime;

public class DialogueUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject _dialoguePanel;
    [SerializeField] TMP_Text   _speakerName;
    [SerializeField] TMP_Text   _bodyText;
    [SerializeField] Transform  _choiceContainer;
    [SerializeField] Button     _continueButton;

    [Header("Prefab")]
    [SerializeField] Button     _choiceButtonPrefab;

    // Static event so TopDownController can subscribe without a direct reference
    public static event System.Action<bool> OnDialogueActiveChanged;

    readonly List<Button> _choiceButtons = new();

    void Awake()
    {
        _dialoguePanel.SetActive(false);
        _continueButton.onClick.AddListener(OnContinuePressed);
    }

    void OnEnable()
    {
        if (InkManager.Instance == null) return;
        InkManager.Instance.OnDialogueLine     += ShowLine;
        InkManager.Instance.OnChoicesPresented += ShowChoices;
        InkManager.Instance.OnDialogueEnd      += HideDialogue;
    }

    void OnDisable()
    {
        if (InkManager.Instance == null) return;
        InkManager.Instance.OnDialogueLine     -= ShowLine;
        InkManager.Instance.OnChoicesPresented -= ShowChoices;
        InkManager.Instance.OnDialogueEnd      -= HideDialogue;
    }

    // ── Show a single line of dialogue ────────────────────────────────────
    void ShowLine(string line)
    {
        _dialoguePanel.SetActive(true);
        OnDialogueActiveChanged?.Invoke(true);
        ClearChoices();
        _continueButton.gameObject.SetActive(true);

        // Parse optional "Speaker: text" format
        int colon = line.IndexOf(':');
        if (colon > 0 && colon < 25)
        {
            _speakerName.text = line.Substring(0, colon).Trim();
            _bodyText.text    = line.Substring(colon + 1).Trim();
        }
        else
        {
            _speakerName.text = "";
            _bodyText.text    = line;
        }
    }

    // ── Show choice buttons ───────────────────────────────────────────────
    void ShowChoices(List<Choice> choices)
    {
        _continueButton.gameObject.SetActive(false);
        ClearChoices();

        for (int i = 0; i < choices.Count; i++)
        {
            int capturedIndex = i;
            Button btn = Instantiate(_choiceButtonPrefab, _choiceContainer);
            btn.GetComponentInChildren<TMP_Text>().text = choices[i].text;
            btn.onClick.AddListener(() => InkManager.Instance.ChooseOption(capturedIndex));
            _choiceButtons.Add(btn);
        }
    }

    // ── Hide panel ────────────────────────────────────────────────────────
    void HideDialogue()
    {
        _dialoguePanel.SetActive(false);
        OnDialogueActiveChanged?.Invoke(false);
        ClearChoices();
    }

    void OnContinuePressed() => InkManager.Instance.AdvanceStory();

    void ClearChoices()
    {
        foreach (var btn in _choiceButtons) Destroy(btn.gameObject);
        _choiceButtons.Clear();
    }
}
