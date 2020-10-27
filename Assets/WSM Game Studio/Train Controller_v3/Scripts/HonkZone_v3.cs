using UnityEngine;

namespace WSMGameStudio.RailroadSystem
{
    public class HonkZone_v3 : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            TrainController_v3 train = other.GetComponent<TrainController_v3>();

            if (train != null && train.acceleration == 1)
                train.Honk();
        }
    } 
}
