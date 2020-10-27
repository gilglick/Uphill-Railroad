using System;
using UnityEngine;

namespace WSMGameStudio.Cameras
{
    public class FlyingCamera : MonoBehaviour
    {
        public float translationSpeed = 10f;
        public float rotationSpeed = 20f;

        public bool autoTranslateForward;
        public bool autoTranslateBackwards;
        public bool autoTranslateRight;
        public bool autoTranslateLeft;
        public bool autoTranslateUp;
        public bool autoTranslateDown;
        public bool autoRotateRight;
        public bool autoRotateLeft;
        public bool autoRotateUp;
        public bool autoRotateDown;

        private Transform _transform;
        private Vector3 _translation;
        private Vector3 _rotation;

        private Vector3 _initPosition;
        private Quaternion _initRotation;

        // Use this for initialization
        void Start()
        {
            _transform = GetComponent<Transform>();
            _initPosition = _transform.position;
            _initRotation = _transform.rotation;
        }

        // Update is called once per frame
        void Update()
        {
            _translation = Vector3.zero;
            _rotation = Vector3.zero;

            // Translation Input
            if (Input.GetKey(KeyCode.W) || autoTranslateForward)
                _translation += WorldToLocal(_transform.forward * translationSpeed * Time.deltaTime);
            if (Input.GetKey(KeyCode.S) || autoTranslateBackwards)
                _translation += WorldToLocal(-(_transform.forward) * translationSpeed * Time.deltaTime);
            if (Input.GetKey(KeyCode.D) || autoTranslateRight)
                _translation += WorldToLocal(_transform.right * translationSpeed * Time.deltaTime);
            if (Input.GetKey(KeyCode.A) || autoTranslateLeft)
                _translation += WorldToLocal(-(_transform.right) * translationSpeed * Time.deltaTime);
            if (Input.GetKey(KeyCode.PageUp) || autoTranslateUp)
                _translation += WorldToLocal(_transform.up * translationSpeed * Time.deltaTime);
            if (Input.GetKey(KeyCode.PageDown) || autoTranslateDown)
                _translation += WorldToLocal(-(_transform.up) * translationSpeed * Time.deltaTime);

            // Rotation Input
            if (Input.GetKey(KeyCode.UpArrow) || autoRotateUp)
                _transform.RotateAround(_transform.position, _transform.right, -rotationSpeed * Time.deltaTime);
            if (Input.GetKey(KeyCode.DownArrow) || autoRotateDown)
                _transform.RotateAround(_transform.position, _transform.right, rotationSpeed * Time.deltaTime);
            if (Input.GetKey(KeyCode.RightArrow) || autoRotateRight)
                _transform.RotateAround(_transform.position, Vector3.up, rotationSpeed * Time.deltaTime);
            if (Input.GetKey(KeyCode.LeftArrow) || autoRotateLeft)
                _transform.RotateAround(_transform.position, Vector3.up, -rotationSpeed * Time.deltaTime);

            // Apply Translation
            _transform.Translate(_translation, Space.Self);

            if (Input.GetKeyDown(KeyCode.R))
                ResetPosition();
        }

        private void ResetPosition()
        {
            _transform.position = _initPosition;
            _transform.rotation = _initRotation;
        }

        private Vector3 WorldToLocal(Vector3 direction)
        {
            return _transform.InverseTransformDirection(direction);
        }
    }
}