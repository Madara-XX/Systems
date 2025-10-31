using UnityEngine;
using RoombaRampage.Player;

namespace RoombaRampage.Enemies
{
    /// <summary>
    /// Main enemy controller that integrates health, AI, and collision damage.
    /// Attach to enemy GameObjects along with EnemyHealth and EnemyAI.
    /// Requires "Enemy" tag.
    /// </summary>
    [RequireComponent(typeof(EnemyHealth))]
    [RequireComponent(typeof(EnemyAI))]
    public class Enemy : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Configuration")]
        [Tooltip("Enemy data containing all stats")]
        [SerializeField] private EnemyData enemyData;

        [Header("Collision Damage")]
        [Tooltip("Layer mask for collision damage (should include Player layer)")]
        [SerializeField] private LayerMask damageLayerMask = -1; // All layers by default

        [Header("Death Behavior")]
        [Tooltip("Delay before destroying enemy after death (for death animation)")]
        [SerializeField] private float deathDestroyDelay = 0.5f;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        #endregion

        #region Private Fields

        private EnemyHealth enemyHealth;
        private EnemyAI enemyAI;
        private float lastDamageTime;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Get components
            enemyHealth = GetComponent<EnemyHealth>();
            enemyAI = GetComponent<EnemyAI>();

            // Validate tag
            if (!CompareTag("Enemy"))
            {
                Debug.LogWarning($"[Enemy] {gameObject.name} does not have 'Enemy' tag! Setting tag now.", this);
                gameObject.tag = "Enemy";
            }

            // Validate enemy data
            if (enemyData == null)
            {
                Debug.LogError("[Enemy] No EnemyData assigned! Enemy needs EnemyData to function.", this);
                enabled = false;
                return;
            }

            // Set enemy data on components
            if (enemyHealth != null)
            {
                enemyHealth.SetEnemyData(enemyData);
            }

            if (enemyAI != null)
            {
                enemyAI.SetEnemyData(enemyData);
            }

            // Subscribe to death event
            if (enemyHealth != null)
            {
                enemyHealth.OnDeath.AddListener(OnDeath);
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (enemyHealth != null)
            {
                enemyHealth.OnDeath.RemoveListener(OnDeath);
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            // Check if can deal collision damage
            if (enemyHealth.IsDead) return;
            if (Time.time - lastDamageTime < enemyData.damageCooldown) return;

            // Check if hit player
            if (collision.gameObject.CompareTag("Player"))
            {
                // Try to damage player
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

                if (playerHealth != null && enemyData.collisionDamage > 0f)
                {
                    playerHealth.TakeDamage(Mathf.RoundToInt(enemyData.collisionDamage));
                    lastDamageTime = Time.time;

                    if (showDebugInfo)
                    {
                        Debug.Log($"[Enemy] {gameObject.name} dealt {enemyData.collisionDamage} collision damage to player");
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes enemy (useful for pooled enemies).
        /// </summary>
        /// <param name="data">Enemy data to use</param>
        public void Initialize(EnemyData data)
        {
            enemyData = data;

            if (enemyHealth != null)
            {
                enemyHealth.SetEnemyData(data);
            }

            if (enemyAI != null)
            {
                enemyAI.SetEnemyData(data);
                enemyAI.EnableAI();
            }

            lastDamageTime = 0f;
        }

        /// <summary>
        /// Gets the enemy data.
        /// </summary>
        public EnemyData GetEnemyData() => enemyData;

        /// <summary>
        /// Gets the enemy health component.
        /// </summary>
        public EnemyHealth GetHealth() => enemyHealth;

        /// <summary>
        /// Gets the enemy AI component.
        /// </summary>
        public EnemyAI GetAI() => enemyAI;

        #endregion

        #region Private Methods

        /// <summary>
        /// Called when enemy dies.
        /// </summary>
        private void OnDeath()
        {
            if (showDebugInfo)
            {
                Debug.Log($"[Enemy] {gameObject.name} ({enemyData.enemyName}) died!");
            }

            // Disable AI
            if (enemyAI != null)
            {
                enemyAI.DisableAI();
            }

            // Disable collision
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.enabled = false;
            }

            // Spawn XP gem
            if (enemyData != null && enemyData.xpValue > 0 && Progression.XPGemPool.Instance != null)
            {
                Progression.XPGemPool.Instance.SpawnGem(transform.position, enemyData.xpValue);
            }

            // TODO: Play death animation/effects here

            // Destroy after delay
            Destroy(gameObject, deathDestroyDelay);
        }

        #endregion

        #region Debug

        private void OnGUI()
        {
            if (!showDebugInfo || !Application.isPlaying) return;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 3f);

            if (screenPos.z > 0)
            {
                Vector2 labelSize = new Vector2(200f, 40f);
                Rect labelRect = new Rect(screenPos.x - labelSize.x / 2f, Screen.height - screenPos.y, labelSize.x, labelSize.y);

                GUI.Label(labelRect, $"{enemyData.enemyName}\nHP: {enemyHealth.CurrentHealth:F0}/{enemyHealth.MaxHealth:F0}");
            }
        }

        #endregion
    }
}
