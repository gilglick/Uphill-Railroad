using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WSMGameStudio.RailroadSystem
{
    public static class TrainPhysics
    {
        /// <summary>
        /// Control speed base on max speed, acceleration and brakes
        /// </summary>
        /// <param name="rigidbody"></param>
        /// <param name="isGrounded"></param>
        /// <param name="maxSpeedKph"></param>
        /// <param name="speed_KPH"></param>
        /// <param name="acceleration"></param>
        /// <param name="brake"></param>
        /// <param name="targetVelocityIn"></param>
        /// <param name="targetVelocityOut"></param>
        /// <param name="currentSpeedIn"></param>
        /// <param name="currentSpeedOut"></param>
        /// <param name="targetSpeedIn"></param>
        /// <param name="targetSpeedOut"></param>
        public static void SpeedControl(Rigidbody rigidbody, bool isGrounded, float maxSpeedKph, float speed_KPH, float acceleration, float brake, Vector3 targetVelocityIn, out Vector3 targetVelocityOut, float currentSpeedIn, out float currentSpeedOut, float targetSpeedIn, out float targetSpeedOut)
        {
            currentSpeedOut = currentSpeedIn;
            targetSpeedOut = targetSpeedIn;
            targetVelocityOut = targetVelocityIn;

            if (isGrounded)
            {
                targetSpeedOut = TrainPhysics.GetTargetSpeed(acceleration, maxSpeedKph);
                targetSpeedOut = TrainPhysics.ApplyBrakes(rigidbody, brake, targetSpeedOut);
                currentSpeedOut = TrainPhysics.SoftAcceleration(currentSpeedOut, targetSpeedOut);

                //Apply velocity
                if (speed_KPH < maxSpeedKph)
                {
                    targetVelocityOut = currentSpeedOut == 0f ? Vector3.zero : rigidbody.velocity + (rigidbody.transform.forward * currentSpeedOut);
                    rigidbody.velocity = Vector3.MoveTowards(rigidbody.velocity, targetVelocityOut, Time.deltaTime * GeneralSettings.AccelerationRate);
                }
            }
        }

        /// <summary>
        /// Calculates target speed
        /// </summary>
        /// <param name="acceleration">Range between -1 and 1</param>
        /// <param name="maxSpeed"></param>
        /// <returns></returns>
        private static float GetTargetSpeed(float acceleration, float maxSpeed)
        {
            float targetSpeed;

            if (acceleration > 0.0f) //Moving forwards
                targetSpeed = Mathf.Lerp(0.0f, maxSpeed, acceleration);
            else if (acceleration < 0.0f) //Moving backwards
                targetSpeed = Mathf.Lerp(0.0f, maxSpeed * (-1), acceleration * (-1));
            else //if (_acceleration == 0.0f) //Stopping
                targetSpeed = 0.0f;

            return targetSpeed;
        }

        /// <summary>
        /// Applies brakes
        /// </summary>
        /// <param name="rigidbody"></param>
        /// <param name="brake"></param>
        /// <param name="brakeDrag"></param>
        /// <param name="idleDrag"></param>
        /// <param name="targetSpeed"></param>
        /// <returns></returns>
        public static float ApplyBrakes(Rigidbody rigidbody, float brake, float targetSpeed)
        {
            if (brake > 0.0f)
            {
                targetSpeed = 0.0f;
                rigidbody.velocity = Vector3.MoveTowards(rigidbody.velocity, Vector3.zero, Time.deltaTime * GeneralSettings.AccelerationRate);
            }
            else
            {
                rigidbody.angularDrag = GeneralSettings.IdleDrag;
            }

            return targetSpeed;
        }

        /// <summary>
        /// Slowly goes from current speed to target speed
        /// </summary>
        /// <param name="accelerationRate"></param>
        /// <param name="currentSpeed"></param>
        /// <param name="targetSpeed"></param>
        /// <returns></returns>
        public static float SoftAcceleration(float currentSpeed, float targetSpeed)
        {
            if (Mathf.Abs(currentSpeed) < Mathf.Abs(targetSpeed))
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, GeneralSettings.AccelerationRate * Time.deltaTime);
            else if (Mathf.Abs(currentSpeed) >= Mathf.Abs(targetSpeed))
                currentSpeed = targetSpeed;

            return currentSpeed;
        }

        /// <summary>
        /// Update wheels properties
        /// </summary>
        /// <param name="wheelsScripts"></param>
        /// <param name="accelerationMode"></param>
        /// <param name="isGrounded"></param>
        /// <param name="acceleration"></param>
        /// <param name="brake"></param>
        /// <param name="maxTorque"></param>
        public static void UpdateWheels(List<TrainWheel_v3> wheelsScripts, float brake, float speed)
        {
            foreach (var wheel in wheelsScripts)
            {
                wheel.Brake = brake;
                wheel.Speed = speed;
            }
        }

        /// <summary>
        /// Connect new car joint to front car
        /// </summary>
        /// <param name="newTrainCar"></param>
        /// <param name="previousCarCoupler"></param>
        public static void ConnectTrainCar(HingeJoint newTrainCar, Rigidbody frontCarCoupler)
        {
            newTrainCar.connectedBody = frontCarCoupler;
        }

        /// <summary>
        /// Disconnect train car
        /// </summary>
        /// <param name="trainCar"></param>
        public static void DisconnectTrainCar(HingeJoint trainCar)
        {
            trainCar.connectedBody = null;
        }
    }
}
