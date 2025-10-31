using UnityEngine;
using TMPro;
using System.Collections;

namespace RoombaRampage.UI
{
    /// <summary>
    /// Displays current wave number and enemy count.
    /// Automatically connects to EnemySpawner reference.
    ///
    /// Setup:
    /// 1. Assign TextMeshProUGUI text component
    /// 2. Assign EnemySpawner reference
    /// 3. Configure display format and update rate
    ///
    /// Features:
    /// - Shows wave number and enemy count
    /// - Configurable display format
    /// - Optional wave start/complete announcements
    /// - Performance-optimized updates (not every frame)
    /// </summary>
    public class WaveDisplay : MonoBehaviour
    {
        #region Serialized Fields

        [Header("References")]
        [Tooltip("Text component to display wave info")]
        [SerializeField] private TextMeshProUGUI waveText;

        [Tooltip("Enemy spawner to track waves")]
        [SerializeField] private Enemies.EnemySpawner enemySpawner;

        [Header("Display Settings")]
        [Tooltip("Display format: {0} = wave number, {1} = enemy count")]
        [SerializeField] private string displayFormat = "Wave {0} - Enemies: {1}";

        [Tooltip("Show enemy count")]
        [SerializeField] private bool showEnemyCount = true;

        [Tooltip("Text when no enemies remain")]
        [SerializeField] private string noEnemiesText = "Wave {0} - Complete!";

        [Header("Update Settings")]
        [Tooltip("Update interval (seconds). Lower = more frequent updates")]
        [SerializeField] private float updateInterval = 0.5f;

        [Header("Wave Announcements")]
        [Tooltip("Show wave start announcement")]
        [SerializeField] private bool showWaveStart = false;

        [Tooltip("Optional text for wave start announcement")]
        [SerializeField] private TextMeshProUGUI waveStartText;

        [Tooltip("Wave start announcement duration")]
        [SerializeField] private float announcementDuration = 2f;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        #endregion

        #region Private Fields

        private int lastKnownWave = 0;
        private int lastKnownEnemyCount = 0;
        private Coroutine updateCoroutine;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Validate references
            if (waveText == null)
            {
                Debug.LogError($"[WaveDisplay] Wave Text not assigned on {gameObject.name}! Trying to find TMP component...");
                waveText = GetComponent<TextMeshProUGUI>();

                if (waveText == null)
                {
                    Debug.LogError($"[WaveDisplay] No TextMeshProUGUI component found on {gameObject.name}. Component disabled.");
                    enabled = false;
                    return;
                }
            }

            // Try to find EnemySpawner if not assigned
            if (enemySpawner == null)
            {
                enemySpawner = FindObjectOfType<Enemies.EnemySpawner>();

                if (enemySpawner == null)
                {
                    Debug.LogWarning($"[WaveDisplay] EnemySpawner not assigned and could not be found in scene.");
                }
            }

            // Hide wave start announcement initially
            if (waveStartText != null)
            {
                waveStartText.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            // Start update coroutine
            updateCoroutine = StartCoroutine(UpdateWaveDisplayCoroutine());

            if (showDebugInfo && enemySpawner != null)
            {
                Debug.Log($"[WaveDisplay] Started tracking wave display. Spawner found: {enemySpawner.name}");
            }
        }

        private void OnDisable()
        {
            // Stop update coroutine
            if (updateCoroutine != null)
            {
                StopCoroutine(updateCoroutine);
                updateCoroutine = null;
            }
        }

        #endregion

        #region Update Loop

        /// <summary>
        /// Coroutine for periodic wave display updates.
        /// More performant than checking every frame in Update().
        /// </summary>
        private IEnumerator UpdateWaveDisplayCoroutine()
        {
            while (true)
            {
                UpdateWaveDisplay();
                yield return new WaitForSeconds(updateInterval);
            }
        }

        /// <summary>
        /// Updates the wave display text.
        /// </summary>
        private void UpdateWaveDisplay()
        {
            if (enemySpawner == null || waveText == null) return;

            int currentWave = enemySpawner.GetCurrentWave();
            int enemyCount = enemySpawner.GetActiveEnemyCount();

            // Check for wave change
            if (currentWave != lastKnownWave)
            {
                OnWaveChanged(currentWave);
            }

            lastKnownWave = currentWave;
            lastKnownEnemyCount = enemyCount;

            // Update text display
            if (showEnemyCount)
            {
                if (enemyCount > 0)
                {
                    waveText.text = string.Format(displayFormat, currentWave, enemyCount);
                }
                else
                {
                    waveText.text = string.Format(noEnemiesText, currentWave);
                }
            }
            else
            {
                waveText.text = $"Wave {currentWave}";
            }
        }

        #endregion

        #region Wave Events

        /// <summary>
        /// Called when wave number changes.
        /// </summary>
        private void OnWaveChanged(int newWave)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[WaveDisplay] Wave changed to: {newWave}");
            }

            // Show wave start announcement
            if (showWaveStart && waveStartText != null)
            {
                ShowWaveAnnouncement(newWave);
            }
        }

        /// <summary>
        /// Shows wave start announcement.
        /// </summary>
        private void ShowWaveAnnouncement(int wave)
        {
            if (waveStartText == null) return;

            waveStartText.text = $"WAVE {wave}";
            waveStartText.gameObject.SetActive(true);
            StartCoroutine(HideAnnouncementCoroutine());
        }

        /// <summary>
        /// Hides wave announcement after duration.
        /// </summary>
        private IEnumerator HideAnnouncementCoroutine()
        {
            yield return new WaitForSeconds(announcementDuration);

            if (waveStartText != null)
            {
                waveStartText.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Manually sets the enemy spawner reference.
        /// </summary>
        public void SetEnemySpawner(Enemies.EnemySpawner spawner)
        {
            enemySpawner = spawner;

            if (showDebugInfo)
            {
                Debug.Log($"[WaveDisplay] EnemySpawner set to: {spawner?.name ?? "null"}");
            }

            // Force update
            UpdateWaveDisplay();
        }

        /// <summary>
        /// Forces immediate update of wave display.
        /// </summary>
        public void ForceUpdate()
        {
            UpdateWaveDisplay();
        }

        /// <summary>
        /// Shows wave complete message.
        /// </summary>
        public void ShowWaveComplete()
        {
            if (waveStartText != null)
            {
                waveStartText.text = "WAVE COMPLETE!";
                waveStartText.gameObject.SetActive(true);
                StartCoroutine(HideAnnouncementCoroutine());
            }
        }

        #endregion

        #region Debug Methods

        [ContextMenu("Test: Force Update")]
        private void TestForceUpdate()
        {
            ForceUpdate();
        }

        [ContextMenu("Test: Show Wave Complete")]
        private void TestWaveComplete()
        {
            ShowWaveComplete();
        }

        #endregion
    }
}
