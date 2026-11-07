namespace StarJourney.Gameplay
{
    /// <summary>
    /// What a <see cref="Tide"/> is doing.
    /// </summary>
    public enum TidePhase
    {
        /// <summary>
        /// Waiting at the low height for the first wave.
        /// </summary>
        Waiting,

        /// <summary>
        /// Rising toward the crest.
        /// </summary>
        Rising,

        /// <summary>
        /// Falling back from the crest.
        /// </summary>
        Falling,

        /// <summary>
        /// Resting at the low height between two waves.
        /// </summary>
        Resting,

        /// <summary>
        /// Done with all of its waves.
        /// </summary>
        Done,
    }
}
