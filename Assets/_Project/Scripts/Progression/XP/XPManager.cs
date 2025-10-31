using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace RoombaRampage.Progression
{
    /// <summary>
    /// Singleton manager for player XP and leveling system.
    /// Handles XP gain, level-up detection, persistence, and event broadcasting.
    ///
    /// Features:
    /// - XP accumulation and level progression
    /// - Exponential XP curve via XPSettings
    /// - Level-up events and effects
    /// - Save/load via PlayerPrefs
    /// - Max level capping
    /// - Integration with skill system (pause for skill selection)
    ///
    /// Usage:
    /// - XPManager.Instance.AddXP(amount);
    /// - Subscribe to OnLevelUp event for skill selection
    /// - Call ResetXP() at start of new run
    /// </summary>
    public class XPManager : MonoBehaviour
    {
        #region Singleton

        public static XPManager Instance { get; private set; }

        #endregion

        #region Serialized Fields

        [Header("Configuration")]
        [Tooltip("XP Settings ScriptableObject (required)")]
        [SerializeField] private XPSettings xpSettings;

        [Header("Events")]
        [Tooltip("Invoked when XP changes. Parameters: (currentXP, xpNeededForNextLevel)")]
        public UnityEvent<int, int> OnXPGained;

        [Tooltip("Invoked when player levels up. Parameter: (newLevel)")]
        public UnityEvent<int> OnLevelUp;

        [Tooltip("Invoked when player reaches max level")]
        public UnityEvent OnMaxLevelReached;

        [Header("References")]
        [Tooltip("Optional: Player health component for heal-on-level-up")]
        [SerializeField] private Player.PlayerHealth playerHealth;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        #endregion

        #region Private Fields

        // XP & Level state
        private int currentXP;
        private int currentLevel = 1;
        private bool isLevelUpPending = false;
        private bool hasReachedMaxLevel = false;

        // Audio
        private AudioSource audioSource;

        #endregion

        #region Public Properties

        /// <summary>
        /// Current XP accumulated toward next level.
        /// </summary>
        public int CurrentXP => currentXP;

        /// <summary>
        /// Current player level (1-based).
        /// </summary>
        public int CurrentLevel => currentLevel;

        /// <summary>
        /// XP required to reach next level.
        /// </summary>
        public int XPForNextLevel => xpSettings != null ? xpSettings.CalculateXPForLevel(currentLevel + 1) : 0;

        /// <summary>
        /// XP progress as normalized value (0-1) for UI display.
        /// </summary>
        public float XPProgress => XPForNextLevel > 0 ? (float)currentXP / XPForNextLevel : 0f;

        /// <summary>
        /// Is a level-up pending skill selection?
        /// </summary>
        public bool IsLevelUpPending => isLevelUpPending;

        /// <summary>
        /// Has player reached maximum level?
        /// </summary>
        public bool HasReachedMaxLevel => hasReachedMaxLevel;

        /// <summary>
        /// Total XP accumulated this run (for stats tracking).
        /// </summary>
        public int TotalXPThisRun { get; private set; }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Validate settings
            if (xpSettings == null)
            {
                Debug.LogError("[XPManager] XPSettings not assigned! XP system will not function.");
                enabled = false;
                return;
            }

            // Setup audio source
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            // Initialize XP state
            ResetXP();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            // Try to find player health if not assigned
            if (playerHealth == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerHealth = player.GetComponent<Player.PlayerHealth>();
                }
            }
        }

        #endregion

        #region Public Methods - XP Management

        /// <summary>
        /// Adds XP and checks for level-up.
        /// </summary>
        /// <param name="amount">XP amount to add</param>
        public void AddXP(int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[XPManager] Invalid XP amount: {amount}");
                return;
            }

            // Ignore XP if at max level
            if (hasReachedMaxLevel)
            {
                if (showDebugInfo)
                {
                    Debug.Log("[XPManager] At max level - XP gain ignored");
                }
                return;
            }

            currentXP += amount;
            TotalXPThisRun += amount;

            if (showDebugInfo)
            {
                Debug.Log($"[XPManager] Gained {amount} XP. Total: {currentXP}/{XPForNextLevel}");
            }

            // Broadcast XP gained event
            OnXPGained?.Invoke(currentXP, XPForNextLevel);

            // Play collection sound
            if (xpSettings.xpCollectSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(xpSettings.xpCollectSound, 0.3f);
            }

            // Check for level-up
            CheckLevelUp();
        }

        /// <summary>
        /// Manually sets player level (for debugging/testing).
        /// </summary>
        /// <param name="level">Target level</param>
        public void SetLevel(int level)
        {
            level = Mathf.Clamp(level, 1, xpSettings.maxLevel);
            currentLevel = level;
            currentXP = 0;

            // Check if at max level
            hasReachedMaxLevel = (currentLevel >= xpSettings.maxLevel);

            if (showDebugInfo)
            {
                Debug.Log($"[XPManager] Level set to {currentLevel}");
            }

            OnXPGained?.Invoke(currentXP, XPForNextLevel);
        }

        /// <summary>
        /// Resets XP and level to starting state.
        /// Call this at the start of a new run.
        /// </summary>
        public void ResetXP()
        {
            currentXP = 0;
            currentLevel = 1;
            TotalXPThisRun = 0;
            isLevelUpPending = false;
            hasReachedMaxLevel = false;

            if (showDebugInfo)
            {
                Debug.Log("[XPManager] XP reset - Starting new run at level 1");
            }

            OnXPGained?.Invoke(currentXP, XPForNextLevel);
        }

        /// <summary>
        /// Call this when skill selection is complete to resume game.
        /// </summary>
        public void CompleteLevelUp()
        {
            isLevelUpPending = false;

            // Resume game if it was paused
            if (Time.timeScale == 0f)
            {
                Time.timeScale = 1f;
            }

            if (showDebugInfo)
            {
                Debug.Log("[XPManager] Level-up completed - Game resumed");
            }
        }

        #endregion

        #region Public Methods - Getters

        /// <summary>
        /// Gets current XP.
        /// </summary>
        public int GetCurrentXP() => currentXP;

        /// <summary>
        /// Gets current level.
        /// </summary>
        public int GetCurrentLevel() => currentLevel;

        /// <summary>
        /// Gets XP needed for next level.
        /// </summary>
        public int GetXPForNextLevel() => XPForNextLevel;

        /// <summary>
        /// Gets XP progress as 0-1 value.
        /// </summary>
        public float GetXPProgress() => XPProgress;

        /// <summary>
        /// Calculates XP required for a specific level.
        /// </summary>
        public int CalculateXPForLevel(int level)
        {
            return xpSettings.CalculateXPForLevel(level);
        }

        /// <summary>
        /// Calculates total cumulative XP needed to reach a level.
        /// </summary>
        public int GetTotalXPForLevel(int level)
        {
            return xpSettings.CalculateTotalXPForLevel(level);
        }

        #endregion

        #region Private Methods - Level-Up Logic

        /// <summary>
        /// Checks if player has enough XP to level up.
        /// Handles multiple level-ups if XP is very high.
        /// </summary>
        private void CheckLevelUp()
        {
            int xpNeeded = XPForNextLevel;

            while (currentXP >= xpNeeded && currentLevel < xpSettings.maxLevel)
            {
                // Level up!
                currentXP -= xpNeeded;
                currentLevel++;

                if (showDebugInfo)
                {
                    Debug.Log($"[XPManager] LEVEL UP! Now level {currentLevel}");
                }

                // Trigger level-up effects
                TriggerLevelUpEffects();

                // Check if reached max level
                if (currentLevel >= xpSettings.maxLevel)
                {
                    hasReachedMaxLevel = true;
                    currentXP = 0; // Cap XP at 0 when max level reached

                    OnMaxLevelReached?.Invoke();

                    if (showDebugInfo)
                    {
                        Debug.Log($"[XPManager] Reached MAX LEVEL {currentLevel}!");
                    }
                    break;
                }

                // Recalculate XP needed for next level
                xpNeeded = XPForNextLevel;
            }

            // Broadcast XP update
            OnXPGained?.Invoke(currentXP, XPForNextLevel);
        }

        /// <summary>
        /// Triggers all level-up effects (VFX, SFX, healing, pause).
        /// </summary>
        private void TriggerLevelUpEffects()
        {
            // Play level-up sound
            if (xpSettings.levelUpSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(xpSettings.levelUpSound, 0.7f);
            }

            // Spawn level-up VFX at player
            if (xpSettings.levelUpVFXPrefab != null && playerHealth != null)
            {
                GameObject vfx = Instantiate(xpSettings.levelUpVFXPrefab, playerHealth.transform.position, Quaternion.identity);
                Destroy(vfx, xpSettings.levelUpEffectDuration);
            }

            // Heal player to full
            if (xpSettings.healOnLevelUp && playerHealth != null)
            {
                int healAmount = playerHealth.MaxHealth - playerHealth.CurrentHealth;
                if (healAmount > 0)
                {
                    playerHealth.Heal(healAmount);

                    if (showDebugInfo)
                    {
                        Debug.Log($"[XPManager] Healed player for {healAmount} HP");
                    }
                }
            }

            // Time slow effect
            if (xpSettings.enableTimeSlowOnLevelUp)
            {
                StartCoroutine(TimeSlowCoroutine());
            }

            // Broadcast level-up event
            OnLevelUp?.Invoke(currentLevel);

            // Pause for skill selection (if enabled)
            if (xpSettings.pauseOnLevelUp)
            {
                StartCoroutine(PauseForSkillSelectionCoroutine());
            }
        }

        /// <summary>
        /// Coroutine for time slow effect on level-up.
        /// </summary>
        private IEnumerator TimeSlowCoroutine()
        {
            float originalTimeScale = Time.timeScale;
            Time.timeScale = xpSettings.levelUpTimeScale;

            yield return new WaitForSecondsRealtime(xpSettings.levelUpTimeSlowDuration);

            // Only restore time scale if not paused for skill selection
            if (!isLevelUpPending)
            {
                Time.timeScale = originalTimeScale;
            }
        }

        /// <summary>
        /// Coroutine for pausing game after level-up delay.
        /// Allows effects to play before pause.
        /// </summary>
        private IEnumerator PauseForSkillSelectionCoroutine()
        {
            // Wait for effects to play
            yield return new WaitForSecondsRealtime(xpSettings.pauseDelay);

            // Set pending flag and pause game
            isLevelUpPending = true;
            Time.timeScale = 0f;

            if (showDebugInfo)
            {
                Debug.Log("[XPManager] Game paused for skill selection");
            }

            // Skill system should detect IsLevelUpPending and show UI
            // Call CompleteLevelUp() when done
        }

        #endregion

        #region Save/Load (PlayerPrefs)

        /// <summary>
        /// Saves current XP and level to PlayerPrefs.
        /// Note: For roguelike, typically reset on death.
        /// This is for mid-run persistence only.
        /// </summary>
        public void SaveProgress()
        {
            PlayerPrefs.SetInt("XP_CurrentXP", currentXP);
            PlayerPrefs.SetInt("XP_CurrentLevel", currentLevel);
            PlayerPrefs.SetInt("XP_TotalXPThisRun", TotalXPThisRun);
            PlayerPrefs.Save();

            if (showDebugInfo)
            {
                Debug.Log($"[XPManager] Progress saved: Level {currentLevel}, XP {currentXP}");
            }
        }

        /// <summary>
        /// Loads XP and level from PlayerPrefs.
        /// </summary>
        public void LoadProgress()
        {
            currentXP = PlayerPrefs.GetInt("XP_CurrentXP", 0);
            currentLevel = PlayerPrefs.GetInt("XP_CurrentLevel", 1);
            TotalXPThisRun = PlayerPrefs.GetInt("XP_TotalXPThisRun", 0);

            // Validate loaded values
            currentLevel = Mathf.Clamp(currentLevel, 1, xpSettings.maxLevel);
            hasReachedMaxLevel = (currentLevel >= xpSettings.maxLevel);

            if (showDebugInfo)
            {
                Debug.Log($"[XPManager] Progress loaded: Level {currentLevel}, XP {currentXP}");
            }

            OnXPGained?.Invoke(currentXP, XPForNextLevel);
        }

        /// <summary>
        /// Clears saved progress.
        /// </summary>
        public void ClearSavedProgress()
        {
            PlayerPrefs.DeleteKey("XP_CurrentXP");
            PlayerPrefs.DeleteKey("XP_CurrentLevel");
            PlayerPrefs.DeleteKey("XP_TotalXPThisRun");
            PlayerPrefs.Save();

            if (showDebugInfo)
            {
                Debug.Log("[XPManager] Saved progress cleared");
            }
        }

        #endregion

        #region Debug Methods

        private void OnGUI()
        {
            if (!showDebugInfo) return;

            GUILayout.BeginArea(new Rect(Screen.width - 310, 10, 300, 200));
            GUILayout.Label("=== XP Manager ===");
            GUILayout.Label($"Level: {currentLevel} / {xpSettings.maxLevel}");
            GUILayout.Label($"XP: {currentXP} / {XPForNextLevel}");
            GUILayout.Label($"Progress: {XPProgress:P0}");
            GUILayout.Label($"Total XP This Run: {TotalXPThisRun}");
            GUILayout.Label($"Level-Up Pending: {isLevelUpPending}");
            GUILayout.Label($"Max Level: {hasReachedMaxLevel}");

            if (GUILayout.Button("Add 50 XP"))
            {
                AddXP(50);
            }

            if (GUILayout.Button("Add 500 XP (Multi-Level)"))
            {
                AddXP(500);
            }

            if (GUILayout.Button("Reset XP"))
            {
                ResetXP();
            }

            GUILayout.EndArea();
        }

        [ContextMenu("Test: Add 100 XP")]
        private void TestAddXP()
        {
            AddXP(100);
        }

        [ContextMenu("Test: Force Level Up")]
        private void TestForceLevelUp()
        {
            int xpNeeded = XPForNextLevel;
            AddXP(xpNeeded);
        }

        [ContextMenu("Test: Set Level 10")]
        private void TestSetLevel10()
        {
            SetLevel(10);
        }

        [ContextMenu("Test: Reset XP")]
        private void TestResetXP()
        {
            ResetXP();
        }

        [ContextMenu("Test: Complete Level-Up")]
        private void TestCompleteLevelUp()
        {
            CompleteLevelUp();
        }

        #endregion
    }
}
