using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldMapController : MonoBehaviour
{
    [System.Serializable]
    public class LocationEntry
    {
        public string SceneName;
        public string DisplayName;
        public Button Btn;
        public bool   UnlockedByDefault;
    }

    [SerializeField] List<LocationEntry> _locations;
    [SerializeField] GameObject          _mapPanel;

    readonly HashSet<string> _unlocked = new();

    void Awake()
    {
        foreach (var loc in _locations)
            if (loc.UnlockedByDefault) _unlocked.Add(loc.SceneName);

        _mapPanel.SetActive(false);
        BindButtons();
    }

    void OnEnable()
    {
        if (InkManager.Instance != null)
            InkManager.Instance.OnSceneTag += HandleSceneTag;
    }

    void OnDisable()
    {
        if (InkManager.Instance != null)
            InkManager.Instance.OnSceneTag -= HandleSceneTag;
    }

    void HandleSceneTag(string scene)
    {
        if (scene == "worldmap") ShowMap();
    }

    public void ShowMap()
    {
        _mapPanel.SetActive(true);
        RefreshButtons();
        // Resume Ink after the player picks a destination (handled inside TravelTo)
    }

    public void HideMap() => _mapPanel.SetActive(false);

    public void UnlockLocation(string sceneName)
    {
        _unlocked.Add(sceneName);
        RefreshButtons();
    }

    void TravelTo(string sceneName)
    {
        HideMap();
        SceneDirector.Instance.LoadScene(sceneName);
    }

    void BindButtons()
    {
        foreach (var loc in _locations)
        {
            string captured = loc.SceneName;
            loc.Btn.onClick.RemoveAllListeners();
            loc.Btn.onClick.AddListener(() => TravelTo(captured));
        }
    }

    void RefreshButtons()
    {
        foreach (var loc in _locations)
        {
            bool canTravel = _unlocked.Contains(loc.SceneName);
            loc.Btn.interactable = canTravel;
            var label = loc.Btn.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = canTravel ? loc.DisplayName : "???";
        }
    }
}
