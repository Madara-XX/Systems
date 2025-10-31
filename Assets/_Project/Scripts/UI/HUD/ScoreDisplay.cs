using UnityEngine;
using TMPro;
using System.Collections;

namespace RoombaRampage.UI
{
    /// <summary>
    /// Displays player score with smooth count-up animation.
    /// Automatically connects to ScoreManager singleton.
    ///
    /// Setup:
    /// 1. Assign TextMeshProUGUI text component
    /// 2. Configure display format and animation settings
    /// 3. Component will automatically find and subscribe to ScoreManager
    ///
    /// Features:
    /// - Smooth count-up animation (no instant jumps)
    /// - Configurable number format (commas, decimal places, etc.)
    /// - Optional particle effects on score increase
    /// - Optional score increase popup (e.g., "+100")
    /// </summary>
    public class ScoreDisplay : MonoBehaviour
    {
        #region Serialized Fields

        [Header("UI References")]
        [Tooltip("Text component to display score")]
        [SerializeField] private TextMeshProUGUI scoreText;

        [Tooltip("Optional text to show score increase (e.g., '+100')")]
        [SerializeField] private TextMeshProUGUI scoreIncrementText;

        [Header("Display Settings")]
        [Tooltip("Text prefix (e.g., 'Score: ')")]
        [SerializeField] private string prefix = "Score: ";

        [Tooltip("Use thousand separators (e.g., 12,340)")]
        [SerializeField] private bool useThousandSeparator = true;

        [Tooltip("Number format (N0 = no decimals with commas, D = no commas)")]
        [SerializeField] private string numberFormat = "N0";

        [Header("Animation Settings")]
        [Tooltip("Count-up animation speed (higher = faster)")]
        [SerializeField] private float countUpSpeed = 50f;

        [Tooltip("Enable smooth count-up animation")]
        [SerializeField] private bool enableAnimation = true;

        [Tooltip("Minimum count-up duration (seconds)")]
        [SerializeField] private float minAnimationDuration = 0.2f;

        [Tooltip("Maximum count-up duration (seconds)")]
        [SerializeField] private float maxAnimationDuration = 1f;

        [Header("Score Increment Display")]
        [Tooltip("Show score increment popup (e.g., '+100')")]
        [SerializeField] private bool showScoreIncrement = false;

        [Tooltip("Duration to show increment text")]
        [SerializeField] private float incrementDisplayDuration = 0.5f;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        #endregion

        #region Private Fields

        private int currentDisplayScore = 0;
        private int targetScore = 0;
        private Coroutine countUpCoroutine;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Validate references
            if (scoreText == null)
            {
                Debug.LogError($"[ScoreDisplay] Score Text not assigned on {gameObject.name}! Trying to find TMP component...");
                scoreText = GetComponent<TextMeshProUGUI>();

                if (scoreText == null)
                {
                    Debug.LogError($"[ScoreDisplay] No TextMeshProUGUI component found on {gameObject.name}. Component disabled.");
                    enabled = false;
                    return;
                }
            }

            // Initialize display
            UpdateScoreText(0);
        }

        private void OnEnable()
        {
            // Subscribe to ScoreManager events
            if (Managers.ScoreManager.Instance != null)
            {
                Managers.ScoreManager.Instance.OnScoreChanged.AddListener(OnScoreChanged);
                Managers.ScoreManager.Instance.OnScoreAdded.AddListener(OnScoreAdded);

                // Initialize with current score
                currentDisplayScore = Managers.ScoreManager.Instance.CurrentScore;
                targetScore = currentDisplayScore;
                UpdateScoreText(currentDisplayScore);

                if (showDebugInfo)
                {
                    Debug.Log($"[ScoreDisplay] Subscribed to ScoreManager. Initial score: {currentDisplayScore}");
                }
            }
            else
            {
                Debug.LogWarning("[ScoreDisplay] ScoreManager.Instance is null! Cannot subscribe to score events.");
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            if (Managers.ScoreManager.Instance != null)
            {
                Managers.ScoreManager.Instance.OnScoreChanged.RemoveListener(OnScoreChanged);
                Managers.ScoreManager.Instance.OnScoreAdded.RemoveListener(OnScoreAdded);
            }

            // Stop any active animations
            if (countUpCoroutine != null)
            {
                StopCoroutine(countUpCoroutine);
                countUpCoroutine = null;
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Called when score changes.
        /// </summary>
        private void OnScoreChanged(int newScore)
        {
            targetScore = newScore;

            if (enableAnimation)
            {
                // Start count-up animation
                if (countUpCoroutine != null)
                {
                    StopCoroutine(countUpCoroutine);
                }
                countUpCoroutine = StartCoroutine(CountUpCoroutine());
            }
            else
            {
                // Update immediately
                currentDisplayScore = newScore;
                UpdateScoreText(newScore);
            }
        }

        /// <summary>
        /// Called when score is added (for increment display).
        /// </summary>
        private void OnScoreAdded(int amount)
        {
            if (showScoreIncrement && scoreIncrementText != null)
            {
                ShowScoreIncrement(amount);
            }
        }

        #endregion

        #region Animation

        /// <summary>
        /// Coroutine for smooth count-up animation.
        /// </summary>
        private IEnumerator CountUpCoroutine()
        {
            int startScore = currentDisplayScore;
            int difference = targetScore - startScore;

            // Calculate animation duration based on score difference
            float duration = Mathf.Abs(difference) / countUpSpeed;
            duration = Mathf.Clamp(duration, minAnimationDuration, maxAnimationDuration);

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Ease out cubic for smoother end
                t = 1f - Mathf.Pow(1f - t, 3f);

                currentDisplayScore = Mathf.RoundToInt(Mathf.Lerp(startScore, targetScore, t));
                UpdateScoreText(currentDisplayScore);

                yield return null;
            }

            // Ensure final value is exact
            currentDisplayScore = targetScore;
            UpdateScoreText(currentDisplayScore);

            countUpCoroutine = null;
        }

        #endregion

        #region UI Updates

        /// <summary>
        /// Updates the score text display.
        /// </summary>
        private void UpdateScoreText(int score)
        {
            if (scoreText != null)
            {
                string formattedScore = useThousandSeparator
                    ? score.ToString(numberFormat)
                    : score.ToString();

                scoreText.text = prefix + formattedScore;
            }
        }

        /// <summary>
        /// Shows score increment popup.
        /// </summary>
        private void ShowScoreIncrement(int amount)
        {
            if (scoreIncrementText != null)
            {
                scoreIncrementText.text = $"+{amount}";
                scoreIncrementText.gameObject.SetActive(true);
                StartCoroutine(HideIncrementTextCoroutine());
            }
        }

        /// <summary>
        /// Hides increment text after duration.
        /// </summary>
        private IEnumerator HideIncrementTextCoroutine()
        {
            yield return new WaitForSeconds(incrementDisplayDuration);

            if (scoreIncrementText != null)
            {
                scoreIncrementText.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Manually sets the displayed score (for testing or custom scenarios).
        /// </summary>
        public void SetScore(int score)
        {
            targetScore = score;
            currentDisplayScore = score;
            UpdateScoreText(score);
        }

        /// <summary>
        /// Forces score display to update immediately to target.
        /// </summary>
        public void ForceUpdate()
        {
            if (countUpCoroutine != null)
            {
                StopCoroutine(countUpCoroutine);
                countUpCoroutine = null;
            }

            currentDisplayScore = targetScore;
            UpdateScoreText(currentDisplayScore);
        }

        #endregion

        #region Debug Methods

        [ContextMenu("Test: Add 100 Score")]
        private void TestAddScore()
        {
            if (Managers.ScoreManager.Instance != null)
            {
                Managers.ScoreManager.Instance.AddScore(100);
            }
        }

        [ContextMenu("Test: Add 1000 Score")]
        private void TestAddLargeScore()
        {
            if (Managers.ScoreManager.Instance != null)
            {
                Managers.ScoreManager.Instance.AddScore(1000);
            }
        }

        [ContextMenu("Test: Reset Score")]
        private void TestResetScore()
        {
            if (Managers.ScoreManager.Instance != null)
            {
                Managers.ScoreManager.Instance.ResetScore();
            }
        }

        #endregion
    }
}
