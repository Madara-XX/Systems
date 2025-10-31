using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoombaRampage.Enemies
{
    /// <summary>
    /// Spawns enemies at random positions around the arena.
    /// Simple wave system for testing and MVP gameplay.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Spawn Configuration")]
        [Tooltip("Enemy prefabs to spawn (each prefab should have Enemy.cs with EnemyData assigned)")]
        [SerializeField] private List<GameObject> enemyPrefabs = new List<GameObject>();

        [Tooltip("Number of enemies to spawn per wave")]
        [SerializeField] private int enemiesPerWave = 5;

        [Tooltip("Time between waves (seconds)")]
        [SerializeField] private float timeBetweenWaves = 10f;

        [Tooltip("Should spawn waves automatically?")]
        [SerializeField] private bool autoSpawn = true;

        [Header("Spawn Area")]
        [Tooltip("Spawn enemies in a circle around this transform")]
        [SerializeField] private Transform spawnCenter;

        [Tooltip("Minimum spawn distance from center")]
        [SerializeField] private float minSpawnRadius = 10f;

        [Tooltip("Maximum spawn distance from center")]
        [SerializeField] private float maxSpawnRadius = 20f;

        [Tooltip("Height offset for spawned enemies (Y position)")]
        [SerializeField] private float spawnHeight = 0f;

        [Header("Wave Progression")]
        [Tooltip("Should waves get harder over time?")]
        [SerializeField] private bool enableProgression = true;

        [Tooltip("Enemy count increase per wave")]
        [SerializeField] private int enemyIncreasePerWave = 2;

        [Tooltip("Maximum enemies per wave")]
        [SerializeField] private int maxEnemiesPerWave = 50;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        #endregion

        #region Private Fields

        private int currentWave = 0;
        private int currentEnemyCount = 0;
        private bool isSpawning = false;
        private List<Enemy> activeEnemies = new List<Enemy>();

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            // Use this transform as spawn center if not set
            if (spawnCenter == null)
            {
                spawnCenter = transform;
            }

            // Validate enemy prefabs
            if (enemyPrefabs.Count == 0)
            {
                Debug.LogWarning("[EnemySpawner] No enemy prefabs assigned! Add enemy prefabs to the list.");
            }

            // Start auto-spawning if enabled
            if (autoSpawn)
            {
                StartCoroutine(AutoSpawnWaves());
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Manually spawns the next wave.
        /// </summary>
        public void SpawnWave()
        {
            if (isSpawning)
            {
                Debug.LogWarning("[EnemySpawner] Already spawning a wave!");
                return;
            }

            StartCoroutine(SpawnWaveCoroutine());
        }

        /// <summary>
        /// Spawns a specific enemy prefab at a random position.
        /// </summary>
        /// <param name="enemyPrefab">Enemy prefab to spawn</param>
        /// <returns>Spawned enemy instance</returns>
        public Enemy SpawnEnemy(GameObject enemyPrefab)
        {
            if (enemyPrefab == null)
            {
                Debug.LogError("[EnemySpawner] Cannot spawn enemy: null prefab!");
                return null;
            }

            // Get random spawn position
            Vector3 spawnPosition = GetRandomSpawnPosition();

            // Instantiate enemy
            GameObject enemyObj = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            Enemy enemy = enemyObj.GetComponent<Enemy>();

            if (enemy != null)
            {
                // Enemy already has EnemyData assigned, no need to initialize
                activeEnemies.Add(enemy);
                currentEnemyCount++;

                // Subscribe to death event to track active enemies
                var enemyHealth = enemy.GetHealth();
                if (enemyHealth != null)
                {
                    enemyHealth.OnDeath.AddListener(() => OnEnemyDied(enemy));
                }

                if (showDebugInfo)
                {
                    Debug.Log($"[EnemySpawner] Spawned {enemyPrefab.name} at {spawnPosition}");
                }
            }
            else
            {
                Debug.LogError($"[EnemySpawner] Enemy prefab {enemyPrefab.name} does not have Enemy component!");
                Destroy(enemyObj);
            }

            return enemy;
        }

        /// <summary>
        /// Clears all active enemies.
        /// </summary>
        public void ClearAllEnemies()
        {
            foreach (Enemy enemy in activeEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy.gameObject);
                }
            }

            activeEnemies.Clear();
            currentEnemyCount = 0;
        }

        /// <summary>
        /// Gets the current wave number.
        /// </summary>
        public int GetCurrentWave() => currentWave;

        /// <summary>
        /// Gets the number of active enemies.
        /// </summary>
        public int GetActiveEnemyCount() => currentEnemyCount;

        #endregion

        #region Private Methods

        /// <summary>
        /// Auto-spawns waves with delay between each.
        /// </summary>
        private IEnumerator AutoSpawnWaves()
        {
            while (true)
            {
                // Spawn wave
                yield return StartCoroutine(SpawnWaveCoroutine());

                // Wait between waves
                yield return new WaitForSeconds(timeBetweenWaves);
            }
        }

        /// <summary>
        /// Spawns a wave of enemies.
        /// </summary>
        private IEnumerator SpawnWaveCoroutine()
        {
            isSpawning = true;
            currentWave++;

            // Calculate enemies for this wave
            int enemiesToSpawn = enemiesPerWave;

            if (enableProgression)
            {
                enemiesToSpawn = Mathf.Min(
                    enemiesPerWave + (currentWave - 1) * enemyIncreasePerWave,
                    maxEnemiesPerWave
                );
            }

            if (showDebugInfo)
            {
                Debug.Log($"[EnemySpawner] Starting wave {currentWave}: {enemiesToSpawn} enemies");
            }

            // Spawn enemies with slight delay for performance
            for (int i = 0; i < enemiesToSpawn; i++)
            {
                // Pick random enemy prefab
                GameObject randomPrefab = GetRandomEnemyPrefab();

                if (randomPrefab != null)
                {
                    SpawnEnemy(randomPrefab);
                }

                // Small delay between spawns to avoid lag spikes
                yield return new WaitForSeconds(0.1f);
            }

            isSpawning = false;
        }

        /// <summary>
        /// Gets a random enemy prefab from the list.
        /// </summary>
        private GameObject GetRandomEnemyPrefab()
        {
            if (enemyPrefabs.Count == 0) return null;

            int randomIndex = Random.Range(0, enemyPrefabs.Count);
            return enemyPrefabs[randomIndex];
        }

        /// <summary>
        /// Gets a random spawn position on XZ plane around the spawn center.
        /// </summary>
        private Vector3 GetRandomSpawnPosition()
        {
            // Random angle
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

            // Random distance between min and max radius
            float distance = Random.Range(minSpawnRadius, maxSpawnRadius);

            // Calculate position on XZ plane
            float x = Mathf.Cos(angle) * distance;
            float z = Mathf.Sin(angle) * distance;

            Vector3 spawnPosition = spawnCenter.position + new Vector3(x, spawnHeight, z);

            return spawnPosition;
        }

        /// <summary>
        /// Called when an enemy dies.
        /// </summary>
        private void OnEnemyDied(Enemy enemy)
        {
            if (enemy != null)
            {
                activeEnemies.Remove(enemy);
                currentEnemyCount--;

                if (showDebugInfo)
                {
                    Debug.Log($"[EnemySpawner] Enemy died. Remaining: {currentEnemyCount}");
                }
            }
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            if (spawnCenter == null) spawnCenter = transform;

            // Draw spawn area circles
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            DrawCircleXZ(spawnCenter.position, minSpawnRadius);

            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            DrawCircleXZ(spawnCenter.position, maxSpawnRadius);
        }

        private void DrawCircleXZ(Vector3 center, float radius, int segments = 64)
        {
            float angleStep = 360f / segments;

            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * angleStep * Mathf.Deg2Rad;
                float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

                Vector3 point1 = center + new Vector3(Mathf.Cos(angle1) * radius, spawnHeight, Mathf.Sin(angle1) * radius);
                Vector3 point2 = center + new Vector3(Mathf.Cos(angle2) * radius, spawnHeight, Mathf.Sin(angle2) * radius);

                Gizmos.DrawLine(point1, point2);
            }
        }

        private void OnGUI()
        {
            if (!showDebugInfo) return;

            GUILayout.BeginArea(new Rect(Screen.width - 310, 10, 300, 150));
            GUILayout.Label("=== Enemy Spawner ===");
            GUILayout.Label($"Current Wave: {currentWave}");
            GUILayout.Label($"Active Enemies: {currentEnemyCount}");
            GUILayout.Label($"Is Spawning: {isSpawning}");

            if (GUILayout.Button("Spawn Wave Now"))
            {
                SpawnWave();
            }

            if (GUILayout.Button("Clear All Enemies"))
            {
                ClearAllEnemies();
            }

            GUILayout.EndArea();
        }

        #endregion
    }
}
