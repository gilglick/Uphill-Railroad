using UnityEngine;

namespace WSMGameStudio.RailroadSystem
{
    public class TrainCarCoupler : MonoBehaviour
    {
        public CarCouplerType couplerType;

        //private HingeJoint _carJoint;
        private TrainController_v3 _locomotive;
        private Wagon_v3 _wagon;

        private bool _isLocomotive = false;
        private bool _isWagon = false;

        //public HingeJoint CarJoint { get { return _carJoint; } }
        public TrainController_v3 Locomotive { get { return _locomotive; } }
        public Wagon_v3 Wagon { get { return _wagon; } }

        public bool IsBackJoint { get { return (couplerType == CarCouplerType.BackCoupler); } }
        public bool IsLocomotive { get { return _isLocomotive; } }
        public bool IsWagon { get { return _isWagon; } }

        private void OnEnable()
        {
            // Parent must be always wagon or locomotive
            _locomotive = this.transform.parent.GetComponent<TrainController_v3>();
            _wagon = this.transform.parent.GetComponent<Wagon_v3>();

            _isLocomotive = (_locomotive != null);
            _isWagon = (_wagon != null);
        }

        private void OnTriggerStay(Collider other)
        {
            ConnectOnCollision(other, false);
        }

        private void OnTriggerEnter(Collider other)
        {
            ConnectOnCollision(other, true);
        }

        /// <summary>
        /// Connect wagons on couplers collision
        /// </summary>
        /// <param name="other"></param>
        private void ConnectOnCollision(Collider other, bool playSFX)
        {
            // Only wagons connect
            if (!_isWagon) return;
            // Ignore already connected wagons 
            if (_wagon.IsConected) return;

            TrainCarCoupler otherCarCoupler = other.GetComponent<TrainCarCoupler>();

            if (otherCarCoupler != null)
            {
                _wagon.Connect(this, otherCarCoupler, playSFX);
            }
        }
    }
}
