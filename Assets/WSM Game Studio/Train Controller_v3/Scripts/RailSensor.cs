using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WSMGameStudio.RailroadSystem
{
    public class RailSensor : MonoBehaviour
    {
        public bool onRails = false;
        public bool grounded = false;

        private void Update()
        {
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, 0.5f))
            {
                grounded = true;

                if (hit.collider.gameObject.tag == "Rails")
                {
                    onRails = true;
                    Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.yellow);
                }
                else
                    onRails = false;
            }
            else
            {
                onRails = false;
                grounded = false;
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * 0.2f, Color.white);
            }
        }
    } 
}
