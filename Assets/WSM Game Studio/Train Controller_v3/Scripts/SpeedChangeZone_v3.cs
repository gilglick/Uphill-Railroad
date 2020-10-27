using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WSMGameStudio.RailroadSystem
{
    public class SpeedChangeZone_v3 : MonoBehaviour
    {
        [Range(0f, 105f)]
        public float targetSpeedKph;

        private void OnTriggerEnter(Collider other)
        {
            TrainController_v3 train = other.GetComponent<TrainController_v3>();

            if (train != null)
                train.maxSpeedKph = targetSpeedKph;
        }
    } 
}
