public enum EndingType { Constellation, Milkshake, Grey }

public class GameState
{
    public int  GoofyMeter        { get; private set; }
    public int  OverthinkerMeter  { get; private set; }
    public int  Lives             { get; private set; } = 3;
    public bool HasStrawberry     { get; set; }
    public bool HasKeychain       { get; set; }
    public bool BadDetourTriggered { get; private set; }

    public void ApplyTag(string tag)
    {
        int colon = tag.IndexOf(':');
        if (colon < 0) return;
        string key    = tag.Substring(0, colon).Trim();
        string valStr = tag.Substring(colon + 1).Trim();
        if (!int.TryParse(valStr, out int delta)) return;

        switch (key)
        {
            case "goofy":
                GoofyMeter = System.Math.Max(0, GoofyMeter + delta);
                break;
            case "overthinker":
                OverthinkerMeter = System.Math.Max(0, OverthinkerMeter + delta);
                break;
            case "lives":
                Lives = System.Math.Max(0, Lives + delta);
                if (Lives == 0) BadDetourTriggered = true;
                break;
        }
    }

    public EndingType DetermineEnding()
    {
        if (BadDetourTriggered)          return EndingType.Grey;
        if (GoofyMeter > OverthinkerMeter) return EndingType.Constellation;
        if (OverthinkerMeter > GoofyMeter) return EndingType.Grey;
        return EndingType.Milkshake;
    }
}
