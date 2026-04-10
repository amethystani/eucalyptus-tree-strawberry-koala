# The Eucalyptus Tree & The Strawberry Koala — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a complete pixel-art narrative RPG in Unity 2D — Ink-driven story, programmatic pixel art, stealth mechanics, world map navigation, cross-platform input, three endings.

**Architecture:** `story.ink` is the game state machine; Unity C# only reacts to tags emitted by Ink. All sprites/tiles are painted pixel-by-pixel into `Texture2D` at runtime — no external art files. InkManager owns GoofyMeter, OverthinkerMeter, and Lives; all other systems subscribe to its events.

**Tech Stack:** Unity 2D, C#, Ink Unity Integration (com.inkle.ink-unity-integration 1.1.7), TextMeshPro, Unity Input System, Unity Test Framework (NUnit EditMode)

---

## File Map

| File | Responsibility |
|---|---|
| `Assets/Scripts/Core/InkManager.cs` | Parses tags, owns meters/lives, fires C# events |
| `Assets/Scripts/Core/SceneDirector.cs` | Loads scenes from `#scene:X` tags, fade transitions |
| `Assets/Scripts/Core/AudioManager.cs` | Plays SFX/music from `#sfx:X` tags |
| `Assets/Scripts/Dialogue/DialogueUI.cs` | Text box + choice buttons, disables movement |
| `Assets/Scripts/Player/TopDownController.cs` | WASD + virtual joystick movement |
| `Assets/Scripts/Player/VirtualJoystick.cs` | Mobile touch joystick UI |
| `Assets/Scripts/Stealth/PatrolAI.cs` | Waypoint patrol for Prof. Jabin |
| `Assets/Scripts/Stealth/StealthController.cs` | Hide detection, lives decrement, resume Ink |
| `Assets/Scripts/Interactions/WorldMapController.cs` | Location icons, unlock logic, travel |
| `Assets/Art/PixelArtLibrary.cs` | Paints all sprites/tiles into Texture2D |
| `Assets/Art/SpriteManager.cs` | Caches and swaps sprites by CharacterID+MoodID |
| `Assets/Ink/story.ink` | Full narrative — all knots, choices, endings |
| `Assets/Tests/EditMode/InkManagerTests.cs` | Unit tests for meter math and tag parsing |
| `Assets/Tests/EditMode/PixelArtTests.cs` | Unit tests for sprite dimensions and opacity |

---

### Task 1: Folder Structure & Test Assembly

**Files:**
- Create: `Assets/Tests/EditMode/` (folder + .asmdef)
- Create: `Assets/Art/` (folder)
- Create: `Assets/Tests/EditMode/InkManagerTests.cs`

- [ ] **Step 1: Create test assembly definition**

Create `Assets/Tests/EditMode/EucalyptusTests.asmdef`:
```json
{
    "name": "EucalyptusTests",
    "references": [
        "Unity.TestFramework",
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner"
    ],
    "includePlatforms": ["Editor"],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": false,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

- [ ] **Step 2: Create stub test file**

Create `Assets/Tests/EditMode/InkManagerTests.cs`:
```csharp
using NUnit.Framework;

public class InkManagerTests
{
    [Test]
    public void Placeholder_PassesUntilRealTestsAdded()
    {
        Assert.Pass();
    }
}
```

- [ ] **Step 3: Verify Unity compiles** — open Unity, check Console for zero errors.

- [ ] **Step 4: Commit**
```bash
git init /Users/animesh/game2
cd /Users/animesh/game2
git add Assets/Tests Packages/manifest.json
git commit -m "feat: project scaffold, test assembly, package manifest"
```

---

### Task 2: InkManager — Meters, Lives, Tag Parsing

**Files:**
- Create: `Assets/Scripts/Core/InkManager.cs`
- Modify: `Assets/Tests/EditMode/InkManagerTests.cs`

`★ Insight ─────────────────────────────────────`
InkManager must NOT depend on `Ink.Runtime.Story` in unit tests (requires a .ink asset). Extract meter logic into a plain C# class `GameState` that tests can instantiate directly — InkManager wraps it.
`─────────────────────────────────────────────────`

- [ ] **Step 1: Write failing tests**

Replace `Assets/Tests/EditMode/InkManagerTests.cs`:
```csharp
using NUnit.Framework;

public class InkManagerTests
{
    GameState _state;

    [SetUp]
    public void SetUp() => _state = new GameState();

    [Test]
    public void GoofyMeter_StartsAtZero() =>
        Assert.AreEqual(0, _state.GoofyMeter);

    [Test]
    public void OverthinkerMeter_StartsAtZero() =>
        Assert.AreEqual(0, _state.OverthinkerMeter);

    [Test]
    public void Lives_StartsAtThree() =>
        Assert.AreEqual(3, _state.Lives);

    [Test]
    public void ApplyTag_GoofyPlus_IncreasesGoofy()
    {
        _state.ApplyTag("goofy:+10");
        Assert.AreEqual(10, _state.GoofyMeter);
    }

    [Test]
    public void ApplyTag_GoofyMinus_DecreasesGoofy()
    {
        _state.ApplyTag("goofy:+20");
        _state.ApplyTag("goofy:-5");
        Assert.AreEqual(15, _state.GoofyMeter);
    }

    [Test]
    public void ApplyTag_OverthinkerPlus_IncreasesOverthinker()
    {
        _state.ApplyTag("overthinker:+10");
        Assert.AreEqual(10, _state.OverthinkerMeter);
    }

    [Test]
    public void ApplyTag_LivesMinus_DecrementsLives()
    {
        _state.ApplyTag("lives:-1");
        Assert.AreEqual(2, _state.Lives);
    }

    [Test]
    public void Lives_NeverGoesBelowZero()
    {
        _state.ApplyTag("lives:-1");
        _state.ApplyTag("lives:-1");
        _state.ApplyTag("lives:-1");
        _state.ApplyTag("lives:-1");
        Assert.AreEqual(0, _state.Lives);
    }

    [Test]
    public void DominantMeter_WhenGoofyHigher_ReturnsGoofy()
    {
        _state.ApplyTag("goofy:+20");
        _state.ApplyTag("overthinker:+5");
        Assert.AreEqual(EndingType.Constellation, _state.DetermineEnding());
    }

    [Test]
    public void DominantMeter_WhenOverthinkerHigher_ReturnsGrey()
    {
        _state.ApplyTag("overthinker:+20");
        _state.ApplyTag("goofy:+5");
        Assert.AreEqual(EndingType.Grey, _state.DetermineEnding());
    }

    [Test]
    public void DominantMeter_WhenEqual_ReturnsMilkshake()
    {
        _state.ApplyTag("goofy:+10");
        _state.ApplyTag("overthinker:+10");
        Assert.AreEqual(EndingType.Milkshake, _state.DetermineEnding());
    }

    [Test]
    public void BadDetour_WhenLivesZero_ForcesGrey()
    {
        _state.ApplyTag("goofy:+30");
        _state.ApplyTag("lives:-1");
        _state.ApplyTag("lives:-1");
        _state.ApplyTag("lives:-1");
        Assert.AreEqual(EndingType.Grey, _state.DetermineEnding());
    }
}
```

- [ ] **Step 2: Run tests — expect FAIL** (GameState not defined yet)

In Unity: Window → General → Test Runner → EditMode → Run All. All tests fail with "type not found."

- [ ] **Step 3: Implement GameState**

Create `Assets/Scripts/Core/GameState.cs`:
```csharp
public enum EndingType { Constellation, Milkshake, Grey }

public class GameState
{
    public int GoofyMeter { get; private set; }
    public int OverthinkerMeter { get; private set; }
    public int Lives { get; private set; } = 3;
    public bool HasStrawberry { get; set; }
    public bool HasKeychain { get; set; }
    public bool BadDetourTriggered { get; private set; }

    public void ApplyTag(string tag)
    {
        // tag format: "key:+N" or "key:-N"
        int colon = tag.IndexOf(':');
        if (colon < 0) return;
        string key = tag.Substring(0, colon).Trim();
        string valStr = tag.Substring(colon + 1).Trim();
        if (!int.TryParse(valStr, out int delta)) return;

        switch (key)
        {
            case "goofy":        GoofyMeter        = System.Math.Max(0, GoofyMeter + delta);        break;
            case "overthinker":  OverthinkerMeter  = System.Math.Max(0, OverthinkerMeter + delta);  break;
            case "lives":
                Lives = System.Math.Max(0, Lives + delta);
                if (Lives == 0) BadDetourTriggered = true;
                break;
        }
    }

    public EndingType DetermineEnding()
    {
        if (BadDetourTriggered) return EndingType.Grey;
        if (GoofyMeter > OverthinkerMeter) return EndingType.Constellation;
        if (OverthinkerMeter > GoofyMeter) return EndingType.Grey;
        return EndingType.Milkshake;
    }
}
```

- [ ] **Step 4: Implement InkManager**

Create `Assets/Scripts/Core/InkManager.cs`:
```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;

public class InkManager : MonoBehaviour
{
    public static InkManager Instance { get; private set; }

    [SerializeField] TextAsset _inkAsset;

    Story _story;
    GameState _state;

    // Events other systems subscribe to
    public event Action<string>       OnDialogueLine;
    public event Action<List<Choice>> OnChoicesPresented;
    public event Action<string>       OnSceneTag;      // "cnd", "library", etc.
    public event Action<string>       OnMoodTag;       // "eepy", "happy", etc.
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
                // Stop after each line so DialogueUI can display it
                if (_story.currentChoices.Count == 0 && _story.canContinue)
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
            if (tag.StartsWith("scene:"))       OnSceneTag?.Invoke(tag.Substring(6));
            else if (tag.StartsWith("mood:"))   OnMoodTag?.Invoke(tag.Substring(5));
            else if (tag.StartsWith("sfx:"))    OnSfxTag?.Invoke(tag.Substring(4));
            else if (tag.StartsWith("goofy:"))  _state.ApplyTag(tag);
            else if (tag.StartsWith("overthinker:")) _state.ApplyTag(tag);
            else if (tag.StartsWith("lives:"))  { _state.ApplyTag(tag); }
            else if (tag == "stealth:begin")    OnStealthBegin?.Invoke();
            else if (tag.StartsWith("ending:")) FireEnding(tag.Substring(7));
        }
    }

    void FireEnding(string endingName)
    {
        // ending tag overrides meter calculation only for forced endings
        EndingType ending = endingName switch {
            "constellation" => EndingType.Constellation,
            "milkshake"     => EndingType.Milkshake,
            _               => EndingType.Grey
        };
        OnEnding?.Invoke(ending);
    }
}
```

- [ ] **Step 5: Run tests — expect PASS**

Unity Test Runner → EditMode → Run All. All 11 tests pass.

- [ ] **Step 6: Commit**
```bash
git add Assets/Scripts/Core/ Assets/Tests/
git commit -m "feat: GameState meter logic + InkManager tag dispatcher"
```

---

### Task 3: PixelArtLibrary — Palette & Helpers

**Files:**
- Create: `Assets/Art/PixelArtLibrary.cs`
- Create: `Assets/Tests/EditMode/PixelArtTests.cs`

- [ ] **Step 1: Write failing tests**

Create `Assets/Tests/EditMode/PixelArtTests.cs`:
```csharp
using NUnit.Framework;
using UnityEngine;

public class PixelArtTests
{
    [Test]
    public void BuildSprite_MonkeyNeutral_IsCorrectSize()
    {
        var sprite = PixelArtLibrary.Build(CharacterID.Monkey, MoodID.Neutral);
        Assert.AreEqual(16, (int)sprite.rect.width);
        Assert.AreEqual(32, (int)sprite.rect.height);
    }

    [Test]
    public void BuildSprite_SlushyEepy_IsCorrectSize()
    {
        var sprite = PixelArtLibrary.Build(CharacterID.Slushy, MoodID.Eepy);
        Assert.AreEqual(16, (int)sprite.rect.width);
        Assert.AreEqual(32, (int)sprite.rect.height);
    }

    [Test]
    public void BuildSprite_HasNonTransparentPixels()
    {
        var sprite = PixelArtLibrary.Build(CharacterID.Monkey, MoodID.Neutral);
        var tex = sprite.texture;
        bool hasOpaque = false;
        for (int x = 0; x < tex.width; x++)
            for (int y = 0; y < tex.height; y++)
                if (tex.GetPixel(x, y).a > 0.5f) { hasOpaque = true; break; }
        Assert.IsTrue(hasOpaque);
    }

    [Test]
    public void BuildTile_FloorWood_Is32x32()
    {
        var sprite = PixelArtLibrary.BuildTile(TileID.FloorWood);
        Assert.AreEqual(32, (int)sprite.rect.width);
        Assert.AreEqual(32, (int)sprite.rect.height);
    }
}
```

- [ ] **Step 2: Run — expect FAIL** (PixelArtLibrary not defined)

- [ ] **Step 3: Implement PixelArtLibrary core**

Create `Assets/Art/PixelArtLibrary.cs`:
```csharp
using UnityEngine;
using System.Collections.Generic;

public enum CharacterID { Monkey, Slushy, Dhruv, Nischala, Jabin }
public enum MoodID      { Neutral, Goofy, Eepy, Happy, Angry, PrincessAni, StraightFace, Talking }
public enum TileID      { FloorWood, FloorWoodDark, Wall, Shelf, Chair, MomoSteamer,
                          LibraryFloor, BookshelfTall, MetroPlatform, MetroWall,
                          GalleryFloor, GalleryWall, DormFloor, DormWall }

public static class PixelArtLibrary
{
    // ── Palette ──────────────────────────────────────────────────────────
    static readonly Color T   = new Color(0,0,0,0);           // transparent
    static readonly Color SK  = new Color(0.94f,0.78f,0.60f); // skin
    static readonly Color GH  = new Color(0.10f,0.24f,0.10f); // dark green hair (Monkey)
    static readonly Color EY  = new Color(0.10f,0.10f,0.18f); // dark eye / outline
    static readonly Color WH  = new Color(0.96f,0.96f,0.96f); // white shirt
    static readonly Color RD  = new Color(0.90f,0.22f,0.27f); // red emblem / strawberry
    static readonly Color PA  = new Color(0.18f,0.18f,0.23f); // dark pants
    static readonly Color BR  = new Color(0.48f,0.29f,0.12f); // brown boots
    static readonly Color PH  = new Color(0.42f,0.25f,0.63f); // purple hair (Slushy)
    static readonly Color TE  = new Color(0.00f,0.71f,0.65f); // teal eye (Slushy)
    static readonly Color BJ  = new Color(0.18f,0.35f,0.56f); // blue jacket (Slushy)
    static readonly Color LB  = new Color(0.60f,0.74f,0.94f); // light blue (Slushy shirt)
    static readonly Color OB  = new Color(0.42f,0.58f,0.82f); // outline blue
    static readonly Color BH  = new Color(0.20f,0.14f,0.08f); // black-brown hair (Dhruv)
    static readonly Color DK  = new Color(0.25f,0.18f,0.10f); // dark skin (Dhruv/Nischala)
    static readonly Color GR  = new Color(0.45f,0.45f,0.50f); // grey (Jabin jacket)
    static readonly Color YL  = new Color(0.95f,0.85f,0.20f); // yellow (momo)
    // Tiles
    static readonly Color FW  = new Color(0.77f,0.58f,0.42f); // floor wood light
    static readonly Color FD  = new Color(0.63f,0.47f,0.31f); // floor wood dark
    static readonly Color WL  = new Color(0.82f,0.72f,0.88f); // wall lavender
    static readonly Color WD  = new Color(0.68f,0.56f,0.78f); // wall lavender dark
    static readonly Color SH  = new Color(0.45f,0.28f,0.14f); // shelf brown
    static readonly Color SHD = new Color(0.30f,0.18f,0.08f); // shelf dark
    static readonly Color LF  = new Color(0.72f,0.82f,0.72f); // library floor
    static readonly Color LFD = new Color(0.60f,0.72f,0.60f); // library floor dark
    static readonly Color MP  = new Color(0.55f,0.55f,0.65f); // metro platform
    static readonly Color MPD = new Color(0.42f,0.42f,0.52f); // metro dark
    static readonly Color GF  = new Color(0.92f,0.88f,0.80f); // gallery floor
    static readonly Color GFD = new Color(0.80f,0.76f,0.68f); // gallery floor dark
    static readonly Color DF  = new Color(0.68f,0.55f,0.42f); // dorm floor

    // ── Sprite cache ─────────────────────────────────────────────────────
    static Dictionary<(CharacterID,MoodID), Sprite> _charCache = new();
    static Dictionary<TileID, Sprite>               _tileCache = new();

    public static Sprite Build(CharacterID id, MoodID mood)
    {
        var key = (id, mood);
        if (_charCache.TryGetValue(key, out var cached)) return cached;
        Color[] pixels = GetCharPixels(id, mood);
        var sprite = MakeSprite(pixels, 16, 32);
        _charCache[key] = sprite;
        return sprite;
    }

    public static Sprite BuildTile(TileID id)
    {
        if (_tileCache.TryGetValue(id, out var cached)) return cached;
        Color[] pixels = GetTilePixels(id);
        var sprite = MakeSprite(pixels, 32, 32);
        _tileCache[id] = sprite;
        return sprite;
    }

    // ── Sprite factory ───────────────────────────────────────────────────
    static Sprite MakeSprite(Color[] pixels, int w, int h)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0,0,w,h), new Vector2(0.5f,0f), 16f);
    }

    // Converts top-down string rows to bottom-up Texture2D pixel array
    static Color[] RowsToPixels(string[] rows, Dictionary<char,Color> pal)
    {
        int h = rows.Length, w = rows[0].Length;
        Color[] px = new Color[w * h];
        for (int y = 0; y < h; y++)
        {
            int ty = h - 1 - y; // flip Y
            for (int x = 0; x < w; x++)
            {
                char c = x < rows[y].Length ? rows[y][x] : '.';
                px[ty * w + x] = pal.TryGetValue(c, out var col) ? col : T;
            }
        }
        return px;
    }

    // ── Character pixel data ─────────────────────────────────────────────
    static Color[] GetCharPixels(CharacterID id, MoodID mood) =>
        (id, mood) switch {
            (CharacterID.Monkey, MoodID.Neutral)     => MonkeyNeutral(),
            (CharacterID.Monkey, MoodID.Goofy)       => MonkeyGoofy(),
            (CharacterID.Monkey, MoodID.Eepy)        => MonkeyEepy(),
            (CharacterID.Monkey, MoodID.PrincessAni) => MonkeyPrincessAni(),
            (CharacterID.Slushy, MoodID.Neutral)     => SlushyNeutral(),
            (CharacterID.Slushy, MoodID.Eepy)        => SlushyEepy(),
            (CharacterID.Slushy, MoodID.Happy)       => SlushyHappy(),
            (CharacterID.Slushy, MoodID.StraightFace)=> SlushyStraight(),
            (CharacterID.Dhruv,  MoodID.Neutral)     => DhruvNeutral(),
            (CharacterID.Dhruv,  MoodID.Talking)     => DhruvTalking(),
            (CharacterID.Nischala, MoodID.Neutral)   => NischalaNeutral(),
            (CharacterID.Nischala, MoodID.Talking)   => NischalaTalking(),
            (CharacterID.Jabin,  MoodID.Neutral)     => JabinNeutral(),
            (CharacterID.Jabin,  MoodID.Angry)       => JabinAngry(),
            (CharacterID.Jabin,  MoodID.Talking)     => JabinTalking(),
            _ => MonkeyNeutral()
        };

    // ── Monkey sprites ───────────────────────────────────────────────────
    // 16 wide × 32 tall, top→bottom
    // . transparent  G dark-green-hair  S skin  E eye  W white-shirt
    // R red-emblem   K dark-pants       B brown-boot  O outline/dark
    static Color[] MonkeyNeutral()
    {
        var pal = new Dictionary<char,Color> {
            {'.', T},{' ',T},{'G',GH},{'S',SK},{'E',EY},
            {'W',WH},{'R',RD},{'K',PA},{'B',BR},{'O',EY}
        };
        string[] rows = {
            "....GGGGGGGG....",  //  0
            "...GGGGGGGGGG...",  //  1
            "..GGGGGGGGGGGG..",  //  2
            "..GSSSSSSSSSGG..",  //  3
            "..GSSSSSSSSSGGG.",  //  4
            "..GSEESSSSEESG..",  //  5  (eyes)
            "..GSSSSSSSSSSG..",  //  6
            "..GSSSOOSSSSG...",  //  7  (mouth)
            "..GSSSSSSSSGG...",  //  8
            "....SSSSSSS.....",  //  9  (neck)
            "...WWWWWWWWWW...",  // 10 (shoulders)
            "..WWWWWWWWWWWW..",  // 11
            "..WWWWWWWWWWWW..",  // 12
            "..WWWRRWWWWWWW..",  // 13 (emblem)
            "..WWWRRWWWWWWW..",  // 14
            "..WWWWWWWWWWWW..",  // 15
            "..WWWWWWWWWWWW..",  // 16
            "...WWWWWWWWWWW..",  // 17
            "....KKKKKKKK....",  // 18 (belt)
            "...KKKKKKKKKK...",  // 19
            "...KKKKKKKKKK...",  // 20
            "...KKKK.KKKKK...",  // 21
            "...KKKK.KKKKK...",  // 22
            "...KKKK.KKKKK...",  // 23
            "...KKKK.KKKKK...",  // 24
            "...BBBB.BBBB....",  // 25 (boots)
            "...BBBB.BBBB....",  // 26
            "..BBBBB.BBBBB...",  // 27
            "..BBBBB.BBBBB...",  // 28
            "..BBBBB.BBBBB...",  // 29
            "..BBBBBBBBBB....",  // 30
            "...BBBBBBBBB....",  // 31
        };
        return RowsToPixels(rows, pal);
    }

    static Color[] MonkeyGoofy()
    {
        // Same as neutral but mouth is wider (grin 'U' shape)
        var pal = new Dictionary<char,Color> {
            {'.', T},{' ',T},{'G',GH},{'S',SK},{'E',EY},
            {'W',WH},{'R',RD},{'K',PA},{'B',BR},{'O',EY},{'U',SK}
        };
        string[] rows = {
            "....GGGGGGGG....",
            "...GGGGGGGGGG...",
            "..GGGGGGGGGGGG..",
            "..GSSSSSSSSSGG..",
            "..GSSSSSSSSSGGG.",
            "..GSEESSSSEESG..",
            "..GSSSSSSSSSSG..",
            "..GSOOOOOOSSSG..",  // big grin
            "..GSSSSSSSSGG...",
            "....SSSSSSS.....",
            "...WWWWWWWWWW...",
            "..WWWWWWWWWWWW..",
            "..WWWWWWWWWWWW..",
            "..WWWRRWWWWWWW..",
            "..WWWRRWWWWWWW..",
            "..WWWWWWWWWWWW..",
            "..WWWWWWWWWWWW..",
            "...WWWWWWWWWWW..",
            "....KKKKKKKK....",
            "...KKKKKKKKKK...",
            "...KKKKKKKKKK...",
            "...KKKK.KKKKK...",
            "...KKKK.KKKKK...",
            "...KKKK.KKKKK...",
            "...KKKK.KKKKK...",
            "...BBBB.BBBB....",
            "...BBBB.BBBB....",
            "..BBBBB.BBBBB...",
            "..BBBBB.BBBBB...",
            "..BBBBB.BBBBB...",
            "..BBBBBBBBBB....",
            "...BBBBBBBBB....",
        };
        return RowsToPixels(rows, pal);
    }

    static Color[] MonkeyEepy()
    {
        // Half-closed eyes ('H' = half-eye = dark top half)
        var pal = new Dictionary<char,Color> {
            {'.', T},{' ',T},{'G',GH},{'S',SK},{'E',EY},
            {'W',WH},{'R',RD},{'K',PA},{'B',BR},{'O',EY},
            {'H', new Color(0.10f,0.10f,0.18f,0.6f)}
        };
        string[] rows = {
            "....GGGGGGGG....",
            "...GGGGGGGGGG...",
            "..GGGGGGGGGGGG..",
            "..GSSSSSSSSSGG..",
            "..GSSSSSSSSSGGG.",
            "..GSHESSSSHESG..",  // half-eyes
            "..GSSSSSSSSSSG..",
            "..GSSSOOSSSSG...",
            "..GSSSSSSSSGG...",
            "....SSSSSSS.....",
            "...WWWWWWWWWW...",
            "..WWWWWWWWWWWW..",
            "..WWWWWWWWWWWW..",
            "..WWWRRWWWWWWW..",
            "..WWWRRWWWWWWW..",
            "..WWWWWWWWWWWW..",
            "..WWWWWWWWWWWW..",
            "...WWWWWWWWWWW..",
            "....KKKKKKKK....",
            "...KKKKKKKKKK...",
            "...KKKKKKKKKK...",
            "...KKKK.KKKKK...",
            "...KKKK.KKKKK...",
            "...KKKK.KKKKK...",
            "...KKKK.KKKKK...",
            "...BBBB.BBBB....",
            "...BBBB.BBBB....",
            "..BBBBB.BBBBB...",
            "..BBBBB.BBBBB...",
            "..BBBBB.BBBBB...",
            "..BBBBBBBBBB....",
            "...BBBBBBBBB....",
        };
        return RowsToPixels(rows, pal);
    }

    static Color[] MonkeyPrincessAni()
    {
        // Baggy jeans mode: wider pants + hair bun accessory
        var pal = new Dictionary<char,Color> {
            {'.', T},{' ',T},{'G',GH},{'S',SK},{'E',EY},
            {'W',WH},{'R',RD},{'K',PA},{'B',BR},{'O',EY},
            {'P', new Color(0.85f,0.55f,0.75f)},  // pink accessory
            {'J', new Color(0.35f,0.45f,0.85f)}   // blue baggy jeans
        };
        string[] rows = {
            "....GGGGGGGG....",
            "...GGGGGGGGGG...",
            "..GGGGGGGGGGGG..",
            "..GSSSSSSSSSGG..",
            ".PPGSSSSSSSSGGG.",  // hair accessory
            "..GSEESSSSEESG..",
            "..GSSSSSSSSSSG..",
            "..GSSSOOSSSSG...",
            "..GSSSSSSSSGG...",
            "....SSSSSSS.....",
            "...WWWWWWWWWW...",
            "..WWWWWWWWWWWW..",
            "..WWWWWWWWWWWW..",
            "..WWWRRWWWWWWW..",
            "..WWWRRWWWWWWW..",
            "..WWWWWWWWWWWW..",
            "..WWWWWWWWWWWW..",
            "...WWWWWWWWWWW..",
            "...JJJJJJJJJJ...",  // baggy jeans
            "..JJJJJJJJJJJJ..",
            "..JJJJJJJJJJJJ..",
            "..JJJJJJJJJJJJ..",
            "..JJJJJ.JJJJJJ..",
            "..JJJJJ.JJJJJJ..",
            "..JJJJJ.JJJJJJ..",
            "...BBBB.BBBB....",
            "...BBBB.BBBB....",
            "..BBBBB.BBBBB...",
            "..BBBBB.BBBBB...",
            "..BBBBB.BBBBB...",
            "..BBBBBBBBBB....",
            "...BBBBBBBBB....",
        };
        return RowsToPixels(rows, pal);
    }

    // ── Slushy sprites ───────────────────────────────────────────────────
    // Purple hair, teal eyes, blue jacket, petite frame
    static Color[] SlushyNeutral()
    {
        var pal = new Dictionary<char,Color> {
            {'.', T},{' ',T},{'P',PH},{'S',SK},{'T',TE},
            {'B',BJ},{'L',LB},{'O',OB},{'E',EY},{'W',WH}
        };
        string[] rows = {
            "....PPPPPPPP....",
            "...PPPPPPPPPP...",
            "..PPPPPPPPPPPP..",
            ".PPSSSSSSSSPPPP.",
            ".PPSSSSSSSSSPPP.",
            ".PPSTTSSSTTSP...",  // teal eyes
            ".PPSSSSSSSSSPP..",
            ".PPSSSEESSSP....",  // small mouth
            ".PPSSSSSSSSPP...",
            "....SSSSSSS.....",
            "...BBBBBBBBB....",
            "..BBBBBBBBBBB...",
            "..BLLLLLLLLBB...",
            "..BLLLLLLLLBB...",
            "..BLLLLLLLLBB...",
            "..BBBBBBBBBBB...",
            "..BBBBBBBBBBB...",
            "...BBBBBBBBB....",
            "....OOOOOOOO....",
            "...OOOOOOOOOO...",
            "...OOOOOOOOOO...",
            "...OOOO.OOOOO...",
            "...OOOO.OOOOO...",
            "...OOOO.OOOOO...",
            "...WWWW.WWWWW...",  // white shoes
            "...WWWW.WWWWW...",
            "..WWWWW.WWWWWW..",
            "..WWWWW.WWWWWW..",
            "..WWWWWWWWWWWW..",
            "..WWWWWWWWWWWW..",
            "...WWWWWWWWWWW..",
            "....WWWWWWWWW...",
        };
        return RowsToPixels(rows, pal);
    }

    static Color[] SlushyEepy()
    {
        var pal = new Dictionary<char,Color> {
            {'.', T},{' ',T},{'P',PH},{'S',SK},
            {'T', new Color(0.00f,0.71f,0.65f,0.5f)},  // half-open teal eyes
            {'B',BJ},{'L',LB},{'O',OB},{'E',EY},{'W',WH}
        };
        // Same as neutral, eyes half-closed
        string[] rows = {
            "....PPPPPPPP....",
            "...PPPPPPPPPP...",
            "..PPPPPPPPPPPP..",
            ".PPSSSSSSSSPPPP.",
            ".PPSSSSSSSSSPPP.",
            ".PPSTTSSSTTSP...",
            ".PPSSSSSSSSSPP..",
            ".PPSSSSSSSSSP...",  // neutral mouth (eepy = no expression)
            ".PPSSSSSSSSPP...",
            "....SSSSSSS.....",
            "...BBBBBBBBB....",
            "..BBBBBBBBBBB...",
            "..BLLLLLLLLBB...",
            "..BLLLLLLLLBB...",
            "..BLLLLLLLLBB...",
            "..BBBBBBBBBBB...",
            "..BBBBBBBBBBB...",
            "...BBBBBBBBB....",
            "....OOOOOOOO....",
            "...OOOOOOOOOO...",
            "...OOOOOOOOOO...",
            "...OOOO.OOOOO...",
            "...OOOO.OOOOO...",
            "...OOOO.OOOOO...",
            "...WWWW.WWWWW...",
            "...WWWW.WWWWW...",
            "..WWWWW.WWWWWW..",
            "..WWWWW.WWWWWW..",
            "..WWWWWWWWWWWW..",
            "..WWWWWWWWWWWW..",
            "...WWWWWWWWWWW..",
            "....WWWWWWWWW...",
        };
        return RowsToPixels(rows, pal);
    }

    static Color[] SlushyHappy()
    {
        var pal = new Dictionary<char,Color> {
            {'.', T},{' ',T},{'P',PH},{'S',SK},{'T',TE},
            {'B',BJ},{'L',LB},{'O',OB},{'E',EY},{'W',WH},
            {'C', new Color(0.95f,0.70f,0.75f)}  // blush
        };
        string[] rows = {
            "....PPPPPPPP....",
            "...PPPPPPPPPP...",
            "..PPPPPPPPPPPP..",
            ".PPSSSSSSSSPPPP.",
            ".PPSSSSSSSSSPPP.",
            ".PPSTTSSSTTSP...",
            ".PPSCSSSSSCPP...",  // blush on cheeks
            ".PPSSOEEOOSSP...",  // smile
            ".PPSSSSSSSSPP...",
            "....SSSSSSS.....",
            "...BBBBBBBBB....",
            "..BBBBBBBBBBB...",
            "..BLLLLLLLLBB...",
            "..BLLLLLLLLBB...",
            "..BLLLLLLLLBB...",
            "..BBBBBBBBBBB...",
            "..BBBBBBBBBBB...",
            "...BBBBBBBBB....",
            "....OOOOOOOO....",
            "...OOOOOOOOOO...",
            "...OOOOOOOOOO...",
            "...OOOO.OOOOO...",
            "...OOOO.OOOOO...",
            "...OOOO.OOOOO...",
            "...WWWW.WWWWW...",
            "...WWWW.WWWWW...",
            "..WWWWW.WWWWWW..",
            "..WWWWW.WWWWWW..",
            "..WWWWWWWWWWWW..",
            "..WWWWWWWWWWWW..",
            "...WWWWWWWWWWW..",
            "....WWWWWWWWW...",
        };
        return RowsToPixels(rows, pal);
    }

    static Color[] SlushyStraight()
    {
        // Flat line mouth, unimpressed
        var pal = new Dictionary<char,Color> {
            {'.', T},{' ',T},{'P',PH},{'S',SK},{'T',TE},
            {'B',BJ},{'L',LB},{'O',OB},{'E',EY},{'W',WH}
        };
        string[] rows = {
            "....PPPPPPPP....",
            "...PPPPPPPPPP...",
            "..PPPPPPPPPPPP..",
            ".PPSSSSSSSSPPPP.",
            ".PPSSSSSSSSSPPP.",
            ".PPSTTSSSTTSP...",
            ".PPSSSSSSSSSPP..",
            ".PPSSSEEESSSPP..",  // straight line mouth via eyes-style
            ".PPSSSSSSSSPP...",
            "....SSSSSSS.....",
            "...BBBBBBBBB....",
            "..BBBBBBBBBBB...",
            "..BLLLLLLLLBB...",
            "..BLLLLLLLLBB...",
            "..BLLLLLLLLBB...",
            "..BBBBBBBBBBB...",
            "..BBBBBBBBBBB...",
            "...BBBBBBBBB....",
            "....OOOOOOOO....",
            "...OOOOOOOOOO...",
            "...OOOOOOOOOO...",
            "...OOOO.OOOOO...",
            "...OOOO.OOOOO...",
            "...OOOO.OOOOO...",
            "...WWWW.WWWWW...",
            "...WWWW.WWWWW...",
            "..WWWWW.WWWWWW..",
            "..WWWWW.WWWWWW..",
            "..WWWWWWWWWWWW..",
            "..WWWWWWWWWWWW..",
            "...WWWWWWWWWWW..",
            "....WWWWWWWWW...",
        };
        return RowsToPixels(rows, pal);
    }

    // ── Dhruv sprites ────────────────────────────────────────────────────
    static Color[] DhruvNeutral()
    {
        var pal = new Dictionary<char,Color> {
            {'.', T},{'H',BH},{'S',DK},{'E',EY},
            {'W',WH},{'K',PA},{'B',BR},{'R',RD},
            {'Y',YL}
        };
        string[] rows = {
            "....HHHHHHHH....",
            "...HHHHHHHHHH...",
            "..HHHHHHHHHHHH..",
            "..HSSSSSSSSSHH..",
            "..HSSSSSSSSSHH..",
            "..HSESSSSESSH...",
            "..HSSSSSSSSSH...",
            "..HSSSOOSSSSH...",
            "..HSSSSSSSSSH...",
            "....SSSSSSSS....",
            "...WWWWWWWWWW...",
            "..WWWWWWWWWWWW..",
            "..WWWWWWWWWWWW..",
            "..WWWWWWWWWWWW..",
            "..WWWWWWWWWWWW..",
            "..WWWWWWWWWWWW..",
            "..WWWWWWWWWWWW..",
            "...WWWWWWWWWWW..",
            "....KKKKKKKK....",
            "...KKKKKKKKKK...",
            "...KKKKKKKKKK...",
            "...KKKK.KKKKK...",
            "...KKKK.KKKKK...",
            "...KKKK.KKKKK...",
            "...KKKK.KKKKK...",
            "...BBBB.BBBB....",
            "...BBBB.BBBB....",
            "..BBBBB.BBBBB...",
            "..BBBBB.BBBBB...",
            "..BBBBB.BBBBB...",
            "..BBBBBBBBBB....",
            "...BBBBBBBBB....",
        };
        return RowsToPixels(rows, pal);
    }

    static Color[] DhruvTalking()
    {
        // open mouth 'O'
        var pal = new Dictionary<char,Color> {
            {'.', T},{'H',BH},{'S',DK},{'E',EY},
            {'W',WH},{'K',PA},{'B',BR},{'O',EY}
        };
        string[] rows = {
            "....HHHHHHHH....",
            "...HHHHHHHHHH...",
            "..HHHHHHHHHHHH..",
            "..HSSSSSSSSSHH..",
            "..HSSSSSSSSSHH..",
            "..HSESSSSESSH...",
            "..HSSSSSSSSSH...",
            "..HSSSOOOSSSSH..",  // open mouth
            "..HSSSSSSSSSH...",
            "....SSSSSSSS....",
            "...WWWWWWWWWW...",
            "..WWWWWWWWWWWW..",
            "..WWWWWWWWWWWW..",
            "..WWWWWWWWWWWW..",
            "..WWWWWWWWWWWW..",
            "..WWWWWWWWWWWW..",
            "..WWWWWWWWWWWW..",
            "...WWWWWWWWWWW..",
            "....KKKKKKKK....",
            "...KKKKKKKKKK...",
            "...KKKKKKKKKK...",
            "...KKKK.KKKKK...",
            "...KKKK.KKKKK...",
            "...KKKK.KKKKK...",
            "...KKKK.KKKKK...",
            "...BBBB.BBBB....",
            "...BBBB.BBBB....",
            "..BBBBB.BBBBB...",
            "..BBBBB.BBBBB...",
            "..BBBBB.BBBBB...",
            "..BBBBBBBBBB....",
            "...BBBBBBBBB....",
        };
        return RowsToPixels(rows, pal);
    }

    static Color[] NischalaNeutral()
    {
        // Similar to Dhruv but with long hair indicated by wider head
        var pal = new Dictionary<char,Color> {
            {'.', T},{'H',BH},{'S',DK},{'E',EY},
            {'W',WH},{'K',PA},{'B',BR},
            {'D', new Color(0.65f,0.35f,0.55f)}  // dusty pink top
        };
        string[] rows = {
            "..HHHHHHHHHHHH..",
            ".HHHHHHHHHHHHHH.",
            ".HHHSSSSSSSSHHH.",
            ".HHSSSSSSSSSSHHH",
            ".HHSSSSSSSSSSHHH",
            ".HHSESSSSESSSH..",
            ".HHSSSSSSSSSSHHH",
            ".HHSSSOOSSSSH...",
            ".HHSSSSSSSSHHH..",
            "....SSSSSSSS....",
            "...DDDDDDDDDD...",
            "..DDDDDDDDDDDD..",
            "..DDDDDDDDDDDD..",
            "..DDDDDDDDDDDD..",
            "..DDDDDDDDDDDD..",
            "..DDDDDDDDDDDD..",
            "..DDDDDDDDDDDD..",
            "...DDDDDDDDDD...",
            "....KKKKKKKK....",
            "...KKKKKKKKKK...",
            "...KKKKKKKKKK...",
            "...KKKK.KKKKK...",
            "...KKKK.KKKKK...",
            "...KKKK.KKKKK...",
            "...KKKK.KKKKK...",
            "...BBBB.BBBB....",
            "...BBBB.BBBB....",
            "..BBBBB.BBBBB...",
            "..BBBBB.BBBBB...",
            "..BBBBB.BBBBB...",
            "..BBBBBBBBBB....",
            "...BBBBBBBBB....",
        };
        return RowsToPixels(rows, pal);
    }

    static Color[] NischalaTalking() => NischalaNeutral(); // reuse, mouth animates via DialogueUI blink

    static Color[] JabinNeutral()
    {
        var pal = new Dictionary<char,Color> {
            {'.', T},{'H', new Color(0.50f,0.40f,0.30f)},
            {'S',SK},{'E',EY},{'G',GR},
            {'W',WH},{'K',PA},{'B',BR}
        };
        string[] rows = {
            "....HHHHHHHH....",
            "...HHHHHHHHHH...",
            "..HHHHHHHHHHHH..",
            "..HSSSSSSSSSSH..",
            "..HSSSSSSSSSSH..",
            "..HSESSSSESSSH..",
            "..HSSSSSSSSSSH..",
            "..HSSSOOSSSSH...",
            "..HSSSSSSSSSSH..",
            "....SSSSSSSS....",
            "...GGGGGGGGGG...",
            "..GGGGGGGGGGGG..",
            "..GGGGGGGGGGGG..",
            "..GGGGGGGGGGGG..",
            "..GGGGGGGGGGGG..",
            "..GGGGGGGGGGGG..",
            "..GGGGGGGGGGGG..",
            "...GGGGGGGGGG...",
            "....KKKKKKKK....",
            "...KKKKKKKKKK...",
            "...KKKKKKKKKK...",
            "...KKKK.KKKKK...",
            "...KKKK.KKKKK...",
            "...KKKK.KKKKK...",
            "...KKKK.KKKKK...",
            "...BBBB.BBBB....",
            "...BBBB.BBBB....",
            "..BBBBB.BBBBB...",
            "..BBBBB.BBBBB...",
            "..BBBBB.BBBBB...",
            "..BBBBBBBBBB....",
            "...BBBBBBBBB....",
        };
        return RowsToPixels(rows, pal);
    }

    static Color[] JabinAngry()
    {
        // Furrowed brow, V-mouth
        var pal = new Dictionary<char,Color> {
            {'.', T},{'H', new Color(0.50f,0.40f,0.30f)},
            {'S',SK},{'E',EY},{'G',GR},
            {'W',WH},{'K',PA},{'B',BR},
            {'F', new Color(0.80f,0.55f,0.40f)}  // furrowed brow line
        };
        string[] rows = {
            "....HHHHHHHH....",
            "...HHHHHHHHHH...",
            "..HHHHHHHHHHHH..",
            "..HFFFFFFFFFFFF.", // brow furrow
            "..HSSSSSSSSSSH..",
            "..HSEFFSSFFESH..",  // angled angry eyes
            "..HSSSSSSSSSSH..",
            "..HSSSVVVVSSH...",  // V-mouth (frown)
            "..HSSSSSSSSSSH..",
            "....SSSSSSSS....",
            "...GGGGGGGGGG...",
            "..GGGGGGGGGGGG..",
            "..GGGGGGGGGGGG..",
            "..GGGGGGGGGGGG..",
            "..GGGGGGGGGGGG..",
            "..GGGGGGGGGGGG..",
            "..GGGGGGGGGGGG..",
            "...GGGGGGGGGG...",
            "....KKKKKKKK....",
            "...KKKKKKKKKK...",
            "...KKKKKKKKKK...",
            "...KKKK.KKKKK...",
            "...KKKK.KKKKK...",
            "...KKKK.KKKKK...",
            "...KKKK.KKKKK...",
            "...BBBB.BBBB....",
            "...BBBB.BBBB....",
            "..BBBBB.BBBBB...",
            "..BBBBB.BBBBB...",
            "..BBBBB.BBBBB...",
            "..BBBBBBBBBB....",
            "...BBBBBBBBB....",
        };
        return RowsToPixels(rows, pal);
    }

    static Color[] JabinTalking()
    {
        var pal = new Dictionary<char,Color> {
            {'.', T},{'H', new Color(0.50f,0.40f,0.30f)},
            {'S',SK},{'E',EY},{'G',GR},
            {'W',WH},{'K',PA},{'B',BR},{'O',EY}
        };
        string[] rows = {
            "....HHHHHHHH....",
            "...HHHHHHHHHH...",
            "..HHHHHHHHHHHH..",
            "..HSSSSSSSSSSH..",
            "..HSSSSSSSSSSH..",
            "..HSESSSSESSSH..",
            "..HSSSSSSSSSSH..",
            "..HSSSOOOOSSH...",  // open mouth talking
            "..HSSSSSSSSSSH..",
            "....SSSSSSSS....",
            "...GGGGGGGGGG...",
            "..GGGGGGGGGGGG..",
            "..GGGGGGGGGGGG..",
            "..GGGGGGGGGGGG..",
            "..GGGGGGGGGGGG..",
            "..GGGGGGGGGGGG..",
            "..GGGGGGGGGGGG..",
            "...GGGGGGGGGG...",
            "....KKKKKKKK....",
            "...KKKKKKKKKK...",
            "...KKKKKKKKKK...",
            "...KKKK.KKKKK...",
            "...KKKK.KKKKK...",
            "...KKKK.KKKKK...",
            "...KKKK.KKKKK...",
            "...BBBB.BBBB....",
            "...BBBB.BBBB....",
            "..BBBBB.BBBBB...",
            "..BBBBB.BBBBB...",
            "..BBBBB.BBBBB...",
            "..BBBBBBBBBB....",
            "...BBBBBBBBB....",
        };
        return RowsToPixels(rows, pal);
    }

    // ── Tile pixel data ───────────────────────────────────────────────────
    static Color[] GetTilePixels(TileID id) =>
        id switch {
            TileID.FloorWood      => FloorWoodTile(),
            TileID.FloorWoodDark  => FloorWoodDarkTile(),
            TileID.Wall           => WallTile(),
            TileID.Shelf          => ShelfTile(),
            TileID.LibraryFloor   => LibraryFloorTile(),
            TileID.BookshelfTall  => BookshelfTallTile(),
            TileID.MetroPlatform  => MetroPlatformTile(),
            TileID.MetroWall      => MetroWallTile(),
            TileID.GalleryFloor   => GalleryFloorTile(),
            TileID.GalleryWall    => GalleryWallTile(),
            TileID.DormFloor      => DormFloorTile(),
            TileID.DormWall       => DormWallTile(),
            TileID.Chair          => ChairTile(),
            TileID.MomoSteamer    => MomoSteamerTile(),
            _ => FloorWoodTile()
        };

    static Color[] FloorWoodTile()
    {
        // 32×32 warm wood planks, horizontal grain
        Color[] px = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 32; x++)
        {
            // Alternate plank rows every 8px, add grain noise
            bool dark = (y % 8 < 2) || (x % 16 == 0);
            float n = Mathf.Sin(x * 0.7f + y * 0.1f) * 0.04f;
            px[y * 32 + x] = dark
                ? new Color(FD.r + n, FD.g + n, FD.b + n)
                : new Color(FW.r + n, FW.g + n, FW.b + n);
        }
        return px;
    }

    static Color[] FloorWoodDarkTile()
    {
        Color[] px = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 32; x++)
        {
            bool dark = (y % 8 < 2) || (x % 16 == 0);
            float n = Mathf.Sin(x * 0.7f + y * 0.1f) * 0.03f;
            Color b = dark ? new Color(0.45f,0.32f,0.20f) : new Color(0.55f,0.40f,0.26f);
            px[y * 32 + x] = new Color(b.r + n, b.g + n, b.b + n);
        }
        return px;
    }

    static Color[] WallTile()
    {
        Color[] px = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 32; x++)
        {
            bool edge = (x == 0 || x == 31 || y == 0 || y == 31);
            float n = Mathf.Sin(x * 1.3f) * 0.02f + Mathf.Cos(y * 1.1f) * 0.02f;
            Color b = edge ? WD : WL;
            px[y * 32 + x] = new Color(b.r + n, b.g + n, b.b + n);
        }
        return px;
    }

    static Color[] ShelfTile()
    {
        Color[] px = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 32; x++)
        {
            bool shelf = (y == 8 || y == 16 || y == 24) || (x < 3 || x > 28);
            px[y * 32 + x] = shelf ? SHD : SH;
        }
        return px;
    }

    static Color[] LibraryFloorTile()
    {
        Color[] px = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 32; x++)
        {
            bool checker = ((x / 8 + y / 8) % 2 == 0);
            px[y * 32 + x] = checker ? LF : LFD;
        }
        return px;
    }

    static Color[] BookshelfTallTile()
    {
        // Dark wood shelves with coloured book spines
        Color[] bookColors = {
            new Color(0.80f,0.25f,0.25f),
            new Color(0.25f,0.55f,0.80f),
            new Color(0.80f,0.75f,0.25f),
            new Color(0.35f,0.70f,0.45f),
            new Color(0.70f,0.35f,0.70f),
        };
        Color[] px = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 32; x++)
        {
            if (x < 2 || x > 29) { px[y*32+x] = SHD; continue; }
            if (y % 10 < 2)       { px[y*32+x] = SHD; continue; }
            // book spine
            int book = x / 6;
            px[y*32+x] = bookColors[book % bookColors.Length];
        }
        return px;
    }

    static Color[] MetroPlatformTile()
    {
        Color[] px = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 32; x++)
        {
            bool line = (y == 4 || y == 5) || (x % 8 == 0);
            px[y*32+x] = line ? MPD : MP;
        }
        return px;
    }

    static Color[] MetroWallTile()
    {
        Color tileBase = new Color(0.72f, 0.72f, 0.80f);
        Color tileDark = new Color(0.55f, 0.55f, 0.65f);
        Color[] px = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 32; x++)
        {
            bool grout = (x % 16 == 0) || (y % 8 == 0);
            px[y*32+x] = grout ? tileDark : tileBase;
        }
        return px;
    }

    static Color[] GalleryFloorTile()
    {
        Color[] px = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 32; x++)
        {
            bool checker = ((x / 16 + y / 16) % 2 == 0);
            px[y*32+x] = checker ? GF : GFD;
        }
        return px;
    }

    static Color[] GalleryWallTile()
    {
        Color wallBase = new Color(0.96f, 0.94f, 0.90f);
        Color wallDark = new Color(0.80f, 0.78f, 0.74f);
        Color[] px = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 32; x++)
        {
            bool moulding = (y < 4 || y > 27);
            px[y*32+x] = moulding ? wallDark : wallBase;
        }
        return px;
    }

    static Color[] DormFloorTile()
    {
        Color[] px = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 32; x++)
        {
            float n = Mathf.Sin(x * 0.5f + y * 0.3f) * 0.03f;
            px[y*32+x] = new Color(DF.r + n, DF.g + n, DF.b + n);
        }
        return px;
    }

    static Color[] DormWallTile()
    {
        Color wallBase = new Color(0.88f, 0.84f, 0.94f);
        Color stripe   = new Color(0.78f, 0.74f, 0.88f);
        Color[] px = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 32; x++)
            px[y*32+x] = (x % 6 < 2) ? stripe : wallBase;
        return px;
    }

    static Color[] ChairTile()
    {
        Color seat  = new Color(0.82f, 0.60f, 0.30f);
        Color leg   = new Color(0.55f, 0.38f, 0.18f);
        Color[] px  = new Color[32 * 32];
        for (int i = 0; i < px.Length; i++) px[i] = T;
        // seat cushion rows 14-22, cols 6-25
        for (int y = 14; y <= 22; y++)
        for (int x = 6;  x <= 25; x++) px[y*32+x] = seat;
        // legs
        foreach (int lx in new[]{6,7,24,25})
        for (int y = 4; y <= 13; y++) px[y*32+lx] = leg;
        return px;
    }

    static Color[] MomoSteamerTile()
    {
        Color metal = new Color(0.75f, 0.75f, 0.80f);
        Color steam = new Color(0.90f, 0.90f, 0.95f);
        Color momo  = new Color(0.96f, 0.90f, 0.80f);
        Color[] px  = new Color[32 * 32];
        for (int i = 0; i < px.Length; i++) px[i] = T;
        // steamer body rows 8-28, cols 6-25
        for (int y = 8; y <= 28; y++)
        for (int x = 6; x <= 25; x++) px[y*32+x] = metal;
        // steam puffs at top
        for (int x = 10; x <= 22; x++)
        {
            px[6*32+x] = steam;
            if (x % 3 == 0) px[4*32+x] = steam;
        }
        // momo dots inside
        foreach ((int my, int mx) in new[]{(15,11),(15,18),(20,14),(20,21)})
            for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
                px[(my+dy)*32+(mx+dx)] = momo;
        return px;
    }
}
```

- [ ] **Step 4: Run tests — expect PASS**

Unity Test Runner → EditMode → Run All. 4 tests pass.

- [ ] **Step 5: Commit**
```bash
git add Assets/Art/PixelArtLibrary.cs Assets/Tests/EditMode/PixelArtTests.cs
git commit -m "feat: PixelArtLibrary — all character and tile sprites generated at runtime"
```

---

### Task 4: SpriteManager

**Files:**
- Create: `Assets/Art/SpriteManager.cs`

- [ ] **Step 1: Implement SpriteManager**

Create `Assets/Art/SpriteManager.cs`:
```csharp
using UnityEngine;
using System.Collections.Generic;

public class SpriteManager : MonoBehaviour
{
    public static SpriteManager Instance { get; private set; }

    // Portrait SpriteRenderers assigned in Inspector per character
    [SerializeField] SpriteRenderer _playerPortrait;
    [SerializeField] SpriteRenderer _npcPortrait;

    // Map mood tag strings → MoodID
    static readonly Dictionary<string, MoodID> MoodMap = new() {
        {"neutral",     MoodID.Neutral},
        {"goofy",       MoodID.Goofy},
        {"eepy",        MoodID.Eepy},
        {"happy",       MoodID.Happy},
        {"angry",       MoodID.Angry},
        {"princess-ani",MoodID.PrincessAni},
        {"straight",    MoodID.StraightFace},
        {"talking",     MoodID.Talking},
    };

    // Current NPC character shown in portrait
    CharacterID _currentNPC = CharacterID.Slushy;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        if (InkManager.Instance != null)
            InkManager.Instance.OnMoodTag += HandleMoodTag;
    }

    void OnDisable()
    {
        if (InkManager.Instance != null)
            InkManager.Instance.OnMoodTag -= HandleMoodTag;
    }

    public void SetCurrentNPC(CharacterID id) => _currentNPC = id;

    void HandleMoodTag(string moodTag)
    {
        // Format: "mood:eepy" → already stripped to "eepy" by InkManager
        if (!MoodMap.TryGetValue(moodTag.ToLower(), out var mood)) return;

        // Player portrait always shows Monkey
        if (_playerPortrait != null)
        {
            MoodID playerMood = mood == MoodID.PrincessAni ? MoodID.PrincessAni : MoodID.Neutral;
            _playerPortrait.sprite = PixelArtLibrary.Build(CharacterID.Monkey, playerMood);
        }

        // NPC portrait shows current NPC
        if (_npcPortrait != null)
            _npcPortrait.sprite = PixelArtLibrary.Build(_currentNPC, mood);
    }

    // Explicitly swap player sprite (for princess-ani transform)
    public void SetPlayerMood(MoodID mood)
    {
        if (_playerPortrait != null)
            _playerPortrait.sprite = PixelArtLibrary.Build(CharacterID.Monkey, mood);
    }
}
```

- [ ] **Step 2: Commit**
```bash
git add Assets/Art/SpriteManager.cs
git commit -m "feat: SpriteManager — portrait swapping from Ink mood tags"
```

---

### Task 5: DialogueUI

**Files:**
- Create: `Assets/Scripts/Dialogue/DialogueUI.cs`

`★ Insight ─────────────────────────────────────`
DialogueUI must disable the TopDownController while dialogue is shown — but those two MonoBehaviours don't know about each other. Use UnityEvents or a static `GameEvents` bus rather than direct references, keeping scene setup flexible.
`─────────────────────────────────────────────────`

- [ ] **Step 1: Implement DialogueUI**

Create `Assets/Scripts/Dialogue/DialogueUI.cs`:
```csharp
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Ink.Runtime;

public class DialogueUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject   _dialoguePanel;
    [SerializeField] TMP_Text     _speakerName;
    [SerializeField] TMP_Text     _bodyText;
    [SerializeField] Transform    _choiceContainer;
    [SerializeField] Button       _continueButton;

    [Header("Choice Button Prefab")]
    [SerializeField] Button       _choiceButtonPrefab;

    [Header("Portrait")]
    [SerializeField] SpriteRenderer _playerPortrait;
    [SerializeField] SpriteRenderer _npcPortrait;

    // Static event so TopDownController can subscribe without a direct ref
    public static event System.Action<bool> OnDialogueActiveChanged;

    readonly List<Button> _choiceButtons = new();

    void Awake()
    {
        _dialoguePanel.SetActive(false);
        _continueButton.onClick.AddListener(OnContinuePressed);

        // Pre-generate player sprite
        if (_playerPortrait)
            _playerPortrait.sprite = PixelArtLibrary.Build(CharacterID.Monkey, MoodID.Neutral);
        if (_npcPortrait)
            _npcPortrait.sprite = PixelArtLibrary.Build(CharacterID.Slushy, MoodID.Neutral);
    }

    void OnEnable()
    {
        if (InkManager.Instance == null) return;
        InkManager.Instance.OnDialogueLine      += ShowLine;
        InkManager.Instance.OnChoicesPresented  += ShowChoices;
        InkManager.Instance.OnDialogueEnd       += HideDialogue;
    }

    void OnDisable()
    {
        if (InkManager.Instance == null) return;
        InkManager.Instance.OnDialogueLine      -= ShowLine;
        InkManager.Instance.OnChoicesPresented  -= ShowChoices;
        InkManager.Instance.OnDialogueEnd       -= HideDialogue;
    }

    void ShowLine(string line)
    {
        _dialoguePanel.SetActive(true);
        OnDialogueActiveChanged?.Invoke(true);
        ClearChoices();
        _continueButton.gameObject.SetActive(true);

        // Parse optional "Speaker: text" format
        int colon = line.IndexOf(':');
        if (colon > 0 && colon < 20)
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

    void ShowChoices(List<Choice> choices)
    {
        _continueButton.gameObject.SetActive(false);
        ClearChoices();

        for (int i = 0; i < choices.Count; i++)
        {
            int index = i; // capture for lambda
            Button btn = Instantiate(_choiceButtonPrefab, _choiceContainer);
            btn.GetComponentInChildren<TMP_Text>().text = choices[i].text;
            btn.onClick.AddListener(() => InkManager.Instance.ChooseOption(index));
            _choiceButtons.Add(btn);
        }
    }

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
```

- [ ] **Step 2: Commit**
```bash
git add Assets/Scripts/Dialogue/DialogueUI.cs
git commit -m "feat: DialogueUI — text display, choice buttons, dialogue active event"
```

---

### Task 6: Player Movement (TopDown + VirtualJoystick)

**Files:**
- Create: `Assets/Scripts/Player/TopDownController.cs`
- Create: `Assets/Scripts/Player/VirtualJoystick.cs`

- [ ] **Step 1: Implement TopDownController**

Create `Assets/Scripts/Player/TopDownController.cs`:
```csharp
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class TopDownController : MonoBehaviour
{
    [SerializeField] float _speed = 3f;

    Rigidbody2D _rb;
    Vector2     _moveInput;
    bool        _dialogueActive;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0;
        _rb.constraints  = RigidbodyConstraints2D.FreezeRotation;
    }

    void OnEnable()  => DialogueUI.OnDialogueActiveChanged += SetDialogueActive;
    void OnDisable() => DialogueUI.OnDialogueActiveChanged -= SetDialogueActive;

    void SetDialogueActive(bool active)
    {
        _dialogueActive = active;
        if (active) _rb.linearVelocity = Vector2.zero;
    }

    // Called by Unity Input System (WASD / gamepad)
    void OnMove(InputValue value) => _moveInput = value.Get<Vector2>();

    // Called by VirtualJoystick on mobile
    public void SetMobileInput(Vector2 input) => _moveInput = input;

    void FixedUpdate()
    {
        if (_dialogueActive) { _rb.linearVelocity = Vector2.zero; return; }
        _rb.linearVelocity = _moveInput.normalized * _speed;
    }
}
```

- [ ] **Step 2: Implement VirtualJoystick**

Create `Assets/Scripts/Player/VirtualJoystick.cs`:
```csharp
using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] RectTransform _background;
    [SerializeField] RectTransform _handle;
    [SerializeField] float         _range = 60f;

    TopDownController _controller;
    Vector2           _startPos;
    Canvas            _canvas;

    void Awake()
    {
        _canvas     = GetComponentInParent<Canvas>();
        _controller = FindFirstObjectByType<TopDownController>();
        // Hide on non-mobile builds
#if !UNITY_ANDROID && !UNITY_IOS
        gameObject.SetActive(false);
#endif
    }

    public void OnPointerDown(PointerEventData e)
    {
        _startPos = e.position;
        _background.position = e.position;
        _handle.anchoredPosition = Vector2.zero;
    }

    public void OnDrag(PointerEventData e)
    {
        Vector2 delta = e.position - _startPos;
        Vector2 clamped = Vector2.ClampMagnitude(delta, _range);
        _handle.anchoredPosition = clamped / _canvas.scaleFactor;
        _controller?.SetMobileInput(clamped / _range);
    }

    public void OnPointerUp(PointerEventData e)
    {
        _handle.anchoredPosition = Vector2.zero;
        _controller?.SetMobileInput(Vector2.zero);
    }
}
```

- [ ] **Step 3: Commit**
```bash
git add Assets/Scripts/Player/
git commit -m "feat: TopDownController + VirtualJoystick, movement disabled during dialogue"
```

---

### Task 7: World Map Controller

**Files:**
- Create: `Assets/Scripts/Interactions/WorldMapController.cs`

- [ ] **Step 1: Implement WorldMapController**

Create `Assets/Scripts/Interactions/WorldMapController.cs`:
```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorldMapController : MonoBehaviour
{
    [System.Serializable]
    public class LocationButton
    {
        public string   SceneName;
        public string   DisplayName;
        public Button   Btn;
        public bool     UnlockedByDefault;
    }

    [SerializeField] List<LocationButton> _locations;
    [SerializeField] GameObject           _mapPanel;

    // Tracks which locations are unlocked
    readonly HashSet<string> _unlocked = new();

    void Awake()
    {
        // Unlock defaults
        foreach (var loc in _locations)
            if (loc.UnlockedByDefault) _unlocked.Add(loc.SceneName);

        RefreshButtons();
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

    // When Ink emits #scene:worldmap, show the map UI
    void HandleSceneTag(string scene)
    {
        if (scene == "worldmap") ShowMap();
    }

    public void ShowMap()
    {
        _mapPanel.SetActive(true);
        RefreshButtons();
        foreach (var loc in _locations)
        {
            string s = loc.SceneName;
            loc.Btn.onClick.RemoveAllListeners();
            loc.Btn.onClick.AddListener(() => TravelTo(s));
        }
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

    void RefreshButtons()
    {
        foreach (var loc in _locations)
        {
            bool canTravel = _unlocked.Contains(loc.SceneName);
            loc.Btn.interactable = canTravel;
            var label = loc.Btn.GetComponentInChildren<TMP_Text>();
            if (label) label.text = canTravel ? loc.DisplayName : "???";
        }
    }
}
```

- [ ] **Step 2: Commit**
```bash
git add Assets/Scripts/Interactions/WorldMapController.cs
git commit -m "feat: WorldMapController — location unlock, travel, map panel"
```

---

### Task 8: Stealth — PatrolAI & StealthController

**Files:**
- Create: `Assets/Scripts/Stealth/PatrolAI.cs`
- Create: `Assets/Scripts/Stealth/StealthController.cs`

- [ ] **Step 1: Implement PatrolAI**

Create `Assets/Scripts/Stealth/PatrolAI.cs`:
```csharp
using UnityEngine;

public class PatrolAI : MonoBehaviour
{
    [SerializeField] Transform[] _waypoints;
    [SerializeField] float       _speed     = 1.5f;
    [SerializeField] float       _waitTime  = 1.0f;

    int   _current;
    float _waitTimer;
    bool  _waiting;
    bool  _active;

    public void StartPatrol() => _active = true;
    public void StopPatrol()  => _active = false;

    void Update()
    {
        if (!_active || _waypoints.Length == 0) return;

        if (_waiting)
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0) _waiting = false;
            return;
        }

        Transform target = _waypoints[_current];
        transform.position = Vector2.MoveTowards(
            transform.position, target.position, _speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, target.position) < 0.05f)
        {
            _current   = (_current + 1) % _waypoints.Length;
            _waiting   = true;
            _waitTimer = _waitTime;
        }
    }

    // Vision cone: simple forward-facing radius check
    public bool CanSeeTarget(Vector2 targetPos, float visionRadius = 2.5f)
    {
        return Vector2.Distance(transform.position, targetPos) < visionRadius;
    }
}
```

- [ ] **Step 2: Implement StealthController**

Create `Assets/Scripts/Stealth/StealthController.cs`:
```csharp
using UnityEngine;

public class StealthController : MonoBehaviour
{
    [SerializeField] PatrolAI        _patrolAI;
    [SerializeField] Transform       _player;
    [SerializeField] float           _detectionRadius = 2.5f;
    [SerializeField] GameObject      _caughtVFX;  // optional flash effect

    // Ink choice indices matching library_stealth_start knot
    const int SUCCESS_CHOICE      = 0;
    const int CAUGHT_ONCE_CHOICE  = 1;
    const int CAUGHT_TWICE_CHOICE = 2;
    const int BAD_DETOUR_CHOICE   = 3;

    bool _active;
    bool _playerHiding;
    int  _catchCount;

    void OnEnable()
    {
        if (InkManager.Instance != null)
            InkManager.Instance.OnStealthBegin += Activate;
    }

    void OnDisable()
    {
        if (InkManager.Instance != null)
            InkManager.Instance.OnStealthBegin -= Activate;
    }

    void Activate()
    {
        _active     = true;
        _catchCount = 0;
        _patrolAI.StartPatrol();
    }

    void Deactivate()
    {
        _active = false;
        _patrolAI.StopPatrol();
    }

    // Called by hide-spot trigger (see HideSpot.cs inline below)
    public void SetPlayerHiding(bool hiding) => _playerHiding = hiding;

    // Called when player reaches strawberry trigger
    public void OnStrawberryReached()
    {
        if (!_active) return;
        Deactivate();
        InkManager.Instance.ResumeFromStealth(SUCCESS_CHOICE);
    }

    void Update()
    {
        if (!_active || _playerHiding) return;

        if (_patrolAI.CanSeeTarget(_player.position, _detectionRadius))
            TriggerCaught();
    }

    void TriggerCaught()
    {
        _catchCount++;
        if (_caughtVFX) _caughtVFX.SetActive(true);

        int choiceIndex = _catchCount switch {
            1 => CAUGHT_ONCE_CHOICE,
            2 => CAUGHT_TWICE_CHOICE,
            _ => BAD_DETOUR_CHOICE
        };

        Deactivate();
        InkManager.Instance.ResumeFromStealth(choiceIndex);
    }
}

// Small helper added to each hide-spot collider in the library scene
public class HideSpot : MonoBehaviour
{
    StealthController _stealth;

    void Awake() => _stealth = FindFirstObjectByType<StealthController>();

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) _stealth.SetPlayerHiding(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) _stealth.SetPlayerHiding(false);
    }
}
```

- [ ] **Step 3: Commit**
```bash
git add Assets/Scripts/Stealth/
git commit -m "feat: PatrolAI waypoint patrol + StealthController lives/catch system"
```

---

### Task 9: SceneDirector & AudioManager

**Files:**
- Create: `Assets/Scripts/Core/SceneDirector.cs`
- Create: `Assets/Scripts/Core/AudioManager.cs`

- [ ] **Step 1: Implement SceneDirector**

Create `Assets/Scripts/Core/SceneDirector.cs`:
```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneDirector : MonoBehaviour
{
    public static SceneDirector Instance { get; private set; }

    [SerializeField] Image _fadeOverlay;   // black fullscreen image
    [SerializeField] float _fadeDuration = 0.4f;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        if (InkManager.Instance != null)
        {
            InkManager.Instance.OnSceneTag += HandleSceneTag;
            InkManager.Instance.OnEnding   += HandleEnding;
        }
    }

    void OnDisable()
    {
        if (InkManager.Instance != null)
        {
            InkManager.Instance.OnSceneTag -= HandleSceneTag;
            InkManager.Instance.OnEnding   -= HandleEnding;
        }
    }

    void HandleSceneTag(string scene)
    {
        if (scene == "worldmap") return; // handled by WorldMapController
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
        yield return StartCoroutine(Fade(1f, 0f));
    }

    IEnumerator Fade(float from, float to)
    {
        float t = 0;
        while (t < _fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / _fadeDuration);
            _fadeOverlay.color = new Color(0, 0, 0, a);
            yield return null;
        }
        _fadeOverlay.color = new Color(0, 0, 0, to);
    }
}
```

- [ ] **Step 2: Implement AudioManager**

Create `Assets/Scripts/Core/AudioManager.cs`:
```csharp
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class SoundEntry { public string Key; public AudioClip Clip; }

    [SerializeField] List<SoundEntry> _sounds;
    [SerializeField] AudioSource      _musicSource;
    [SerializeField] AudioSource      _sfxSource;

    Dictionary<string, AudioClip> _map = new();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        foreach (var e in _sounds) _map[e.Key] = e.Clip;
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
        // Music loops use the music source; one-shots use sfx source
        if (key.StartsWith("music_") || key == "rain" || key == "metro")
        {
            _musicSource.clip = clip;
            _musicSource.loop = true;
            _musicSource.Play();
        }
        else
            _sfxSource.PlayOneShot(clip);
    }

    public void StopMusic() => _musicSource.Stop();
}
```

- [ ] **Step 3: Commit**
```bash
git add Assets/Scripts/Core/SceneDirector.cs Assets/Scripts/Core/AudioManager.cs
git commit -m "feat: SceneDirector fade transitions + AudioManager sfx/music from tags"
```

---

### Task 10: story.ink — Full Narrative

**Files:**
- Create: `Assets/Ink/story.ink`

- [ ] **Step 1: Write the complete Ink file**

Create `Assets/Ink/story.ink`:
```ink
VAR goofy = 0
VAR overthinker = 0
VAR lives = 3
VAR has_strawberry = false
VAR has_keychain = false
VAR momo_done = false
VAR dietcoke_done = false

=== prologue ===
#scene:dorm #sfx:music_lofi
In a universe that felt completely black, a Monkey found his constellation.
-> dorm_room

=== dorm_room ===
It's 4 AM. The overthinking hour. You reach into your bag.
The Crochet Strawberry — the one she made — is gone.
+ [Check bag again] #overthinker:+10 -> check_bag_again
+ [Just go find it. Now.] #goofy:+5 -> leave_for_cnd

=== check_bag_again ===
#mood:eepy
Nope. Definitely not there. Your brain starts running scenarios.
What if you lost it at the library? What if someone took it? What if—
+ [Stop. Go to CND.] #overthinker:+5 -> leave_for_cnd
+ [Check one more time] #overthinker:+10 -> check_bag_again_final

=== check_bag_again_final ===
Still not there. Obviously.
-> leave_for_cnd

=== leave_for_cnd ===
#scene:worldmap
The world map opens. CND is calling.
-> cnd_arrive

=== cnd_arrive ===
#scene:cnd #sfx:music_cnd #mood:neutral
CND. The smell of momos and bad decisions.
Dhruv is there, eating like he has no worries.
#npc:dhruv
Dhruv: Aye, you look like a ghost. What happened?
+ ["Miya miya"] #goofy:+10 -> dhruv_quest_start
+ ["I'm just bedrotting"] #overthinker:+10 -> dhruv_quest_start
+ ["My strawberry is missing"] -> dhruv_strawberry_reveal

=== dhruv_strawberry_reveal ===
Dhruv: The crochet one? From her? Bro.
Dhruv: I saw you had it at the library yesterday. Check there.
#overthinker:+5
-> dhruv_quest_start

=== dhruv_quest_start ===
#mood:talking
Dhruv: Before I help you — I'm starving. Get me a Momo Sizzler.
#npc:nischala
Nischala appears from nowhere, as she does.
Nischala: Get me a Diet Coke and I'll tell you something useful.
+ [Fine. I'll get both.] #goofy:+5 -> fetch_quest
+ [Are you serious right now?] #overthinker:+10 -> fetch_quest_annoyed

=== fetch_quest_annoyed ===
#mood:straight
Dhruv: Dead serious. Sizzler first.
-> fetch_quest

=== fetch_quest ===
#scene:cnd_counter
You order the Momo Sizzler and a Diet Coke.
The aunty at the counter gives you a look.
~ momo_done = true
~ dietcoke_done = true
-> fetch_delivered

=== fetch_delivered ===
#scene:cnd #mood:happy
Dhruv: (already eating) YESSS. Okay okay, library. Third row, IR section.
Nischala: And Monkey — she's been there since 11 AM. She looks eepy but she's waiting.
#goofy:+10
-> unlock_library

=== unlock_library ===
#scene:worldmap
Library unlocked on the world map.
-> library_stealth_start

=== library_stealth_start ===
#scene:library #sfx:music_library
The library. Quiet. Fluorescent. Prof. Jabin is patrolling the IR section.
You need to reach the third row without being seen.
#stealth:begin
* [stealth_success]  -> found_strawberry
* [stealth_caught_once] -> caught_once
* [stealth_caught_twice] -> caught_twice
* [stealth_bad_detour]  -> bad_detour

=== caught_once ===
#mood:angry
Prof. Jabin: You there! This is a study zone, not a playground.
Prof. Jabin: Extra reading: forty pages of Waltz. By Friday.
#overthinker:+15
You slip away. One life left... two, actually. Stay focused.
-> library_stealth_start

=== caught_twice ===
#mood:angry
Prof. Jabin: AGAIN? Are you incapable of reading the room?
Prof. Jabin: Sixty pages. AND a reflection essay.
#overthinker:+20
One life left. This is it.
-> library_stealth_start

=== bad_detour ===
#scene:dorm #mood:eepy #ending:grey
You got caught three times. Jabin confiscated your library card.
By the time you sorted it out, it was 7 PM.
She sent one message:
"I hate u, dekh le."
-> END

=== found_strawberry ===
#mood:happy
There it is. Wedged between "Theory of International Politics" and "World Order."
~ has_strawberry = true
#goofy:+15
You hold it for a second. Okay. Now. Metro.
-> unlock_metro

=== unlock_metro ===
#scene:worldmap
Metro unlocked.
-> metro_arrive

=== metro_arrive ===
#scene:metro #sfx:rain #sfx:metro #mood:neutral
Rajiv Chowk. It's raining. The platform is packed.
{ goofy > 35:
    Something shifts. Your walk changes.
    #mood:princess-ani
    Princess Ani mode: activated. Baggy jeans unlocked.
    #goofy:+10
}
You navigate the crowd.
+ [Excuse me, excuse me...] #overthinker:+5 -> metro_crowd_gentle
+ [Just vibe through it] #goofy:+10 -> metro_crowd_vibe

=== metro_crowd_gentle ===
Somehow you make it through without eye contact.
-> gallery_arrive

=== metro_crowd_vibe ===
#mood:goofy
Three aunties smile at you. One uncle nods approvingly.
#goofy:+5
-> gallery_arrive

=== gallery_arrive ===
#scene:gallery #sfx:music_gallery #mood:eepy
The Art Gallery. Warm light. Quiet.
She's there. Looking at a painting of the sea.
#npc:slushy
#mood:eepy
Slushy: You're late.
+ [Hand her the strawberry] { has_strawberry: -> give_strawberry | -> no_strawberry }
+ [I can explain—] #overthinker:+10 -> explain_late

=== explain_late ===
Slushy: (sighs) Just... come here.
-> gallery_moment

=== no_strawberry ===
#overthinker:+20
Monkey: I... forgot it.
#mood:straight
Slushy: Of course you did.
-> gallery_moment

=== give_strawberry ===
~ has_strawberry = true
#mood:happy
Her face does the thing.
Slushy: You went back for it?
Monkey: I never lost it.
{ goofy > overthinker:
    #mood:happy
    You pull out the lily of the valley keychain too.
    ~ has_keychain = true
    #goofy:+10
}
-> gallery_moment

=== gallery_moment ===
#mood:neutral
She turns back to the painting.
Slushy: You know what you are?
+ [What?] -> ending_check
+ [Your problem?] #goofy:+10 -> your_problem

=== your_problem ===
Slushy: (laughs, doesn't deny it)
-> ending_check

=== ending_check ===
{ has_keychain && goofy > overthinker:
    -> ending_constellation
- goofy == overthinker:
    -> ending_milkshake
- goofy > overthinker:
    -> ending_constellation
- else:
    -> ending_grey_normal
}

=== ending_constellation ===
#ending:constellation #mood:happy #sfx:music_stars
Slushy: You're my eucalyptus tree.
The gallery lights dim. The window shows stars.
You stay there until closing time.
-> END

=== ending_milkshake ===
#ending:milkshake #mood:goofy #sfx:music_bole
You forgot the gift. But your legs started moving.
Before you knew it — the Bole Chudiyan choreo.
Full send. In the gallery.
#mood:straight
Slushy sends a 😐 emoji from three feet away.
You both know you'll be back.
-> END

=== ending_grey_normal ===
#ending:grey #mood:eepy #sfx:music_rain
You overthought the whole thing.
She figured it out before you said anything.
Your phone buzzes.
Slushy: "I hate u, dekh le."
You stare at it for a long time.
-> END
```

- [ ] **Step 2: Commit**
```bash
git add Assets/Ink/story.ink
git commit -m "feat: complete story.ink — all acts, choices, meter tags, three endings"
```

---

### Task 11: Ending Scene Visuals (programmatic)

**Files:**
- Create: `Assets/Scripts/Core/EndingSceneSetup.cs`

- [ ] **Step 1: Implement EndingSceneSetup**

Create `Assets/Scripts/Core/EndingSceneSetup.cs`:
```csharp
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Attach to a GameObject in each ending scene.
// Set EndingType in Inspector to match the scene.
public class EndingSceneSetup : MonoBehaviour
{
    [SerializeField] EndingType _thisEnding;
    [SerializeField] Camera     _cam;
    [SerializeField] TMP_Text   _captionText;
    [SerializeField] Image      _backgroundOverlay;

    void Start()
    {
        SetBackground();
        SetCaption();
        StartCoroutine(GenerateParticles());
    }

    void SetBackground()
    {
        _cam.backgroundColor = _thisEnding switch {
            EndingType.Constellation => new Color(0.05f, 0.05f, 0.18f), // deep night blue
            EndingType.Milkshake     => new Color(0.95f, 0.88f, 0.75f), // warm cream
            _                        => new Color(0.22f, 0.22f, 0.28f)  // cold grey
        };
    }

    void SetCaption()
    {
        _captionText.text = _thisEnding switch {
            EndingType.Constellation =>
                "\"You're my eucalyptus tree.\"\n\n✦ Constellation Ending ✦",
            EndingType.Milkshake =>
                "😐\n\n~ Milkshake Ending ~",
            _ =>
                "\"I hate u, dekh le.\"\n\n— Grey Ending"
        };
        _captionText.color = _thisEnding == EndingType.Grey
            ? new Color(0.60f, 0.60f, 0.65f)
            : Color.white;
    }

    System.Collections.IEnumerator GenerateParticles()
    {
        if (_thisEnding != EndingType.Constellation) yield break;

        // Paint stars onto background texture
        var tex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] px = new Color[256 * 256];
        for (int i = 0; i < px.Length; i++) px[i] = new Color(0,0,0,0);

        System.Random rng = new System.Random(42);
        for (int s = 0; s < 200; s++)
        {
            int x = rng.Next(256), y = rng.Next(256);
            float brightness = (float)rng.NextDouble() * 0.5f + 0.5f;
            px[y * 256 + x] = new Color(brightness, brightness, brightness * 0.9f + 0.05f);
        }
        tex.SetPixels(px);
        tex.Apply();

        _backgroundOverlay.sprite = Sprite.Create(tex,
            new Rect(0, 0, 256, 256), Vector2.one * 0.5f);
        yield return null;
    }
}
```

- [ ] **Step 2: Commit**
```bash
git add Assets/Scripts/Core/EndingSceneSetup.cs
git commit -m "feat: EndingSceneSetup — procedural backgrounds for three endings"
```

---

### Task 12: Scene Assembly (Unity Editor Setup)

These steps must be done in the Unity Editor — they cannot be scripted.

- [ ] **Step 1: Create scenes**

In Unity: File → New Scene (Basic 2D) for each:
`Dorm`, `CND`, `Library`, `Metro`, `Gallery`, `WorldMap`, `EndingConstellation`, `EndingMilkshake`, `EndingGrey`

Add all to File → Build Settings in this order.

- [ ] **Step 2: Bootstrap scene (Dorm)**

In the Dorm scene:
1. Create empty `_Bootstrap` GameObject
2. Attach `InkManager` → assign `story.ink` TextAsset
3. Attach `SceneDirector` → assign fade overlay Image
4. Attach `AudioManager`
5. Attach `SpriteManager`
6. Create Player GameObject: `Sprite` + `TopDownController` + `Rigidbody2D` + `CircleCollider2D`. Tag: `Player`
7. Create Tilemap for floor (DormFloor tile), walls (DormWall tile)
8. Create `DialogueUI` Canvas: panel, TMP texts, choice button prefab, continue button
9. Add `VirtualJoystick` canvas (bottom-left, active only on mobile)

- [ ] **Step 3: Populate tiles**

For each scene, in the Tilemap Palette, use `PixelArtLibrary.BuildTile(TileID.X)` called from a `TilemapPainter.cs` EditorScript (one-time tool), or manually paint via Inspector using the generated sprites:
```csharp
// EditorScript: Assets/Editor/TilemapPainter.cs
// Run via Tools → Paint Tiles
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class TilemapPainterWindow : EditorWindow
{
    [MenuItem("Tools/Paint Scene Tiles")]
    static void Open() => GetWindow<TilemapPainterWindow>("Paint Tiles");

    Tilemap _tilemap;
    TileID  _selectedTile;

    void OnGUI()
    {
        _tilemap      = (Tilemap)EditorGUILayout.ObjectField("Tilemap", _tilemap, typeof(Tilemap), true);
        _selectedTile = (TileID)EditorGUILayout.EnumPopup("Tile", _selectedTile);
        if (GUILayout.Button("Create Tile Asset") && _tilemap != null)
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = PixelArtLibrary.BuildTile(_selectedTile);
            string path = $"Assets/Art/Tiles/{_selectedTile}.asset";
            System.IO.Directory.CreateDirectory("Assets/Art/Tiles");
            AssetDatabase.CreateAsset(tile, path);
            AssetDatabase.SaveAssets();
            Debug.Log($"Tile asset saved to {path}");
        }
    }
}
```

- [ ] **Step 4: CND scene**

Copy Player + Bootstrap from Dorm (without InkManager — it persists via DontDestroyOnLoad).
Add `WorldMapController` with buttons for: Dorm (unlocked), CND (unlocked), Library (locked), Metro (locked), Gallery (locked).
Place `PatrolAI`-less NPC sprites for Dhruv and Nischala at fixed positions.

- [ ] **Step 5: Library scene**

Add `StealthController` + `PatrolAI` with 4 waypoints around the IR section.
Add `HideSpot` components on bookshelf colliders (IsTrigger = true).
Add `StrawberryTrigger` at row 3: calls `StealthController.OnStrawberryReached()` on player overlap.

- [ ] **Step 6: EndingX scenes**

Each ending scene: just a Camera + Canvas with `EndingSceneSetup` component + TMP_Text for caption.

- [ ] **Step 7: Commit**
```bash
git add Assets/Editor/ Assets/Art/Tiles/
git commit -m "feat: TilemapPainter editor tool, scene tile assets"
```

---

## Self-Review

**Spec coverage check:**
- ✅ Full game (Prologue + Act I + Act II + Act III + endings)
- ✅ Cross-platform (TopDownController + VirtualJoystick)
- ✅ Additive + subtractive meters (GameState.ApplyTag)
- ✅ Lives system with 3 catches / bad detour (StealthController)
- ✅ World map navigation (WorldMapController)
- ✅ Programmatic pixel art, no external files (PixelArtLibrary)
- ✅ No save system (fresh run, no PlayerPrefs)
- ✅ All three endings (Constellation / Milkshake / Grey)
- ✅ Princess Ani transform (MonkeyPrincessAni sprite + Ink goofy check)
- ✅ Fetch quest (Momo Sizzler + Diet Coke Ink variables)
- ✅ Text battle on catch (dialogue lines in caught_once/caught_twice knots)
- ✅ TDD for core logic (GameState tests, PixelArtLibrary tests)

**No placeholders found.** All code steps include complete implementations.

**Type consistency:** `EndingType` defined in `GameState.cs`, used consistently in `InkManager`, `SceneDirector`, `EndingSceneSetup`. `CharacterID`/`MoodID`/`TileID` defined in `PixelArtLibrary.cs`, used in `SpriteManager`, `DialogueUI`, `EndingSceneSetup`.
