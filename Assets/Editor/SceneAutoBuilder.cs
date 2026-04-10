// ═══════════════════════════════════════════════════════════════════════════
// SceneAutoBuilder — Tools → Build All Scenes
// Programmatically creates every scene in the game with correct GameObjects,
// components, tilemaps, and cross-references. Run once after opening the project.
// ═══════════════════════════════════════════════════════════════════════════
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using TMPro;

public static class SceneAutoBuilder
{
    const string SCENES_PATH = "Assets/Scenes";
    const string TILES_PATH  = "Assets/Art/Tiles";

    [MenuItem("Tools/Build All Scenes")]
    public static void BuildAll()
    {
        // Ensure Tile assets exist first
        TilemapPainterWindow_Static.CreateAllTiles();

        Directory.CreateDirectory(SCENES_PATH);

        BuildDorm();
        BuildCND();
        BuildLibrary();
        BuildMetro();
        BuildGallery();
        BuildEndingConstellation();
        BuildEndingMilkshake();
        BuildEndingGrey();
        ConfigureBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✓ All scenes built. Open Dorm scene and press Play.");
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    static UnityEngine.SceneManagement.Scene NewScene(string name)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        // Ensure scene path exists
        Directory.CreateDirectory(SCENES_PATH);
        EditorSceneManager.SaveScene(scene, $"{SCENES_PATH}/{name}.unity");
        return scene;
    }

    static GameObject NewGO(string goName, params System.Type[] components)
    {
        var go = new GameObject(goName, components);
        return go;
    }

    // Stamp a grid of tiles in a Tilemap
    static void FillTiles(Tilemap map, int x0, int y0, int x1, int y1, TileID id)
    {
        string path = $"{TILES_PATH}/{id}.asset";
        var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
        if (tile == null)
        {
            Debug.LogWarning($"Tile asset not found: {path} — run Tools→Paint Scene Tiles first.");
            return;
        }
        for (int x = x0; x <= x1; x++)
        for (int y = y0; y <= y1; y++)
            map.SetTile(new Vector3Int(x, y, 0), tile);
    }

    // Create a Camera with 2D orthographic settings
    static Camera AddCamera(Color bg, float size = 5f)
    {
        var camGO = NewGO("Main Camera", typeof(Camera), typeof(AudioListener));
        camGO.tag = "MainCamera";
        var cam = camGO.GetComponent<Camera>();
        cam.orthographic     = true;
        cam.orthographicSize = size;
        cam.backgroundColor  = bg;
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.transform.position = new Vector3(0, 0, -10);
        return cam;
    }

    // Create a Canvas with all dialogue UI elements wired up
    static (GameObject panel, DialogueUI ui) AddDialogueCanvas(GameObject inkManagerGO)
    {
        var canvasGO = NewGO("DialogueCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas   = canvasGO.GetComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);

        // Fade overlay
        var fadeGO    = new GameObject("FadeOverlay", typeof(Image));
        fadeGO.transform.SetParent(canvasGO.transform, false);
        var fadeRect  = fadeGO.GetComponent<RectTransform>();
        fadeRect.anchorMin = Vector2.zero;
        fadeRect.anchorMax = Vector2.one;
        fadeRect.offsetMin = Vector2.zero;
        fadeRect.offsetMax = Vector2.zero;
        var fadeImage = fadeGO.GetComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.raycastTarget = false;

        // Dialogue panel (bottom third)
        var panelGO  = new GameObject("DialoguePanel", typeof(Image));
        panelGO.transform.SetParent(canvasGO.transform, false);
        var panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(1f, 0.35f);
        panelRect.offsetMin = new Vector2(20, 10);
        panelRect.offsetMax = new Vector2(-20, -10);
        panelGO.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.10f, 0.88f);

        // Speaker name
        var speakerGO = new GameObject("SpeakerName", typeof(TextMeshProUGUI));
        speakerGO.transform.SetParent(panelGO.transform, false);
        var speakerRect = speakerGO.GetComponent<RectTransform>();
        speakerRect.anchorMin = new Vector2(0f, 0.72f);
        speakerRect.anchorMax = new Vector2(0.6f, 1f);
        speakerRect.offsetMin = new Vector2(16, 4);
        speakerRect.offsetMax = new Vector2(-8, -4);
        var speakerTMP = speakerGO.GetComponent<TextMeshProUGUI>();
        speakerTMP.fontSize = 18;
        speakerTMP.fontStyle = FontStyles.Bold;
        speakerTMP.color = new Color(0.9f, 0.85f, 0.6f);

        // Body text
        var bodyGO = new GameObject("BodyText", typeof(TextMeshProUGUI));
        bodyGO.transform.SetParent(panelGO.transform, false);
        var bodyRect = bodyGO.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0f, 0.1f);
        bodyRect.anchorMax = new Vector2(0.8f, 0.72f);
        bodyRect.offsetMin = new Vector2(16, 8);
        bodyRect.offsetMax = new Vector2(-8, -4);
        var bodyTMP = bodyGO.GetComponent<TextMeshProUGUI>();
        bodyTMP.fontSize = 16;
        bodyTMP.color    = Color.white;

        // Choice container
        var choiceContainerGO = new GameObject("ChoiceContainer", typeof(RectTransform), typeof(VerticalLayoutGroup));
        choiceContainerGO.transform.SetParent(panelGO.transform, false);
        var choiceRect = choiceContainerGO.GetComponent<RectTransform>();
        choiceRect.anchorMin = new Vector2(0.82f, 0f);
        choiceRect.anchorMax = new Vector2(1f, 1f);
        choiceRect.offsetMin = new Vector2(4, 4);
        choiceRect.offsetMax = new Vector2(-4, -4);
        var vlg = choiceContainerGO.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 6;
        vlg.childControlWidth  = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;

        // Continue button
        var continueGO  = new GameObject("ContinueButton", typeof(Image), typeof(Button));
        continueGO.transform.SetParent(panelGO.transform, false);
        var continueRect = continueGO.GetComponent<RectTransform>();
        continueRect.anchorMin = new Vector2(0.85f, 0.05f);
        continueRect.anchorMax = new Vector2(0.99f, 0.45f);
        continueRect.offsetMin = Vector2.zero;
        continueRect.offsetMax = Vector2.zero;
        continueGO.GetComponent<Image>().color = new Color(0.25f, 0.50f, 0.80f);
        var continueLabelGO = new GameObject("Label", typeof(TextMeshProUGUI));
        continueLabelGO.transform.SetParent(continueGO.transform, false);
        var continueLabelRect = continueLabelGO.GetComponent<RectTransform>();
        continueLabelRect.anchorMin = Vector2.zero;
        continueLabelRect.anchorMax = Vector2.one;
        continueLabelGO.GetComponent<TextMeshProUGUI>().text = "▶";

        // Choice button prefab (saved as a prefab)
        var choicePrefabGO = new GameObject("ChoiceButtonPrefab", typeof(Image), typeof(Button));
        choicePrefabGO.transform.SetParent(panelGO.transform, false);
        choicePrefabGO.SetActive(false);
        var choiceLabelGO = new GameObject("Label", typeof(TextMeshProUGUI));
        choiceLabelGO.transform.SetParent(choicePrefabGO.transform, false);
        choicePrefabGO.GetComponent<Image>().color = new Color(0.18f, 0.30f, 0.55f);
        var choiceRect2 = choicePrefabGO.GetComponent<RectTransform>();
        choiceRect2.sizeDelta = new Vector2(0, 36);
        choiceLabelGO.GetComponent<TextMeshProUGUI>().fontSize = 13;

        // DialogueUI component
        var ui = panelGO.AddComponent<DialogueUI>();
        // Wire via SerializedObject
        var so = new SerializedObject(ui);
        so.FindProperty("_dialoguePanel").objectReferenceValue  = panelGO;
        so.FindProperty("_speakerName").objectReferenceValue    = speakerTMP;
        so.FindProperty("_bodyText").objectReferenceValue       = bodyTMP;
        so.FindProperty("_choiceContainer").objectReferenceValue = choiceContainerGO.transform;
        so.FindProperty("_continueButton").objectReferenceValue  = continueGO.GetComponent<Button>();
        so.FindProperty("_choiceButtonPrefab").objectReferenceValue = choicePrefabGO.GetComponent<Button>();
        so.ApplyModifiedProperties();

        // Wire SceneDirector fade overlay
        var sdList = Object.FindObjectsByType<SceneDirector>(FindObjectsSortMode.None);
        if (sdList.Length > 0)
        {
            var sdSO = new SerializedObject(sdList[0]);
            sdSO.FindProperty("_fadeOverlay").objectReferenceValue = fadeImage;
            sdSO.ApplyModifiedProperties();
        }

        return (panelGO, ui);
    }

    // Create a Grid + Tilemap pair and return the Tilemap
    static Tilemap AddTilemap(string goName)
    {
        var gridGO  = new GameObject(goName, typeof(Grid));
        var mapGO   = new GameObject("Tilemap", typeof(Tilemap), typeof(TilemapRenderer));
        mapGO.transform.SetParent(gridGO.transform, false);

        // Tilemap collider for walls
        var colliderGO = new GameObject("TilemapCollider", typeof(Tilemap), typeof(TilemapRenderer), typeof(TilemapCollider2D), typeof(CompositeCollider2D), typeof(Rigidbody2D));
        colliderGO.transform.SetParent(gridGO.transform, false);
        colliderGO.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        colliderGO.GetComponent<TilemapCollider2D>().usedByComposite = true;

        return mapGO.GetComponent<Tilemap>();
    }

    // Create Player GameObject
    static (GameObject go, TopDownController ctrl) AddPlayer(Vector3 pos)
    {
        var playerGO = NewGO("Player",
            typeof(SpriteRenderer),
            typeof(CircleCollider2D),
            typeof(Rigidbody2D),
            typeof(TopDownController),
            typeof(PlayerInput));

        playerGO.tag           = "Player";
        playerGO.transform.position = pos;

        var rb = playerGO.GetComponent<Rigidbody2D>();
        rb.gravityScale  = 0f;
        rb.constraints   = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var sr = playerGO.GetComponent<SpriteRenderer>();
        sr.sprite      = PixelArtLibrary.Build(CharacterID.Monkey, MoodID.Neutral);
        sr.sortingOrder = 5;

        return (playerGO, playerGO.GetComponent<TopDownController>());
    }

    // Create NPC SpriteRenderer at position
    static SpriteRenderer AddNPC(CharacterID id, MoodID mood, Vector3 pos, string goName = null)
    {
        var npcGO = new GameObject(goName ?? id.ToString(), typeof(SpriteRenderer));
        npcGO.transform.position = pos;
        var sr = npcGO.GetComponent<SpriteRenderer>();
        sr.sprite       = PixelArtLibrary.Build(id, mood);
        sr.sortingOrder = 4;
        return sr;
    }

    // Create portrait SpriteRenderers for dialogue (off-screen, used by DialogueUI)
    static (SpriteRenderer player, SpriteRenderer npc) AddPortraits()
    {
        var playerPortrait = new GameObject("PlayerPortrait", typeof(SpriteRenderer));
        playerPortrait.transform.position = new Vector3(-100, 0, 0); // off-screen
        playerPortrait.GetComponent<SpriteRenderer>().sprite =
            PixelArtLibrary.Build(CharacterID.Monkey, MoodID.Neutral);

        var npcPortrait = new GameObject("NpcPortrait", typeof(SpriteRenderer));
        npcPortrait.transform.position = new Vector3(-101, 0, 0);
        npcPortrait.GetComponent<SpriteRenderer>().sprite =
            PixelArtLibrary.Build(CharacterID.Slushy, MoodID.Neutral);

        return (playerPortrait.GetComponent<SpriteRenderer>(),
                npcPortrait.GetComponent<SpriteRenderer>());
    }

    // ── DORM SCENE ─────────────────────────────────────────────────────────
    static void BuildDorm()
    {
        var scene = NewScene("Dorm");
        AddCamera(new Color(0.10f, 0.08f, 0.12f));

        // Bootstrap singletons
        var bootstrapGO = new GameObject("_Bootstrap");

        // InkManager
        var inkMgr = bootstrapGO.AddComponent<InkManager>();
        var inkAsset = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Ink/story.ink");
        if (inkAsset != null)
        {
            var inkSO = new SerializedObject(inkMgr);
            inkSO.FindProperty("_inkAsset").objectReferenceValue = inkAsset;
            inkSO.ApplyModifiedProperties();
        }

        // SceneDirector
        bootstrapGO.AddComponent<SceneDirector>();

        // AudioManager
        var audio = bootstrapGO.AddComponent<AudioManager>();
        var audioSO = new SerializedObject(audio);
        // Add two AudioSources
        var musicSrc = bootstrapGO.AddComponent<AudioSource>();
        musicSrc.playOnAwake = false;
        musicSrc.loop        = true;
        var sfxSrc = bootstrapGO.AddComponent<AudioSource>();
        sfxSrc.playOnAwake = false;
        audioSO.FindProperty("_musicSource").objectReferenceValue = musicSrc;
        audioSO.FindProperty("_sfxSource").objectReferenceValue   = sfxSrc;
        audioSO.ApplyModifiedProperties();

        // SpriteManager
        var sm = bootstrapGO.AddComponent<SpriteManager>();
        var (playerPortrait, npcPortrait) = AddPortraits();
        var smSO = new SerializedObject(sm);
        smSO.FindProperty("_playerPortrait").objectReferenceValue = playerPortrait;
        smSO.FindProperty("_npcPortrait").objectReferenceValue    = npcPortrait;
        smSO.ApplyModifiedProperties();

        // Tilemap
        var map = AddTilemap("DormGrid");
        FillTiles(map, -6, -4, 6, 4, TileID.DormFloor);
        FillTiles(map, -6, 4,  6, 5, TileID.DormWall);
        FillTiles(map, -6, -5, 6, -4, TileID.DormWall);

        // Player
        AddPlayer(Vector3.zero);

        // Dialogue canvas
        AddDialogueCanvas(bootstrapGO);

        EditorSceneManager.SaveScene(scene, $"{SCENES_PATH}/Dorm.unity");
        Debug.Log("Built: Dorm");
    }

    // ── CND SCENE ──────────────────────────────────────────────────────────
    static void BuildCND()
    {
        var scene = NewScene("CND");
        AddCamera(new Color(0.18f, 0.12f, 0.08f));

        var map = AddTilemap("CNDGrid");
        FillTiles(map, -8, -5, 8, 4,  TileID.FloorWood);
        FillTiles(map, -8, 4,  8, 5,  TileID.Wall);
        FillTiles(map, -8, -5, 8, -4, TileID.Wall);

        AddPlayer(new Vector3(-1, 0, 0));

        // NPCs
        AddNPC(CharacterID.Dhruv,    MoodID.Neutral, new Vector3(-3, 1, 0));
        AddNPC(CharacterID.Nischala, MoodID.Neutral, new Vector3( 2, 1, 0));

        // Props — represented as SpriteRenderers using tile sprites
        var chairSR = new GameObject("Chair", typeof(SpriteRenderer));
        chairSR.transform.position = new Vector3(-3.5f, 0, 0);
        chairSR.GetComponent<SpriteRenderer>().sprite = PixelArtLibrary.BuildTile(TileID.Chair);

        var steamerSR = new GameObject("MomoSteamer", typeof(SpriteRenderer));
        steamerSR.transform.position = new Vector3(4f, 1.5f, 0);
        steamerSR.GetComponent<SpriteRenderer>().sprite = PixelArtLibrary.BuildTile(TileID.MomoSteamer);

        // World Map UI
        AddWorldMapCanvas();

        AddDialogueCanvas(null);

        EditorSceneManager.SaveScene(scene, $"{SCENES_PATH}/CND.unity");
        Debug.Log("Built: CND");
    }

    static void AddWorldMapCanvas()
    {
        var canvasGO = NewGO("WorldMapCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.GetComponent<Canvas>().sortingOrder = 90;
        canvasGO.SetActive(false); // hidden until worldmap tag fires

        var mapPanel = new GameObject("MapPanel", typeof(Image));
        mapPanel.transform.SetParent(canvasGO.transform, false);
        var panelRect = mapPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0.1f);
        panelRect.anchorMax = new Vector2(0.9f, 0.9f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        mapPanel.GetComponent<Image>().color = new Color(0.08f, 0.06f, 0.14f, 0.95f);

        // Location buttons
        string[] locations = { "Dorm", "CND", "Library", "Metro", "Gallery" };
        bool[]   defaults  = { true,   true,  false,    false,   false    };
        float[]  xPos      = { 0.15f,  0.35f, 0.55f,   0.70f,  0.85f   };
        var locationEntries = new SerializedProperty[locations.Length];

        var wmc = canvasGO.AddComponent<WorldMapController>();
        var wmcSO = new SerializedObject(wmc);
        wmcSO.FindProperty("_mapPanel").objectReferenceValue = mapPanel;

        var locList = wmcSO.FindProperty("_locations");
        locList.arraySize = locations.Length;

        for (int i = 0; i < locations.Length; i++)
        {
            var btnGO = new GameObject($"Btn_{locations[i]}", typeof(Image), typeof(Button));
            btnGO.transform.SetParent(mapPanel.transform, false);
            var btnRect = btnGO.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(xPos[i] - 0.08f, 0.3f);
            btnRect.anchorMax = new Vector2(xPos[i] + 0.08f, 0.7f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;
            btnGO.GetComponent<Image>().color = new Color(0.25f, 0.20f, 0.45f);

            var labelGO = new GameObject("Label", typeof(TextMeshProUGUI));
            labelGO.transform.SetParent(btnGO.transform, false);
            var lRect = labelGO.GetComponent<RectTransform>();
            lRect.anchorMin = Vector2.zero;
            lRect.anchorMax = Vector2.one;
            labelGO.GetComponent<TextMeshProUGUI>().text      = locations[i];
            labelGO.GetComponent<TextMeshProUGUI>().fontSize  = 14;
            labelGO.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

            var entry = locList.GetArrayElementAtIndex(i);
            entry.FindPropertyRelative("SceneName").stringValue    = locations[i];
            entry.FindPropertyRelative("DisplayName").stringValue  = locations[i];
            entry.FindPropertyRelative("Btn").objectReferenceValue = btnGO.GetComponent<Button>();
            entry.FindPropertyRelative("UnlockedByDefault").boolValue = defaults[i];
        }
        wmcSO.ApplyModifiedProperties();
    }

    // ── LIBRARY SCENE ──────────────────────────────────────────────────────
    static void BuildLibrary()
    {
        var scene = NewScene("Library");
        AddCamera(new Color(0.12f, 0.14f, 0.12f));

        var map = AddTilemap("LibraryGrid");
        FillTiles(map, -8, -5, 8, 4,  TileID.LibraryFloor);
        FillTiles(map, -8, 4,  8, 5,  TileID.Wall);
        FillTiles(map, -8, -5, 8, -4, TileID.Wall);

        // Bookshelves as rows
        for (int x = -7; x <= 7; x += 3)
        {
            var shelfGO = new GameObject($"Shelf_{x}", typeof(SpriteRenderer), typeof(BoxCollider2D));
            shelfGO.transform.position = new Vector3(x, 2, 0);
            shelfGO.GetComponent<SpriteRenderer>().sprite = PixelArtLibrary.BuildTile(TileID.BookshelfTall);
            shelfGO.GetComponent<SpriteRenderer>().sortingOrder = 3;
            var col = shelfGO.GetComponent<BoxCollider2D>();
            col.size      = new Vector2(1f, 2f);
            col.isTrigger = true;
            shelfGO.AddComponent<HideSpot>();
        }

        // Jabin patrol NPC
        var jabinGO = new GameObject("ProfJabin",
            typeof(SpriteRenderer), typeof(PatrolAI));
        jabinGO.GetComponent<SpriteRenderer>().sprite       = PixelArtLibrary.Build(CharacterID.Jabin, MoodID.Neutral);
        jabinGO.GetComponent<SpriteRenderer>().sortingOrder = 4;

        // Waypoints
        Vector3[] waypointPositions = {
            new Vector3(-6, 0, 0),
            new Vector3( 0, 0, 0),
            new Vector3( 6, 0, 0),
            new Vector3( 0, -2, 0),
        };
        var waypoints = new Transform[waypointPositions.Length];
        for (int i = 0; i < waypointPositions.Length; i++)
        {
            var wp = new GameObject($"Waypoint_{i}");
            wp.transform.position = waypointPositions[i];
            waypoints[i] = wp.transform;
        }
        var patrolSO = new SerializedObject(jabinGO.GetComponent<PatrolAI>());
        var wpProp   = patrolSO.FindProperty("_waypoints");
        wpProp.arraySize = waypoints.Length;
        for (int i = 0; i < waypoints.Length; i++)
            wpProp.GetArrayElementAtIndex(i).objectReferenceValue = waypoints[i];
        patrolSO.ApplyModifiedProperties();

        // Player
        var (playerGO, _) = AddPlayer(new Vector3(-6, -3, 0));

        // Strawberry trigger (IR section, row 3)
        var triggerGO = new GameObject("StrawberryTrigger", typeof(BoxCollider2D));
        triggerGO.transform.position = new Vector3(4, 2, 0);
        triggerGO.GetComponent<BoxCollider2D>().size      = new Vector2(1.5f, 1.5f);
        triggerGO.GetComponent<BoxCollider2D>().isTrigger = true;
        triggerGO.AddComponent<StrawberryTrigger>();

        // Red caught flash
        var flashGO = new GameObject("CaughtFlash", typeof(Image));
        flashGO.SetActive(false);

        // StealthController
        var stealthGO = new GameObject("StealthController");
        var sc = stealthGO.AddComponent<StealthController>();
        var scSO = new SerializedObject(sc);
        scSO.FindProperty("_patrolAI").objectReferenceValue = jabinGO.GetComponent<PatrolAI>();
        scSO.FindProperty("_player").objectReferenceValue   = playerGO.transform;
        scSO.FindProperty("_caughtFlash").objectReferenceValue = flashGO;
        scSO.ApplyModifiedProperties();

        AddDialogueCanvas(null);

        EditorSceneManager.SaveScene(scene, $"{SCENES_PATH}/Library.unity");
        Debug.Log("Built: Library");
    }

    // ── METRO SCENE ────────────────────────────────────────────────────────
    static void BuildMetro()
    {
        var scene = NewScene("Metro");
        AddCamera(new Color(0.12f, 0.12f, 0.18f));

        var map = AddTilemap("MetroGrid");
        FillTiles(map, -8, -4, 8, 2,  TileID.MetroPlatform);
        FillTiles(map, -8, 2,  8, 4,  TileID.MetroWall);
        FillTiles(map, -8, -5, 8, -4, TileID.MetroWall);

        AddPlayer(new Vector3(-5, 0, 0));

        // Crowd NPCs (simple moving obstacles)
        for (int i = 0; i < 5; i++)
        {
            float x = -3f + i * 1.8f;
            var crowdGO = new GameObject($"CrowdNPC_{i}", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CrowdNPC));
            crowdGO.transform.position = new Vector3(x, 0, 0);
            crowdGO.GetComponent<SpriteRenderer>().sprite       = PixelArtLibrary.Build(CharacterID.Dhruv, MoodID.Neutral);
            crowdGO.GetComponent<SpriteRenderer>().sortingOrder = 3;
            var rb = crowdGO.GetComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints  = RigidbodyConstraints2D.FreezeRotation;
        }

        // Exit trigger at right edge (loads Gallery via world map unlock)
        var exitGO = new GameObject("MetroExit", typeof(BoxCollider2D), typeof(MetroExitTrigger));
        exitGO.transform.position = new Vector3(7, 0, 0);
        exitGO.GetComponent<BoxCollider2D>().size      = new Vector2(1.5f, 4f);
        exitGO.GetComponent<BoxCollider2D>().isTrigger = true;

        AddDialogueCanvas(null);

        EditorSceneManager.SaveScene(scene, $"{SCENES_PATH}/Metro.unity");
        Debug.Log("Built: Metro");
    }

    // ── GALLERY SCENE ──────────────────────────────────────────────────────
    static void BuildGallery()
    {
        var scene = NewScene("Gallery");
        AddCamera(new Color(0.22f, 0.20f, 0.18f));

        var map = AddTilemap("GalleryGrid");
        FillTiles(map, -8, -4, 8, 3,  TileID.GalleryFloor);
        FillTiles(map, -8, 3,  8, 4,  TileID.GalleryWall);

        AddPlayer(new Vector3(-3, -1, 0));

        // Slushy waiting by a painting
        AddNPC(CharacterID.Slushy, MoodID.Eepy, new Vector3(2, 0, 0), "Slushy");

        // Painting (simple colored quad)
        var paintingGO = new GameObject("Painting", typeof(SpriteRenderer));
        paintingGO.transform.position = new Vector3(2, 2.5f, 0);
        var paintTex = new Texture2D(32, 48);
        var paintPx  = new Color[32 * 48];
        for (int i = 0; i < paintPx.Length; i++)
        {
            float t = (i / 32f) / 48f;
            paintPx[i] = Color.Lerp(new Color(0.2f, 0.4f, 0.7f), new Color(0.6f, 0.8f, 0.9f), t);
        }
        paintTex.filterMode = FilterMode.Point;
        paintTex.SetPixels(paintPx);
        paintTex.Apply();
        paintingGO.GetComponent<SpriteRenderer>().sprite =
            Sprite.Create(paintTex, new Rect(0, 0, 32, 48), Vector2.one * 0.5f, 16f);
        paintingGO.GetComponent<SpriteRenderer>().sortingOrder = 2;

        AddDialogueCanvas(null);

        EditorSceneManager.SaveScene(scene, $"{SCENES_PATH}/Gallery.unity");
        Debug.Log("Built: Gallery");
    }

    // ── ENDING SCENES ──────────────────────────────────────────────────────
    static void BuildEndingConstellation()
    {
        var scene = NewScene("EndingConstellation");
        var cam   = AddCamera(new Color(0.04f, 0.04f, 0.16f));

        var canvasGO = NewGO("EndingCanvas", typeof(Canvas), typeof(CanvasScaler));
        canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        var bgImage = AddFullscreenImage(canvasGO, new Color(0, 0, 0, 0));
        var caption = AddCenteredText(canvasGO, "");

        var setup   = canvasGO.AddComponent<EndingSceneSetup>();
        var setupSO = new SerializedObject(setup);
        setupSO.FindProperty("_thisEnding").enumValueIndex          = (int)EndingType.Constellation;
        setupSO.FindProperty("_cam").objectReferenceValue           = cam;
        setupSO.FindProperty("_captionText").objectReferenceValue   = caption;
        setupSO.FindProperty("_backgroundImage").objectReferenceValue = bgImage;
        setupSO.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene, $"{SCENES_PATH}/EndingConstellation.unity");
        Debug.Log("Built: EndingConstellation");
    }

    static void BuildEndingMilkshake()
    {
        var scene   = NewScene("EndingMilkshake");
        var cam     = AddCamera(new Color(0.96f, 0.90f, 0.76f));

        var canvasGO = NewGO("EndingCanvas", typeof(Canvas), typeof(CanvasScaler));
        canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        var bgImage = AddFullscreenImage(canvasGO, new Color(0.96f, 0.90f, 0.76f, 1f));
        var caption = AddCenteredText(canvasGO, "");

        var setup   = canvasGO.AddComponent<EndingSceneSetup>();
        var setupSO = new SerializedObject(setup);
        setupSO.FindProperty("_thisEnding").enumValueIndex          = (int)EndingType.Milkshake;
        setupSO.FindProperty("_cam").objectReferenceValue           = cam;
        setupSO.FindProperty("_captionText").objectReferenceValue   = caption;
        setupSO.FindProperty("_backgroundImage").objectReferenceValue = bgImage;
        setupSO.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene, $"{SCENES_PATH}/EndingMilkshake.unity");
        Debug.Log("Built: EndingMilkshake");
    }

    static void BuildEndingGrey()
    {
        var scene   = NewScene("EndingGrey");
        var cam     = AddCamera(new Color(0.20f, 0.20f, 0.26f));

        var canvasGO = NewGO("EndingCanvas", typeof(Canvas), typeof(CanvasScaler));
        canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        var bgImage = AddFullscreenImage(canvasGO, new Color(0.20f, 0.20f, 0.26f, 1f));
        var caption = AddCenteredText(canvasGO, "");

        var setup   = canvasGO.AddComponent<EndingSceneSetup>();
        var setupSO = new SerializedObject(setup);
        setupSO.FindProperty("_thisEnding").enumValueIndex          = (int)EndingType.Grey;
        setupSO.FindProperty("_cam").objectReferenceValue           = cam;
        setupSO.FindProperty("_captionText").objectReferenceValue   = caption;
        setupSO.FindProperty("_backgroundImage").objectReferenceValue = bgImage;
        setupSO.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene, $"{SCENES_PATH}/EndingGrey.unity");
        Debug.Log("Built: EndingGrey");
    }

    static Image AddFullscreenImage(GameObject parent, Color color)
    {
        var go = new GameObject("Background", typeof(Image));
        go.transform.SetParent(parent.transform, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var img = go.GetComponent<Image>();
        img.color = color;
        return img;
    }

    static TextMeshProUGUI AddCenteredText(GameObject parent, string text)
    {
        var go = new GameObject("Caption", typeof(TextMeshProUGUI));
        go.transform.SetParent(parent.transform, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.1f, 0.2f);
        rect.anchorMax = new Vector2(0.9f, 0.8f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
        return tmp;
    }

    // ── BUILD SETTINGS ─────────────────────────────────────────────────────
    static void ConfigureBuildSettings()
    {
        var scenes = new[] {
            "Dorm", "CND", "Library", "Metro", "Gallery",
            "EndingConstellation", "EndingMilkshake", "EndingGrey"
        };
        var buildScenes = new EditorBuildSettingsScene[scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            string path = $"{SCENES_PATH}/{scenes[i]}.unity";
            buildScenes[i] = new EditorBuildSettingsScene(path, true);
        }
        EditorBuildSettings.scenes = buildScenes;
        Debug.Log($"Build Settings updated with {scenes.Length} scenes.");
    }
}

// ── Crowd NPC (simple back-and-forth movement) ─────────────────────────────
public class CrowdNPC : MonoBehaviour
{
    float _dir = 1f;
    float _speed = 0.8f;
    float _boundary = 6f;

    void Update()
    {
        transform.Translate(Vector3.right * _dir * _speed * Time.deltaTime);
        if (Mathf.Abs(transform.position.x) > _boundary) _dir *= -1f;
    }
}

// ── Metro exit trigger (unlocks Gallery, resumes Ink) ─────────────────────
public class MetroExitTrigger : MonoBehaviour
{
    bool _triggered;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered || !other.CompareTag("Player")) return;
        _triggered = true;
        // Resume story — Ink has already run the metro knot, SceneDirector loads Gallery
        InkManager.Instance?.AdvanceStory();
    }
}

// Helper shim so TilemapPainterWindow can be called statically from SceneAutoBuilder
public static class TilemapPainterWindow_Static
{
    public static void CreateAllTiles()
    {
        string dir = "Assets/Art/Tiles";
        System.IO.Directory.CreateDirectory(dir);
        foreach (TileID id in System.Enum.GetValues(typeof(TileID)))
        {
            string path = $"{dir}/{id}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (existing != null)
            {
                existing.sprite = PixelArtLibrary.BuildTile(id);
                EditorUtility.SetDirty(existing);
            }
            else
            {
                var tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = PixelArtLibrary.BuildTile(id);
                AssetDatabase.CreateAsset(tile, path);
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("All tile assets created.");
    }
}
#endif
