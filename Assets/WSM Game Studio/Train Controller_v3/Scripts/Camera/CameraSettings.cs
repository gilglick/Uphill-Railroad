using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WSMGameStudio.Cameras
{
    [Serializable]
    public class CameraSettings
    {
        public float cameraSpeed = 0;
        public Vector3 offset;
        public bool rotateAround = false;
        public bool lookAtTarget = true;
    } 
}
