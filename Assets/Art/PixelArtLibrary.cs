using UnityEngine;
using System.Collections.Generic;

public enum CharacterID { Monkey, Slushy, Dhruv, Nischala, Jabin }
public enum MoodID      { Neutral, Goofy, Eepy, Happy, Angry, PrincessAni, StraightFace, Talking }
public enum TileID      {
    FloorWood, FloorWoodDark, Wall, Shelf, Chair, MomoSteamer,
    LibraryFloor, BookshelfTall, MetroPlatform, MetroWall,
    GalleryFloor, GalleryWall, DormFloor, DormWall
}

public static class PixelArtLibrary
{
    // ── Palette ───────────────────────────────────────────────────────────
    static readonly Color T   = new Color(0,0,0,0);                       // transparent
    static readonly Color SK  = new Color(0.94f, 0.78f, 0.60f);           // skin (Monkey/Jabin)
    static readonly Color GH  = new Color(0.10f, 0.24f, 0.10f);           // dark green hair
    static readonly Color EY  = new Color(0.10f, 0.10f, 0.18f);           // eye / dark outline
    static readonly Color WH  = new Color(0.96f, 0.96f, 0.96f);           // white shirt
    static readonly Color RD  = new Color(0.90f, 0.22f, 0.27f);           // red emblem
    static readonly Color PA  = new Color(0.18f, 0.18f, 0.23f);           // dark pants
    static readonly Color BR  = new Color(0.48f, 0.29f, 0.12f);           // brown boots
    static readonly Color PH  = new Color(0.42f, 0.25f, 0.63f);           // purple hair (Slushy)
    static readonly Color TE  = new Color(0.00f, 0.71f, 0.65f);           // teal eye
    static readonly Color BJ  = new Color(0.18f, 0.35f, 0.56f);           // blue jacket (Slushy)
    static readonly Color LB  = new Color(0.60f, 0.74f, 0.94f);           // light blue shirt
    static readonly Color OB  = new Color(0.30f, 0.45f, 0.72f);           // outline blue (Slushy pants)
    static readonly Color BH  = new Color(0.20f, 0.14f, 0.08f);           // dark brown hair (Dhruv)
    static readonly Color DK  = new Color(0.68f, 0.48f, 0.32f);           // warm dark skin (Dhruv/Nischala)
    static readonly Color GR  = new Color(0.45f, 0.45f, 0.50f);           // grey jacket (Jabin)
    static readonly Color JH  = new Color(0.50f, 0.40f, 0.30f);           // Jabin hair
    static readonly Color PK  = new Color(0.85f, 0.55f, 0.75f);           // pink accessory
    static readonly Color BL  = new Color(0.35f, 0.45f, 0.85f);           // blue baggy jeans
    static readonly Color DP  = new Color(0.65f, 0.35f, 0.55f);           // dusty pink top (Nischala)
    static readonly Color CH  = new Color(0.95f, 0.70f, 0.75f);           // blush
    // Tiles
    static readonly Color FW  = new Color(0.77f, 0.58f, 0.42f);           // floor wood light
    static readonly Color FD  = new Color(0.63f, 0.47f, 0.31f);           // floor wood dark
    static readonly Color WLT = new Color(0.82f, 0.72f, 0.88f);           // wall lavender
    static readonly Color WLD = new Color(0.68f, 0.56f, 0.78f);           // wall lavender dark
    static readonly Color SHB = new Color(0.45f, 0.28f, 0.14f);           // shelf brown
    static readonly Color SHD = new Color(0.30f, 0.18f, 0.08f);           // shelf dark
    static readonly Color LFL = new Color(0.72f, 0.82f, 0.72f);           // library floor
    static readonly Color LFD = new Color(0.60f, 0.72f, 0.60f);           // library floor dark
    static readonly Color MTP = new Color(0.55f, 0.55f, 0.65f);           // metro platform
    static readonly Color MTD = new Color(0.42f, 0.42f, 0.52f);           // metro dark
    static readonly Color GFL = new Color(0.92f, 0.88f, 0.80f);           // gallery floor
    static readonly Color GFD = new Color(0.80f, 0.76f, 0.68f);           // gallery floor dark
    static readonly Color DFL = new Color(0.68f, 0.55f, 0.42f);           // dorm floor

    // ── Sprite cache ──────────────────────────────────────────────────────
    static readonly Dictionary<(CharacterID, MoodID), Sprite> _charCache = new();
    static readonly Dictionary<TileID, Sprite>                _tileCache = new();

    public static Sprite Build(CharacterID id, MoodID mood)
    {
        var key = (id, mood);
        if (!_charCache.TryGetValue(key, out var cached))
        {
            Color[] pixels = GetCharPixels(id, mood);
            cached = MakeSprite(pixels, 16, 32);
            _charCache[key] = cached;
        }
        return cached;
    }

    public static Sprite BuildTile(TileID id)
    {
        if (!_tileCache.TryGetValue(id, out var cached))
        {
            Color[] pixels = GetTilePixels(id);
            cached = MakeSprite(pixels, 32, 32);
            _tileCache[id] = cached;
        }
        return cached;
    }

    // ── Internal helpers ──────────────────────────────────────────────────
    static Sprite MakeSprite(Color[] pixels, int w, int h)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode   = TextureWrapMode.Clamp;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), 16f);
    }

    // Converts top-down string rows → bottom-up Texture2D Color array.
    // Each char maps to a color in pal; '.' and ' ' → transparent.
    static Color[] RowsToPixels(string[] rows, Dictionary<char, Color> pal)
    {
        int h = rows.Length;
        int w = rows[0].Length;
        var px = new Color[w * h];
        for (int y = 0; y < h; y++)
        {
            int ty = h - 1 - y; // flip Y for Unity
            for (int x = 0; x < w; x++)
            {
                char c = x < rows[y].Length ? rows[y][x] : '.';
                px[ty * w + x] = pal.TryGetValue(c, out var col) ? col : T;
            }
        }
        return px;
    }

    // ── Character dispatch ────────────────────────────────────────────────
    static Color[] GetCharPixels(CharacterID id, MoodID mood) =>
        (id, mood) switch {
            (CharacterID.Monkey,   MoodID.Neutral)     => MonkeyNeutral(),
            (CharacterID.Monkey,   MoodID.Goofy)       => MonkeyGoofy(),
            (CharacterID.Monkey,   MoodID.Eepy)        => MonkeyEepy(),
            (CharacterID.Monkey,   MoodID.PrincessAni) => MonkeyPrincessAni(),
            (CharacterID.Slushy,   MoodID.Neutral)     => SlushyNeutral(),
            (CharacterID.Slushy,   MoodID.Eepy)        => SlushyEepy(),
            (CharacterID.Slushy,   MoodID.Happy)       => SlushyHappy(),
            (CharacterID.Slushy,   MoodID.StraightFace)=> SlushyStraight(),
            (CharacterID.Dhruv,    MoodID.Neutral)     => DhruvNeutral(),
            (CharacterID.Dhruv,    MoodID.Talking)     => DhruvTalking(),
            (CharacterID.Nischala, MoodID.Neutral)     => NischalaNeutral(),
            (CharacterID.Nischala, MoodID.Talking)     => NischalaNeutral(), // same frame, mouth animates via blink
            (CharacterID.Jabin,    MoodID.Neutral)     => JabinNeutral(),
            (CharacterID.Jabin,    MoodID.Angry)       => JabinAngry(),
            (CharacterID.Jabin,    MoodID.Talking)     => JabinTalking(),
            _                                          => MonkeyNeutral()
        };

    // ═══════════════════════════════════════════════════════════════════════
    // MONKEY  (16 × 32)
    // Palette keys: . transparent  G dark-green-hair  S skin  E eye/outline
    //               W white-shirt  R red-emblem  K dark-pants  B brown-boot
    // ═══════════════════════════════════════════════════════════════════════
    static Dictionary<char, Color> MonkeyPal() => new() {
        {'.', T}, {' ', T}, {'G', GH}, {'S', SK}, {'E', EY},
        {'W', WH}, {'R', RD}, {'K', PA}, {'B', BR}
    };

    static Color[] MonkeyNeutral() => RowsToPixels(new[] {
        "....GGGGGGGG....",  //  0 hair top
        "...GGGGGGGGGG...",  //  1
        "..GGGGGGGGGGGG..",  //  2
        "..GSSSSSSSSSGG..",  //  3 face begins
        "..GSSSSSSSSSGGG.",  //  4
        "..GSEESSSSEESG..",  //  5 eyes
        "..GSSSSSSSSSSG..",  //  6
        "..GSSSEESSSSG...",  //  7 subtle mouth
        "..GSSSSSSSSGG...",  //  8
        "....SSSSSSS.....",  //  9 neck
        "...WWWWWWWWWW...",  // 10 shoulders
        "..WWWWWWWWWWWW..",  // 11
        "..WWWWWWWWWWWW..",  // 12
        "..WWWRRWWWWWWW..",  // 13 red emblem
        "..WWWRRWWWWWWW..",  // 14
        "..WWWWWWWWWWWW..",  // 15
        "..WWWWWWWWWWWW..",  // 16
        "...WWWWWWWWWWW..",  // 17
        "....KKKKKKKK....",  // 18 belt
        "...KKKKKKKKKK...",  // 19 pants
        "...KKKKKKKKKK...",  // 20
        "...KKKK.KKKKK...",  // 21
        "...KKKK.KKKKK...",  // 22
        "...KKKK.KKKKK...",  // 23
        "...KKKK.KKKKK...",  // 24
        "...BBBB.BBBB....",  // 25 boots
        "...BBBB.BBBB....",  // 26
        "..BBBBB.BBBBB...",  // 27
        "..BBBBB.BBBBB...",  // 28
        "..BBBBB.BBBBB...",  // 29
        "..BBBBBBBBBB....",  // 30
        "...BBBBBBBBB....",  // 31
    }, MonkeyPal());

    static Color[] MonkeyGoofy() => RowsToPixels(new[] {
        "....GGGGGGGG....",
        "...GGGGGGGGGG...",
        "..GGGGGGGGGGGG..",
        "..GSSSSSSSSSGG..",
        "..GSSSSSSSSSGGG.",
        "..GSEESSSSEESG..",
        "..GSSSSSSSSSSG..",
        "..GSEEEEEEEEG...",  // big grin
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
    }, MonkeyPal());

    static Color[] MonkeyEepy()
    {
        var pal = MonkeyPal();
        pal['H'] = new Color(EY.r, EY.g, EY.b, 0.55f); // half-closed eye
        return RowsToPixels(new[] {
            "....GGGGGGGG....",
            "...GGGGGGGGGG...",
            "..GGGGGGGGGGGG..",
            "..GSSSSSSSSSGG..",
            "..GSSSSSSSSSGGG.",
            "..GSHESSSSHESG..",  // H = half-eye lid
            "..GSSSSSSSSSSG..",
            "..GSSSEESSSSG...",
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
        }, pal);
    }

    static Color[] MonkeyPrincessAni()
    {
        var pal = MonkeyPal();
        pal['P'] = PK;  // pink accessory
        pal['J'] = BL;  // baggy blue jeans
        return RowsToPixels(new[] {
            "....GGGGGGGG....",
            "...GGGGGGGGGG...",
            "..GGGGGGGGGGGG..",
            "..GSSSSSSSSSGG..",
            ".PPGSSSSSSSSGGG.",  // hair accessories
            "..GSEESSSSEESG..",
            "..GSSSSSSSSSSG..",
            "..GSSSEESSSSG...",
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
            "...JJJJJJJJJJ...",  // baggy jeans start
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
        }, pal);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // SLUSHY  (16 × 32)
    // P purple-hair  S skin  T teal-eye  B blue-jacket  L light-shirt
    // O outline-blue  E eye/dark  W white-shoe  C blush
    // ═══════════════════════════════════════════════════════════════════════
    static Dictionary<char, Color> SlushyPal() => new() {
        {'.', T}, {' ', T}, {'P', PH}, {'S', SK}, {'T', TE},
        {'B', BJ}, {'L', LB}, {'O', OB}, {'E', EY}, {'W', WH}
    };

    static Color[] SlushyNeutral() => RowsToPixels(new[] {
        "....PPPPPPPP....",  //  0 hair
        "...PPPPPPPPPP...",  //  1
        "..PPPPPPPPPPPP..",  //  2
        ".PPSSSSSSSSPPPP.",  //  3 face
        ".PPSSSSSSSSSPPP.",  //  4
        ".PPSTTSSSTTSP...",  //  5 teal eyes
        ".PPSSSSSSSSSPP..",  //  6
        ".PPSSSEESSSSP...",  //  7 small mouth
        ".PPSSSSSSSSSPP..",  //  8
        "....SSSSSSS.....",  //  9 neck
        "...BBBBBBBBB....",  // 10 jacket
        "..BBBBBBBBBBB...",  // 11
        "..BLLLLLLLLBB...",  // 12 shirt inside jacket
        "..BLLLLLLLLBB...",  // 13
        "..BLLLLLLLLBB...",  // 14
        "..BBBBBBBBBBB...",  // 15
        "..BBBBBBBBBBB...",  // 16
        "...BBBBBBBBB....",  // 17
        "....OOOOOOOO....",  // 18 pants
        "...OOOOOOOOOO...",  // 19
        "...OOOOOOOOOO...",  // 20
        "...OOOO.OOOOO...",  // 21
        "...OOOO.OOOOO...",  // 22
        "...OOOO.OOOOO...",  // 23
        "...WWWW.WWWWW...",  // 24 white shoes
        "...WWWW.WWWWW...",  // 25
        "..WWWWW.WWWWWW..",  // 26
        "..WWWWW.WWWWWW..",  // 27
        "..WWWWWWWWWWWW..",  // 28
        "..WWWWWWWWWWWW..",  // 29
        "...WWWWWWWWWWW..",  // 30
        "....WWWWWWWWW...",  // 31
    }, SlushyPal());

    static Color[] SlushyEepy()
    {
        var pal = SlushyPal();
        pal['T'] = new Color(TE.r, TE.g, TE.b, 0.45f); // half-open teal
        return RowsToPixels(new[] {
            "....PPPPPPPP....",
            "...PPPPPPPPPP...",
            "..PPPPPPPPPPPP..",
            ".PPSSSSSSSSPPPP.",
            ".PPSSSSSSSSSPPP.",
            ".PPSTTSSSTTSP...",
            ".PPSSSSSSSSSPP..",
            ".PPSSSSSSSSSPP..",  // no expression (eepy = blank)
            ".PPSSSSSSSSSPP..",
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
        }, pal);
    }

    static Color[] SlushyHappy()
    {
        var pal = SlushyPal();
        pal['C'] = CH;  // blush
        return RowsToPixels(new[] {
            "....PPPPPPPP....",
            "...PPPPPPPPPP...",
            "..PPPPPPPPPPPP..",
            ".PPSSSSSSSSPPPP.",
            ".PPSSSSSSSSSPPP.",
            ".PPSTTSSSTTSP...",
            ".PPSCSSSSSCPPP..",  // blush C
            ".PPSSSEEESSSP...",  // smile via E
            ".PPSSSSSSSSSPP..",
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
        }, pal);
    }

    static Color[] SlushyStraight() => RowsToPixels(new[] {
        "....PPPPPPPP....",
        "...PPPPPPPPPP...",
        "..PPPPPPPPPPPP..",
        ".PPSSSSSSSSPPPP.",
        ".PPSSSSSSSSSPPP.",
        ".PPSTTSSSTTSP...",
        ".PPSSSSSSSSSPP..",
        ".PPSSEEEEEESPP..",  // flat line mouth
        ".PPSSSSSSSSSPP..",
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
    }, SlushyPal());

    // ═══════════════════════════════════════════════════════════════════════
    // DHRUV  (16 × 32)   H dark-hair  S warm-dark-skin  E eye  W white  K pants  B boots
    // ═══════════════════════════════════════════════════════════════════════
    static Dictionary<char, Color> DhruvPal() => new() {
        {'.', T}, {' ', T}, {'H', BH}, {'S', DK}, {'E', EY},
        {'W', WH}, {'K', PA}, {'B', BR}
    };

    static Color[] DhruvNeutral() => RowsToPixels(new[] {
        "....HHHHHHHH....",
        "...HHHHHHHHHH...",
        "..HHHHHHHHHHHH..",
        "..HSSSSSSSSSHH..",
        "..HSSSSSSSSSHH..",
        "..HSESSSSESSH...",
        "..HSSSSSSSSSH...",
        "..HSSSEESSSSH...",
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
    }, DhruvPal());

    static Color[] DhruvTalking() => RowsToPixels(new[] {
        "....HHHHHHHH....",
        "...HHHHHHHHHH...",
        "..HHHHHHHHHHHH..",
        "..HSSSSSSSSSHH..",
        "..HSSSSSSSSSHH..",
        "..HSESSSSESSH...",
        "..HSSSSSSSSSH...",
        "..HSSEEEEEESH...",  // open mouth
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
    }, DhruvPal());

    // ═══════════════════════════════════════════════════════════════════════
    // NISCHALA  (16 × 32)   longer hair, dusty-pink top
    // ═══════════════════════════════════════════════════════════════════════
    static Color[] NischalaNeutral()
    {
        var pal = DhruvPal();
        pal['D'] = DP;  // dusty pink top
        return RowsToPixels(new[] {
            "..HHHHHHHHHHHH..",  // wider hair
            ".HHHHHHHHHHHHHH.",
            ".HHHSSSSSSSSHHH.",
            ".HHSSSSSSSSSSHHH",
            ".HHSSSSSSSSSSHHH",
            ".HHSESSSSESSSH..",
            ".HHSSSSSSSSSSHHH",
            ".HHSSSEESSSSSH..",
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
        }, pal);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // PROF. JABIN  (16 × 32)   grey jacket, stern face
    // J jabin-hair  S skin  E eye  G grey-jacket  K pants  B boots
    // ═══════════════════════════════════════════════════════════════════════
    static Dictionary<char, Color> JabinPal() => new() {
        {'.', T}, {' ', T}, {'J', JH}, {'S', SK}, {'E', EY},
        {'G', GR}, {'K', PA}, {'B', BR}, {'F', new Color(0.75f, 0.50f, 0.35f)}
    };

    static Color[] JabinNeutral() => RowsToPixels(new[] {
        "....JJJJJJJJ....",
        "...JJJJJJJJJJ...",
        "..JJJJJJJJJJJJ..",
        "..JSSSSSSSSSSJJ..",
        "..JSSSSSSSSSSJJ..",
        "..JSESSSSESSSJJ..",
        "..JSSSSSSSSSSJ...",
        "..JSSSEJESSSSJ...",  // flat mouth — neutral stern
        "..JSSSSSSSSSSJJ..",
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
    }, JabinPal());

    static Color[] JabinAngry()
    {
        var pal = JabinPal();
        pal['V'] = new Color(0.55f, 0.28f, 0.18f); // frown dark
        return RowsToPixels(new[] {
            "....JJJJJJJJ....",
            "...JJJJJJJJJJ...",
            "..JJJJJJJJJJJJ..",
            "..JFFFFFFFFFFFJ..",  // furrowed brow
            "..JSSSSSSSSSSJJ..",
            "..JSEFFSSFFESJ...",  // angled angry brows
            "..JSSSSSSSSSSJ...",
            "..JSSSVVVVSSSJJ..",  // V frown
            "..JSSSSSSSSSSJJ..",
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
        }, pal);
    }

    static Color[] JabinTalking() => RowsToPixels(new[] {
        "....JJJJJJJJ....",
        "...JJJJJJJJJJ...",
        "..JJJJJJJJJJJJ..",
        "..JSSSSSSSSSSJJ..",
        "..JSSSSSSSSSSJJ..",
        "..JSESSSSESSSJJ..",
        "..JSSSSSSSSSSJ...",
        "..JSSSEEEESSSJJ..",  // open mouth
        "..JSSSSSSSSSSJJ..",
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
    }, JabinPal());

    // ═══════════════════════════════════════════════════════════════════════
    // TILES  (32 × 32)
    // ═══════════════════════════════════════════════════════════════════════
    static Color[] GetTilePixels(TileID id) =>
        id switch {
            TileID.FloorWood      => FloorWoodTile(),
            TileID.FloorWoodDark  => FloorWoodDarkTile(),
            TileID.Wall           => WallTile(),
            TileID.Shelf          => ShelfTile(),
            TileID.Chair          => ChairTile(),
            TileID.MomoSteamer    => MomoSteamerTile(),
            TileID.LibraryFloor   => LibraryFloorTile(),
            TileID.BookshelfTall  => BookshelfTallTile(),
            TileID.MetroPlatform  => MetroPlatformTile(),
            TileID.MetroWall      => MetroWallTile(),
            TileID.GalleryFloor   => GalleryFloorTile(),
            TileID.GalleryWall    => GalleryWallTile(),
            TileID.DormFloor      => DormFloorTile(),
            TileID.DormWall       => DormWallTile(),
            _                     => FloorWoodTile()
        };

    static Color[] FloorWoodTile()
    {
        var px = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 32; x++)
        {
            bool dark = (y % 8 < 2) || (x % 16 == 0);
            float n   = Mathf.Sin(x * 0.7f + y * 0.1f) * 0.04f;
            Color b   = dark ? FD : FW;
            px[y * 32 + x] = new Color(b.r + n, b.g + n, b.b + n);
        }
        return px;
    }

    static Color[] FloorWoodDarkTile()
    {
        Color light = new Color(0.55f, 0.40f, 0.26f);
        Color dark  = new Color(0.45f, 0.32f, 0.20f);
        var px = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 32; x++)
        {
            bool d = (y % 8 < 2) || (x % 16 == 0);
            float n = Mathf.Sin(x * 0.7f + y * 0.1f) * 0.03f;
            Color b = d ? dark : light;
            px[y * 32 + x] = new Color(b.r + n, b.g + n, b.b + n);
        }
        return px;
    }

    static Color[] WallTile()
    {
        var px = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 32; x++)
        {
            bool edge = x == 0 || x == 31 || y == 0 || y == 31;
            float n = Mathf.Sin(x * 1.3f) * 0.02f + Mathf.Cos(y * 1.1f) * 0.02f;
            Color b = edge ? WLD : WLT;
            px[y * 32 + x] = new Color(b.r + n, b.g + n, b.b + n);
        }
        return px;
    }

    static Color[] ShelfTile()
    {
        var px = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 32; x++)
        {
            bool shelf = y == 8 || y == 16 || y == 24 || x < 3 || x > 28;
            px[y * 32 + x] = shelf ? SHD : SHB;
        }
        return px;
    }

    static Color[] ChairTile()
    {
        var px = new Color[32 * 32];
        for (int i = 0; i < px.Length; i++) px[i] = T;
        Color seat = new Color(0.82f, 0.60f, 0.30f);
        Color leg  = new Color(0.55f, 0.38f, 0.18f);
        for (int y = 14; y <= 22; y++)
        for (int x = 6;  x <= 25; x++) px[y * 32 + x] = seat;
        foreach (int lx in new[] { 6, 7, 24, 25 })
        for (int y = 4; y <= 13; y++)  px[y * 32 + lx] = leg;
        return px;
    }

    static Color[] MomoSteamerTile()
    {
        var px = new Color[32 * 32];
        for (int i = 0; i < px.Length; i++) px[i] = T;
        Color metal = new Color(0.75f, 0.75f, 0.80f);
        Color steam = new Color(0.90f, 0.90f, 0.95f);
        Color momo  = new Color(0.96f, 0.90f, 0.80f);
        for (int y = 8; y <= 28; y++)
        for (int x = 6; x <= 25; x++) px[y * 32 + x] = metal;
        for (int x = 10; x <= 22; x++)
        {
            px[6 * 32 + x] = steam;
            if (x % 3 == 0) px[4 * 32 + x] = steam;
        }
        foreach ((int my, int mx) in new[] { (15, 11), (15, 18), (20, 14), (20, 21) })
        for (int dy = -1; dy <= 1; dy++)
        for (int dx = -1; dx <= 1; dx++)
            px[(my + dy) * 32 + (mx + dx)] = momo;
        return px;
    }

    static Color[] LibraryFloorTile()
    {
        var px = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 32; x++)
            px[y * 32 + x] = ((x / 8 + y / 8) % 2 == 0) ? LFL : LFD;
        return px;
    }

    static Color[] BookshelfTallTile()
    {
        Color[] bookColors = {
            new Color(0.80f, 0.25f, 0.25f),
            new Color(0.25f, 0.55f, 0.80f),
            new Color(0.80f, 0.75f, 0.25f),
            new Color(0.35f, 0.70f, 0.45f),
            new Color(0.70f, 0.35f, 0.70f),
        };
        var px = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 32; x++)
        {
            if (x < 2 || x > 29)  { px[y * 32 + x] = SHD; continue; }
            if (y % 10 < 2)        { px[y * 32 + x] = SHD; continue; }
            px[y * 32 + x] = bookColors[(x / 6) % bookColors.Length];
        }
        return px;
    }

    static Color[] MetroPlatformTile()
    {
        var px = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 32; x++)
            px[y * 32 + x] = (y == 4 || y == 5 || x % 8 == 0) ? MTD : MTP;
        return px;
    }

    static Color[] MetroWallTile()
    {
        Color tileBase = new Color(0.72f, 0.72f, 0.80f);
        Color tileDark = new Color(0.55f, 0.55f, 0.65f);
        var px = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 32; x++)
            px[y * 32 + x] = (x % 16 == 0 || y % 8 == 0) ? tileDark : tileBase;
        return px;
    }

    static Color[] GalleryFloorTile()
    {
        var px = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 32; x++)
            px[y * 32 + x] = ((x / 16 + y / 16) % 2 == 0) ? GFL : GFD;
        return px;
    }

    static Color[] GalleryWallTile()
    {
        Color wallBase = new Color(0.96f, 0.94f, 0.90f);
        Color moulding = new Color(0.80f, 0.78f, 0.74f);
        var px = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 32; x++)
            px[y * 32 + x] = (y < 4 || y > 27) ? moulding : wallBase;
        return px;
    }

    static Color[] DormFloorTile()
    {
        var px = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 32; x++)
        {
            float n = Mathf.Sin(x * 0.5f + y * 0.3f) * 0.03f;
            px[y * 32 + x] = new Color(DFL.r + n, DFL.g + n, DFL.b + n);
        }
        return px;
    }

    static Color[] DormWallTile()
    {
        Color wallBase = new Color(0.88f, 0.84f, 0.94f);
        Color stripe   = new Color(0.78f, 0.74f, 0.88f);
        var px = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 32; x++)
            px[y * 32 + x] = (x % 6 < 2) ? stripe : wallBase;
        return px;
    }
}
