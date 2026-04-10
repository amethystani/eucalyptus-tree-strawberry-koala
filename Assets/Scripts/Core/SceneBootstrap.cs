using UnityEngine;

/// <summary>
/// Place this on the _Bootstrap GameObject in the Dorm (first) scene only.
/// All singleton managers use DontDestroyOnLoad, so they persist automatically.
///
/// Scene setup checklist (do once in Unity Editor per scene):
///
/// ── ALL SCENES ──────────────────────────────────────────────────────────
///  □ Camera: set Background Color to match tile palette
///  □ Tilemap: add Grid → Tilemap → TilemapRenderer
///  □ Player: Sprite + CircleCollider2D + Rigidbody2D (Gravity=0) + TopDownController
///    Tag the Player GameObject as "Player"
///  □ UI Canvas (Screen Space Overlay):
///      - DialoguePanel (Image bg) → SpeakerName (TMP) + BodyText (TMP) + ChoiceContainer + ContinueButton
///      - VirtualJoystick (bottom-left, mobile only)
///      - FadeOverlay (fullscreen black Image, alpha=0)
///
/// ── DORM SCENE (bootstrap) ──────────────────────────────────────────────
///  □ _Bootstrap GameObject:
///      InkManager   → assign story.ink TextAsset
///      SceneDirector → assign FadeOverlay Image
///      AudioManager  → assign music/sfx AudioSources + sound entries
///      SpriteManager → assign playerPortrait + npcPortrait SpriteRenderers
///  □ Floor tiles:  DormFloor
///  □ Wall tiles:   DormWall
///
/// ── CND SCENE ────────────────────────────────────────────────────────────
///  □ WorldMapController on a Canvas:
///      Locations (assign Buttons in Inspector):
///        - Dorm      | UnlockedByDefault=true
///        - CND       | UnlockedByDefault=true
///        - Library   | UnlockedByDefault=false
///        - Metro     | UnlockedByDefault=false
///        - Gallery   | UnlockedByDefault=false
///  □ Place NPC SpriteRenderers for Dhruv and Nischala at fixed positions
///  □ Floor tiles: FloorWood, FloorWoodDark
///  □ Props:  Chair × 4, MomoSteamer × 1
///
/// ── LIBRARY SCENE ────────────────────────────────────────────────────────
///  □ StealthController + PatrolAI on Jabin GameObject
///      PatrolAI: assign 4 Waypoints around the IR section
///  □ Bookshelves: BookshelfTall tiles + BoxCollider2D (IsTrigger=true) + HideSpot
///  □ IR-section trigger: BoxCollider2D (IsTrigger=true) + StrawberryTrigger
///  □ Floor tiles: LibraryFloor, BookshelfTall
///
/// ── METRO SCENE ──────────────────────────────────────────────────────────
///  □ Crowd NPCs: 4-6 sprites with simple LinearVelocity movement
///  □ Floor tiles: MetroPlatform, MetroWall
///
/// ── GALLERY SCENE ─────────────────────────────────────────────────────────
///  □ Slushy NPC SpriteRenderer at a fixed position
///  □ Floor tiles: GalleryFloor, GalleryWall
///
/// ── ENDING SCENES ────────────────────────────────────────────────────────
///  □ EndingConstellation, EndingMilkshake, EndingGrey
///      Each: Camera + Canvas with EndingSceneSetup component
///      Assign _thisEnding, _cam, _captionText (TMP), _backgroundImage
///
/// ── BUILD SETTINGS ────────────────────────────────────────────────────────
///  Add all scenes in this order:
///    0. Dorm
///    1. CND
///    2. Library
///    3. Metro
///    4. Gallery
///    5. WorldMap  (optional overlay scene)
///    6. EndingConstellation
///    7. EndingMilkshake
///    8. EndingGrey
/// </summary>
public class SceneBootstrap : MonoBehaviour
{
    // No runtime logic needed — singleton managers handle themselves.
    // This component exists only to carry the checklist above.
}
