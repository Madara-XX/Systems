using UnityEngine;

namespace RoombaRampage.Progression
{
    /// <summary>
    /// ScriptableObject containing all configuration for the XP and Leveling System.
    /// Create instance via: Right-click > Create > RoombaRampage > XP Settings
    ///
    /// XP Curve Formula: XP Required = BaseXP * (Level ^ Exponent)
    /// Example: Level 5 = 100 * (5 ^ 1.5) = 1118 XP
    ///
    /// Recommended Settings for 10-minute runs:
    /// - Base XP: 100
    /// - Exponent: 1.5
    /// - Max Level: 30
    /// - Target: 10-15 level-ups per run
    /// </summary>
    [CreateAssetMenu(fileName = "XPSettings", menuName = "RoombaRampage/XP Settings", order = 10)]
    public class XPSettings : ScriptableObject
    {
        #region XP Curve Configuration

        [Header("XP Curve")]
        [Tooltip("Base XP required for level 1→2. Formula: XP = BaseXP * (Level ^ Exponent)")]
        [Min(1f)]
        public float baseXP = 100f;

        [Tooltip("Exponential growth rate. Higher = steeper curve. Recommended: 1.3 - 1.7")]
        [Range(1f, 2.5f)]
        public float exponent = 1.5f;

        [Tooltip("Maximum level cap. Player cannot exceed this level.")]
        [Min(1)]
        public int maxLevel = 30;

        #endregion

        #region XP Gem Visual Settings

        [Header("XP Gem Configuration")]
        [Tooltip("Distance at which XP gems start flying toward player")]
        [Min(0.5f)]
        public float magnetRange = 6f;

        [Tooltip("Speed multiplier for XP gem movement toward player")]
        [Min(1f)]
        public float magnetSpeed = 8f;

        [Tooltip("Acceleration curve - faster as gem gets closer to player")]
        public AnimationCurve magnetAccelerationCurve = AnimationCurve.EaseInOut(0f, 0.3f, 1f, 1f);

        [Tooltip("XP gem lifetime before auto-collection (seconds, 0 = infinite)")]
        [Min(0f)]
        public float gemLifetime = 30f;

        [Tooltip("Height offset for spawning XP gems above ground")]
        [Min(0f)]
        public float gemSpawnHeight = 0.5f;

        [Tooltip("Random spread radius when spawning XP gems")]
        [Min(0f)]
        public float gemSpawnSpread = 0.5f;

        #endregion

        #region XP Gem Pooling

        [Header("Object Pooling")]
        [Tooltip("Initial pool size for XP gems")]
        [Min(10)]
        public int initialPoolSize = 100;

        [Tooltip("Auto-expand pool if depleted")]
        public bool autoExpandPool = true;

        [Tooltip("Pool expansion amount when depleted")]
        [Min(10)]
        public int poolExpandAmount = 50;

        #endregion

        #region Level-Up Effects

        [Header("Level-Up VFX/SFX")]
        [Tooltip("Particle effect prefab for level-up (spawned at player position)")]
        public GameObject levelUpVFXPrefab;

        [Tooltip("Sound effect for level-up")]
        public AudioClip levelUpSound;

        [Tooltip("Sound effect for XP gem collection")]
        public AudioClip xpCollectSound;

        [Tooltip("Duration of level-up effects (seconds)")]
        [Min(0.1f)]
        public float levelUpEffectDuration = 1f;

        [Tooltip("Enable time slow on level-up")]
        public bool enableTimeSlowOnLevelUp = true;

        [Tooltip("Time scale during level-up slow (0.5 = half speed)")]
        [Range(0.1f, 1f)]
        public float levelUpTimeScale = 0.5f;

        [Tooltip("Duration of time slow effect (seconds)")]
        [Min(0.1f)]
        public float levelUpTimeSlowDuration = 0.5f;

        [Tooltip("Restore player health to full on level-up")]
        public bool healOnLevelUp = true;

        #endregion

        #region Game Pause Settings

        [Header("Skill Selection Pause")]
        [Tooltip("Pause game when level-up occurs (for skill selection)")]
        public bool pauseOnLevelUp = true;

        [Tooltip("Delay before pausing (allows effects to play first)")]
        [Min(0f)]
        public float pauseDelay = 0.3f;

        #endregion

        #region Validation & Helpers

        /// <summary>
        /// Calculates XP required to reach a specific level.
        /// Uses formula: XP = BaseXP * (Level ^ Exponent)
        /// </summary>
        /// <param name="level">Target level (1-based)</param>
        /// <returns>XP required for that level</returns>
        public int CalculateXPForLevel(int level)
        {
            if (level <= 1) return 0;

            // Formula: XP = BaseXP * (Level ^ Exponent)
            float xpRequired = baseXP * Mathf.Pow(level, exponent);
            return Mathf.RoundToInt(xpRequired);
        }

        /// <summary>
        /// Calculates total XP needed from level 1 to reach target level.
        /// </summary>
        /// <param name="targetLevel">Target level</param>
        /// <returns>Cumulative XP required</returns>
        public int CalculateTotalXPForLevel(int targetLevel)
        {
            int totalXP = 0;
            for (int i = 2; i <= targetLevel; i++)
            {
                totalXP += CalculateXPForLevel(i);
            }
            return totalXP;
        }

        /// <summary>
        /// Estimates how many level-ups player can achieve with given XP.
        /// </summary>
        /// <param name="totalXP">Total XP available</param>
        /// <returns>Estimated level achievable</returns>
        public int EstimateLevelFromXP(int totalXP)
        {
            int level = 1;
            int accumulatedXP = 0;

            while (level < maxLevel)
            {
                int xpForNextLevel = CalculateXPForLevel(level + 1);
                if (accumulatedXP + xpForNextLevel > totalXP)
                {
                    break;
                }
                accumulatedXP += xpForNextLevel;
                level++;
            }

            return level;
        }

        /// <summary>
        /// Validates and clamps settings to reasonable values.
        /// </summary>
        private void OnValidate()
        {
            // Clamp values to safe ranges
            if (baseXP < 1f) baseXP = 1f;
            if (exponent < 1f) exponent = 1f;
            if (maxLevel < 1) maxLevel = 1;
            if (magnetRange < 0.5f) magnetRange = 0.5f;
            if (magnetSpeed < 1f) magnetSpeed = 1f;
            if (gemLifetime < 0f) gemLifetime = 0f;
            if (initialPoolSize < 10) initialPoolSize = 10;
            if (poolExpandAmount < 10) poolExpandAmount = 10;
        }

        #endregion

        #region Debug Helpers

        /// <summary>
        /// Prints XP curve to console for balancing.
        /// </summary>
        [ContextMenu("Print XP Curve (Levels 1-20)")]
        private void PrintXPCurve()
        {
            Debug.Log("=== XP CURVE ===");
            Debug.Log($"Base XP: {baseXP}, Exponent: {exponent}");
            Debug.Log("---");

            for (int i = 1; i <= 20; i++)
            {
                int xpForLevel = CalculateXPForLevel(i);
                int totalXP = CalculateTotalXPForLevel(i);
                Debug.Log($"Level {i-1}→{i}: {xpForLevel} XP | Total XP: {totalXP}");
            }
        }

        /// <summary>
        /// Estimates levels achievable in a 10-minute run.
        /// </summary>
        [ContextMenu("Estimate Levels in 10-Min Run")]
        private void EstimateLevelsInRun()
        {
            // Assumptions for 10-minute run
            int enemiesPerMinute = 25; // Average
            int runDurationMinutes = 10;
            int averageXPPerEnemy = 30; // Mix of basic (25) and fast (40)

            int totalEnemies = enemiesPerMinute * runDurationMinutes;
            int totalXPAvailable = totalEnemies * averageXPPerEnemy;

            int estimatedLevel = EstimateLevelFromXP(totalXPAvailable);

            Debug.Log("=== 10-MINUTE RUN ESTIMATE ===");
            Debug.Log($"Enemies Killed: ~{totalEnemies}");
            Debug.Log($"Total XP Available: ~{totalXPAvailable}");
            Debug.Log($"Estimated Final Level: {estimatedLevel}");
            Debug.Log($"Level-Ups: {estimatedLevel - 1}");
        }

        #endregion
    }
}
