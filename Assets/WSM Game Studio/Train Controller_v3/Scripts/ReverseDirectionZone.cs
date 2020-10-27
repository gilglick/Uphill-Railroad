using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WSMGameStudio.RailroadSystem
{
    public class ReverseDirectionZone : MonoBehaviour
    {
        public ReverseDirectionMode reverseDirectionMode;

        private void OnTriggerEnter(Collider other)
        {
            TrainController_v3 train = other.GetComponent<TrainController_v3>();

            if (train != null)
            {
                switch (reverseDirectionMode)
                {
                    case ReverseDirectionMode.Always:
                        train.acceleration *= -1;
                        break;
                    case ReverseDirectionMode.OnlyIfMovingForward:
                        if (train.acceleration > 0f) train.acceleration *= -1;
                        break;
                    case ReverseDirectionMode.OnlyIfMovingBackwards:
                        if (train.acceleration < 0f) train.acceleration *= -1;
                        break;
                }
            }
        }
    }
}
