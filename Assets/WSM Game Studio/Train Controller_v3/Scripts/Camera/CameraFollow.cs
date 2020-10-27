using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WSMGameStudio.Cameras
{
    public class CameraFollow : MonoBehaviour
    {
        public bool follow = true;
        public Transform target;
        public CameraSettings cameraSettings;

        private Vector3 newPos;
        private Quaternion newRot;

        // Update is called once per frame
        void Update()
        {
            ApplyOffsetToPosition(cameraSettings);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cameraSettings"></param>
        private void ApplyOffsetToPosition(CameraSettings cameraSettings)
        {
            if (!follow)
                return;

            if (target != null)
            {
                if (this.cameraSettings.lookAtTarget)
                {
                    newPos = transform.position;
                    newPos.x = target.position.x + cameraSettings.offset.x;
                    newPos.z = target.position.z + cameraSettings.offset.z;
                    newPos.y = target.position.y + cameraSettings.offset.y;
                }
                else
                {
                    newPos = target.position
                        + (target.right * cameraSettings.offset.x)
                        + (target.up * cameraSettings.offset.y)
                        + (target.forward * cameraSettings.offset.z);

                    newRot = target.rotation * Quaternion.Euler(target.forward);
                }

                transform.position = newPos;

                if (!this.cameraSettings.rotateAround)
                    transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, Time.deltaTime);

                if (this.cameraSettings.lookAtTarget)
                    transform.LookAt(target.transform);
            }
        }
    }
}

