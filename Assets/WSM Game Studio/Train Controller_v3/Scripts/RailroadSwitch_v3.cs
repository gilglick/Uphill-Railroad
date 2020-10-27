using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace WSMGameStudio.RailroadSystem
{
    public class RailroadSwitch_v3 : MonoBehaviour
    {
        private bool _activated = false;

        public List<GameObject> _railsColliders;
        public UnityEvent _onActivate;
        public UnityEvent _onDeactivate;

        public bool Activated
        {
            get { return _activated; }
        }

        /// <summary>
        /// Rail switching
        /// </summary>
        public void SwitchRails()
        {
            if (_railsColliders == null)
            {
                Debug.LogWarning("Rail colliders not set");
            }

            foreach (var collider in _railsColliders)
            {
                collider.SetActive(!collider.activeInHierarchy);
            }

            _activated = !_activated;

            if (_activated)
                _onActivate.Invoke();
            else
                _onDeactivate.Invoke();
        }
    } 
}
