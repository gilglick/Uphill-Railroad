using UnityEngine;

[CreateAssetMenu(fileName = "New Train Input Settings", menuName = "WSM Game Studio/Train Controller/Train Input Settings", order = 1)]
public class TrainInputSettings : ScriptableObject
{
    public KeyCode toggleEngine = KeyCode.E;
    public KeyCode forward = KeyCode.W;
    public KeyCode reverse = KeyCode.S;
    public KeyCode increaseSpeed = KeyCode.PageUp;
    public KeyCode decreaseSpeed = KeyCode.PageDown;
    public float speedIncreaseAmount = 5f;
    public KeyCode brakes = KeyCode.Space;
    public KeyCode lights = KeyCode.L;
    public KeyCode internalLights = KeyCode.I;
    public KeyCode honk = KeyCode.H;
    public KeyCode bell = KeyCode.B;
    public KeyCode cabinLeftDoor = KeyCode.Alpha1;
    public KeyCode cabinRightDoor = KeyCode.Alpha2;
    public KeyCode passengerLeftDoor = KeyCode.Alpha3;
    public KeyCode passengerRightDoor = KeyCode.Alpha4;

    public KeyCode[] customEventTriggers;
}
