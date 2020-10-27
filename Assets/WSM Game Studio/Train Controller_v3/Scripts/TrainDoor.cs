using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WSMGameStudio.RailroadSystem
{
    public class TrainDoor : MonoBehaviour
    {
        private bool _isOpen = false;

        public List<Animator> animators;

        public bool IsOpen
        {
            get { return _isOpen; }
            set { _isOpen = value; }
        }

        /// <summary>
        /// Play open door animation
        /// </summary>
        /// <returns>True if opened successfully, false if already open</returns>
        public bool Open()
        {
            if (!_isOpen)
            {
                _isOpen = true;
                UpdateDoorAnimator(_isOpen);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Play close door animation
        /// </summary>
        /// <returns>True if closed successfully, false if already closed</returns>
        public bool Close()
        {
            if (_isOpen)
            {
                _isOpen = false;
                UpdateDoorAnimator(_isOpen);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Set animator parameters
        /// </summary>
        /// <param name="open"></param>
        private void UpdateDoorAnimator(bool open)
        {
            if (animators == null)
                return;

            foreach (var animator in animators)
            {
                animator.SetBool(AnimationParameters.Open, open);
            }
        }
    } 
}
