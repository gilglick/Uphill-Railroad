using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WSMGameStudio.RailroadSystem
{
    public class TrainDoorsController : MonoBehaviour, ITrainDoorsController
    {
        private bool _cabinLeftDoorOpen = false;
        private bool _cabinRightDoorOpen = false;
        private bool _passengerLeftDoorOpen = false;
        private bool _passengerRightDoorOpen = false;

        private StationDoorDirection _stationDoorDirection;
        private TrainController_v3 _trainController;
        private List<TrainDoorsController> _wagonsDoorsControllers;

        public AudioSource openDoorSFX;
        public AudioSource closeDoorSFX;

        public TrainDoor cabinDoorLeft;
        public TrainDoor cabinDoorRight;
        public List<TrainDoor> passengerDoorsLeft;
        public List<TrainDoor> passengerDoorsRight;

        public StationDoorDirection StationDoorDirection
        {
            get { return _stationDoorDirection; }
            set
            {
                _stationDoorDirection = value;
                UpdateWagonDoorsDirection();
            }
        }

        public bool CabinLeftDoorOpen { get { return _cabinLeftDoorOpen; } }
        public bool CabinRightDoorOpen { get { return _cabinRightDoorOpen; } }
        public bool PassengerLeftDoorOpen { get { return _passengerLeftDoorOpen; } }
        public bool PassengerRightDoorOpen { get { return _passengerRightDoorOpen; } }

        private void Start()
        {
            _trainController = GetComponent<TrainController_v3>();
            UpdateWagonsDoorsControllers();
        }

        #region Public Methods

        /// <summary>
        /// Open left cabin door
        /// </summary>
        public void OpenCabinDoorLeft()
        {
            if (!_cabinLeftDoorOpen)
                _cabinLeftDoorOpen = OpenDoor(cabinDoorLeft, true);
        }

        /// <summary>
        /// Open right cabin door
        /// </summary>
        public void OpenCabinDoorRight()
        {
            if (!_cabinRightDoorOpen)
                _cabinRightDoorOpen = OpenDoor(cabinDoorRight, true);
        }

        /// <summary>
        /// Close left cabin door
        /// </summary>
        public void CloseCabinDoorLeft()
        {
            if (_cabinLeftDoorOpen)
                _cabinLeftDoorOpen = !CloseDoor(cabinDoorLeft, true);
        }

        /// <summary>
        /// Close right cabin door
        /// </summary>
        public void CloseCabinDoorRight()
        {
            if (_cabinRightDoorOpen)
                _cabinRightDoorOpen = !CloseDoor(cabinDoorRight, true);
        }

        public void OpenPassengersDoors(StationDoorDirection doorsDiretion)
        {
            _stationDoorDirection = doorsDiretion;
            OpenPassengersDoors();
        }

        /// <summary>
        /// Open passengers doors taking in consideration the station door direction
        /// </summary>
        public void OpenPassengersDoors()
        {
            switch (_stationDoorDirection)
            {
                case StationDoorDirection.BothSides:
                    if (!_passengerLeftDoorOpen) _passengerLeftDoorOpen = OpenDoor(passengerDoorsLeft);
                    if (!_passengerRightDoorOpen) _passengerRightDoorOpen = OpenDoor(passengerDoorsRight);
                    break;
                case StationDoorDirection.Left:
                    if (!_passengerLeftDoorOpen) _passengerLeftDoorOpen = OpenDoor(passengerDoorsLeft);
                    break;
                case StationDoorDirection.Right:
                    if (!_passengerRightDoorOpen) _passengerRightDoorOpen = OpenDoor(passengerDoorsRight);
                    break;
            }

            if (_wagonsDoorsControllers == null)
                return;

            foreach (var item in _wagonsDoorsControllers)
            {
                item.OpenPassengersDoors();
                _passengerLeftDoorOpen = item.PassengerLeftDoorOpen;
                _passengerRightDoorOpen = item.PassengerRightDoorOpen;
            }
        }

        /// <summary>
        /// Close passengers doors on BOTH sides
        /// </summary>
        public void ClosePassengersDoors()
        {
            if(_passengerLeftDoorOpen) _passengerLeftDoorOpen = !CloseDoor(passengerDoorsLeft);
            if(_passengerRightDoorOpen) _passengerRightDoorOpen = !CloseDoor(passengerDoorsRight);

            if (_wagonsDoorsControllers == null)
                return;

            foreach (var item in _wagonsDoorsControllers)
            {
                item.ClosePassengersDoors();
                _passengerLeftDoorOpen = item.PassengerLeftDoorOpen;
                _passengerRightDoorOpen = item.PassengerRightDoorOpen;
            }
        }

        /// <summary>
        /// Close left passengers doors
        /// </summary>
        public void ClosePassengersLeftDoors()
        {
            if (_passengerLeftDoorOpen) _passengerLeftDoorOpen = !CloseDoor(passengerDoorsLeft);

            if (_wagonsDoorsControllers == null)
                return;

            foreach (var item in _wagonsDoorsControllers)
            {
                item.ClosePassengersLeftDoors();
                _passengerLeftDoorOpen = item.PassengerLeftDoorOpen;
            }
        }

        /// <summary>
        /// Close right passengers doors
        /// </summary>
        public void ClosePassengersRightDoors()
        {
            if (_passengerRightDoorOpen) _passengerRightDoorOpen = !CloseDoor(passengerDoorsRight);

            if (_wagonsDoorsControllers == null)
                return;

            foreach (var item in _wagonsDoorsControllers)
            {
                item.ClosePassengersRightDoors();
                _passengerRightDoorOpen = item.PassengerRightDoorOpen;
            }
        }

        /// <summary>
        /// Update wagons doors controllers list, if the script is attached to a locomotive
        /// </summary>
        public void UpdateWagonsDoorsControllers()
        {
            //If null wagon script is attached to wagon
            if (_trainController == null)
                return;

            _wagonsDoorsControllers = new List<TrainDoorsController>();

            if (_trainController.wagons == null)
                return;

            foreach (var wagon in _trainController.wagons)
            {
                TrainDoorsController doorController = wagon.GetComponent<TrainDoorsController>();

                if (doorController != null)
                    _wagonsDoorsControllers.Add(doorController);
            }
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Updates wagons station doors direction
        /// </summary>
        private void UpdateWagonDoorsDirection()
        {
            if (_wagonsDoorsControllers == null)
                return;

            foreach (var item in _wagonsDoorsControllers)
            {
                item.StationDoorDirection = _stationDoorDirection;
            }
        }

        /// <summary>
        /// Open door
        /// </summary>
        /// <param name="door"></param>
        /// <param name="playSFX"></param>
        /// <returns>True if opened successfully</returns>
        private bool OpenDoor(TrainDoor door, bool playSFX)
        {
            if (door == null)
                return false;

            bool opened = door.Open();

            if (opened && playSFX)
                PlayDoorSFX(openDoorSFX);

            return opened;
        }

        /// <summary>
        /// Open doors
        /// </summary>
        /// <param name="doors"></param>
        private bool OpenDoor(List<TrainDoor> doors)
        {
            if (doors == null)
                return false;

            bool opened = false;
            foreach (var door in doors)
                opened = OpenDoor(door, false);

            if (opened)
                PlayDoorSFX(openDoorSFX);

            return opened;
        }

        /// <summary>
        /// Close door
        /// </summary>
        /// <param name="door"></param>
        /// <param name="playSFX"></param>
        /// <returns>True if closed successfully</returns>
        private bool CloseDoor(TrainDoor door, bool playSFX)
        {
            if (door == null)
                return false;

            bool closed = door.Close();

            if (closed && playSFX)
                PlayDoorSFX(closeDoorSFX);

            return closed;
        }

        /// <summary>
        /// Close doors
        /// </summary>
        /// <param name="doors"></param>
        private bool CloseDoor(List<TrainDoor> doors)
        {
            if (doors == null)
                return false;

            bool closed = false;
            foreach (var door in doors)
                closed = CloseDoor(door, false);

            if (closed)
                PlayDoorSFX(closeDoorSFX);

            return closed;
        }

        /// <summary>
        /// Play door SFX
        /// </summary>
        private void PlayDoorSFX(AudioSource sfx)
        {
            if (sfx != null)
                sfx.Play();
        }

        #endregion
    }
}
