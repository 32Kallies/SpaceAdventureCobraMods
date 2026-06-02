namespace MusicReplacer.CustomTriggers;

public enum TriggerShape
{
    CenterBox,
    CornerBox,
    VerticalLine
}

public abstract class CustomTrigger
{
    public string Name { get; set; } = "";
    public string Music { get; set; } = "";
    public string Level { get; set; } = "";
    public int Priority { get; set; } = 0;
    public TriggerShape Shape { get; protected set; }
}

public class CenterBoxTrigger : CustomTrigger
{
    public (float X, float Y) Center { get; set; }
    public (float W, float H) Size { get; set; }

    public CenterBoxTrigger()
    {
        Shape = TriggerShape.CenterBox;
    }
}

public class CornerBoxTrigger : CustomTrigger
{
    public (float X, float Y) Corner1 { get; set; }
    public (float X, float Y) Corner2 { get; set; }

    public CornerBoxTrigger()
    {
        Shape = TriggerShape.CornerBox;
    }
}

public class VerticalLineTrigger : CustomTrigger
{
    public float Left { get; set; }
    public float Right { get; set; }

    public VerticalLineTrigger()
    {
        Shape = TriggerShape.VerticalLine;
    }
}