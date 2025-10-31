using UnityEngine;
using UnityEngine.Events;

namespace RoombaRampage.Managers
{
    /// <summary>
    /// Singleton manager for tracking player score.
    /// Handles score addition, retrieval, and UI update events.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        #region Singleton

        public static ScoreManager Instance { get; private set; }

        #endregion

        #region Serialized Fields

        [Header("Score Configuration")]
        [Tooltip("Starting score at game start")]
        [SerializeField] private int startingScore = 0;

        [Header("Events")]
        [Tooltip("Invoked when score changes (passes new score value)")]
        public UnityEvent<int> OnScoreChanged;

        [Tooltip("Invoked when score is added (passes amount added)")]
        public UnityEvent<int> OnScoreAdded;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        #endregion

        #region Private Fields

        private int currentScore;
        private int highScore;

        #endregion

        #region Public Properties

        /// <summary>
        /// Current score.
        /// </summary>
        public int CurrentScore => currentScore;

        /// <summary>
        /// Highest score this session.
        /// </summary>
        public int HighScore => highScore;

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
            DontDestroyOnLoad(gameObject); // Persist across scenes

            // Initialize score
            currentScore = startingScore;
            LoadHighScore();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds score and invokes events.
        /// </summary>
        /// <param name="amount">Score to add</param>
        public void AddScore(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"[ScoreManager] Negative score amount: {amount}. Use RemoveScore() instead.");
                return;
            }

            currentScore += amount;

            // Update high score
            if (currentScore > highScore)
            {
                highScore = currentScore;
                SaveHighScore();
            }

            if (showDebugInfo)
            {
                Debug.Log($"[ScoreManager] Added {amount} score. Total: {currentScore}");
            }

            // Invoke events
            OnScoreAdded?.Invoke(amount);
            OnScoreChanged?.Invoke(currentScore);
        }

        /// <summary>
        /// Removes score (useful for penalties).
        /// </summary>
        /// <param name="amount">Score to remove</param>
        public void RemoveScore(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning($"[ScoreManager] Negative score amount: {amount}. Use AddScore() instead.");
                return;
            }

            currentScore = Mathf.Max(0, currentScore - amount);

            if (showDebugInfo)
            {
                Debug.Log($"[ScoreManager] Removed {amount} score. Total: {currentScore}");
            }

            // Invoke events
            OnScoreChanged?.Invoke(currentScore);
        }

        /// <summary>
        /// Sets score to a specific value.
        /// </summary>
        /// <param name="score">New score value</param>
        public void SetScore(int score)
        {
            currentScore = Mathf.Max(0, score);

            // Update high score
            if (currentScore > highScore)
            {
                highScore = currentScore;
                SaveHighScore();
            }

            if (showDebugInfo)
            {
                Debug.Log($"[ScoreManager] Set score to {currentScore}");
            }

            // Invoke events
            OnScoreChanged?.Invoke(currentScore);
        }

        /// <summary>
        /// Resets score to starting value.
        /// </summary>
        public void ResetScore()
        {
            currentScore = startingScore;

            if (showDebugInfo)
            {
                Debug.Log($"[ScoreManager] Reset score to {startingScore}");
            }

            // Invoke events
            OnScoreChanged?.Invoke(currentScore);
        }

        /// <summary>
        /// Resets high score.
        /// </summary>
        public void ResetHighScore()
        {
            highScore = 0;
            SaveHighScore();

            if (showDebugInfo)
            {
                Debug.Log("[ScoreManager] Reset high score");
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Loads high score from PlayerPrefs.
        /// </summary>
        private void LoadHighScore()
        {
            highScore = PlayerPrefs.GetInt("HighScore", 0);

            if (showDebugInfo)
            {
                Debug.Log($"[ScoreManager] Loaded high score: {highScore}");
            }
        }

        /// <summary>
        /// Saves high score to PlayerPrefs.
        /// </summary>
        private void SaveHighScore()
        {
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();

            if (showDebugInfo)
            {
                Debug.Log($"[ScoreManager] Saved high score: {highScore}");
            }
        }

        #endregion

        #region Debug

        private void OnGUI()
        {
            if (!showDebugInfo) return;

            GUILayout.BeginArea(new Rect(Screen.width - 310, 170, 300, 100));
            GUILayout.Label("=== Score Manager ===");
            GUILayout.Label($"Current Score: {currentScore}");
            GUILayout.Label($"High Score: {highScore}");

            if (GUILayout.Button("Add 100 Score"))
            {
                AddScore(100);
            }

            GUILayout.EndArea();
        }

        #endregion
    }
}
