using UnityEngine;
using WSMGameStudio.RailroadSystem;
using System.Collections.Generic;

public interface IRailroadCar
{
    Rigidbody BackJoint { get; set; }
    List<TrainWheel_v3> Wheels { get; set; }
    AudioSource WheelsSFX { get; set; }
    Sensors Sensors { get; set; }
    List<Light> ExternalLights { get; set; }
    List<Light> InternalLights { get; set; }
}
