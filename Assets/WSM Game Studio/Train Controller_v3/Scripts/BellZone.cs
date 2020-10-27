using UnityEngine;

namespace WSMGameStudio.RailroadSystem
{
    public class BellZone : MonoBehaviour
    {
        public ZoneTriggerType triggerType;

        private void OnTriggerEnter(Collider other)
        {
            TrainController_v3 train = other.GetComponent<TrainController_v3>();

            if (train != null)
            {
                if ((triggerType == ZoneTriggerType.Activate && !train.BellOn) || (triggerType == ZoneTriggerType.Deactivate && train.BellOn))
                    train.ToogleBell();
            }
        }
    }
}
