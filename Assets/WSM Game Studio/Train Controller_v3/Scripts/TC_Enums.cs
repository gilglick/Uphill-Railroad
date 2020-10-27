namespace WSMGameStudio.RailroadSystem
{
    /// <summary>
    /// Units of speed
    /// </summary>
    public enum SpeedUnits
    {
        kph,
        mph
    }

    /// <summary>
    /// Rail switchin modes
    /// </summary>
    public enum SwitchMode
    {
        Always,
        Once,
        Random,
        IfActivated,
        IfDeactivated
    }
    /// <summary>
    /// Station stop mode
    /// </summary>
    public enum StopMode
    {
        Always,
        Once,
        Random
    }

    /// <summary>
    /// Reverse zone settings
    /// </summary>
    public enum ReverseDirectionMode
    {
        Always,
        OnlyIfMovingForward,
        OnlyIfMovingBackwards
    }

    /// <summary>
    /// Train behaviour after stoping at station
    /// </summary>
    public enum StationBehaviour
    {
        LeaveAfterTime,
        StopForever
    }

    /// <summary>
    /// Which doors must open on station
    /// </summary>
    public enum StationDoorDirection
    {
        BothSides,
        Left,
        Right
    }

    /// <summary>
    /// Identifies couple direction
    /// </summary>
    public enum CarCouplerType
    {
        FrontCoupler,
        BackCoupler
    }

    public enum WagonCoupling
    {
        Enabled,
        Disabled
    }

    public enum WagonDecouplingSettings
    {
        AllowRecoupling,
        NeverRecouple
    }

    public enum TrainControllerInspectorMenu
    {
        Controls,
        Wagons,
        Lights,
        SFX,
        OtherSettings
    }

    public enum WagonInspectorMenu
    {
        Coupling,
        Lights,
        SFX,
        OtherSettings
    }

    public enum WagonType
    {
        Locomotive,
        Wagon
    }

    public enum ZoneTriggerType
    {
        Activate,
        Deactivate
    }
}
