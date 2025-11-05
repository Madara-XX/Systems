using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RoombaRampage.UI
{
    /// <summary>
    /// XP bar UI component that displays player XP and level progress.
    /// Integrates with XPManager to show real-time XP gain and level-ups.
    ///
    /// Setup:
    /// 1. Assign UI Image references (background, fill, border)
    /// 2. Assign TextMeshProUGUI for level text (optional)
    /// 3. Automatically connects to XPManager on enable
    ///
    /// Features:
    /// - Smooth fill animation (lerp)
    /// - Level number display
    /// - Visual style matches health bar
    /// - Automatic XP system integration
    /// - Level-up pulse animation
    /// </summary>
    public class XPBar : MonoBehaviour
    {
        #region Serialized Fields

        [Header("UI References")]
        [Tooltip("Background image (optional)")]
        [SerializeField] private Image backgroundImage;

        [Tooltip("Fill image (shows XP progress)")]
        [SerializeField] private Image fillImage;

        [Tooltip("Border/frame image (optional)")]
        [SerializeField] private Image borderImage;

        [Tooltip("Text display for level (optional, e.g., 'LVL 5')")]
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("Animation Settings")]
        [Tooltip("Speed of XP bar fill animation")]
        [SerializeField] private float fillSpeed = 3f;

        [Header("Color Settings")]
        [Tooltip("XP bar fill color")]
        [SerializeField] private Color xpColor = new Color(0.3f, 0.8f, 1f); // Cyan

        [Tooltip("Flash color for level up effect")]
        [SerializeField] private Color levelUpFlashColor = new Color(1f, 1f, 0.5f); // Gold

        [Header("Display Settings")]
        [Tooltip("Show level text")]
        [SerializeField] private bool showLevelText = true;

        [Tooltip("Level text format: {0} = level number")]
        [SerializeField] private string levelTextFormat = "LVL {0}";

        [Header("Placeholder Test Data")]
        [Tooltip("Test level (placeholder until XP system implemented)")]
        [SerializeField] private int testLevel = 1;

        [Tooltip("Test XP progress (0-1, placeholder)")]
        [SerializeField] [Range(0f, 1f)] private float testXPProgress = 0.5f;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        #endregion

        #region Private Fields

        private float currentFillAmount = 0f;
        private float targetFillAmount = 0f;
        private int currentLevel = 1;
        private bool isSubscribed = false;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Validate references
            if (fillImage == null)
            {
                Debug.LogError($"[XPBar] Fill Image not assigned on {gameObject.name}! XP bar will not display.");
                enabled = false;
                return;
            }

            // Set initial color
            fillImage.color = xpColor;

            // Initialize with test data
            currentLevel = testLevel;
            currentFillAmount = testXPProgress;
            targetFillAmount = testXPProgress;
            fillImage.fillAmount = currentFillAmount;

            UpdateLevelText();

            if (showDebugInfo)
            {
                Debug.Log($"[XPBar] Initialized with test level {testLevel}, progress {testXPProgress:P0}");
            }
        }

        private void Start()
        {
            // Try subscribing if not already subscribed (fallback for timing issues)
            if (!isSubscribed)
            {
                SubscribeToXPManager();
            }
        }

        private void Update()
        {
            // Smooth fill animation
            if (Mathf.Abs(currentFillAmount - targetFillAmount) > 0.001f)
            {
                currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * fillSpeed);
                fillImage.fillAmount = currentFillAmount;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates XP bar with smooth animation.
        /// </summary>
        /// <param name="xpProgress">XP progress as 0-1 value</param>
        public void UpdateXP(float xpProgress)
        {
            targetFillAmount = Mathf.Clamp01(xpProgress);

            if (showDebugInfo)
            {
                Debug.Log($"[XPBar] XP updated to {xpProgress:P0}");
            }
        }

        /// <summary>
        /// Updates XP bar with current and max XP values.
        /// </summary>
        /// <param name="currentXP">Current XP amount</param>
        /// <param name="maxXP">XP needed for next level</param>
        public void UpdateXP(int currentXP, int maxXP)
        {
            float progress = maxXP > 0 ? (float)currentXP / maxXP : 0f;
            UpdateXP(progress);
        }

        /// <summary>
        /// Sets the current level.
        /// </summary>
        /// <param name="level">New level number</param>
        public void SetLevel(int level)
        {
            currentLevel = level;
            UpdateLevelText();

            if (showDebugInfo)
            {
                Debug.Log($"[XPBar] Level set to {level}");
            }
        }

        /// <summary>
        /// Triggers level up animation/effect.
        /// </summary>
        public void TriggerLevelUp()
        {
            // Reset XP bar to empty
            currentFillAmount = 0f;
            targetFillAmount = 0f;
            fillImage.fillAmount = 0f;

            // TODO: Add level up particle effects
            // TODO: Add level up sound
            // TODO: Add flash animation

            if (showDebugInfo)
            {
                Debug.Log($"[XPBar] Level up to {currentLevel}!");
            }
        }

        /// <summary>
        /// Sets XP bar visibility.
        /// </summary>
        /// <param name="visible">Visible state</param>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates level text display.
        /// </summary>
        private void UpdateLevelText()
        {
            if (levelText != null && showLevelText)
            {
                levelText.text = string.Format(levelTextFormat, currentLevel);
            }
        }

        #endregion

        #region XP System Integration

        private void OnEnable()
        {
            // Try to subscribe to XP system events
            SubscribeToXPManager();
        }

        /// <summary>
        /// Subscribes to XPManager events (called in OnEnable and Start as fallback).
        /// </summary>
        private void SubscribeToXPManager()
        {
            if (isSubscribed)
            {
                if (showDebugInfo)
                {
                    Debug.Log("[XPBar] Already subscribed to XPManager");
                }
                return;
            }

            // Check if XPManager exists
            if (Progression.XPManager.Instance == null)
            {
                Debug.LogWarning("[XPBar] XPManager.Instance is null! Cannot subscribe to XP events. Will retry in Start().");
                return;
            }

            // Subscribe to XP system events
            Progression.XPManager.Instance.OnXPGained.AddListener(OnXPChanged);
            Progression.XPManager.Instance.OnLevelUp.AddListener(OnLevelUp);

            // Initialize with current values
            UpdateXP(Progression.XPManager.Instance.CurrentXP, Progression.XPManager.Instance.XPForNextLevel);
            SetLevel(Progression.XPManager.Instance.CurrentLevel);

            isSubscribed = true;

            Debug.Log($"[XPBar] Successfully subscribed to XPManager! Current level: {Progression.XPManager.Instance.CurrentLevel}");
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            if (isSubscribed && Progression.XPManager.Instance != null)
            {
                Progression.XPManager.Instance.OnXPGained.RemoveListener(OnXPChanged);
                Progression.XPManager.Instance.OnLevelUp.RemoveListener(OnLevelUp);
                isSubscribed = false;

                if (showDebugInfo)
                {
                    Debug.Log("[XPBar] Unsubscribed from XPManager");
                }
            }
        }

        private void OnXPChanged(int currentXP, int maxXP)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[XPBar] OnXPChanged called! XP: {currentXP}/{maxXP}");
            }

            UpdateXP(currentXP, maxXP);
        }

        private void OnLevelUp(int newLevel)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[XPBar] OnLevelUp called! New level: {newLevel}");
            }

            SetLevel(newLevel);
            TriggerLevelUp();
        }

        #endregion

        #region Debug Methods

        [ContextMenu("Test: Set 25% XP")]
        private void TestLowXP()
        {
            UpdateXP(0.25f);
        }

        [ContextMenu("Test: Set 75% XP")]
        private void TestHighXP()
        {
            UpdateXP(0.75f);
        }

        [ContextMenu("Test: Level Up")]
        private void TestLevelUp()
        {
            currentLevel++;
            SetLevel(currentLevel);
            TriggerLevelUp();
        }

        [ContextMenu("Test: Set Level 10")]
        private void TestSetLevel10()
        {
            SetLevel(10);
        }

        #endregion

        #region Editor Validation

        private void OnValidate()
        {
            // Update test data in editor
            if (Application.isPlaying && fillImage != null)
            {
                targetFillAmount = testXPProgress;
                currentLevel = testLevel;
                UpdateLevelText();
            }
        }

        #endregion
    }
}
