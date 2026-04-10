using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneDirector : MonoBehaviour
{
    public static SceneDirector Instance { get; private set; }

    [SerializeField] Image _fadeOverlay;    // fullscreen black Image, starts alpha=0
    [SerializeField] float _fadeDuration = 0.4f;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (_fadeOverlay != null)
            _fadeOverlay.color = new Color(0, 0, 0, 0);
    }

    void OnEnable()
    {
        if (InkManager.Instance == null) return;
        InkManager.Instance.OnSceneTag += HandleSceneTag;
        InkManager.Instance.OnEnding   += HandleEnding;
    }

    void OnDisable()
    {
        if (InkManager.Instance == null) return;
        InkManager.Instance.OnSceneTag -= HandleSceneTag;
        InkManager.Instance.OnEnding   -= HandleEnding;
    }

    void HandleSceneTag(string scene)
    {
        // worldmap is handled by WorldMapController, not a scene load
        if (scene == "worldmap") return;
        // sub-areas of the same scene (e.g. cnd_counter) are not scene loads
        if (scene.Contains("_")) return;
        LoadScene(scene);
    }

    void HandleEnding(EndingType ending)
    {
        string sceneName = ending switch {
            EndingType.Constellation => "EndingConstellation",
            EndingType.Milkshake     => "EndingMilkshake",
            _                        => "EndingGrey"
        };
        LoadScene(sceneName);
    }

    public void LoadScene(string sceneName) =>
        StartCoroutine(FadeAndLoad(sceneName));

    IEnumerator FadeAndLoad(string sceneName)
    {
        yield return StartCoroutine(Fade(0f, 1f));
        SceneManager.LoadScene(sceneName);
        // Give Unity one frame to load before fading back in
        yield return null;
        yield return StartCoroutine(Fade(1f, 0f));
    }

    IEnumerator Fade(float from, float to)
    {
        if (_fadeOverlay == null) yield break;
        float elapsed = 0f;
        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(from, to, elapsed / _fadeDuration);
            _fadeOverlay.color = new Color(0, 0, 0, a);
            yield return null;
        }
        _fadeOverlay.color = new Color(0, 0, 0, to);
    }
}
