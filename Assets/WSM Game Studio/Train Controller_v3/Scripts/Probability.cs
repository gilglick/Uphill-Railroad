using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WSMGameStudio.RailroadSystem
{
    public static class Probability
    {
        /// <summary>
        /// Random event lottery
        /// </summary>
        /// <param name="randomSwitchProbability"></param>
        /// <returns></returns>
        public static bool RandomEvent(int randomProbability)
        {
            return Random.Range(1, 101) <= randomProbability;
        }
    }
}
