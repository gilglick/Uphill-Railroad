using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WSMGameStudio.RailroadSystem
{
    public class StationStopTrigger : MonoBehaviour
    {
        public StopMode stopMode;
        public StationDoorDirection stationDoorDirection;
        public StationBehaviour stationBehaviour;
        public float stopTimeout = 10f;
        [Range(0, 100)]
        public int randomStopProbability = 50;
        public bool turnOffEngines = false;
        public TrainController_v3 trainController_V3;

        private bool _alreadyStopped = false;
        

        private void OnTriggerEnter(Collider other)
        {
            TrainStationController trainStationController = other.GetComponent<TrainStationController>();
            trainController_V3 = other.GetComponent<TrainController_v3>();
            if (trainStationController != null && trainController_V3.acceleration == 1)
            {
                switch (stopMode)
                {
                    case StopMode.Always:
                        trainStationController.StopAtStation(stationBehaviour, stopTimeout, turnOffEngines);
                        break;
                    case StopMode.Once:
                        if (!_alreadyStopped)
                        {
                            trainStationController.StopAtStation(stationBehaviour, stopTimeout, turnOffEngines);
                            _alreadyStopped = true;
                        }
                        break;
                    case StopMode.Random:
                        if (Probability.RandomEvent(randomStopProbability))
                            trainStationController.StopAtStation(stationBehaviour, stopTimeout, turnOffEngines);
                        break;
                }

                ITrainDoorsController trainDoorsController = other.GetComponent<ITrainDoorsController>();

                if (trainDoorsController != null)
                    trainDoorsController.StationDoorDirection = stationDoorDirection;
            }
        }
    }
}
