using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RoombaRampage.UI
{
    /// <summary>
    /// Reusable health bar component with smooth animations and color gradients.
    /// Can be used for player health, boss health, or any health display.
    ///
    /// Setup:
    /// 1. Assign UI Image references (background, fill, border)
    /// 2. Assign TextMeshProUGUI for HP text (optional)
    /// 3. Configure colors and animation settings
    /// 4. Call UpdateHealth() or UpdateHealthSmooth() to update display
    ///
    /// Features:
    /// - Smooth fill animation (lerp)
    /// - Color gradient (green → yellow → red)
    /// - Damage flash effect
    /// - HP text display (e.g., "85/100")
    /// </summary>
    public class HealthBar : MonoBehaviour
    {
        #region Serialized Fields

        [Header("UI References")]
        [Tooltip("Background image (optional)")]
        [SerializeField] private Image backgroundImage;

        [Tooltip("Fill image (shows current health)")]
        [SerializeField] private Image fillImage;

        [Tooltip("Border/frame image (optional)")]
        [SerializeField] private Image borderImage;

        [Tooltip("Text display for HP numbers (optional, e.g., '85/100')")]
        [SerializeField] private TextMeshProUGUI healthText;

        [Header("Animation Settings")]
        [Tooltip("Speed of health bar fill animation (higher = faster)")]
        [SerializeField] private float fillSpeed = 5f;

        [Tooltip("Enable damage flash effect")]
        [SerializeField] private bool enableDamageFlash = true;

        [Tooltip("Duration of damage flash")]
        [SerializeField] private float flashDuration = 0.2f;

        [Header("Color Settings")]
        [Tooltip("Color when health is high (>60%)")]
        [SerializeField] private Color highHealthColor = new Color(0.2f, 1f, 0.2f); // Green

        [Tooltip("Color when health is medium (30-60%)")]
        [SerializeField] private Color mediumHealthColor = new Color(1f, 1f, 0f); // Yellow

        [Tooltip("Color when health is low (<30%)")]
        [SerializeField] private Color lowHealthColor = new Color(1f, 0.2f, 0.2f); // Red

        [Tooltip("Flash color for damage effect")]
        [SerializeField] private Color flashColor = Color.white;

        [Header("Display Settings")]
        [Tooltip("Show HP text (requires healthText reference)")]
        [SerializeField] private bool showHealthText = true;

        [Tooltip("Text format: {0} = current HP, {1} = max HP")]
        [SerializeField] private string textFormat = "{0}/{1}";

        #endregion

        #region Private Fields

        private float currentFillAmount = 1f;
        private float targetFillAmount = 1f;
        private bool isFlashing = false;
        private float flashTimer = 0f;
        private Color originalFillColor;

        private int currentHealth;
        private int maxHealth;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Validate references
            if (fillImage == null)
            {
                Debug.LogError($"[HealthBar] Fill Image not assigned on {gameObject.name}! Health bar will not display.");
                enabled = false;
                return;
            }

            // Store original color
            originalFillColor = fillImage.color;

            // Initialize fill amount
            currentFillAmount = fillImage.fillAmount;
            targetFillAmount = currentFillAmount;
        }

        private void Update()
        {
            // Smooth fill animation
            if (Mathf.Abs(currentFillAmount - targetFillAmount) > 0.001f)
            {
                currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * fillSpeed);
                fillImage.fillAmount = currentFillAmount;
            }

            // Handle damage flash
            if (isFlashing)
            {
                flashTimer -= Time.deltaTime;
                if (flashTimer <= 0f)
                {
                    isFlashing = false;
                    fillImage.color = originalFillColor;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates health bar immediately (no animation).
        /// </summary>
        /// <param name="current">Current health value</param>
        /// <param name="max">Maximum health value</param>
        public void UpdateHealth(int current, int max)
        {
            currentHealth = current;
            maxHealth = max;

            float fillAmount = max > 0 ? (float)current / max : 0f;
            targetFillAmount = Mathf.Clamp01(fillAmount);
            currentFillAmount = targetFillAmount;

            fillImage.fillAmount = currentFillAmount;

            UpdateColor();
            UpdateText();
        }

        /// <summary>
        /// Updates health bar with smooth animation.
        /// </summary>
        /// <param name="current">Current health value</param>
        /// <param name="max">Maximum health value</param>
        /// <param name="triggerFlash">Trigger damage flash effect if health decreased</param>
        public void UpdateHealthSmooth(int current, int max, bool triggerFlash = true)
        {
            int previousHealth = currentHealth;
            currentHealth = current;
            maxHealth = max;

            float fillAmount = max > 0 ? (float)current / max : 0f;
            targetFillAmount = Mathf.Clamp01(fillAmount);

            UpdateColor();
            UpdateText();

            // Trigger flash if damaged
            if (triggerFlash && enableDamageFlash && current < previousHealth)
            {
                TriggerDamageFlash();
            }
        }

        /// <summary>
        /// Updates health bar using normalized value (0-1).
        /// </summary>
        /// <param name="normalizedHealth">Health as 0-1 value</param>
        public void UpdateHealthNormalized(float normalizedHealth)
        {
            targetFillAmount = Mathf.Clamp01(normalizedHealth);
            UpdateColor();
        }

        /// <summary>
        /// Triggers damage flash effect manually.
        /// </summary>
        public void TriggerDamageFlash()
        {
            if (!enableDamageFlash) return;

            isFlashing = true;
            flashTimer = flashDuration;
            fillImage.color = flashColor;
        }

        /// <summary>
        /// Sets health bar visibility.
        /// </summary>
        /// <param name="visible">Visible state</param>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates fill color based on health percentage.
        /// </summary>
        private void UpdateColor()
        {
            Color newColor;

            if (targetFillAmount > 0.6f)
            {
                // High health: Green
                newColor = highHealthColor;
            }
            else if (targetFillAmount > 0.3f)
            {
                // Medium health: Lerp between yellow and green
                float t = (targetFillAmount - 0.3f) / 0.3f;
                newColor = Color.Lerp(mediumHealthColor, highHealthColor, t);
            }
            else
            {
                // Low health: Lerp between red and yellow
                float t = targetFillAmount / 0.3f;
                newColor = Color.Lerp(lowHealthColor, mediumHealthColor, t);
            }

            originalFillColor = newColor;

            // Only update color if not flashing
            if (!isFlashing)
            {
                fillImage.color = newColor;
            }
        }

        /// <summary>
        /// Updates health text display.
        /// </summary>
        private void UpdateText()
        {
            if (healthText != null && showHealthText)
            {
                healthText.text = string.Format(textFormat, currentHealth, maxHealth);
            }
        }

        #endregion

        #region Debug Methods

        [ContextMenu("Test: Set 100% Health")]
        private void TestFullHealth()
        {
            UpdateHealthSmooth(100, 100);
        }

        [ContextMenu("Test: Set 50% Health")]
        private void TestHalfHealth()
        {
            UpdateHealthSmooth(50, 100);
        }

        [ContextMenu("Test: Set 25% Health")]
        private void TestLowHealth()
        {
            UpdateHealthSmooth(25, 100);
        }

        [ContextMenu("Test: Trigger Damage Flash")]
        private void TestDamageFlash()
        {
            TriggerDamageFlash();
        }

        #endregion
    }
}
