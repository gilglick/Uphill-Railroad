using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace WSMGameStudio.RailroadSystem
{
    [RequireComponent(typeof(TrainController_v3))]
    public class TrainPlayerInput : MonoBehaviour
    {
        public bool enablePlayerInput = false;
        public TrainInputSettings inputSettings;

        public UnityEvent[] customEvents;

        private TrainController_v3 _locomotive;
        private TrainDoorsController _doorController;

        // Use this for initialization
        void Start()
        {
            _locomotive = GetComponent<TrainController_v3>();
            _doorController = GetComponent<TrainDoorsController>();
        }

        // Update is called once per frame
        void Update()
        {
            if (enablePlayerInput)
            {
                if (inputSettings == null)
                    return;

                #region Movement Controls
                if (Input.GetKey(inputSettings.forward))
                    _locomotive.acceleration = 1f;
                else if (Input.GetKey(inputSettings.reverse))
                    _locomotive.acceleration = -1f;
                else
                    _locomotive.acceleration = 0f;

                if (!_locomotive.automaticBrakes)
                    _locomotive.brake = Input.GetKey(inputSettings.brakes) ? 1f : 0f;
                #endregion

                #region Max Speed Control
                inputSettings.speedIncreaseAmount = Mathf.Abs(inputSettings.speedIncreaseAmount);

                if (Input.GetKeyDown(inputSettings.increaseSpeed))
                    _locomotive.maxSpeedKph = (_locomotive.maxSpeedKph < GeneralSettings.MaxSpeed) ? _locomotive.maxSpeedKph + inputSettings.speedIncreaseAmount : GeneralSettings.MaxSpeed;
                else if (Input.GetKeyDown(inputSettings.decreaseSpeed))
                    _locomotive.maxSpeedKph = (_locomotive.maxSpeedKph > GeneralSettings.MinSpeed) ? _locomotive.maxSpeedKph - inputSettings.speedIncreaseAmount : GeneralSettings.MinSpeed;
                #endregion

                #region Default Train Events
                if (Input.GetKeyDown(inputSettings.lights))
                    _locomotive.ToggleLights();

                if (Input.GetKeyDown(inputSettings.internalLights))
                    _locomotive.ToggleInternalLights();

                if (Input.GetKeyDown(inputSettings.honk))
                    _locomotive.Honk();

                if (Input.GetKeyDown(inputSettings.bell))
                    _locomotive.ToogleBell();

                if (Input.GetKeyDown(inputSettings.toggleEngine))
                    _locomotive.ToggleEngine();

                if (_doorController != null)
                {
                    if (Input.GetKeyDown(inputSettings.cabinLeftDoor))
                    {
                        if (_doorController.CabinLeftDoorOpen)
                            _doorController.CloseCabinDoorLeft();
                        else
                            _doorController.OpenCabinDoorLeft();
                    }

                    if (Input.GetKeyDown(inputSettings.cabinRightDoor))
                    {
                        if (_doorController.CabinRightDoorOpen)
                            _doorController.CloseCabinDoorRight();
                        else
                            _doorController.OpenCabinDoorRight();
                    }

                    if (Input.GetKeyDown(inputSettings.passengerLeftDoor))
                    {
                        if (_doorController.PassengerLeftDoorOpen)
                            _doorController.ClosePassengersLeftDoors();
                        else
                        {
                            _doorController.OpenPassengersDoors(StationDoorDirection.Left);
                        }
                    }

                    if (Input.GetKeyDown(inputSettings.passengerRightDoor))
                    {
                        if (_doorController.PassengerRightDoorOpen)
                            _doorController.ClosePassengersRightDoors();
                        else
                        {
                            _doorController.OpenPassengersDoors(StationDoorDirection.Right);
                        }
                    }
                }

                #endregion

                #region Player Custom Events
                for (int i = 0; i < inputSettings.customEventTriggers.Length; i++)
                {
                    if (Input.GetKeyDown(inputSettings.customEventTriggers[i]))
                    {
                        if (customEvents.Length > i)
                            customEvents[i].Invoke();
                    }
                }
                #endregion
            }
        }
    }
}
