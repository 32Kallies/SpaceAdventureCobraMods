namespace NoFramerateCap;

public static class CustomFramerateUtils
{
    public static int GetNewFramerateInt()
    {
        if (Plugin.FrameRate.Value < 1)
            return 60;
        return Plugin.FrameRate.Value;
    }

    public static float GetNewFramerateFloat()
    {
        if (Plugin.FrameRate.Value < 1)
            return 60f;
        return Plugin.FrameRate.Value;
    }
}