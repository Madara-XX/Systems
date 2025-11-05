using UnityEngine;
using UnityEngine.Events;

namespace RoombaRampage.Enemies
{
    /// <summary>
    /// Manages enemy health, damage, and death.
    /// Attached to enemy GameObjects.
    /// </summary>
    public class EnemyHealth : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Configuration")]
        [Tooltip("Enemy data containing max health and other stats")]
        [SerializeField] private EnemyData enemyData;

        [Header("Event References")]
        [Tooltip("PlayerEvents ScriptableObject for kill notifications (optional, auto-finds)")]
        [SerializeField] private Player.PlayerEvents playerEvents;

        [Header("Events")]
        [Tooltip("Event invoked when enemy takes damage")]
        public UnityEvent<float> OnDamageTaken;

        [Tooltip("Event invoked when enemy dies")]
        public UnityEvent OnDeath;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        #endregion

        #region Private Fields

        private float currentHealth;
        private bool isDead = false;

        #endregion

        #region Public Properties

        /// <summary>
        /// Current health of the enemy.
        /// </summary>
        public float CurrentHealth => currentHealth;

        /// <summary>
        /// Maximum health of the enemy.
        /// </summary>
        public float MaxHealth => enemyData != null ? enemyData.maxHealth : 0f;

        /// <summary>
        /// Health as a normalized value (0-1).
        /// </summary>
        public float HealthPercent => MaxHealth > 0f ? currentHealth / MaxHealth : 0f;

        /// <summary>
        /// Is the enemy dead?
        /// </summary>
        public bool IsDead => isDead;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (enemyData == null)
            {
                Debug.LogError("[EnemyHealth] No EnemyData assigned! Enemy needs EnemyData to function.", this);
                enabled = false;
                return;
            }

            // Try to find PlayerEvents if not assigned
            if (playerEvents == null)
            {
                playerEvents = Resources.Load<Player.PlayerEvents>("PlayerEvents");

                if (playerEvents == null)
                {
                    Debug.LogWarning("[EnemyHealth] PlayerEvents not found. Kill counter will not update. Assign PlayerEvents in Inspector or place in Resources folder.");
                }
            }

            Initialize();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes health to maximum.
        /// Call this when spawning/resetting enemy.
        /// </summary>
        public void Initialize()
        {
            if (enemyData == null) return;

            currentHealth = enemyData.maxHealth;
            isDead = false;

            if (showDebugInfo)
            {
                Debug.Log($"[EnemyHealth] {gameObject.name} initialized with {currentHealth} HP");
            }
        }

        /// <summary>
        /// Deals damage to the enemy.
        /// </summary>
        /// <param name="damage">Amount of damage to deal</param>
        public void TakeDamage(float damage)
        {
            if (isDead) return;

            if (damage < 0f)
            {
                Debug.LogWarning($"[EnemyHealth] Negative damage value: {damage}. Use Heal() instead.");
                return;
            }

            currentHealth -= damage;

            if (showDebugInfo)
            {
                Debug.Log($"[EnemyHealth] {gameObject.name} took {damage} damage. Health: {currentHealth}/{MaxHealth}");
            }

            // Invoke damage event
            OnDamageTaken?.Invoke(damage);

            // Check for death
            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        /// <summary>
        /// Heals the enemy.
        /// </summary>
        /// <param name="amount">Amount to heal</param>
        public void Heal(float amount)
        {
            if (isDead) return;

            currentHealth = Mathf.Min(currentHealth + amount, MaxHealth);

            if (showDebugInfo)
            {
                Debug.Log($"[EnemyHealth] {gameObject.name} healed {amount}. Health: {currentHealth}/{MaxHealth}");
            }
        }

        /// <summary>
        /// Instantly kills the enemy.
        /// </summary>
        public void Kill()
        {
            if (isDead) return;

            currentHealth = 0f;
            Die();
        }

        /// <summary>
        /// Sets enemy data (useful for pooled enemies).
        /// </summary>
        /// <param name="data">Enemy data to use</param>
        public void SetEnemyData(EnemyData data)
        {
            enemyData = data;
            Initialize();
        }

        /// <summary>
        /// Sets the PlayerEvents reference (optional).
        /// </summary>
        /// <param name="events">PlayerEvents ScriptableObject</param>
        public void SetPlayerEvents(Player.PlayerEvents events)
        {
            playerEvents = events;

            if (showDebugInfo)
            {
                Debug.Log($"[EnemyHealth] PlayerEvents set to: {events?.name ?? "null"}");
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles enemy death.
        /// </summary>
        private void Die()
        {
            if (isDead) return;

            isDead = true;
            currentHealth = 0f;

            if (showDebugInfo)
            {
                Debug.Log($"[EnemyHealth] {gameObject.name} died!");
            }

            // Get score value for events
            int scoreValue = enemyData?.scoreValue ?? 0;

            // Award score
            if (scoreValue > 0)
            {
                if (Managers.ScoreManager.Instance != null)
                {
                    Managers.ScoreManager.Instance.AddScore(scoreValue);
                }
            }

            // Raise enemy killed event for kill counter
            if (playerEvents != null)
            {
                playerEvents.RaiseEnemyKilled(scoreValue);

                if (showDebugInfo)
                {
                    Debug.Log($"[EnemyHealth] Raised OnEnemyKilled event with score value: {scoreValue}");
                }
            }
            else if (showDebugInfo)
            {
                Debug.LogWarning("[EnemyHealth] PlayerEvents is null! Kill counter will not update.");
            }

            // Invoke death event
            OnDeath?.Invoke();

            // Destroy or return to pool
            // (Enemy.cs will handle the actual destruction)
        }

        #endregion

        #region Debug

        private void OnGUI()
        {
            if (!showDebugInfo || !Application.isPlaying) return;

            // Draw health bar above enemy
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);

            if (screenPos.z > 0)
            {
                float barWidth = 100f;
                float barHeight = 10f;
                Rect barRect = new Rect(screenPos.x - barWidth / 2f, Screen.height - screenPos.y, barWidth, barHeight);

                // Background
                GUI.color = Color.black;
                GUI.DrawTexture(barRect, Texture2D.whiteTexture);

                // Health bar
                GUI.color = Color.Lerp(Color.red, Color.green, HealthPercent);
                Rect healthRect = new Rect(barRect.x, barRect.y, barRect.width * HealthPercent, barRect.height);
                GUI.DrawTexture(healthRect, Texture2D.whiteTexture);

                // Reset color
                GUI.color = Color.white;
            }
        }

        #endregion
    }
}
