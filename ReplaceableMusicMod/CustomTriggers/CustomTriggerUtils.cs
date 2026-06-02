using UnityEngine;

namespace MusicReplacer.CustomTriggers;

public static class CustomTriggerUtils
{
    private const float TriggerDepth = 100f;
    private const float VerticalTriggerHeight = 100f;

    public static CustomTriggerBounds GetCustomTriggerBounds(CustomTrigger data)
    {
        switch (data.Shape)
        {
            case TriggerShape.CenterBox:
                var box = data as CenterBoxTrigger;
                return GetCenterBoxTriggerBounds(box);
            case TriggerShape.VerticalLine:
                var line = data as VerticalLineTrigger;
                return GetVerticalLineTriggerBounds(line);
            case TriggerShape.CornerBox:
                var corners = data as CornerBoxTrigger;
                return GetCornerTriggerBounds(corners);
            default: throw new System.ArgumentOutOfRangeException(nameof(data.Shape));
        }
    }
    
    private static CustomTriggerBounds GetCenterBoxTriggerBounds(CenterBoxTrigger data)
    {
        return new CustomTriggerBounds(
            new Vector3(data.Center.X, data.Center.Y, 0),
            new Vector3(data.Size.W, data.Size.H, TriggerDepth)
        );
    }
    
    private static CustomTriggerBounds GetVerticalLineTriggerBounds(VerticalLineTrigger data)
    {
        return new CustomTriggerBounds(
            new Vector3((data.Left + data.Right) / 2, 0),
            new Vector3(Mathf.Abs(data.Right - data.Left), VerticalTriggerHeight, TriggerDepth)
        );
    }
    
    private static CustomTriggerBounds GetCornerTriggerBounds(CornerBoxTrigger data)
    {
        return new CustomTriggerBounds(
            GetCenterBetweenCorners(data),
            GetTriggerSizeBetweenCorners(data, TriggerDepth)
        );
    }

    private static Vector3 GetCenterBetweenCorners(CornerBoxTrigger data)
    {
        return new Vector3(
            (data.Corner1.X + data.Corner2.X) / 2,
            (data.Corner1.Y + data.Corner2.Y) / 2,
            0);
    }
    
    private static Vector3 GetTriggerSizeBetweenCorners(CornerBoxTrigger data, float depth)
    {
        return new Vector3(
            Mathf.Abs(data.Corner1.X - data.Corner2.X),
            Mathf.Abs(data.Corner1.Y - data.Corner2.Y),
            depth);
    }
}