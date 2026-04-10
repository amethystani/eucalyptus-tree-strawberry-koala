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
    public void DominantMeter_WhenGoofyHigher_ReturnsConstellation()
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
