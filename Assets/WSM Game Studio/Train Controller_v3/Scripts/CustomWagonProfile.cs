using UnityEngine;
using WSMGameStudio.RailroadSystem;

[CreateAssetMenu(fileName = "New Wagon Profile", menuName = "WSM Game Studio/Train Controller/Custom Wagon Profile", order = 1)]
public class CustomWagonProfile : ScriptableObject
{
    public WagonType type;
    public SpeedUnits speedUnits;
    public TrainInputSettings inputSettings;
    public Vector3 modelOffset;
    public Vector3 controlZoneTriggerPosition;
    public CustomWagonComponent wheelsPhysics;
    public CustomWagonComponent wheelsVisuals;
    public CustomWagonComponent frontCoupler;
    public CustomWagonComponent backCoupler;
    public CustomWagonComponent defaultJointAnchor;
    public CustomWagonComponent bumper;
    public CustomWagonComponent externalLights;
    public CustomWagonComponent internalLights;
    public CustomWagonComponent railSensor;
    public CustomWagonComponent passengerSensor;
    public CustomWagonComponent suspensionCollider;
    public CustomWagonComponent colliders;
    public CustomWagonComponent cabinDoorLeft;
    public CustomWagonComponent cabinDoorRight;
    public CustomWagonComponent passengerDoorLeft;
    public CustomWagonComponent passengerDoorRight;
    public CustomWagonComponent wheelsSFX;
    public CustomWagonComponent wagonConnectionSFX;
    public CustomWagonComponent engineSFX;
    public CustomWagonComponent brakesSFX;
    public CustomWagonComponent hornSFX;
    public CustomWagonComponent bellSFX;
    public CustomWagonComponent openDoorSFX;
    public CustomWagonComponent closeDoorSFX;
    public CustomWagonComponent internalDetails;
}
