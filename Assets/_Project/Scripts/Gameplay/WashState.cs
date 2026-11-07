namespace StarJourney.Gameplay
{
    /// <summary>
    /// What the water is doing with a <see cref="WashedProp"/>.
    /// </summary>
    public enum WashState
    {
        /// <summary>
        /// Not washed: a prop placed by hand, or waiting in its pool.
        /// </summary>
        Idle,

        /// <summary>
        /// Carried up the beach at the water's edge.
        /// </summary>
        Riding,

        /// <summary>
        /// Thrown in an arc, onto the beach at the crest or into the water when swept out.
        /// </summary>
        Tossing,

        /// <summary>
        /// Lying still on the beach.
        /// </summary>
        Resting,

        /// <summary>
        /// Dragged toward the sea by the receding water, then drifting out if it goes all the way.
        /// </summary>
        DraggingBack,

        /// <summary>
        /// Left from the previous wave; waiting for the rising water to cover it.
        /// </summary>
        SweepingOut,
    }
}
