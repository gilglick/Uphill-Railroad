using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WSMGameStudio.RailroadSystem
{
    public class SwitchTrigger : MonoBehaviour
    {
        public SwitchMode switchMode;
        [Range(0, 100)]
        public int randomSwitchProbability = 50;
        public List<RailroadSwitch_v3> railroadSwitches;

        private bool _alreadySwitched = false;

        private void OnTriggerEnter(Collider other)
        {
            TrainController_v3 train = other.GetComponent<TrainController_v3>();

            if (train != null)
            {
                if (railroadSwitches == null || railroadSwitches.Count == 0)
                {
                    Debug.LogWarning("Railroad Switch not set on Switch Trigger");
                    return;
                }

                switch (switchMode)
                {
                    case SwitchMode.Always:
                        SwitchRails();
                        break;
                    case SwitchMode.Once:
                        if (!_alreadySwitched)
                        {
                            SwitchRails();
                            _alreadySwitched = true;
                        }
                        break;
                    case SwitchMode.Random:
                        if (Probability.RandomEvent(randomSwitchProbability))
                            SwitchRails();
                        break;
                    case SwitchMode.IfActivated:
                        SwitchActivatedRails();
                        break;
                    case SwitchMode.IfDeactivated:
                        SwitchDeactivatedRails();
                        break;
                }
            }
        }

        private void SwitchRails()
        {
            foreach (var railSwitch in railroadSwitches)
            {
                railSwitch.SwitchRails();
            }
        }

        private void SwitchActivatedRails()
        {
            foreach (var railSwitch in railroadSwitches)
            {
                if (railSwitch.Activated)
                    railSwitch.SwitchRails();
            }
        }

        private void SwitchDeactivatedRails()
        {
            foreach (var railSwitch in railroadSwitches)
            {
                if (!railSwitch.Activated)
                    railSwitch.SwitchRails();
            }
        }
    }
}
