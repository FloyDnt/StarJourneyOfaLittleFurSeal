namespace StarJourney.Gameplay
{
    /// <summary>
    /// What the fur seal is doing with its jump.
    /// </summary>
    public enum JumpState
    {
        /// <summary>
        /// On the ground and turning; a press starts a charge.
        /// </summary>
        Idle,

        /// <summary>
        /// On the ground with the button held: not turning, the jump preview shown.
        /// </summary>
        Charging,

        /// <summary>
        /// In the air after a jump or a fall: no turning and no jumping until it lands.
        /// </summary>
        Airborne,

        /// <summary>
        /// Just landed and settling; it turns again and a press starts a charge.
        /// </summary>
        Landing,
    }
}
