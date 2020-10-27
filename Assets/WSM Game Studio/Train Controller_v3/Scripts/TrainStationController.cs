using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace WSMGameStudio.RailroadSystem
{
    [RequireComponent(typeof(TrainController_v3))]
    public class TrainStationController : MonoBehaviour
    {
        public Animator animator;
        private TrainController_v3 _trainController;
        private bool _turnOffEngines = false;
        private bool _stopping = false;
        private float _stopTimeout;
        private float _lastDirection = 0f;
        private StationBehaviour _stationBehaviour;

        /// <summary>
        /// Execute custom event stack when activating brakes to stop at station
        /// </summary>
        public UnityEvent onBrakesActivation;
        /// <summary>
        /// Execute custom event stack when the train stops moving
        /// </summary>
        public UnityEvent onStop;
        /// <summary>
        /// Execute custom event stack before leaving the station (Only after stop has been executed)
        /// </summary>
        public UnityEvent onLeave;

        // Use this for initialization
        void Start()
        {
            _trainController = GetComponent<TrainController_v3>();
        }

        // Update is called once per frame
        void Update()
        {
            if (_stopping && _trainController.Speed_MPS <= 0.1f)
            {
                onStop.Invoke();
                _stopping = false;

                if (_turnOffEngines)
                    _trainController.enginesOn = false;

                if (_stationBehaviour == StationBehaviour.LeaveAfterTime && _stopTimeout >= 0f)
                    Invoke("Leave", _stopTimeout);
            }
        }

        /// <summary>
        /// Set stop parameters and activate brakes
        /// </summary>
        /// <param name="stationBehaviour">Leave after or stop forever</param>
        /// <param name="stopTimeout">Optional</param>
        public void StopAtStation(StationBehaviour stationBehaviour, float stopTimeout, bool turnOffEngines)
        {
            _turnOffEngines = turnOffEngines;
            _stopTimeout = stopTimeout;
            _stationBehaviour = stationBehaviour;
            Stop();
            animator.SetTrigger("Open");
        }

        /// <summary>
        /// Activate brakes to stop at station
        /// </summary>
        private void Stop()
        {
            _lastDirection = _trainController.acceleration;
            _trainController.acceleration = 0f;
            _trainController.brake = 1f;

            _stopping = true;

            onBrakesActivation.Invoke();
        }

        /// <summary>
        /// Leave station
        /// </summary>
        private void Leave()
        {
            _stopping = false;

            _trainController.enginesOn = true;
            _trainController.acceleration = _lastDirection;
            _trainController.brake = 0f;
            animator.SetTrigger("Close");

            onLeave.Invoke();
        }
    }
}
