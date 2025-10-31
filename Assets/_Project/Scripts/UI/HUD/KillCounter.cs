using UnityEngine;
using TMPro;

namespace RoombaRampage.UI
{
    /// <summary>
    /// Tracks and displays enemy kills for the current run.
    /// Connects to PlayerEvents ScriptableObject for enemy kill notifications.
    ///
    /// Setup:
    /// 1. Assign TextMeshProUGUI text component
    /// 2. Assign PlayerEvents ScriptableObject reference
    /// 3. Configure display format
    ///
    /// Features:
    /// - Tracks total kills this run
    /// - Optional combo counter (kills in quick succession)
    /// - Configurable display format
    /// - Resets on game restart
    ///
    /// Note: Requires PlayerEvents.OnEnemyKilled to be raised when enemies die.
    /// See EnemyHealth.cs or Enemy.cs for integration.
    /// </summary>
    public class KillCounter : MonoBehaviour
    {
        #region Serialized Fields

        [Header("UI References")]
        [Tooltip("Text component to display kill count")]
        [SerializeField] private TextMeshProUGUI killCountText;

        [Tooltip("Optional text for combo display")]
        [SerializeField] private TextMeshProUGUI comboText;

        [Header("Event References")]
        [Tooltip("PlayerEvents ScriptableObject for enemy kill notifications")]
        [SerializeField] private Player.PlayerEvents playerEvents;

        [Header("Display Settings")]
        [Tooltip("Text prefix (e.g., 'Kills: ')")]
        [SerializeField] private string prefix = "Kills: ";

        [Tooltip("Number format (N0 = no decimals with commas, D = no commas)")]
        [SerializeField] private string numberFormat = "N0";

        [Header("Combo Settings")]
        [Tooltip("Enable combo counter")]
        [SerializeField] private bool enableCombo = false;

        [Tooltip("Time window for combo (seconds)")]
        [SerializeField] private float comboWindow = 3f;

        [Tooltip("Minimum kills for combo display")]
        [SerializeField] private int minComboKills = 2;

        [Tooltip("Combo text format")]
        [SerializeField] private string comboFormat = "x{0} COMBO!";

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        #endregion

        #region Private Fields

        private int totalKills = 0;
        private int currentCombo = 0;
        private float lastKillTime = 0f;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Validate references
            if (killCountText == null)
            {
                Debug.LogError($"[KillCounter] Kill Count Text not assigned on {gameObject.name}! Trying to find TMP component...");
                killCountText = GetComponent<TextMeshProUGUI>();

                if (killCountText == null)
                {
                    Debug.LogError($"[KillCounter] No TextMeshProUGUI component found on {gameObject.name}. Component disabled.");
                    enabled = false;
                    return;
                }
            }

            // Try to find PlayerEvents if not assigned
            if (playerEvents == null)
            {
                // Look for PlayerEvents asset in Resources or Data folder
                playerEvents = Resources.Load<Player.PlayerEvents>("PlayerEvents");

                if (playerEvents == null)
                {
                    Debug.LogWarning($"[KillCounter] PlayerEvents not assigned. Kill tracking will not work until assigned.");
                }
            }

            // Initialize display
            UpdateKillDisplay();

            // Hide combo text initially
            if (comboText != null)
            {
                comboText.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            // Subscribe to enemy killed event
            if (playerEvents != null)
            {
                playerEvents.OnEnemyKilled += OnEnemyKilled;

                if (showDebugInfo)
                {
                    Debug.Log($"[KillCounter] Subscribed to PlayerEvents.OnEnemyKilled");
                }
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            if (playerEvents != null)
            {
                playerEvents.OnEnemyKilled -= OnEnemyKilled;
            }
        }

        private void Update()
        {
            // Check combo timeout
            if (enableCombo && currentCombo > 0)
            {
                float timeSinceLastKill = Time.time - lastKillTime;

                if (timeSinceLastKill >= comboWindow)
                {
                    // Combo expired
                    ResetCombo();
                }
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Called when an enemy is killed.
        /// </summary>
        private void OnEnemyKilled(int scoreValue)
        {
            // Increment kill count
            totalKills++;
            UpdateKillDisplay();

            // Handle combo
            if (enableCombo)
            {
                float timeSinceLastKill = Time.time - lastKillTime;

                if (timeSinceLastKill <= comboWindow)
                {
                    // Continue combo
                    currentCombo++;
                }
                else
                {
                    // Start new combo
                    currentCombo = 1;
                }

                lastKillTime = Time.time;

                // Show combo if above minimum
                if (currentCombo >= minComboKills)
                {
                    UpdateComboDisplay();
                }
            }

            if (showDebugInfo)
            {
                Debug.Log($"[KillCounter] Enemy killed! Total kills: {totalKills}, Combo: {currentCombo}");
            }
        }

        #endregion

        #region UI Updates

        /// <summary>
        /// Updates the kill count text display.
        /// </summary>
        private void UpdateKillDisplay()
        {
            if (killCountText != null)
            {
                string formattedKills = totalKills.ToString(numberFormat);
                killCountText.text = prefix + formattedKills;
            }
        }

        /// <summary>
        /// Updates the combo display.
        /// </summary>
        private void UpdateComboDisplay()
        {
            if (comboText != null)
            {
                comboText.text = string.Format(comboFormat, currentCombo);
                comboText.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Resets the combo counter.
        /// </summary>
        private void ResetCombo()
        {
            currentCombo = 0;

            if (comboText != null)
            {
                comboText.gameObject.SetActive(false);
            }

            if (showDebugInfo)
            {
                Debug.Log($"[KillCounter] Combo reset");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Resets kill count to zero.
        /// </summary>
        public void ResetKills()
        {
            totalKills = 0;
            currentCombo = 0;
            lastKillTime = 0f;

            UpdateKillDisplay();
            ResetCombo();

            if (showDebugInfo)
            {
                Debug.Log($"[KillCounter] Kill count reset");
            }
        }

        /// <summary>
        /// Manually adds a kill (for testing or special cases).
        /// </summary>
        public void AddKill()
        {
            OnEnemyKilled(0);
        }

        /// <summary>
        /// Sets the PlayerEvents reference manually.
        /// </summary>
        public void SetPlayerEvents(Player.PlayerEvents events)
        {
            // Unsubscribe from old events
            if (playerEvents != null)
            {
                playerEvents.OnEnemyKilled -= OnEnemyKilled;
            }

            playerEvents = events;

            // Subscribe to new events
            if (playerEvents != null && enabled)
            {
                playerEvents.OnEnemyKilled += OnEnemyKilled;
            }

            if (showDebugInfo)
            {
                Debug.Log($"[KillCounter] PlayerEvents set to: {events?.name ?? "null"}");
            }
        }

        /// <summary>
        /// Gets current kill count.
        /// </summary>
        public int GetKillCount() => totalKills;

        /// <summary>
        /// Gets current combo count.
        /// </summary>
        public int GetComboCount() => currentCombo;

        #endregion

        #region Debug Methods

        [ContextMenu("Test: Add Kill")]
        private void TestAddKill()
        {
            AddKill();
        }

        [ContextMenu("Test: Add 5 Kills")]
        private void TestAddMultipleKills()
        {
            for (int i = 0; i < 5; i++)
            {
                AddKill();
            }
        }

        [ContextMenu("Test: Reset Kills")]
        private void TestResetKills()
        {
            ResetKills();
        }

        #endregion
    }
}
