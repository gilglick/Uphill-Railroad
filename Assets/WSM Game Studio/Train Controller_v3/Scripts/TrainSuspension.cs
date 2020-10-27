using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WSMGameStudio.RailroadSystem
{
    /// <summary>
    /// Simulates train suspension (side bearings) by reducing angular velocity transferred from wheels and rails collision to the wagon body
    /// </summary>
    public class TrainSuspension : MonoBehaviour
    {
        private Rigidbody _rigidbody;
        private Vector3 _angularVelocity;

        private float _xAngularDamp = 0.8f;
        private float _yAngularDamp = 1f;
        private float _zAngularDamp = 0f;

        /// <summary>
        /// Initialize
        /// </summary>
        void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _angularVelocity = new Vector3();
        }

        /// <summary>
        /// Apply constraints
        /// </summary>
        void FixedUpdate()
        {
            _angularVelocity.x = _rigidbody.angularVelocity.x * _xAngularDamp;
            _angularVelocity.y = _rigidbody.angularVelocity.y * _yAngularDamp;
            _angularVelocity.z = _rigidbody.angularVelocity.z * _zAngularDamp;

            _rigidbody.angularVelocity = _angularVelocity;
        }
    }
}
