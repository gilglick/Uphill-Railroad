using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WSMGameStudio.RailroadSystem
{
    [RequireComponent(typeof(TrainController_v3))]
    public class TrainSpeedMonitor : MonoBehaviour
    {
        private TrainController_v3 _trainController;

        public SpeedUnits speedUnit;
        public Text outputText;
        public float kph;
        public float mph;

        // Use this for initialization
        void Start()
        {
            _trainController = GetComponent<TrainController_v3>();
        }

        // Update is called once per frame
        void Update()
        {
            kph = _trainController.Speed_KPH;
            mph = _trainController.Speed_MPH;

            if (outputText != null)
            {
                switch (speedUnit)
                {
                    case SpeedUnits.kph:
                        outputText.text = string.Format("{0} KPH", kph.ToString("0"));
                        break;
                    case SpeedUnits.mph:
                        outputText.text = string.Format("{0} MPH", mph.ToString("0"));
                        break;
                }
            }
        }
    }
}
