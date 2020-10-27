using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WSMGameStudio.RailroadSystem
{
    public class DirectionIndicator : MonoBehaviour
    {
        public Animator animator;

        public void TurnRight()
        {
            if (animator != null)
            {
                animator.SetBool("TurnRight", true);
                animator.SetBool("TurnLeft", false);
            }
        }

        public void TurnLeft()
        {
            if (animator != null)
            {
                animator.SetBool("TurnRight", false);
                animator.SetBool("TurnLeft", true);
            }
        }

        public void ReturnToDefaultPosition()
        {
            if (animator != null)
            {
                animator.SetBool("TurnRight", false);
                animator.SetBool("TurnLeft", false);
            }
        }
    } 
}
