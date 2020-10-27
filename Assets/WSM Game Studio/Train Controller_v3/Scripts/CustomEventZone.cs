using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace WSMGameStudio.RailroadSystem
{
    public class CustomEventZone : MonoBehaviour
    {
        public UnityEvent customEvents;

        private void OnTriggerEnter(Collider other)
        {
            TrainController_v3 train = other.GetComponent<TrainController_v3>();

            if (train != null)
            {
                if (customEvents != null)
                    customEvents.Invoke();
            }
        }
    } 
}
