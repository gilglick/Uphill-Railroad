using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WSMGameStudio.RailroadSystem
{
    public class DemoUI_v3 : MonoBehaviour
    {
        public TrainController_v3 train;
        public Slider maxSpeedSlider;
        public Slider accelerationSlider;
        public Slider brakeSlider;
        public Toggle automaticBrakes;
        public Text playerInputText;

        private ITrainDoorsController _doorController;
        private TrainPlayerInput _playerInput;

        private void Start()
        {
            if (train == null) return;

            _doorController = train.GetComponent<ITrainDoorsController>();
            _playerInput = train.GetComponent<TrainPlayerInput>();

            ConfigureInputText();
        }

        private void Update()
        {
            if (train == null)
                return;

            if (_playerInput != null && _playerInput.enablePlayerInput)
                return;

            if (maxSpeedSlider != null)
                train.maxSpeedKph = maxSpeedSlider.value;

            if (accelerationSlider != null)
                train.acceleration = accelerationSlider.value;

            if (automaticBrakes != null)
                train.automaticBrakes = automaticBrakes.isOn;

            if (brakeSlider != null)
            {
                brakeSlider.enabled = !train.automaticBrakes;

                if (train.automaticBrakes)
                    brakeSlider.value = train.brake;
                else
                    train.brake = brakeSlider.value;
            }
        }

        private void ConfigureInputText()
        {
            if (playerInputText != null && _playerInput != null && _playerInput.inputSettings != null)
            {
                playerInputText.text = string.Format("Engines: {1}{0}" +
                    "Forward: {2}{0}" +
                    "Reverse: {3}{0}" +
                    "Speed (+): {4}{0}" +
                    "Speed (-): {5}{0}" +
                    "Brakes: {6}{0}" +
                    "Lights: {7}{0}" +
                    "Cabin Lights: {8}{0}" +
                    "Honk: {9}{0}" +
                    "Bell: {10}{0}" +
                    "Cabin Door: {11}{0}"
                    , System.Environment.NewLine, _playerInput.inputSettings.toggleEngine
                    , _playerInput.inputSettings.forward
                    , _playerInput.inputSettings.reverse
                    , _playerInput.inputSettings.increaseSpeed
                    , _playerInput.inputSettings.decreaseSpeed
                    , _playerInput.inputSettings.brakes
                    , _playerInput.inputSettings.lights
                    , _playerInput.inputSettings.internalLights
                    , _playerInput.inputSettings.honk
                    , _playerInput.inputSettings.bell
                    , _playerInput.inputSettings.cabinRightDoor
                    );
            }
        }

        public void ToggleEngine()
        {
            if (train == null)
                return;

            train.ToggleEngine();
        }

        public void ToggleLights()
        {
            if (train == null)
                return;

            train.ToggleLights();
        }

        public void ToggleInternalLights()
        {
            if (train == null)
                return;

            train.ToggleInternalLights();
        }

        public void Honk()
        {
            if (train == null)
                return;

            train.Honk();
        }

        public void ToggleBell()
        {
            if (train == null)
                return;

            train.ToogleBell();
        }

        public void CabinDoor()
        {
            if (_doorController.CabinRightDoorOpen)
                _doorController.CloseCabinDoorRight();
            else
            {
                _doorController.OpenCabinDoorRight();
            }
        }
    }
}
