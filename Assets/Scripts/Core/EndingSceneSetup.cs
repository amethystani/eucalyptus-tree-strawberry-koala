using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to the root GameObject in each ending scene.
/// Set EndingType in the Inspector to match the scene name.
/// Generates a procedural background and displays the ending caption.
/// </summary>
public class EndingSceneSetup : MonoBehaviour
{
    [SerializeField] EndingType _thisEnding;
    [SerializeField] Camera     _cam;
    [SerializeField] TMP_Text   _captionText;
    [SerializeField] Image      _backgroundImage;  // fullscreen RawImage / Image

    void Start()
    {
        ApplyBackground();
        ApplyCaption();
        StartCoroutine(BuildProcedural());
    }

    // ── Camera background colour ──────────────────────────────────────────
    void ApplyBackground()
    {
        if (_cam == null) return;
        _cam.backgroundColor = _thisEnding switch {
            EndingType.Constellation => new Color(0.04f, 0.04f, 0.16f), // deep night blue
            EndingType.Milkshake     => new Color(0.96f, 0.90f, 0.76f), // warm cream
            _                        => new Color(0.20f, 0.20f, 0.26f)  // cold grey
        };
    }

    // ── Caption text ──────────────────────────────────────────────────────
    void ApplyCaption()
    {
        if (_captionText == null) return;

        (_captionText.text, _captionText.color) = _thisEnding switch {
            EndingType.Constellation => (
                "\"You're my eucalyptus tree.\"\n\n✦  Constellation Ending  ✦",
                Color.white
            ),
            EndingType.Milkshake => (
                "😐\n\n~ Milkshake Ending ~\n\n\"You're so embarrassing\"",
                new Color(0.85f, 0.70f, 0.45f)
            ),
            _ => (
                "\"I hate u, dekh le.\"\n\n— Grey Ending",
                new Color(0.60f, 0.60f, 0.65f)
            )
        };

        _captionText.fontSize = 28;
        _captionText.alignment = TextAlignmentOptions.Center;
    }

    // ── Procedural background texture ─────────────────────────────────────
    IEnumerator BuildProcedural()
    {
        yield return null; // wait one frame

        switch (_thisEnding)
        {
            case EndingType.Constellation:
                ApplyStarField();
                break;
            case EndingType.Milkshake:
                ApplyWarmPattern();
                break;
            default:
                ApplyRainPattern();
                break;
        }
    }

    void ApplyStarField()
    {
        int w = 256, h = 256;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        var px = new Color[w * h];
        // Deep blue base
        for (int i = 0; i < px.Length; i++)
            px[i] = new Color(0.04f, 0.04f, 0.18f);

        // Scatter stars with seeded random for reproducibility
        var rng = new System.Random(2606);
        for (int s = 0; s < 220; s++)
        {
            int x = rng.Next(w), y = rng.Next(h);
            float b = (float)rng.NextDouble() * 0.55f + 0.45f;
            // Star colour varies from cool white to warm ivory
            px[y * w + x] = new Color(b, b, b * 0.88f + 0.08f);
            // Some stars have a soft 1px glow
            if (rng.NextDouble() > 0.6)
            {
                float glow = b * 0.35f;
                if (x > 0)   px[y * w + x - 1] = new Color(glow, glow, glow * 0.9f);
                if (x < w-1) px[y * w + x + 1] = new Color(glow, glow, glow * 0.9f);
                if (y > 0)   px[(y-1) * w + x] = new Color(glow, glow, glow * 0.9f);
                if (y < h-1) px[(y+1) * w + x] = new Color(glow, glow, glow * 0.9f);
            }
        }

        // Lily of the valley silhouette — simple 8px white flowers
        DrawFlower(px, w, 30, 200);
        DrawFlower(px, w, 38, 195);
        DrawFlower(px, w, 46, 205);

        tex.SetPixels(px);
        tex.Apply();
        SetBackground(tex, w, h);
    }

    void DrawFlower(Color[] px, int w, int fx, int fy)
    {
        // Tiny 3×3 white dot = flower head
        for (int dy = -1; dy <= 1; dy++)
        for (int dx = -1; dx <= 1; dx++)
            px[(fy + dy) * w + (fx + dx)] = Color.white;
        // Stem
        for (int sy = fy - 8; sy < fy; sy++)
            px[sy * w + fx] = new Color(0.75f, 0.90f, 0.75f);
    }

    void ApplyWarmPattern()
    {
        int w = 64, h = 64;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        var px = new Color[w * h];

        // Warm cream checkerboard with pink accents
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            bool check = ((x / 8 + y / 8) % 2 == 0);
            float n = Mathf.Sin(x * 0.5f + y * 0.7f) * 0.03f;
            Color b = check
                ? new Color(0.96f + n, 0.90f + n, 0.76f + n)
                : new Color(0.88f + n, 0.80f + n, 0.68f + n);
            px[y * w + x] = b;
        }
        tex.SetPixels(px);
        tex.Apply();
        SetBackground(tex, w, h);
    }

    void ApplyRainPattern()
    {
        int w = 128, h = 128;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        var px = new Color[w * h];

        // Grey diagonal rain streaks
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            float n = Mathf.Sin((x + y * 0.7f) * 0.4f) * 0.04f;
            bool streak = ((x + y) % 14 < 2);
            Color b = streak
                ? new Color(0.45f + n, 0.45f + n, 0.52f + n)
                : new Color(0.20f + n, 0.20f + n, 0.26f + n);
            px[y * w + x] = b;
        }
        tex.SetPixels(px);
        tex.Apply();
        SetBackground(tex, w, h);
    }

    void SetBackground(Texture2D tex, int w, int h)
    {
        if (_backgroundImage == null) return;
        _backgroundImage.sprite = Sprite.Create(
            tex,
            new Rect(0, 0, w, h),
            Vector2.one * 0.5f
        );
    }
}
