using UnityEngine;

namespace ZenGrid
{
    /// <summary>
    /// ScriptableObject that describes a ZenGrid game mode.
    /// Create assets via Assets > Create > ZenGrid > GameModeConfig.
    ///
    /// Add new Lotus-variant toggles here for future A/B testing
    /// (e.g. lotusCanSpread, maxSimultaneousLotus) without changing any C# game logic.
    /// </summary>
    [CreateAssetMenu(fileName = "GameModeConfig", menuName = "ZenGrid/GameModeConfig")]
    public class GameModeConfig : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Internal enum key for this mode.")]
        public GameModeType modeType = GameModeType.Classic;

        [Tooltip("Human-readable name shown in UI.")]
        public string displayName = "Classic";

        [Tooltip("Short flavour text shown on the main menu.")]
        [TextArea(2, 4)]
        public string description = "Place shapes, clear lines, and survive the Lotus.";

        // ── Future Lotus tuning knobs (ignored in PureZen) ──────────────────
        [Header("Lotus Settings (Classic only)")]
        [Tooltip("How many turns between Lotus seeds.")]
        public int turnsBetweenLotus = 5;

        [Tooltip("If false, existing Lotus cells never spread to neighbours.")]
        public bool lotusCanSpread = true;

        [Tooltip("Maximum simultaneous Lotus seeds on the board. 0 = unlimited.")]
        public int maxSimultaneousLotus = 0;
    }
}
