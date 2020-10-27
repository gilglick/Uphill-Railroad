using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WSMGameStudio.RailroadSystem
{
    public class TrainWheel_v3 : MonoBehaviour
    {
        [Tooltip("Optimized wheels don't use physics and applied animated rotation instead")]
        public bool optimized = false;

        private float _speed = 0f;
        [Range(0f, 1f)]
        private float _brake = 0f;

        private Rigidbody _rigidbody;
        private Transform _transform;

        public float Brake
        {
            get { return _brake; }
            set { _brake = value; }
        }

        public float Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }

        /// <summary>
        /// Initialize wheel
        /// </summary>
        void Start()
        {
            _transform = GetComponent<Transform>();
            _rigidbody = GetComponent<Rigidbody>();

            if (!optimized)
            {
                _rigidbody.maxAngularVelocity = GeneralSettings.WheelsMaxAngularVelocity;
                _rigidbody.angularDrag = GeneralSettings.IdleDrag;
            }
        }

        /// <summary>
        /// Fixed update
        /// </summary>
        void FixedUpdate()
        {
            if (optimized)
            {
                _transform.Rotate(_speed, 0f, 0f, Space.Self);
            }
            else
            {
                TrainPhysics.ApplyBrakes(_rigidbody, _brake, 0f);
            }
        }

        /// <summary>
        /// Applies downforce
        /// </summary>
        private void ApplyDownForce()
        {
            _rigidbody.AddForceAtPosition(GeneralSettings.DownForceFactor * _rigidbody.velocity.magnitude * -_transform.up, _transform.position);
        }
    }
}
