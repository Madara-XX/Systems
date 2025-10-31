using UnityEngine;
using System.Collections;

namespace RoombaRampage.Player
{
    /// <summary>
    /// Manages player health, damage, death, and invulnerability.
    /// Broadcasts health events via PlayerEvents ScriptableObject.
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Configuration")]
        [SerializeField] private PlayerStats stats;
        [Tooltip("Optional: Event channel for broadcasting health events")]
        [SerializeField] private PlayerEvents playerEvents;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        #endregion

        #region Private Fields

        // Health state
        private int currentHealth;
        private int maxHealth;
        private bool isInvulnerable;
        private bool isAlive = true;

        // Health regeneration
        private Coroutine regenCoroutine;

        // Invulnerability
        private Coroutine invulnerabilityCoroutine;

        #endregion

        #region Public Properties

        /// <summary>
        /// Current health points (read-only).
        /// </summary>
        public int CurrentHealth => currentHealth;

        /// <summary>
        /// Maximum health points (read-only).
        /// </summary>
        public int MaxHealth => maxHealth;

        /// <summary>
        /// Is the player currently invulnerable? (read-only)
        /// </summary>
        public bool IsInvulnerable => isInvulnerable;

        /// <summary>
        /// Is the player alive? (read-only)
        /// </summary>
        public bool IsAlive => isAlive;

        /// <summary>
        /// Current health as a normalized value (0-1).
        /// </summary>
        public float HealthNormalized => maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Validate configuration
            if (stats == null)
            {
                Debug.LogError($"[PlayerHealth] PlayerStats not assigned on {gameObject.name}. Please assign in Inspector.");
                enabled = false;
                return;
            }

            // Initialize health
            InitializeHealth();
        }

        private void OnEnable()
        {
            // Start health regeneration if configured
            if (stats.healthRegenRate > 0f)
            {
                StartHealthRegeneration();
            }
        }

        private void OnDisable()
        {
            // Stop all coroutines
            StopAllCoroutines();
            regenCoroutine = null;
            invulnerabilityCoroutine = null;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes health from PlayerStats.
        /// </summary>
        private void InitializeHealth()
        {
            maxHealth = stats.maxHealth;
            currentHealth = maxHealth;
            isInvulnerable = false;
            isAlive = true;

            // Broadcast initial health
            BroadcastHealthChanged();
        }

        #endregion

        #region Damage & Healing

        /// <summary>
        /// Applies damage to the player.
        /// Ignores damage if invulnerable or dead.
        /// </summary>
        /// <param name="amount">Damage amount</param>
        public void TakeDamage(int amount)
        {
            // Ignore damage if invulnerable or dead
            if (isInvulnerable || !isAlive)
            {
                return;
            }

            // Apply damage
            currentHealth -= amount;
            currentHealth = Mathf.Max(0, currentHealth);

            // Broadcast damage event
            BroadcastDamaged(amount);

            // Check for death
            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                // Start invulnerability period
                StartInvulnerability(stats.invulnerabilityDuration);
            }

            // Debug log
            if (showDebugInfo)
            {
                Debug.Log($"[PlayerHealth] Took {amount} damage. Health: {currentHealth}/{maxHealth}");
            }
        }

        /// <summary>
        /// Heals the player by specified amount.
        /// Cannot exceed max health.
        /// </summary>
        /// <param name="amount">Heal amount</param>
        public void Heal(int amount)
        {
            // Ignore healing if dead
            if (!isAlive) return;

            // Store previous health for event
            int previousHealth = currentHealth;

            // Apply healing
            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);

            // Only broadcast if health actually changed
            if (currentHealth != previousHealth)
            {
                BroadcastHealed(amount);

                // Debug log
                if (showDebugInfo)
                {
                    Debug.Log($"[PlayerHealth] Healed {amount}. Health: {currentHealth}/{maxHealth}");
                }
            }
        }

        /// <summary>
        /// Resets health to maximum.
        /// Used for respawn/level start.
        /// </summary>
        public void ResetHealth()
        {
            currentHealth = maxHealth;
            isInvulnerable = false;
            isAlive = true;

            // Stop any active coroutines
            if (invulnerabilityCoroutine != null)
            {
                StopCoroutine(invulnerabilityCoroutine);
                invulnerabilityCoroutine = null;
            }

            // Broadcast health changed
            BroadcastHealthChanged();

            // Broadcast respawn event
            if (playerEvents != null)
            {
                playerEvents.RaiseRespawned();
            }

            // Debug log
            if (showDebugInfo)
            {
                Debug.Log($"[PlayerHealth] Health reset to {currentHealth}/{maxHealth}");
            }
        }

        #endregion

        #region Death

        /// <summary>
        /// Handles player death.
        /// </summary>
        private void Die()
        {
            if (!isAlive) return; // Already dead

            isAlive = false;
            currentHealth = 0;

            // Broadcast death event
            BroadcastDied();

            // Debug log
            if (showDebugInfo)
            {
                Debug.Log("[PlayerHealth] Player died!");
            }
        }

        #endregion

        #region Invulnerability

        /// <summary>
        /// Sets invulnerability state manually.
        /// Useful for cutscenes, power-ups, etc.
        /// </summary>
        /// <param name="state">Invulnerable state</param>
        public void SetInvulnerable(bool state)
        {
            isInvulnerable = state;

            // Stop invulnerability coroutine if disabling
            if (!state && invulnerabilityCoroutine != null)
            {
                StopCoroutine(invulnerabilityCoroutine);
                invulnerabilityCoroutine = null;
            }
        }

        /// <summary>
        /// Starts temporary invulnerability for specified duration.
        /// </summary>
        /// <param name="duration">Invulnerability duration in seconds</param>
        public void StartInvulnerability(float duration)
        {
            // Stop existing invulnerability if any
            if (invulnerabilityCoroutine != null)
            {
                StopCoroutine(invulnerabilityCoroutine);
            }

            // Start new invulnerability period
            invulnerabilityCoroutine = StartCoroutine(InvulnerabilityCoroutine(duration));
        }

        /// <summary>
        /// Coroutine for temporary invulnerability.
        /// </summary>
        private IEnumerator InvulnerabilityCoroutine(float duration)
        {
            isInvulnerable = true;

            yield return new WaitForSeconds(duration);

            isInvulnerable = false;
            invulnerabilityCoroutine = null;
        }

        #endregion

        #region Health Regeneration

        /// <summary>
        /// Starts health regeneration coroutine.
        /// </summary>
        private void StartHealthRegeneration()
        {
            if (regenCoroutine != null)
            {
                StopCoroutine(regenCoroutine);
            }

            regenCoroutine = StartCoroutine(HealthRegenerationCoroutine());
        }

        /// <summary>
        /// Coroutine for continuous health regeneration.
        /// </summary>
        private IEnumerator HealthRegenerationCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);

                // Only regenerate if alive and not at full health
                if (isAlive && currentHealth < maxHealth)
                {
                    int regenAmount = Mathf.RoundToInt(stats.healthRegenRate);
                    if (regenAmount > 0)
                    {
                        Heal(regenAmount);
                    }
                }
            }
        }

        #endregion

        #region Event Broadcasting

        /// <summary>
        /// Broadcasts health changed event.
        /// </summary>
        private void BroadcastHealthChanged()
        {
            if (playerEvents != null)
            {
                playerEvents.RaiseDamaged(currentHealth, maxHealth);
            }
        }

        /// <summary>
        /// Broadcasts damage taken event.
        /// </summary>
        private void BroadcastDamaged(int damageAmount)
        {
            if (playerEvents != null)
            {
                playerEvents.RaiseDamaged(currentHealth, maxHealth);
            }
        }

        /// <summary>
        /// Broadcasts healing received event.
        /// </summary>
        private void BroadcastHealed(int healAmount)
        {
            if (playerEvents != null)
            {
                playerEvents.RaiseHealed(currentHealth, maxHealth);
            }
        }

        /// <summary>
        /// Broadcasts death event.
        /// </summary>
        private void BroadcastDied()
        {
            if (playerEvents != null)
            {
                playerEvents.RaiseDied();
            }
        }

        #endregion

        #region Collision Handling

        /// <summary>
        /// Handles 3D collision with damaging objects (optional).
        /// Tag objects with "Enemy" or "EnemyProjectile" to auto-damage.
        /// </summary>
        private void OnCollisionEnter(Collision collision)
        {
            // Check for enemy collision (safe - won't crash if tag doesn't exist)
            if (collision.gameObject.tag == "Enemy")
            {
                // Take damage (amount can be configurable per enemy)
                TakeDamage(10);
            }

            // Check for enemy projectile
            if (collision.gameObject.tag == "EnemyProjectile")
            {
                // Take damage (amount can be configurable per projectile)
                TakeDamage(5);
            }
        }

        #endregion

        #region Debug Methods

        /// <summary>
        /// Debug method to test damage.
        /// </summary>
        [ContextMenu("Test: Take 10 Damage")]
        private void TestTakeDamage()
        {
            TakeDamage(10);
        }

        /// <summary>
        /// Debug method to test healing.
        /// </summary>
        [ContextMenu("Test: Heal 20 HP")]
        private void TestHeal()
        {
            Heal(20);
        }

        /// <summary>
        /// Debug method to test death.
        /// </summary>
        [ContextMenu("Test: Kill Player")]
        private void TestDeath()
        {
            TakeDamage(currentHealth);
        }

        /// <summary>
        /// Debug method to reset health.
        /// </summary>
        [ContextMenu("Test: Reset Health")]
        private void TestResetHealth()
        {
            ResetHealth();
        }

        private void OnGUI()
        {
            if (!showDebugInfo) return;

            // Display health state on screen
            GUILayout.BeginArea(new Rect(10, 220, 300, 150));
            GUILayout.Label($"Health: {currentHealth}/{maxHealth}");
            GUILayout.Label($"Health %: {HealthNormalized:P0}");
            GUILayout.Label($"Is Alive: {isAlive}");
            GUILayout.Label($"Is Invulnerable: {isInvulnerable}");
            GUILayout.EndArea();
        }

        #endregion

        #region Validation

        private void OnValidate()
        {
            // Ensure PlayerStats is assigned
            if (stats == null)
            {
                Debug.LogWarning($"[PlayerHealth] PlayerStats not assigned on {gameObject.name}.");
            }
        }

        #endregion
    }
}
