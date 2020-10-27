using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace WSMGameStudio.RailroadSystem
{
    public class TrainController_v3 : MonoBehaviour, IRailroadCar
    {
        #region VARIABLES
        private Rigidbody _rigidbody;
        private Transform _transform;
        private bool _isGrounded = false;
        private bool _onRails = false;
        private float _speed;
        private float _speed_KPH;
        private float _speed_MPH;
        //Movement
        private Vector3 _targetVelocity;
        private float _targetSpeed;
        private float _currentSpeed;
        private Vector3 _localVelocity;
        //SFX
        private SFX _sfx;
        private ITrainDoorsController _doorController;

        public bool enginesOn = false;
        [Range(0f, 105f)]
        public float maxSpeedKph = 65f;
        [Range(-1f, 1f)]
        public float acceleration = 0f;
        public bool automaticBrakes = true;
        [Range(0f, 1f)]
        public float brake = 0f;
        public List<TrainWheel_v3> wheelsScripts;
        public Sensors sensors;
        public List<Wagon_v3> wagons;
        public List<Light> externalLights;
        public List<Light> internalLights;
        public AudioSource hornSFX;
        public AudioSource bellSFX;
        public AudioSource engineSFX;
        public AudioSource wheelsSFX;
        public AudioSource brakesSFX;
        public Rigidbody backJoint;

        #endregion

        #region PROPERTIES
        /// <summary>
        /// Train speed at meters per second
        /// </summary>
        public float Speed_MPS
        {
            get { return _speed; }
        }

        /// <summary>
        /// Train speed at Kilometers per second
        /// </summary>
        public float Speed_KPH
        {
            get { return _speed_KPH; }
        }

        /// <summary>
        /// Train speed at Miles per hour
        /// </summary>
        public float Speed_MPH
        {
            get { return _speed_MPH; }
        }

        /// <summary>
        /// Ground check
        /// </summary>
        public bool IsGrounded
        {
            get { return _isGrounded; }
            set { _isGrounded = value; }
        }

        /// <summary>
        /// Rails check
        /// </summary>
        public bool OnRails
        {
            get { return _onRails; }
        }

        public ITrainDoorsController DoorsController
        {
            get
            {
                if (_doorController == null)
                    _doorController = GetComponent<ITrainDoorsController>();

                return _doorController;
            }
        }

        public bool BellOn
        {
            get
            {
                if (_sfx.bellSFX != null)
                    return _sfx.bellSFX.isPlaying;

                return false;
            }
        }

        public Rigidbody BackJoint
        {
            get { return backJoint; }
            set { backJoint = value; }
        }

        public List<TrainWheel_v3> Wheels
        {
            get { return wheelsScripts; }
            set { wheelsScripts = value; }
        }

        public AudioSource WheelsSFX
        {
            get { return wheelsSFX; }
            set { wheelsSFX = value; }
        }

        public Sensors Sensors
        {
            get { return sensors; }
            set { sensors = value; }
        }

        public List<Light> ExternalLights
        {
            get { return externalLights; }
            set { externalLights = value; }
        }

        public List<Light> InternalLights
        {
            get { return internalLights; }
            set { internalLights = value; }
        }
        #endregion

        #region UNITY EVENTS
        /// <summary>
        /// Initialize train
        /// </summary>
        void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _transform = GetComponent<Transform>();
            _doorController = GetComponent<ITrainDoorsController>();
            ConnectWagons();
            InitializeSFX();
        }

        /// <summary>
        /// Train physics
        /// </summary>
        void FixedUpdate()
        {
            brake = automaticBrakes ? 1f - Mathf.Abs(acceleration) : brake;

            IsGrounded = sensors.leftSensor.grounded || sensors.rightSensor.grounded;
            _onRails = sensors.leftSensor.onRails || sensors.rightSensor.onRails;
            _speed = _rigidbody.velocity.magnitude;
            _speed_MPH = Speed.Convert_MPS_To_MPH(_speed);
            _speed_KPH = Speed.Convert_MPS_To_KPH(_speed);

            TrainAudio.PlaySFX(_sfx, _speed_KPH, brake, enginesOn, _isGrounded);

            _localVelocity = _transform.InverseTransformDirection(_rigidbody.velocity);

            if (!enginesOn)
            {
                acceleration = Mathf.MoveTowards(acceleration, 0f, GeneralSettings.DeaccelerationRate * Time.deltaTime);
                brake = 1f;
            }

            TrainPhysics.UpdateWheels(wheelsScripts, brake, _localVelocity.z);

            TrainPhysics.SpeedControl(_rigidbody, _isGrounded, maxSpeedKph, _speed_KPH, acceleration, brake, _targetVelocity, out _targetVelocity, _currentSpeed, out _currentSpeed, _targetSpeed, out _targetSpeed);
        }
        #endregion

        #region PRIVATE METHODS
        /// <summary>
        /// Connect wagons hinges
        /// </summary>
        private void ConnectWagons()
        {
            for (int i = 0; i < wagons.Count; i++)
            {
                if (i == 0) // Connect wagon to locomotive
                    TrainPhysics.ConnectTrainCar(wagons[i].GetComponent<HingeJoint>(), backJoint);
                else // Connect wagon to wagon
                    TrainPhysics.ConnectTrainCar(wagons[i].GetComponent<HingeJoint>(), wagons[i - 1].backJoint);

                wagons[i].Locomotive = this;
            }
        }

        /// <summary>
        /// Initialize SFX
        /// </summary>
        private void InitializeSFX()
        {
            _sfx = new SFX();
            _sfx.hornSFX = hornSFX;
            _sfx.bellSFX = bellSFX;
            _sfx.engineSFX = engineSFX;
            _sfx.wheelsSFX = wheelsSFX;
            _sfx.brakesSFX = brakesSFX;
        }

        /// <summary>
        /// Update door controller wagons references
        /// </summary>
        public void UpdateDoorController()
        {
            if (_doorController != null)
                _doorController.UpdateWagonsDoorsControllers();
        }
        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Disconnect last wagon
        /// </summary>
        public void DecoupleLastWagon()
        {
            if (wagons == null || wagons.Count == 0)
                return;

            DecoupleWagon(wagons.Count - 1);
        }

        /// <summary>
        /// Disconnect first wagons connected to the locomotive
        /// </summary>
        public void DecoupleFirstWagon()
        {
            DecoupleWagon(0);
        }

        /// <summary>
        /// Diconnect wagon by index
        /// </summary>
        /// <param name="index"></param>
        public void DecoupleWagon(int index)
        {
            if (wagons == null || index > wagons.Count - 1)
                return;

            for (int i = (wagons.Count - 1); i >= index; i--)
            {
                wagons[i].Disconnect((i == index));
                wagons.RemoveAt(i);
            }

            UpdateDoorController();
        }

        /// <summary>
        /// Turn external lights on/off
        /// </summary>
        public void ToggleLights()
        {
            if (externalLights != null)
            {
                foreach (Light light in externalLights)
                {
                    light.enabled = !light.enabled;
                }
            }

            if (wagons != null)
            {
                foreach (var wagon in wagons)
                {
                    wagon.ToggleLights();
                }
            }
        }

        /// <summary>
        /// Turn internal lights on/off
        /// </summary>
        public void ToggleInternalLights()
        {
            if (internalLights != null)
            {
                foreach (Light light in internalLights)
                {
                    light.enabled = !light.enabled;
                }
            }

            if (wagons != null)
            {
                foreach (var wagon in wagons)
                {
                    wagon.ToggleInternalLights();
                }
            }
        }

        /// <summary>
        ///  Toggle engine on/off
        /// </summary>
        public void ToggleEngine()
        {
            enginesOn = !enginesOn;
        }

        /// <summary>
        /// play the train horn
        /// </summary>
        public void Honk()
        {
            if (_sfx.hornSFX == null)
                return;

            if (!_sfx.hornSFX.isPlaying)
                _sfx.hornSFX.Play();
        }

        /// <summary>
        /// Toggle train security bell
        /// </summary>
        public void ToogleBell()
        {
            if (_sfx.bellSFX != null)
            {
                if (_sfx.bellSFX.isPlaying)
                    _sfx.bellSFX.Stop();
                else
                    _sfx.bellSFX.Play();
            }
        }

        #endregion
    }
}
