using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WSMGameStudio.RailroadSystem
{
    [RequireComponent(typeof(TrainController_v3))]
    public class PassengerTags : MonoBehaviour
    {
        /// <summary>
        /// Set passengers to kinematic while moving
        /// </summary>
        public bool kinematicWhileMoving = false;

        /// <summary>
        /// Object tags for attaching passengers
        /// </summary>
        public List<string> passengerTags;

        private TrainController_v3 _trainController;

        // Use this for initialization
        void Start()
        {
            _trainController = GetComponent<TrainController_v3>();
            UpdateWagonsPassengerTags();
        }

        /// <summary>
        /// Updade wagons passenger tags
        /// </summary>
        private void UpdateWagonsPassengerTags()
        {
            SetPassengerTags(_trainController.gameObject);

            //If null wagon script is attached to wagon
            if (_trainController == null)
                return;

            if (_trainController.wagons == null)
                return;

            foreach (var wagon in _trainController.wagons)
            {
                SetPassengerTags(wagon.gameObject);
            }
        }

        private void SetPassengerTags(GameObject wagon)
        {
            TrainAttachPassenger trainAttachPassenger = wagon.GetComponentInChildren<TrainAttachPassenger>();

            if (trainAttachPassenger != null)
            {
                trainAttachPassenger.PassengerTags = passengerTags;
                trainAttachPassenger.KinematicWhileMoving = kinematicWhileMoving;
            }
        }
    } 
}
