using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WSMGameStudio.RailroadSystem
{
    public class Wagon_v3 : MonoBehaviour, IRailroadCar
    {
        #region VARIABLES
        private TrainController_v3 _locomotive;
        private HingeJoint _carJoint;

        private Rigidbody _rigidbody;
        private Transform _transform;
        private bool _isGrounded;
        [Range(0f, 105f)]
        private float _maxSpeedKph = 65f;
        [Range(-1f, 1f)]
        private float _acceleration = 0f;
        [Range(0f, 1f)]
        private float _brake = 0f;
        private float _speed;
        //private bool _shouldBeStatic = false;
        private bool _reverseAcceleration = false;

        //Movement
        private Vector3 _targetVelocity;
        private float _targetSpeed;
        private float _currentSpeed;
        private float _wagonAccel;
        private Vector3 _localVelocity;
        //SFX
        private SFX _sfx;

        public AudioSource wheelsSFX;
        public AudioSource wagonConnectionSFX;
        public List<TrainWheel_v3> wheelsScripts;
        public Sensors sensors;
        public List<Light> externalLights;
        public List<Light> internalLights;
        public Rigidbody backJoint; //Must be rigibody for hinge connection
        public Rigidbody frontJoint;
        public Rigidbody jointAnchor;
        public WagonCoupling coupling;
        public WagonDecouplingSettings decouplingSettings;
        public float recouplingTimeout = 5f;
        #endregion

        #region PROPERTIES
        // Locomotive that commands this wagon
        public TrainController_v3 Locomotive
        {
            get { return _locomotive; }
            set { _locomotive = value; }
        }

        // Joint for wagon connection
        public HingeJoint CarJoint
        {
            get { return _carJoint; }
        }

        public bool IsConected
        { get { return _locomotive != null; } }

        public bool IsGrounded
        {
            get { return _isGrounded; }
        }

        public float MaxSpeedKph
        {
            get { return _maxSpeedKph; }
            set { _maxSpeedKph = value; }
        }

        public float Acceleration
        {
            get { return _acceleration; }
            set { _acceleration = value; }
        }

        public float Brake
        {
            get { return _brake; }
            set { _brake = value; }
        }

        /// <summary>
        /// Distance between front and back joints
        /// </summary>
        public float JoinDistance
        {
            get
            {
                if (frontJoint == null || backJoint == null)
                {
                    Debug.LogError("Wagons Joints not set. Please manually set the joints and try again");
                    return 0f;
                }

                return Mathf.Abs(frontJoint.transform.localPosition.z) + Mathf.Abs(backJoint.transform.localPosition.z); ;
            }
        }

        /// <summary>
        /// Identify if wagon was connected backwards and needs to move the oposite direction
        /// </summary>
        public bool ReverseAcceleration
        {
            get { return _reverseAcceleration; }
            set { _reverseAcceleration = value; }
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
        /// Initialie wagon
        /// </summary>
        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _transform = GetComponent<Transform>();
            _carJoint = GetComponent<HingeJoint>();
            InitializeSFX();
        }

        /// <summary>
        /// Physics
        /// </summary>
        void FixedUpdate()
        {
            EnforceAnchorPosition();

            UpdateVelocity();

            _isGrounded = sensors.leftSensor.grounded || sensors.rightSensor.grounded;

            _speed = _rigidbody.velocity.magnitude;

            _wagonAccel = _reverseAcceleration ? (_acceleration * (-1)) : _acceleration;

            _localVelocity = _transform.InverseTransformDirection(_rigidbody.velocity);

            TrainPhysics.UpdateWheels(wheelsScripts, _brake, _localVelocity.z);

            TrainPhysics.SpeedControl(_rigidbody, _isGrounded, _maxSpeedKph, Speed.Convert_MPS_To_KPH(_speed), _wagonAccel, _brake, _targetVelocity, out _targetVelocity, _currentSpeed, out _currentSpeed, _targetSpeed, out _targetSpeed);

            TrainAudio.PlaySFX(_sfx, Speed.Convert_MPS_To_KPH(_speed), _brake, false, _isGrounded);
        }
        #endregion

        #region METHODS

        /// <summary>
        /// Initialize SFX
        /// </summary>
        private void InitializeSFX()
        {
            _sfx = new SFX();
            _sfx.wheelsSFX = wheelsSFX;
            _sfx.wagonConnectionSFX = wagonConnectionSFX;
        }

        /// <summary>
        /// Turn external lights on/off
        /// </summary>
        public void ToggleLights()
        {
            if (externalLights == null)
                return;

            foreach (Light light in externalLights)
            {
                light.enabled = !light.enabled;
            }
        }

        /// <summary>
        /// Turn internal lights on/off
        /// </summary>
        public void ToggleInternalLights()
        {
            if (internalLights == null)
                return;

            foreach (Light light in internalLights)
            {
                light.enabled = !light.enabled;
            }
        }

        /// <summary>
        /// Connect wagon to train
        /// </summary>
        /// <param name="carCoupler">Current wagon coupler (front or back)</param>
        /// <param name="otherCarCoupler">Other wagon coupler</param>
        public void Connect(TrainCarCoupler carCoupler, TrainCarCoupler otherCarCoupler, bool playSFX)
        {
            if (coupling == WagonCoupling.Enabled)
            {
                if (otherCarCoupler.IsLocomotive)
                {
                    _locomotive = otherCarCoupler.Locomotive;
                    _reverseAcceleration = (carCoupler.IsBackJoint == otherCarCoupler.IsBackJoint);
                }
                else if (otherCarCoupler.IsWagon)
                {
                    if (!otherCarCoupler.Wagon.IsConected)
                        return;

                    _locomotive = otherCarCoupler.Wagon.Locomotive;
                    _reverseAcceleration = (carCoupler.IsBackJoint != otherCarCoupler.IsBackJoint) ? otherCarCoupler.Wagon.ReverseAcceleration : !otherCarCoupler.Wagon.ReverseAcceleration;
                }

                TrainPhysics.ConnectTrainCar(_carJoint, otherCarCoupler.GetComponent<Rigidbody>());
                _locomotive.wagons.Add(this);
                _locomotive.UpdateDoorController();

                if (playSFX && _sfx.wagonConnectionSFX != null)
                    _sfx.wagonConnectionSFX.Play();
            }
        }

        /// <summary>
        /// Disconnect wagon from train
        /// </summary>
        public void Disconnect(bool disconnectJoint)
        {
            if (disconnectJoint)
                _carJoint.connectedBody = jointAnchor;
            _locomotive = null;

            if (_sfx.wagonConnectionSFX != null)
                _sfx.wagonConnectionSFX.Play();

            coupling = WagonCoupling.Disabled;

            if (decouplingSettings == WagonDecouplingSettings.AllowRecoupling)
            {
                Invoke("ReenabledCoupling", Mathf.Abs(recouplingTimeout));
            }
        }

        /// <summary>
        /// Reenable wagon coupling
        /// </summary>
        private void ReenabledCoupling()
        {
            coupling = WagonCoupling.Enabled;
        }

        /// <summary>
        /// Updates wagon velocity
        /// </summary>
        private void UpdateVelocity()
        {
            if (_locomotive != null && _locomotive.enginesOn)
            {
                _acceleration = _locomotive.OnRails ? _locomotive.acceleration : 0;
                _maxSpeedKph = _locomotive.OnRails ? _locomotive.maxSpeedKph : 0;
                _brake = _locomotive.OnRails ? _locomotive.brake : 0;
            }
            else
            {
                _acceleration = Mathf.MoveTowards(_acceleration, 0f, GeneralSettings.DeaccelerationRate * Time.deltaTime);
                _maxSpeedKph = Mathf.MoveTowards(_maxSpeedKph, 0f, GeneralSettings.DeaccelerationRate * Time.deltaTime);
                _brake = 1f;
                _targetVelocity = Vector3.zero;
            }
        }

        /// <summary>
        /// Always keeps anchor in the right place
        /// </summary>
        private void EnforceAnchorPosition()
        {
            jointAnchor.transform.localPosition = _carJoint.anchor;
            jointAnchor.transform.localRotation = Quaternion.identity;
        } 
        #endregion
    }
}
