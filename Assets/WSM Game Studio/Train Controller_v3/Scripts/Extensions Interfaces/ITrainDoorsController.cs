using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WSMGameStudio.RailroadSystem;

public interface ITrainDoorsController
{
    StationDoorDirection StationDoorDirection { get; set; }
    bool CabinLeftDoorOpen { get; }
    bool CabinRightDoorOpen { get; }
    bool PassengerLeftDoorOpen { get; }
    bool PassengerRightDoorOpen { get; }

    void OpenCabinDoorLeft();
    void OpenCabinDoorRight();
    void CloseCabinDoorLeft();
    void CloseCabinDoorRight();
    void OpenPassengersDoors();
    void OpenPassengersDoors(StationDoorDirection doorsDiretion);
    void ClosePassengersDoors();
    void ClosePassengersLeftDoors();
    void ClosePassengersRightDoors();
    void UpdateWagonsDoorsControllers();
}
