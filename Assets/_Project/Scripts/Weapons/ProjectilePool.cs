using System.Collections.Generic;
using UnityEngine;

namespace RoombaRampage.Weapons
{
    /// <summary>
    /// Object pool for projectiles to optimize performance in bullet hell scenarios.
    /// Manages spawning and recycling of projectile instances.
    /// Singleton pattern for easy access.
    /// </summary>
    public class ProjectilePool : MonoBehaviour
    {
        #region Singleton

        public static ProjectilePool Instance { get; private set; }

        #endregion

        #region Serialized Fields

        [Header("Pool Configuration")]
        [Tooltip("Initial pool size per projectile type")]
        [SerializeField] private int initialPoolSize = 50;

        [Tooltip("Should pool expand if all projectiles are in use?")]
        [SerializeField] private bool allowPoolExpansion = true;

        [Tooltip("Maximum pool size per type (if expansion is enabled)")]
        [SerializeField] private int maxPoolSize = 200;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        #endregion

        #region Private Fields

        // Pool dictionary: prefab -> list of available instances
        private Dictionary<GameObject, Queue<Projectile>> pools = new Dictionary<GameObject, Queue<Projectile>>();

        // Active projectiles tracking
        private Dictionary<GameObject, HashSet<Projectile>> activeProjectiles = new Dictionary<GameObject, HashSet<Projectile>>();

        // Parent transform for organization
        private Transform poolParent;

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

            // Create parent transform for organization
            poolParent = new GameObject("ProjectilePool_Instances").transform;
            poolParent.SetParent(transform);
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
        /// Gets a projectile from the pool or creates a new one.
        /// </summary>
        /// <param name="prefab">Projectile prefab to spawn</param>
        /// <param name="position">World position to spawn at</param>
        /// <param name="direction">Direction to fire (normalized)</param>
        /// <param name="speed">Projectile speed</param>
        /// <param name="damage">Projectile damage</param>
        /// <param name="lifetime">Projectile lifetime</param>
        /// <returns>Initialized Projectile instance</returns>
        public Projectile GetProjectile(GameObject prefab, Vector3 position, Vector3 direction, float speed, float damage, float lifetime)
        {
            if (prefab == null)
            {
                Debug.LogError("[ProjectilePool] Cannot spawn null prefab!");
                return null;
            }

            // Initialize pool for this prefab if needed
            if (!pools.ContainsKey(prefab))
            {
                InitializePool(prefab);
            }

            Projectile projectile = null;

            // Try to get from pool
            if (pools[prefab].Count > 0)
            {
                projectile = pools[prefab].Dequeue();
            }
            // Create new if pool is empty and expansion is allowed
            else if (allowPoolExpansion && activeProjectiles[prefab].Count < maxPoolSize)
            {
                projectile = CreateProjectileInstance(prefab);

                if (showDebugInfo)
                {
                    Debug.Log($"[ProjectilePool] Expanded pool for {prefab.name}. Active: {activeProjectiles[prefab].Count}");
                }
            }
            else
            {
                if (showDebugInfo)
                {
                    Debug.LogWarning($"[ProjectilePool] Pool exhausted for {prefab.name}. Consider increasing pool size.");
                }
                return null;
            }

            // Activate and initialize projectile
            projectile.gameObject.SetActive(true);
            projectile.transform.position = position;
            projectile.Initialize(direction, speed, damage, lifetime);

            // Track as active
            activeProjectiles[prefab].Add(projectile);

            return projectile;
        }

        /// <summary>
        /// Returns a projectile to the pool for reuse.
        /// </summary>
        /// <param name="projectile">Projectile to return</param>
        public void ReturnProjectile(Projectile projectile)
        {
            if (projectile == null) return;

            // Find which pool this projectile belongs to
            GameObject prefabKey = FindPrefabKey(projectile);
            if (prefabKey == null)
            {
                // If we can't find the pool, just destroy it
                Destroy(projectile.gameObject);
                return;
            }

            // Remove from active tracking
            if (activeProjectiles.ContainsKey(prefabKey))
            {
                activeProjectiles[prefabKey].Remove(projectile);
            }

            // Deactivate and return to pool
            projectile.gameObject.SetActive(false);
            projectile.transform.SetParent(poolParent);
            pools[prefabKey].Enqueue(projectile);
        }

        /// <summary>
        /// Pre-warms the pool for a specific projectile type.
        /// Useful for preventing lag spikes during gameplay.
        /// </summary>
        /// <param name="prefab">Projectile prefab to warm</param>
        /// <param name="count">Number of instances to create</param>
        public void WarmPool(GameObject prefab, int count)
        {
            if (prefab == null) return;

            if (!pools.ContainsKey(prefab))
            {
                InitializePool(prefab);
            }

            for (int i = 0; i < count; i++)
            {
                Projectile projectile = CreateProjectileInstance(prefab);
                projectile.gameObject.SetActive(false);
                pools[prefab].Enqueue(projectile);
            }

            if (showDebugInfo)
            {
                Debug.Log($"[ProjectilePool] Warmed pool for {prefab.name} with {count} instances.");
            }
        }

        /// <summary>
        /// Clears all pools and destroys all projectiles.
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var pool in pools.Values)
            {
                while (pool.Count > 0)
                {
                    var projectile = pool.Dequeue();
                    if (projectile != null)
                    {
                        Destroy(projectile.gameObject);
                    }
                }
            }

            foreach (var activeSet in activeProjectiles.Values)
            {
                foreach (var projectile in activeSet)
                {
                    if (projectile != null)
                    {
                        Destroy(projectile.gameObject);
                    }
                }
                activeSet.Clear();
            }

            pools.Clear();
            activeProjectiles.Clear();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initializes a new pool for a projectile prefab.
        /// </summary>
        private void InitializePool(GameObject prefab)
        {
            pools[prefab] = new Queue<Projectile>();
            activeProjectiles[prefab] = new HashSet<Projectile>();

            // Pre-create initial pool size
            for (int i = 0; i < initialPoolSize; i++)
            {
                Projectile projectile = CreateProjectileInstance(prefab);
                projectile.gameObject.SetActive(false);
                pools[prefab].Enqueue(projectile);
            }

            if (showDebugInfo)
            {
                Debug.Log($"[ProjectilePool] Initialized pool for {prefab.name} with {initialPoolSize} instances.");
            }
        }

        /// <summary>
        /// Creates a new projectile instance.
        /// </summary>
        private Projectile CreateProjectileInstance(GameObject prefab)
        {
            GameObject instance = Instantiate(prefab, poolParent);
            Projectile projectile = instance.GetComponent<Projectile>();

            if (projectile == null)
            {
                Debug.LogError($"[ProjectilePool] Prefab {prefab.name} does not have Projectile component!");
                Destroy(instance);
                return null;
            }

            return projectile;
        }

        /// <summary>
        /// Finds the prefab key for a projectile instance.
        /// </summary>
        private GameObject FindPrefabKey(Projectile projectile)
        {
            foreach (var kvp in activeProjectiles)
            {
                if (kvp.Value.Contains(projectile))
                {
                    return kvp.Key;
                }
            }
            return null;
        }

        #endregion

        #region Debug

        private void OnGUI()
        {
            if (!showDebugInfo) return;

            GUILayout.BeginArea(new Rect(10, 250, 400, 300));
            GUILayout.Label("=== Projectile Pool Status ===");

            foreach (var kvp in pools)
            {
                string prefabName = kvp.Key != null ? kvp.Key.name : "NULL";
                int pooled = kvp.Value.Count;
                int active = activeProjectiles.ContainsKey(kvp.Key) ? activeProjectiles[kvp.Key].Count : 0;
                int total = pooled + active;

                GUILayout.Label($"{prefabName}: {active} active, {pooled} pooled, {total} total");
            }

            GUILayout.EndArea();
        }

        #endregion
    }
}
