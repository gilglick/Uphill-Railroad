namespace WSMGameStudio.RailroadSystem
{
    public static class TrainAudio
    {
        /// <summary>
        /// Trains SFX, motor, wheels on trails, etc
        /// </summary>
        public static void PlaySFX(SFX sfx, float speedKMH, float brake, bool enginesOn, bool isGrounded)
        {
            if (sfx.engineSFX != null)
            {
                if (enginesOn && !sfx.engineSFX.isPlaying)
                    sfx.engineSFX.Play();
                else if (!enginesOn && sfx.engineSFX.isPlaying)
                    sfx.engineSFX.Stop();
            }

            if (isGrounded)
            {
                if (sfx.wheelsSFX != null)
                {
                    if (speedKMH >= 1f && !sfx.wheelsSFX.isPlaying)
                        sfx.wheelsSFX.Play();
                    else if (speedKMH <= 1f && sfx.wheelsSFX.isPlaying)
                        sfx.wheelsSFX.Stop();
                }

                if (sfx.brakesSFX != null)
                {
                    if (speedKMH >= 0.5f && brake > 0.5f && !sfx.brakesSFX.isPlaying)
                        sfx.brakesSFX.Play();
                    else if (sfx.brakesSFX.isPlaying && speedKMH <= 0.5f || brake < 0.5f)
                        sfx.brakesSFX.Stop();
                }
            }
            else
            {
                if (sfx.wheelsSFX != null) sfx.wheelsSFX.Stop();
                if (sfx.brakesSFX != null) sfx.brakesSFX.Stop();
            }
        }
    } 
}
