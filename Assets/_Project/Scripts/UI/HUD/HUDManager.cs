using UnityEngine;

namespace RoombaRampage.UI
{
    /// <summary>
    /// Central manager for all HUD elements.
    /// Initializes and connects HUD components to game systems.
    ///
    /// Setup:
    /// 1. Attach to the main Canvas or HUD parent GameObject
    /// 2. Assign all HUD component references
    /// 3. Assign game system references (PlayerHealth, EnemySpawner, etc.)
    /// 4. HUD will auto-initialize on Start()
    ///
    /// Features:
    /// - Centralized HUD initialization
    /// - Show/hide HUD visibility
    /// - Connect HUD components to game systems
    /// - Public API for other systems to interact with HUD
    ///
    /// Usage from other scripts:
    ///   HUDManager.Instance.ShowHUD(false); // Hide HUD
    ///   HUDManager.Instance.UpdatePlayerHealth(50, 100); // Manual update
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        #region Singleton

        public static HUDManager Instance { get; private set; }

        #endregion

        #region Serialized Fields

        [Header("HUD Components")]
        [Tooltip("Player health bar component")]
        [SerializeField] private HealthBar playerHealthBar;

        [Tooltip("Boss health bar component (optional)")]
        [SerializeField] private HealthBar bossHealthBar;

        [Tooltip("Score display component")]
        [SerializeField] private ScoreDisplay scoreDisplay;

        [Tooltip("Wave display component")]
        [SerializeField] private WaveDisplay waveDisplay;

        [Tooltip("Kill counter component")]
        [SerializeField] private KillCounter killCounter;

        [Tooltip("XP bar component")]
        [SerializeField] private XPBar xpBar;

        [Header("Game System References")]
        [Tooltip("Player health component (will auto-find if not assigned)")]
        [SerializeField] private Player.PlayerHealth playerHealth;

        [Tooltip("Enemy spawner (will auto-find if not assigned)")]
        [SerializeField] private Enemies.EnemySpawner enemySpawner;

        [Tooltip("PlayerEvents ScriptableObject (optional)")]
        [SerializeField] private Player.PlayerEvents playerEvents;

        [Header("Settings")]
        [Tooltip("Show HUD on start")]
        [SerializeField] private bool showOnStart = true;

        [Tooltip("Auto-initialize HUD connections on Start")]
        [SerializeField] private bool autoInitialize = true;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        #endregion

        #region Private Fields

        private bool isInitialized = false;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"[HUDManager] Multiple HUDManager instances detected. Destroying duplicate on {gameObject.name}");
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // Validate critical components
            ValidateComponents();
        }

        private void Start()
        {
            if (autoInitialize)
            {
                Initialize();
            }

            // Set initial visibility
            if (!showOnStart)
            {
                ShowHUD(false);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes all HUD components and connects them to game systems.
        /// </summary>
        public void Initialize()
        {
            if (isInitialized)
            {
                Debug.LogWarning("[HUDManager] Already initialized. Skipping.");
                return;
            }

            if (showDebugInfo)
            {
                Debug.Log("[HUDManager] Initializing HUD...");
            }

            // Auto-find game system references if not assigned
            FindGameSystems();

            // Initialize player health bar
            InitializePlayerHealthBar();

            // Initialize boss health bar (hide by default)
            InitializeBossHealthBar();

            // Initialize score display
            InitializeScoreDisplay();

            // Initialize wave display
            InitializeWaveDisplay();

            // Initialize kill counter
            InitializeKillCounter();

            // Initialize XP bar
            InitializeXPBar();

            isInitialized = true;

            if (showDebugInfo)
            {
                Debug.Log("[HUDManager] HUD initialization complete!");
            }
        }

        /// <summary>
        /// Finds game system references automatically.
        /// </summary>
        private void FindGameSystems()
        {
            // Find PlayerHealth if not assigned
            if (playerHealth == null)
            {
                playerHealth = FindObjectOfType<Player.PlayerHealth>();

                if (playerHealth != null && showDebugInfo)
                {
                    Debug.Log($"[HUDManager] Auto-found PlayerHealth: {playerHealth.gameObject.name}");
                }
            }

            // Find EnemySpawner if not assigned
            if (enemySpawner == null)
            {
                enemySpawner = FindObjectOfType<Enemies.EnemySpawner>();

                if (enemySpawner != null && showDebugInfo)
                {
                    Debug.Log($"[HUDManager] Auto-found EnemySpawner: {enemySpawner.gameObject.name}");
                }
            }

            // Try to load PlayerEvents if not assigned
            if (playerEvents == null)
            {
                playerEvents = Resources.Load<Player.PlayerEvents>("PlayerEvents");

                if (playerEvents != null && showDebugInfo)
                {
                    Debug.Log($"[HUDManager] Auto-loaded PlayerEvents: {playerEvents.name}");
                }
            }
        }

        /// <summary>
        /// Initializes player health bar and connects to PlayerHealth.
        /// </summary>
        private void InitializePlayerHealthBar()
        {
            if (playerHealthBar == null)
            {
                Debug.LogWarning("[HUDManager] Player Health Bar not assigned!");
                return;
            }

            if (playerHealth != null)
            {
                // Set initial health display
                playerHealthBar.UpdateHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);

                // Subscribe to health events via PlayerEvents
                if (playerEvents != null)
                {
                    playerEvents.OnPlayerDamaged += OnPlayerHealthChanged;
                    playerEvents.OnPlayerHealed += OnPlayerHealthChanged;
                    playerEvents.OnPlayerRespawned += OnPlayerRespawned;
                }

                if (showDebugInfo)
                {
                    Debug.Log($"[HUDManager] Player health bar initialized: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}");
                }
            }
            else
            {
                Debug.LogWarning("[HUDManager] PlayerHealth not found! Health bar will not update.");
            }
        }

        /// <summary>
        /// Initializes boss health bar (hidden by default).
        /// </summary>
        private void InitializeBossHealthBar()
        {
            if (bossHealthBar != null)
            {
                bossHealthBar.SetVisible(false);

                if (showDebugInfo)
                {
                    Debug.Log("[HUDManager] Boss health bar initialized (hidden)");
                }
            }
        }

        /// <summary>
        /// Initializes score display.
        /// </summary>
        private void InitializeScoreDisplay()
        {
            if (scoreDisplay == null)
            {
                Debug.LogWarning("[HUDManager] Score Display not assigned!");
                return;
            }

            // ScoreDisplay auto-connects to ScoreManager
            if (showDebugInfo)
            {
                Debug.Log("[HUDManager] Score display initialized");
            }
        }

        /// <summary>
        /// Initializes wave display and connects to EnemySpawner.
        /// </summary>
        private void InitializeWaveDisplay()
        {
            if (waveDisplay == null)
            {
                Debug.LogWarning("[HUDManager] Wave Display not assigned!");
                return;
            }

            if (enemySpawner != null)
            {
                waveDisplay.SetEnemySpawner(enemySpawner);

                if (showDebugInfo)
                {
                    Debug.Log("[HUDManager] Wave display initialized");
                }
            }
            else
            {
                Debug.LogWarning("[HUDManager] EnemySpawner not found! Wave display will not update.");
            }
        }

        /// <summary>
        /// Initializes kill counter and connects to PlayerEvents.
        /// </summary>
        private void InitializeKillCounter()
        {
            if (killCounter == null)
            {
                Debug.LogWarning("[HUDManager] Kill Counter not assigned!");
                return;
            }

            if (playerEvents != null)
            {
                killCounter.SetPlayerEvents(playerEvents);

                if (showDebugInfo)
                {
                    Debug.Log("[HUDManager] Kill counter initialized");
                }
            }
            else
            {
                Debug.LogWarning("[HUDManager] PlayerEvents not found! Kill counter will not update.");
            }
        }

        /// <summary>
        /// Initializes XP bar (placeholder).
        /// </summary>
        private void InitializeXPBar()
        {
            if (xpBar == null)
            {
                Debug.LogWarning("[HUDManager] XP Bar not assigned!");
                return;
            }

            // XP bar is placeholder - will connect to XP system when implemented
            if (showDebugInfo)
            {
                Debug.Log("[HUDManager] XP bar initialized (placeholder)");
            }
        }

        /// <summary>
        /// Validates critical HUD components.
        /// </summary>
        private void ValidateComponents()
        {
            bool hasErrors = false;

            if (playerHealthBar == null)
            {
                Debug.LogWarning("[HUDManager] Player Health Bar not assigned!");
                hasErrors = true;
            }

            if (scoreDisplay == null)
            {
                Debug.LogWarning("[HUDManager] Score Display not assigned!");
                hasErrors = true;
            }

            if (hasErrors)
            {
                Debug.LogWarning("[HUDManager] Some HUD components are missing. HUD may not function correctly.");
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Called when player health changes.
        /// </summary>
        private void OnPlayerHealthChanged(int currentHealth, int maxHealth)
        {
            if (playerHealthBar != null)
            {
                playerHealthBar.UpdateHealthSmooth(currentHealth, maxHealth);
            }
        }

        /// <summary>
        /// Called when player respawns.
        /// </summary>
        private void OnPlayerRespawned()
        {
            if (playerHealth != null && playerHealthBar != null)
            {
                playerHealthBar.UpdateHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }

            if (killCounter != null)
            {
                killCounter.ResetKills();
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Shows or hides the entire HUD.
        /// </summary>
        public void ShowHUD(bool show)
        {
            gameObject.SetActive(show);

            if (showDebugInfo)
            {
                Debug.Log($"[HUDManager] HUD visibility: {show}");
            }
        }

        /// <summary>
        /// Manually updates player health bar (if needed).
        /// </summary>
        public void UpdatePlayerHealth(int currentHealth, int maxHealth)
        {
            if (playerHealthBar != null)
            {
                playerHealthBar.UpdateHealthSmooth(currentHealth, maxHealth);
            }
        }

        /// <summary>
        /// Shows boss health bar.
        /// </summary>
        public void ShowBossHealthBar(string bossName, int currentHealth, int maxHealth)
        {
            if (bossHealthBar != null)
            {
                bossHealthBar.SetVisible(true);
                bossHealthBar.UpdateHealth(currentHealth, maxHealth);

                if (showDebugInfo)
                {
                    Debug.Log($"[HUDManager] Boss health bar shown: {bossName} ({currentHealth}/{maxHealth})");
                }
            }
        }

        /// <summary>
        /// Updates boss health bar.
        /// </summary>
        public void UpdateBossHealth(int currentHealth, int maxHealth)
        {
            if (bossHealthBar != null)
            {
                bossHealthBar.UpdateHealthSmooth(currentHealth, maxHealth);
            }
        }

        /// <summary>
        /// Hides boss health bar.
        /// </summary>
        public void HideBossHealthBar()
        {
            if (bossHealthBar != null)
            {
                bossHealthBar.SetVisible(false);

                if (showDebugInfo)
                {
                    Debug.Log("[HUDManager] Boss health bar hidden");
                }
            }
        }

        /// <summary>
        /// Resets all HUD elements (for game restart).
        /// </summary>
        public void ResetHUD()
        {
            if (playerHealth != null && playerHealthBar != null)
            {
                playerHealthBar.UpdateHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }

            if (killCounter != null)
            {
                killCounter.ResetKills();
            }

            HideBossHealthBar();

            if (showDebugInfo)
            {
                Debug.Log("[HUDManager] HUD reset");
            }
        }

        #endregion

        #region Cleanup

        private void OnDisable()
        {
            // Unsubscribe from events
            if (playerEvents != null)
            {
                playerEvents.OnPlayerDamaged -= OnPlayerHealthChanged;
                playerEvents.OnPlayerHealed -= OnPlayerHealthChanged;
                playerEvents.OnPlayerRespawned -= OnPlayerRespawned;
            }
        }

        #endregion

        #region Debug Methods

        [ContextMenu("Test: Show HUD")]
        private void TestShowHUD()
        {
            ShowHUD(true);
        }

        [ContextMenu("Test: Hide HUD")]
        private void TestHideHUD()
        {
            ShowHUD(false);
        }

        [ContextMenu("Test: Show Boss Health Bar")]
        private void TestShowBossHealthBar()
        {
            ShowBossHealthBar("Test Boss", 1000, 1000);
        }

        [ContextMenu("Test: Hide Boss Health Bar")]
        private void TestHideBossHealthBar()
        {
            HideBossHealthBar();
        }

        [ContextMenu("Test: Reset HUD")]
        private void TestResetHUD()
        {
            ResetHUD();
        }

        [ContextMenu("Force Reinitialize")]
        private void ForceReinitialize()
        {
            isInitialized = false;
            Initialize();
        }

        #endregion
    }
}
