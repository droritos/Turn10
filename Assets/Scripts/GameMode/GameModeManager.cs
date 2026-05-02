using UnityEngine;

namespace ZenGrid
{
    /// <summary>
    /// Singleton that holds the currently selected GameModeConfig.
    /// Call <see cref="SetMode"/> from the main menu before <see cref="ZenGridManager.StartGame"/>.
    /// All other systems query <see cref="IsLotusEnabled"/> (and other knobs) from here.
    /// </summary>
    public class GameModeManager : MonoBehaviour
    {
        public static GameModeManager Instance { get; private set; }

        [SerializeField]
        [Tooltip("Fallback config used if the player never explicitly picks a mode.")]
        private GameModeConfig _defaultConfig;

        private GameModeConfig _activeConfig;

        // ── Public API ───────────────────────────────────────────────────────

        /// <summary>The active mode enum value.</summary>
        public GameModeType ActiveModeType => _activeConfig?.modeType ?? GameModeType.Classic;

        /// <summary>Human-readable name of the active mode.</summary>
        public string ModeName => _activeConfig?.displayName ?? "Classic";

        /// <summary>True when Lotus seeds should spawn and spread.</summary>
        public bool IsLotusEnabled => ActiveModeType == GameModeType.Classic;

        /// <summary>Whether Lotus cells are allowed to spread (Classic only tuning knob).</summary>
        public bool LotusCanSpread => _activeConfig?.lotusCanSpread ?? true;

        /// <summary>Turns between Lotus seed spawns (Classic only tuning knob).</summary>
        public int TurnsBetweenLotus => _activeConfig?.turnsBetweenLotus ?? 5;

        /// <summary>Max simultaneous Lotus seeds (0 = unlimited).</summary>
        public int MaxSimultaneousLotus => _activeConfig?.maxSimultaneousLotus ?? 0;

        // ── Lifecycle ────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                _activeConfig = _defaultConfig;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // ── Public Methods ───────────────────────────────────────────────────

        /// <summary>
        /// Call this from the main menu (before StartGame) to set the desired mode.
        /// </summary>
        public void SetMode(GameModeConfig config)
        {
            if (config == null)
            {
                Debug.LogWarning("[GameModeManager] SetMode called with null config — keeping current mode.");
                return;
            }

            _activeConfig = config;
            Debug.Log($"[GameModeManager] Mode set to: {config.displayName} ({config.modeType})");
        }
    }
}
