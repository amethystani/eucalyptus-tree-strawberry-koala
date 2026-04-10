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
    public void BuildSprite_MonkeyNeutral_HasNonTransparentPixels()
    {
        var sprite = PixelArtLibrary.Build(CharacterID.Monkey, MoodID.Neutral);
        var tex = sprite.texture;
        bool hasOpaque = false;
        for (int x = 0; x < tex.width && !hasOpaque; x++)
            for (int y = 0; y < tex.height && !hasOpaque; y++)
                if (tex.GetPixel(x, y).a > 0.5f) hasOpaque = true;
        Assert.IsTrue(hasOpaque);
    }

    [Test]
    public void BuildTile_FloorWood_Is32x32()
    {
        var sprite = PixelArtLibrary.BuildTile(TileID.FloorWood);
        Assert.AreEqual(32, (int)sprite.rect.width);
        Assert.AreEqual(32, (int)sprite.rect.height);
    }

    [Test]
    public void BuildSprite_AllCharacterVariants_DoNotThrow()
    {
        Assert.DoesNotThrow(() => PixelArtLibrary.Build(CharacterID.Monkey,   MoodID.Goofy));
        Assert.DoesNotThrow(() => PixelArtLibrary.Build(CharacterID.Monkey,   MoodID.Eepy));
        Assert.DoesNotThrow(() => PixelArtLibrary.Build(CharacterID.Monkey,   MoodID.PrincessAni));
        Assert.DoesNotThrow(() => PixelArtLibrary.Build(CharacterID.Slushy,   MoodID.Happy));
        Assert.DoesNotThrow(() => PixelArtLibrary.Build(CharacterID.Slushy,   MoodID.StraightFace));
        Assert.DoesNotThrow(() => PixelArtLibrary.Build(CharacterID.Dhruv,    MoodID.Talking));
        Assert.DoesNotThrow(() => PixelArtLibrary.Build(CharacterID.Nischala, MoodID.Neutral));
        Assert.DoesNotThrow(() => PixelArtLibrary.Build(CharacterID.Jabin,    MoodID.Angry));
        Assert.DoesNotThrow(() => PixelArtLibrary.Build(CharacterID.Jabin,    MoodID.Talking));
    }

    [Test]
    public void BuildTile_AllVariants_DoNotThrow()
    {
        foreach (TileID id in System.Enum.GetValues(typeof(TileID)))
            Assert.DoesNotThrow(() => PixelArtLibrary.BuildTile(id), $"TileID.{id} threw");
    }
}
