namespace WSMGameStudio.Splines
{
    public enum MeshGenerationMethod
    {
        Manual,
        Realtime
    }

    public enum HandlesVisibility
    {
        ShowOnlyActiveHandles,
        ShowAllHandles
    }

    public enum BezierHandlesAlignment
    {
        Aligned,
        Mirrored,
        Free,
        Automatic
    }

    public enum SplineFollowerBehaviour
    {
        StopAtTheEnd,
        Loop,
        BackAndForward
    }

    public enum SplineFollowerReference
    {
        Spline,
        Terrain
    }

    public enum SplineFollowerStops
    {
        Disabled, 
        LastSpline,
        EachSpline
    }

    public enum SplineInspectorMenu
    {
        CurveSettings,
        SplineSettings,
        HandlesSettings
    }

    public enum SplineUpwardsDirection
    {
        Up,
        Down,
        Right,
        Left,
        Foward, 
        Back
    }

    public enum MeshRendererInspectorMenu
    {
        MeshGenerationSettings,
        CollisionSettings
    }

    public enum MessageType
    {
        Success,
        Error,
        Warning
    }
}
