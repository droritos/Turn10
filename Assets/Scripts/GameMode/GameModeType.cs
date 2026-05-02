namespace ZenGrid
{
    /// <summary>
    /// Defines the available game modes in ZenGrid.
    /// Add new entries here when introducing additional Lotus variants for A/B testing.
    /// </summary>
    public enum GameModeType
    {
        /// <summary>Full experience — Lotus seeds spawn and spread over time.</summary>
        Classic,

        /// <summary>No Lotus at all — clean, pressure-free grid gameplay.</summary>
        PureZen
    }
}
