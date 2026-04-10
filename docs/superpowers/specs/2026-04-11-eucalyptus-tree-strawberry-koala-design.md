# The Eucalyptus Tree & The Strawberry Koala — Design Spec
Date: 2026-04-11

## Overview
A pixel-art narrative RPG in Unity 2D. Ink-driven architecture where `story.ink` is the game state machine. C# is a renderer/reactor only. Cross-platform (PC + Mobile). No save system — fresh run each time.

## Architecture

### Core Systems
- **InkManager.cs** — parses tags each story step, fires C# events, owns GoofyMeter / OverthinkerMeter / Lives
- **SceneDirector.cs** — loads Unity scenes based on `#scene:X` tags; manages world map
- **DialogueUI.cs** — text box + choice buttons, disables player movement when active
- **SpriteManager.cs** — swaps character portraits on `#mood:X` tags via a cached sprite dictionary
- **AudioManager.cs** — plays SFX/music loops on `#sfx:X` tags
- **PixelArtLibrary.cs** — generates all sprites programmatically via Texture2D pixel painting (FilterMode.Point)

### Tag Contract (Ink → C#)
| Tag | Effect |
|---|---|
| `#scene:dorm` / `#scene:cnd` / `#scene:library` / `#scene:metro` / `#scene:gallery` | SceneDirector loads scene |
| `#goofy:+10` / `#goofy:-5` | InkManager adjusts GoofyMeter |
| `#overthinker:+10` / `#overthinker:-5` | InkManager adjusts OverthinkerMeter |
| `#lives:-1` | InkManager decrements lives; triggers bad-detour branch if 0 |
| `#mood:eepy` / `#mood:happy` / `#mood:straight` | SpriteManager swaps portrait |
| `#ending:constellation` / `#ending:milkshake` / `#ending:grey` | SceneDirector loads ending |
| `#sfx:rain` / `#sfx:metro` | AudioManager plays loop |

## Scenes & Acts

### Prologue — Dorm Room
- Top-down 16×16 dorm room tilemap
- Player wakes at 4AM, discovers strawberry missing
- First dialogue choice: "Miya miya" (+10 Goofy) vs "I'm just bedrotting" (+10 Overthinker)

### Act I — CND Cafe (World Map → CND)
- World map screen: tap/click location icons to travel
- Meet Dhruv & Nischala; fetch quest for Momo Sizzler + Diet Coke
- WASD/touch top-down movement within scene
- Completing fetch quest unlocks Library on world map

### Act II — Library Stealth
- StealthController.cs: player hides behind bookshelves (overlap detection)
- PatrolAI.cs: Prof. Jabin follows a waypoint path
- Lives system: 3 strikes
  - Strike 1 & 2: "text battle" dialogue (40-page assignment), lose stat points
  - Strike 3: `#lives:-1` → 0 → Ink jumps to bad_detour knot → Grey ending branch
- On success: strawberry found inside IR book, `has_strawberry = true`

### Act III — Metro + Art Gallery
- Rainy metro platform: crowd navigation (moving obstacle NPCs)
- High Goofy → sprite swaps to Princess Ani (baggy jeans mode)
- Art Gallery: Slushy waiting, final gift exchange
- Ending determined by meter comparison at `=== ending_check ===`

## Endings
| Ending | Condition | Scene |
|---|---|---|
| Constellation | goofy > overthinker, lives > 0 | Starry sky, lily of the valley keychain, "You're my eucalyptus tree" |
| Milkshake | goofy == overthinker OR lives == 1 | Bole Chudiyan dance, 😐 emoji notification |
| Grey | overthinker > goofy OR bad_detour | WhatsApp notification: "I hate u, dekh le" |

## Pixel Art System
- All sprites generated at runtime via `PixelArtLibrary.cs`
- Character resolution: 16×32px; Tiles: 32×32px
- FilterMode.Point on all textures (no blur)
- Pastel palette: soft purples, warm ambers, dusty pinks, deep teals
- Characters: Monkey (4 variants), Slushy/Koala (4 variants), Dhruv, Nischala, Prof. Jabin (3 variants)
- Tilesets: dorm, CND cafe, library shelves, metro platform, art gallery

## Ink Story Structure
Single `Assets/Ink/story.ink` file. Key knots:
- `prologue` → `dorm_room` → `world_map`
- `cnd` → `dhruv_quest` → `nischala_chat` → `world_map`
- `library_stealth` (C# stealth controller runs; Ink resumes on caught/clear)
- `metro` → `art_gallery`
- `ending_check` → `ending_constellation` / `ending_milkshake` / `ending_grey`

## Input
- PC: WASD movement, mouse click for choices
- Mobile: virtual joystick (left thumb), tap for choices
- DialogueUI active → movement disabled

## File Structure
```
Assets/
  Scripts/
    Core/         InkManager.cs, SceneDirector.cs, AudioManager.cs
    Dialogue/     DialogueUI.cs
    Player/       TopDownController.cs, VirtualJoystick.cs
    Stealth/      StealthController.cs, PatrolAI.cs
    Interactions/ WorldMapController.cs, ItemPickup.cs
  Ink/            story.ink
  Art/            PixelArtLibrary.cs, SpriteManager.cs
  Scenes/         Dorm.unity, CND.unity, Library.unity, Metro.unity, Gallery.unity, WorldMap.unity, Endings.unity
```
