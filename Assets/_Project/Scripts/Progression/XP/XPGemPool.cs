using UnityEngine;
using System.Collections.Generic;

namespace RoombaRampage.Progression
{
    /// <summary>
    /// Object pool for XP gems to optimize performance in bullet hell scenarios.
    /// Singleton pattern for global access.
    ///
    /// Features:
    /// - Pre-spawns pool of XP gems at start
    /// - Reuses gems instead of instantiate/destroy
    /// - Auto-expands if pool depleted (configurable)
    /// - Supports multiple gem types/values (future)
    ///
    /// Usage:
    /// - XPGemPool.Instance.SpawnGem(position, xpValue);
    /// - Gem automatically returns to pool when collected
    /// </summary>
    public class XPGemPool : MonoBehaviour
    {
        #region Singleton

        public static XPGemPool Instance { get; private set; }

        #endregion

        #region Serialized Fields

        [Header("Configuration")]
        [Tooltip("XP Settings ScriptableObject (required)")]
        [SerializeField] private XPSettings xpSettings;

        [Tooltip("XP Gem prefab to pool (must have XPGem component)")]
        [SerializeField] private GameObject gemPrefab;

        [Header("Pool Parent")]
        [Tooltip("Parent transform for pooled gems (keeps hierarchy clean)")]
        [SerializeField] private Transform poolParent;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        #endregion

        #region Private Fields

        // Pool storage
        private Queue<XPGem> availableGems = new Queue<XPGem>();
        private HashSet<XPGem> activeGems = new HashSet<XPGem>();

        // Stats
        private int totalGemsCreated = 0;
        private int peakActiveGems = 0;

        #endregion

        #region Public Properties

        /// <summary>
        /// Number of gems currently available in pool.
        /// </summary>
        public int AvailableCount => availableGems.Count;

        /// <summary>
        /// Number of gems currently active (spawned).
        /// </summary>
        public int ActiveCount => activeGems.Count;

        /// <summary>
        /// Total number of gems created (for stats).
        /// </summary>
        public int TotalCreated => totalGemsCreated;

        /// <summary>
        /// Peak number of active gems (for optimization).
        /// </summary>
        public int PeakActive => peakActiveGems;

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

            // Validate configuration
            if (xpSettings == null)
            {
                Debug.LogError("[XPGemPool] XPSettings not assigned!");
                enabled = false;
                return;
            }

            if (gemPrefab == null)
            {
                Debug.LogError("[XPGemPool] Gem Prefab not assigned!");
                enabled = false;
                return;
            }

            // Validate prefab has XPGem component
            if (gemPrefab.GetComponent<XPGem>() == null)
            {
                Debug.LogError("[XPGemPool] Gem Prefab must have XPGem component!");
                enabled = false;
                return;
            }

            // Create pool parent if not assigned
            if (poolParent == null)
            {
                GameObject poolParentObj = new GameObject("XP_Gem_Pool");
                poolParent = poolParentObj.transform;
                poolParent.SetParent(transform);
            }

            // Pre-spawn initial pool
            InitializePool();
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
        /// Pre-spawns initial pool of gems.
        /// </summary>
        private void InitializePool()
        {
            int initialSize = xpSettings.initialPoolSize;

            for (int i = 0; i < initialSize; i++)
            {
                CreateNewGem();
            }

            if (showDebugInfo)
            {
                Debug.Log($"[XPGemPool] Initialized with {initialSize} gems");
            }
        }

        #endregion

        #region Pool Management

        /// <summary>
        /// Creates a new gem and adds it to the pool.
        /// </summary>
        private XPGem CreateNewGem()
        {
            GameObject gemObj = Instantiate(gemPrefab, poolParent);
            gemObj.SetActive(false);

            XPGem gem = gemObj.GetComponent<XPGem>();
            availableGems.Enqueue(gem);
            totalGemsCreated++;

            return gem;
        }

        /// <summary>
        /// Gets a gem from the pool (or creates new if depleted).
        /// </summary>
        private XPGem GetGem()
        {
            XPGem gem;

            // Try to get from pool
            if (availableGems.Count > 0)
            {
                gem = availableGems.Dequeue();
            }
            else
            {
                // Pool depleted - expand if enabled
                if (xpSettings.autoExpandPool)
                {
                    if (showDebugInfo)
                    {
                        Debug.LogWarning($"[XPGemPool] Pool depleted! Expanding by {xpSettings.poolExpandAmount}");
                    }

                    // Expand pool
                    for (int i = 0; i < xpSettings.poolExpandAmount; i++)
                    {
                        CreateNewGem();
                    }

                    // Try again
                    gem = availableGems.Dequeue();
                }
                else
                {
                    Debug.LogError("[XPGemPool] Pool depleted and auto-expand disabled!");
                    return null;
                }
            }

            // Add to active set
            activeGems.Add(gem);

            // Track peak
            if (activeGems.Count > peakActiveGems)
            {
                peakActiveGems = activeGems.Count;
            }

            return gem;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Spawns an XP gem at specified position with given value.
        /// </summary>
        /// <param name="position">Spawn position</param>
        /// <param name="xpValue">XP value of gem</param>
        /// <returns>The spawned XPGem</returns>
        public XPGem SpawnGem(Vector3 position, int xpValue)
        {
            XPGem gem = GetGem();
            if (gem == null) return null;

            // Add random spawn spread
            if (xpSettings.gemSpawnSpread > 0f)
            {
                Vector2 randomOffset = Random.insideUnitCircle * xpSettings.gemSpawnSpread;
                position += new Vector3(randomOffset.x, 0f, randomOffset.y);
            }

            // Add height offset
            position.y += xpSettings.gemSpawnHeight;

            // Initialize and activate gem
            gem.gameObject.SetActive(true);
            gem.Initialize(position, xpValue, xpSettings);

            if (showDebugInfo)
            {
                Debug.Log($"[XPGemPool] Spawned gem at {position} with {xpValue} XP. Active: {activeGems.Count}");
            }

            return gem;
        }

        /// <summary>
        /// Spawns multiple XP gems at position (useful for high-value enemies).
        /// </summary>
        /// <param name="position">Spawn position</param>
        /// <param name="totalXPValue">Total XP to split into gems</param>
        /// <param name="gemCount">Number of gems to spawn</param>
        public void SpawnMultipleGems(Vector3 position, int totalXPValue, int gemCount)
        {
            int xpPerGem = Mathf.Max(1, totalXPValue / gemCount);
            int remainder = totalXPValue % gemCount;

            for (int i = 0; i < gemCount; i++)
            {
                // Distribute remainder across first few gems
                int gemValue = xpPerGem + (i < remainder ? 1 : 0);
                SpawnGem(position, gemValue);
            }
        }

        /// <summary>
        /// Returns a gem to the pool for reuse.
        /// </summary>
        /// <param name="gem">Gem to return</param>
        public void ReturnGem(XPGem gem)
        {
            if (gem == null) return;

            // Remove from active set
            activeGems.Remove(gem);

            // Deactivate and return to pool
            gem.gameObject.SetActive(false);
            gem.transform.SetParent(poolParent);
            availableGems.Enqueue(gem);

            if (showDebugInfo)
            {
                Debug.Log($"[XPGemPool] Gem returned to pool. Available: {availableGems.Count}");
            }
        }

        /// <summary>
        /// Clears all active gems (useful for scene transitions).
        /// </summary>
        public void ClearAllGems()
        {
            // Copy active gems to list (avoid modifying during iteration)
            List<XPGem> gemsToReturn = new List<XPGem>(activeGems);

            foreach (XPGem gem in gemsToReturn)
            {
                ReturnGem(gem);
            }

            if (showDebugInfo)
            {
                Debug.Log($"[XPGemPool] Cleared all {gemsToReturn.Count} active gems");
            }
        }

        /// <summary>
        /// Resets pool statistics.
        /// </summary>
        public void ResetStats()
        {
            peakActiveGems = 0;

            if (showDebugInfo)
            {
                Debug.Log("[XPGemPool] Stats reset");
            }
        }

        #endregion

        #region Debug Methods

        private void OnGUI()
        {
            if (!showDebugInfo) return;

            GUILayout.BeginArea(new Rect(Screen.width - 310, 220, 300, 180));
            GUILayout.Label("=== XP Gem Pool ===");
            GUILayout.Label($"Available: {availableGems.Count}");
            GUILayout.Label($"Active: {activeGems.Count}");
            GUILayout.Label($"Total Created: {totalGemsCreated}");
            GUILayout.Label($"Peak Active: {peakActiveGems}");

            if (GUILayout.Button("Spawn 10 Gems"))
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Vector3 offset = Random.insideUnitSphere * 5f;
                        offset.y = 0f;
                        SpawnGem(player.transform.position + offset, 25);
                    }
                }
            }

            if (GUILayout.Button("Clear All Gems"))
            {
                ClearAllGems();
            }

            if (GUILayout.Button("Reset Stats"))
            {
                ResetStats();
            }

            GUILayout.EndArea();
        }

        [ContextMenu("Test: Spawn 50 Gems")]
        private void TestSpawn50Gems()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                for (int i = 0; i < 50; i++)
                {
                    Vector3 offset = Random.insideUnitSphere * 10f;
                    offset.y = 0f;
                    SpawnGem(player.transform.position + offset, Random.Range(10, 50));
                }
            }
        }

        [ContextMenu("Test: Clear All Gems")]
        private void TestClearGems()
        {
            ClearAllGems();
        }

        [ContextMenu("Print Pool Stats")]
        private void PrintPoolStats()
        {
            Debug.Log("=== XP Gem Pool Stats ===");
            Debug.Log($"Available Gems: {availableGems.Count}");
            Debug.Log($"Active Gems: {activeGems.Count}");
            Debug.Log($"Total Created: {totalGemsCreated}");
            Debug.Log($"Peak Active: {peakActiveGems}");
            Debug.Log($"Pool Efficiency: {(float)availableGems.Count / totalGemsCreated:P0}");
        }

        #endregion
    }
}
