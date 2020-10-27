using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace WSMGameStudio.RailroadSystem
{
    public class TrainAttachPassenger : MonoBehaviour
    {
        private Dictionary<int, UnityAction> _onStopRuntimeEvents = new Dictionary<int, UnityAction>();
        private Dictionary<int, UnityAction> _onLeaveRuntimeEvents = new Dictionary<int, UnityAction>();

        private List<string> _passengerTags;
        private bool _kinematicWhileMoving;

        public List<string> PassengerTags
        {
            get { return _passengerTags; }
            set { _passengerTags = value; }
        }

        public bool KinematicWhileMoving
        {
            get { return _kinematicWhileMoving; }
            set { _kinematicWhileMoving = value; }
        }

        /// <summary>
        /// Handles objects inside the train
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            SetPassengerParent(other.gameObject, transform.parent);
        }

        /// <summary>
        /// Handles objects inside the train
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerStay(Collider other)
        {
            SetPassengerParent(other.gameObject, transform.parent);
        }

        /// <summary>
        /// Handles object exit from train
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerExit(Collider other)
        {
            RemovePassengerParent(other.gameObject);
        }

        /// <summary>
        /// Set passenger parent transform
        /// </summary>
        /// <param name="other"></param>
        /// <param name="parent"></param>
        private void SetPassengerParent(GameObject other, Transform parent)
        {
            if (IsPassenger(other.tag))
            {
                other.transform.parent = parent;

                if (_kinematicWhileMoving)
                {
                    TrainStationController trainStationController = parent.GetComponent<TrainStationController>();

                    if (trainStationController != null)
                    {
                        int objectId = other.gameObject.GetInstanceID();
                        UnityAction stopAction;
                        UnityAction leaveAction;

                        if (!_onStopRuntimeEvents.TryGetValue(objectId, out stopAction))
                        {
                            stopAction = new UnityAction((delegate { GameObject localGameObject = other; SetPassengerKinematic(localGameObject, false); }));

                            _onStopRuntimeEvents.Add(objectId, stopAction);
                            trainStationController.onStop.AddListener(_onStopRuntimeEvents[objectId]);
                        }

                        if (!_onLeaveRuntimeEvents.TryGetValue(objectId, out leaveAction))
                        {
                            leaveAction = new UnityAction((delegate { GameObject localGameObject = other; SetPassengerKinematic(localGameObject, true); }));

                            _onLeaveRuntimeEvents.Add(objectId, leaveAction);
                            trainStationController.onLeave.AddListener(_onLeaveRuntimeEvents[objectId]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Remove passenger
        /// </summary>
        /// <param name="other"></param>
        private void RemovePassengerParent(GameObject other)
        {
            if (IsPassenger(other.tag))
            {
                if (_kinematicWhileMoving)
                {
                    TrainStationController trainStationController = other.transform.parent.GetComponent<TrainStationController>();

                    if (trainStationController != null)
                    {
                        int objectId = other.gameObject.GetInstanceID();
                        UnityAction stopAction;
                        UnityAction leaveAction;

                        if (_onStopRuntimeEvents.TryGetValue(objectId, out stopAction))
                        {
                            trainStationController.onStop.RemoveListener(_onStopRuntimeEvents[objectId]);
                            _onStopRuntimeEvents.Remove(objectId);
                        }

                        if (_onLeaveRuntimeEvents.TryGetValue(objectId, out leaveAction))
                        {
                            trainStationController.onLeave.RemoveListener(_onLeaveRuntimeEvents[objectId]);
                            _onLeaveRuntimeEvents.Remove(objectId);
                        }
                    }
                }

                other.transform.parent = null;
            }
        }

        /// <summary>
        /// Set kinematic if game object as a ribidbody attached to it
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="isKinematic"></param>
        public void SetPassengerKinematic(GameObject gameObject, bool isKinematic)
        {
            if (isKinematic && gameObject.transform.parent == null)
                return;

            Rigidbody rb = gameObject.GetComponent<Rigidbody>();

            if (rb != null)
                rb.isKinematic = isKinematic;
        }

        /// <summary>
        /// Check if object is configured as an valid passenger
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        private bool IsPassenger(string tag)
        {
            if (_passengerTags == null)
                return false;

            return _passengerTags.Contains(tag);
        }
    }
}