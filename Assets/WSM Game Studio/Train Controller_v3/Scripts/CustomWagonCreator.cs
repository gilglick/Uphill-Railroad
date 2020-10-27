using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WSMGameStudio.RailroadSystem;
using WSMGameStudio.Splines;

public static class CustomWagonCreator
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="profile"></param>
    /// <param name="modelPrefab"></param>
    /// <param name="name"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static bool Validate(CustomWagonProfile profile, GameObject modelPrefab, string name, out string message)
    {
        message = string.Empty;

        if (profile == null)
        {
            message = string.Format("Profile cannot be null.{0}Please select a profile and try again.", System.Environment.NewLine);
            return false;
        }

        if (modelPrefab == null)
        {
            message = string.Format("Model cannot be null.{0}Please select your custom model and try again.", System.Environment.NewLine);
            return false;
        }

        if (name == null || name == string.Empty)
        {
            message = string.Format("Name cannot be null.{0}Please select a name and try again.", System.Environment.NewLine);
            return false;
        }

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="profile"></param>
    /// <param name="modelPrefab"></param>
    /// <param name="offset"></param>
    /// <param name="name"></param>
    public static bool Create(CustomWagonProfile profile, GameObject modelPrefab, string name, out string message)
    {
        message = string.Empty;

        Vector3 railsOffset = new Vector3(0f, 0.14f, 0f);

        //Wagon/locomotive game object
        GameObject wagonInstance = new GameObject(name);
        //Model
        GameObject modelInstance;
        if (modelPrefab != null)
            modelInstance = InstantiateChild(modelPrefab, modelPrefab.name, profile.modelOffset, Quaternion.identity, wagonInstance.transform);
        //Default child instances
        GameObject wheels = InstantiateChild("Wheels", railsOffset, Quaternion.identity, wagonInstance.transform);
        GameObject sfx = InstantiateChild("SFX", Vector3.zero, Quaternion.identity, wagonInstance.transform);
        GameObject sensors = InstantiateChild("Sensors", railsOffset, Quaternion.identity, wagonInstance.transform);
        GameObject colliders = InstantiateChild("Colliders", Vector3.zero, Quaternion.identity, wagonInstance.transform);
        GameObject bumpers = InstantiateChild("Bumpers", Vector3.zero, Quaternion.identity, wagonInstance.transform);
        GameObject doors = InstantiateChild("Doors", Vector3.zero, Quaternion.identity, wagonInstance.transform);
        GameObject lights = InstantiateChild("Lights", Vector3.zero, Quaternion.identity, wagonInstance.transform);
        GameObject suspension = InstantiateChild("Suspension", railsOffset, Quaternion.identity, wheels.transform);

        //Add common unity components
        Rigidbody rigidbody = wagonInstance.AddComponent<Rigidbody>();
        //Configure common unity components
        rigidbody.mass = 10000f;

        //Instantiate common profile components
        InstantiateWagonComponents(profile.bumper, bumpers.transform, null);
        InstantiateWagonComponents(profile.suspensionCollider, suspension.transform, null);
        InstantiateWagonComponents(profile.colliders, colliders.transform, null);
        InstantiateWagonComponents(profile.internalDetails, wagonInstance.transform, null);
        InstantiateWagonComponents(profile.passengerSensor, wagonInstance.transform, null);

        //Instantiate specific profile components
        if (profile.type == WagonType.Wagon)
        {
            //Add specific unity components
            Wagon_v3 wagonScript = wagonInstance.AddComponent<Wagon_v3>();
            HingeJoint hingeJoint = wagonInstance.AddComponent<HingeJoint>();

            //Configure specific unity components
            hingeJoint.axis = new Vector3(1f, 1f, 0f);

            //Instantiate specific unity components
            InstantiateWagonComponents(profile.wheelsSFX, sfx.transform, new Action<Wagon_v3, List<GameObject>>(ConfigureWheelSFX), wagonScript);
            InstantiateWagonComponents(profile.wagonConnectionSFX, sfx.transform, new Action<Wagon_v3, List<GameObject>>(ConfigureConnectionSFX), wagonScript);
            //Couplers
            InstantiateWagonComponents(profile.frontCoupler, wagonInstance.transform, new Action<Rigidbody, Wagon_v3, HingeJoint, List<GameObject>>(ConfigureFrontCoupler), rigidbody, wagonScript, hingeJoint);
            InstantiateWagonComponents(profile.backCoupler, wagonInstance.transform, new Action<Rigidbody, Wagon_v3, List<GameObject>>(ConfigureBackCoupler), rigidbody, wagonScript);
            InstantiateWagonComponents(profile.defaultJointAnchor, wagonInstance.transform, new Action<Wagon_v3, List<GameObject>>(ConfigureDefaultJointAnchor), wagonScript);
            //Wheels
            InstantiateWagonComponents(profile.wheelsPhysics, wheels.transform, new Action<Rigidbody, Wagon_v3, List<GameObject>>(ConfigurePhysicsWheels), rigidbody, wagonScript);
            InstantiateWagonComponents(profile.wheelsVisuals, wheels.transform, new Action<Wagon_v3, List<GameObject>>(ConfigureWheels), wagonScript);
            //Sensors
            InstantiateWagonComponents(profile.railSensor, sensors.transform, new Action<Wagon_v3, List<GameObject>>(ConfigureSensors), wagonScript);
            //Lights
            InstantiateWagonComponents(profile.externalLights, lights.transform, new Action<Wagon_v3, List<GameObject>>(ConfigureExternalLights), wagonScript);
            InstantiateWagonComponents(profile.internalLights, lights.transform, new Action<Wagon_v3, List<GameObject>>(ConfigureInternalLights), wagonScript);
        }
        else if (profile.type == WagonType.Locomotive)
        {
            //Add specific unity components
            TrainController_v3 locomotiveScript = wagonInstance.AddComponent<TrainController_v3>();
            wagonInstance.AddComponent<TrainStationController>();
            TrainSpeedMonitor speedMonitor = wagonInstance.AddComponent<TrainSpeedMonitor>();
            TrainPlayerInput playerInput = wagonInstance.AddComponent<TrainPlayerInput>();
            PassengerTags passengerTags = wagonInstance.AddComponent<PassengerTags>();
            passengerTags.passengerTags = new List<string>() { "Player", "NPC" };
            BoxCollider locomotiveTrigger = wagonInstance.AddComponent<BoxCollider>();

            //Configure specific unity components
            locomotiveScript.enginesOn = true;
            locomotiveScript.acceleration = 1f;
            playerInput.inputSettings = profile.inputSettings;
            locomotiveTrigger.isTrigger = true;
            locomotiveTrigger.size = new Vector3(0.5f, 0.5f, 0.5f);
            locomotiveTrigger.center = profile.controlZoneTriggerPosition;
            speedMonitor.speedUnit = profile.speedUnits;

            //Instantitate specific unity components
            InstantiateWagonComponents(profile.engineSFX, sfx.transform, new Action<TrainController_v3, List<GameObject>>(ConfigureEngineSFX), locomotiveScript);
            InstantiateWagonComponents(profile.brakesSFX, sfx.transform, new Action<TrainController_v3, List<GameObject>>(ConfigureBrakesSFX), locomotiveScript);
            InstantiateWagonComponents(profile.wheelsSFX, sfx.transform, new Action<TrainController_v3, List<GameObject>>(ConfigureWheelSFX), locomotiveScript);
            InstantiateWagonComponents(profile.hornSFX, sfx.transform, new Action<TrainController_v3, List<GameObject>>(ConfigureHornSFX), locomotiveScript);
            InstantiateWagonComponents(profile.bellSFX, sfx.transform, new Action<TrainController_v3, List<GameObject>>(ConfigureBellSFX), locomotiveScript);
            //Couplers
            InstantiateWagonComponents(profile.frontCoupler, wagonInstance.transform, new Action<Rigidbody, List<GameObject>>(ConnectHingeAnchor), rigidbody);
            InstantiateWagonComponents(profile.backCoupler, wagonInstance.transform, new Action<Rigidbody, TrainController_v3, List<GameObject>>(ConfigureBackCoupler), rigidbody, locomotiveScript);
            //Wheels
            InstantiateWagonComponents(profile.wheelsPhysics, wheels.transform, new Action<Rigidbody, TrainController_v3, List<GameObject>>(ConfigurePhysicsWheels), rigidbody, locomotiveScript);
            InstantiateWagonComponents(profile.wheelsVisuals, wheels.transform, new Action<TrainController_v3, List<GameObject>>(ConfigureWheels), locomotiveScript);
            //Sensors
            InstantiateWagonComponents(profile.railSensor, sensors.transform, new Action<TrainController_v3, List<GameObject>>(ConfigureSensors), locomotiveScript);
            //Lights
            InstantiateWagonComponents(profile.externalLights, lights.transform, new Action<TrainController_v3, List<GameObject>>(ConfigureExternalLights), locomotiveScript);
            InstantiateWagonComponents(profile.internalLights, lights.transform, new Action<TrainController_v3, List<GameObject>>(ConfigureInternalLights), locomotiveScript);
        }
        
        TrainDoorsController doorsController = wagonInstance.AddComponent<TrainDoorsController>();
        wagonInstance.AddComponent<SMR_IgnoredObject>();
        wagonInstance.AddComponent<TrainSuspension>();

        InstantiateWagonComponents(profile.cabinDoorLeft, doors.transform, new Action<TrainDoorsController, List<GameObject>>(ConfigureLeftCabinDoor), doorsController);
        InstantiateWagonComponents(profile.cabinDoorRight, doors.transform, new Action<TrainDoorsController, List<GameObject>>(ConfigureRightCabinDoor), doorsController);
        InstantiateWagonComponents(profile.passengerDoorLeft, doors.transform, new Action<TrainDoorsController, List<GameObject>>(ConfigureLeftPassengerDoors), doorsController);
        InstantiateWagonComponents(profile.passengerDoorRight, doors.transform, new Action<TrainDoorsController, List<GameObject>>(ConfigureRightPassengerDoors), doorsController);
        InstantiateWagonComponents(profile.openDoorSFX, sfx.transform, new Action<TrainDoorsController, List<GameObject>>(ConfigureOpenDoorSFX), doorsController);
        InstantiateWagonComponents(profile.closeDoorSFX, sfx.transform, new Action<TrainDoorsController, List<GameObject>>(ConfigureCloseDoorSFX), doorsController);

        string carType = profile.type.ToString();
        message = string.Format("{0} created successfully!", carType);
        return true;
    }

    /// <summary>
    /// Instantiate new child based on prefab
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="name"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    private static GameObject InstantiateChild(GameObject prefab, string name, Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject child = GameObject.Instantiate(prefab, position, rotation, parent);
        child.name = name;

        return child;
    }

    /// <summary>
    /// Creates a new child game object instance
    /// </summary>
    /// <param name="name"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    private static GameObject InstantiateChild(string name, Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent);
        child.transform.position = position;
        child.transform.rotation = rotation;

        return child;
    }

    /// <summary>
    /// Instatiate wagon component based on profile settings
    /// </summary>
    /// <param name="wagonComponent"></param>
    /// <param name="parent"></param>
    private static void InstantiateWagonComponents(CustomWagonComponent wagonComponent, Transform parent, Delegate configMethod, params object[] configParams)
    {
        if (wagonComponent.prefab == null)
            return;

        string instanceName = string.Empty;

        List<GameObject> instances = new List<GameObject>();

        for (int i = 0; i < wagonComponent.positions.Length; i++)
        {

            instanceName = wagonComponent.customName != string.Empty ? wagonComponent.customName : wagonComponent.prefab.name;

            GameObject newComponentInstance = InstantiateChild(wagonComponent.prefab, instanceName, wagonComponent.positions[i].position, Quaternion.Euler(wagonComponent.positions[i].rotation), parent);

            instances.Add(newComponentInstance);
        }

        if (configMethod != null)
        {
            List<object> args = new List<object>();
            for (int i = 0; i < configParams.Length; i++)
                args.Add(configParams[i]);
            args.Add(instances);

            configParams = args.ToArray();

            configMethod.DynamicInvoke(configParams);
        }
    }

    #region COMPONENTS CONFIGURATION METHODS
    /// <summary>
    /// 
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="wheels"></param>
    private static void ConfigurePhysicsWheels(Rigidbody parent, IRailroadCar carScript, List<GameObject> wheels)
    {
        ConnectHingeAnchor(parent, wheels);
        ConfigureWheels(carScript, wheels);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="carScript"></param>
    /// <param name="wheels"></param>
    private static void ConfigureWheels(IRailroadCar carScript, List<GameObject> wheels)
    {
        carScript.Wheels = new List<TrainWheel_v3>();

        foreach (var item in wheels)
        {
            foreach (Transform t in item.transform)
            {
                TrainWheel_v3[] wheelScripts = t.GetComponentsInChildren<TrainWheel_v3>();
                if (wheelScripts != null)
                    carScript.Wheels.AddRange(wheelScripts);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="wagonScript"></param>
    /// <param name="joints"></param>
    private static void ConfigureDefaultJointAnchor(Wagon_v3 wagonScript, List<GameObject> joints)
    {
        if (joints != null && joints.Count > 0)
        {
            foreach (var item in joints)
            {
                Rigidbody rigid = item.GetComponent<Rigidbody>();
                if (rigid != null) wagonScript.jointAnchor = rigid;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="wagonScript"></param>
    /// <param name="couplers"></param>
    private static void ConfigureFrontCoupler(Rigidbody parent, Wagon_v3 wagonScript, HingeJoint parentJoint, List<GameObject> couplers)
    {
        ConnectHingeAnchor(parent, couplers);

        if (couplers != null && couplers.Count > 0)
        {
            foreach (var item in couplers)
            {
                Rigidbody rigid = item.GetComponent<Rigidbody>();
                if (rigid != null) wagonScript.frontJoint = rigid;
                parentJoint.anchor = item.transform.position;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="carScript">wagon or locomotive script</param>
    /// <param name="couplers"></param>
    private static void ConfigureBackCoupler(Rigidbody parent, IRailroadCar carScript, List<GameObject> couplers)
    {
        ConnectHingeAnchor(parent, couplers);

        if (couplers != null && couplers.Count > 0)
        {
            foreach (var item in couplers)
            {
                Rigidbody rigid = item.GetComponent<Rigidbody>();
                if (rigid != null) carScript.BackJoint = rigid;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="connectedBody"></param>
    /// <param name="toConnect"></param>
    private static void ConnectHingeAnchor(Rigidbody connectedBody, List<GameObject> toConnect)
    {
        foreach (var item in toConnect)
        {
            HingeJoint joint = item.GetComponent<HingeJoint>();
            if (joint != null) joint.connectedBody = connectedBody;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="carScript"></param>
    /// <param name="sfxs"></param>
    private static void ConfigureWheelSFX(IRailroadCar carScript, List<GameObject> sfxs)
    {
        if (sfxs != null && sfxs.Count > 0)
        {
            foreach (var item in sfxs)
            {
                AudioSource wheelSFX = item.GetComponent<AudioSource>();
                if (wheelSFX != null) carScript.WheelsSFX = wheelSFX;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="carScript"></param>
    /// <param name="sfxs"></param>
    private static void ConfigureConnectionSFX(Wagon_v3 carScript, List<GameObject> sfxs)
    {
        if (sfxs != null && sfxs.Count > 0)
        {
            foreach (var item in sfxs)
            {
                AudioSource connectionSFX = item.GetComponent<AudioSource>();
                if (connectionSFX != null) carScript.wagonConnectionSFX = connectionSFX;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="carScript"></param>
    /// <param name="sfxs"></param>
    private static void ConfigureHornSFX(TrainController_v3 carScript, List<GameObject> sfxs)
    {
        if (sfxs != null && sfxs.Count > 0)
        {
            foreach (var item in sfxs)
            {
                AudioSource audio = item.GetComponent<AudioSource>();
                if (audio != null) carScript.hornSFX = audio;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="carScript"></param>
    /// <param name="sfxs"></param>
    private static void ConfigureBellSFX(TrainController_v3 carScript, List<GameObject> sfxs)
    {
        if (sfxs != null && sfxs.Count > 0)
        {
            foreach (var item in sfxs)
            {
                AudioSource audio = item.GetComponent<AudioSource>();
                if (audio != null) carScript.bellSFX = audio;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="carScript"></param>
    /// <param name="sfxs"></param>
    private static void ConfigureEngineSFX(TrainController_v3 carScript, List<GameObject> sfxs)
    {
        if (sfxs != null && sfxs.Count > 0)
        {
            foreach (var item in sfxs)
            {
                AudioSource audio = item.GetComponent<AudioSource>();
                if (audio != null) carScript.engineSFX = audio;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="carScript"></param>
    /// <param name="sfxs"></param>
    private static void ConfigureBrakesSFX(TrainController_v3 carScript, List<GameObject> sfxs)
    {
        if (sfxs != null && sfxs.Count > 0)
        {
            foreach (var item in sfxs)
            {
                AudioSource audio = item.GetComponent<AudioSource>();
                if (audio != null) carScript.brakesSFX = audio;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="carScript"></param>
    /// <param name="sensors"></param>
    private static void ConfigureSensors(IRailroadCar carScript, List<GameObject> sensors)
    {
        if (sensors != null && sensors.Count > 0)
        {
            carScript.Sensors = new Sensors();

            for (int i = 0; i < sensors.Count; i++)
            {
                if (i % 2 == 0)
                    carScript.Sensors.leftSensor = sensors[i].GetComponent<RailSensor>();
                else
                    carScript.Sensors.rightSensor = sensors[i].GetComponent<RailSensor>();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="carScript"></param>
    /// <param name="lights"></param>
    private static void ConfigureExternalLights(IRailroadCar carScript, List<GameObject> lights)
    {
        if (lights != null && lights.Count > 0)
        {
            carScript.ExternalLights = new List<Light>();

            foreach (var item in lights)
            {
                Light light = item.GetComponent<Light>();
                if (light != null) carScript.ExternalLights.Add(light);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="carScript"></param>
    /// <param name="lights"></param>
    private static void ConfigureInternalLights(IRailroadCar carScript, List<GameObject> lights)
    {
        if (lights != null && lights.Count > 0)
        {
            if (carScript.InternalLights == null) carScript.InternalLights = new List<Light>();

            foreach (var item in lights)
            {
                Light light = item.GetComponent<Light>();
                if (light != null) carScript.InternalLights.Add(light);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="doorController"></param>
    /// <param name="doors"></param>
    private static void ConfigureLeftCabinDoor(TrainDoorsController doorController, List<GameObject> doors)
    {
        if (doors != null && doors.Count > 0)
        {
            foreach (var item in doors)
            {
                TrainDoor doorScript = item.GetComponent<TrainDoor>();
                if (doorScript != null) doorController.cabinDoorLeft = doorScript;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="doorController"></param>
    /// <param name="doors"></param>
    private static void ConfigureRightCabinDoor(TrainDoorsController doorController, List<GameObject> doors)
    {
        if (doors != null && doors.Count > 0)
        {
            foreach (var item in doors)
            {
                TrainDoor doorScript = item.GetComponent<TrainDoor>();
                if (doorScript != null) doorController.cabinDoorRight = doorScript;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="doorController"></param>
    /// <param name="doors"></param>
    private static void ConfigureLeftPassengerDoors(TrainDoorsController doorController, List<GameObject> doors)
    {
        if (doors != null && doors.Count > 0)
        {
            doorController.passengerDoorsLeft = new List<TrainDoor>();

            foreach (var item in doors)
            {
                TrainDoor doorScript = item.GetComponent<TrainDoor>();
                if (doorScript != null) doorController.passengerDoorsLeft.Add(doorScript);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="doorController"></param>
    /// <param name="doors"></param>
    private static void ConfigureRightPassengerDoors(TrainDoorsController doorController, List<GameObject> doors)
    {
        if (doors != null && doors.Count > 0)
        {
            doorController.passengerDoorsRight = new List<TrainDoor>();

            foreach (var item in doors)
            {
                TrainDoor doorScript = item.GetComponent<TrainDoor>();
                if (doorScript != null) doorController.passengerDoorsRight.Add(doorScript);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="doorController"></param>
    /// <param name="sfxs"></param>
    private static void ConfigureOpenDoorSFX(TrainDoorsController doorController, List<GameObject> sfxs)
    {
        if (sfxs != null && sfxs.Count > 0)
        {
            foreach (var item in sfxs)
            {
                AudioSource openDoorSFX = item.GetComponent<AudioSource>();
                if (openDoorSFX != null) doorController.openDoorSFX = openDoorSFX;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="doorController"></param>
    /// <param name="sfxs"></param>
    private static void ConfigureCloseDoorSFX(TrainDoorsController doorController, List<GameObject> sfxs)
    {
        if (sfxs != null && sfxs.Count > 0)
        {
            foreach (var item in sfxs)
            {
                AudioSource closeDoorSFX = item.GetComponent<AudioSource>();
                if (closeDoorSFX != null) doorController.closeDoorSFX = closeDoorSFX;
            }
        }
    }
    #endregion
}
