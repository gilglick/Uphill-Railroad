namespace WSMGameStudio.RailroadSystem
{
    public static class Speed
    {
        /// <summary>
        /// Converts meters per second to kilometers per hour
        /// </summary>
        /// <param name="metersPerSecond"></param>
        /// <returns></returns>
        public static float Convert_MPS_To_KPH(float metersPerSecond)
        {
            return metersPerSecond * 3.6f;
        }

        /// <summary>
        /// Converts kilometers per hour to meters per second
        /// </summary>
        /// <param name="kilometersPerHour"></param>
        /// <returns></returns>
        public static float Convert_KPH_To_MPS(float kilometersPerHour)
        {
            return kilometersPerHour / 3.6f;
        }

        /// <summary>
        /// Converts meters per second to miles per hour
        /// </summary>
        /// <param name="metersPerSecond"></param>
        /// <returns></returns>
        public static float Convert_MPS_To_MPH(float metersPerSecond)
        {
            return metersPerSecond * 2.23694f;
        }

        /// <summary>
        /// Converts miles per hour to meters per second
        /// </summary>
        /// <param name="milesPerHour"></param>
        /// <returns></returns>
        public static float Convert_MPH_To_MPS(float milesPerHour)
        {
            return milesPerHour / 2.23694f;
        }
    }
}
