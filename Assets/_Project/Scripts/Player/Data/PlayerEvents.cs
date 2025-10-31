using UnityEngine;
using UnityEngine.Events;

namespace RoombaRampage.Player
{
    /// <summary>
    /// ScriptableObject event channel for player-related events.
    /// Provides decoupled communication between player systems and other game systems.
    /// Create instance via: Right-click > Create > RoombaRampage > Player Events
    ///
    /// Usage:
    /// - Create a single instance in Assets/_Project/Data/Events/
    /// - Reference this asset in all systems that need player events
    /// - Systems raise events via RaiseX() methods
    /// - Other systems subscribe to events in OnEnable/OnDisable
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerEvents", menuName = "RoombaRampage/Player Events", order = 2)]
    public class PlayerEvents : ScriptableObject
    {
        #region Health Events

        /// <summary>
        /// Invoked when player takes damage or health changes.
        /// Parameters: (currentHealth, maxHealth)
        /// </summary>
        public UnityAction<int, int> OnPlayerDamaged;

        /// <summary>
        /// Invoked when player receives healing.
        /// Parameters: (currentHealth, maxHealth)
        /// </summary>
        public UnityAction<int, int> OnPlayerHealed;

        /// <summary>
        /// Invoked when player dies (health reaches 0).
        /// </summary>
        public UnityAction OnPlayerDied;

        /// <summary>
        /// Invoked when player respawns or health is reset.
        /// </summary>
        public UnityAction OnPlayerRespawned;

        #endregion

        #region Movement Events

        /// <summary>
        /// Invoked when player speed changes.
        /// Parameter: (currentSpeed)
        /// </summary>
        public UnityAction<float> OnSpeedChanged;

        /// <summary>
        /// Invoked when player position changes significantly.
        /// Parameter: (newPosition)
        /// </summary>
        public UnityAction<Vector2> OnPositionChanged;

        #endregion

        #region Combat Events (Future)

        /// <summary>
        /// Invoked when player fires a weapon.
        /// </summary>
        public UnityAction OnPlayerAttack;

        /// <summary>
        /// Invoked when player kills an enemy.
        /// Parameter: (enemy score value)
        /// </summary>
        public UnityAction<int> OnEnemyKilled;

        #endregion

        #region Upgrade Events (Future)

        /// <summary>
        /// Invoked when player collects an upgrade.
        /// Parameter: (upgrade name/type)
        /// </summary>
        public UnityAction<string> OnUpgradeCollected;

        /// <summary>
        /// Invoked when player collects a power-up.
        /// Parameter: (power-up name/type)
        /// </summary>
        public UnityAction<string> OnPowerUpCollected;

        #endregion

        #region Score Events (Future)

        /// <summary>
        /// Invoked when player score changes.
        /// Parameter: (newScore)
        /// </summary>
        public UnityAction<int> OnScoreChanged;

        #endregion

        #region Helper Methods - Health

        /// <summary>
        /// Raises the OnPlayerDamaged event.
        /// </summary>
        /// <param name="currentHealth">Current health value</param>
        /// <param name="maxHealth">Maximum health value</param>
        public void RaiseDamaged(int currentHealth, int maxHealth)
        {
            OnPlayerDamaged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// Raises the OnPlayerHealed event.
        /// </summary>
        /// <param name="currentHealth">Current health value</param>
        /// <param name="maxHealth">Maximum health value</param>
        public void RaiseHealed(int currentHealth, int maxHealth)
        {
            OnPlayerHealed?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// Raises the OnPlayerDied event.
        /// </summary>
        public void RaiseDied()
        {
            OnPlayerDied?.Invoke();
        }

        /// <summary>
        /// Raises the OnPlayerRespawned event.
        /// </summary>
        public void RaiseRespawned()
        {
            OnPlayerRespawned?.Invoke();
        }

        #endregion

        #region Helper Methods - Movement

        /// <summary>
        /// Raises the OnSpeedChanged event.
        /// </summary>
        /// <param name="currentSpeed">Current speed value</param>
        public void RaiseSpeedChanged(float currentSpeed)
        {
            OnSpeedChanged?.Invoke(currentSpeed);
        }

        /// <summary>
        /// Raises the OnPositionChanged event.
        /// </summary>
        /// <param name="newPosition">New position</param>
        public void RaisePositionChanged(Vector2 newPosition)
        {
            OnPositionChanged?.Invoke(newPosition);
        }

        #endregion

        #region Helper Methods - Combat

        /// <summary>
        /// Raises the OnPlayerAttack event.
        /// </summary>
        public void RaisePlayerAttack()
        {
            OnPlayerAttack?.Invoke();
        }

        /// <summary>
        /// Raises the OnEnemyKilled event.
        /// </summary>
        /// <param name="scoreValue">Score value of killed enemy</param>
        public void RaiseEnemyKilled(int scoreValue)
        {
            OnEnemyKilled?.Invoke(scoreValue);
        }

        #endregion

        #region Helper Methods - Upgrades

        /// <summary>
        /// Raises the OnUpgradeCollected event.
        /// </summary>
        /// <param name="upgradeName">Name or type of upgrade</param>
        public void RaiseUpgradeCollected(string upgradeName)
        {
            OnUpgradeCollected?.Invoke(upgradeName);
        }

        /// <summary>
        /// Raises the OnPowerUpCollected event.
        /// </summary>
        /// <param name="powerUpName">Name or type of power-up</param>
        public void RaisePowerUpCollected(string powerUpName)
        {
            OnPowerUpCollected?.Invoke(powerUpName);
        }

        #endregion

        #region Helper Methods - Score

        /// <summary>
        /// Raises the OnScoreChanged event.
        /// </summary>
        /// <param name="newScore">New score value</param>
        public void RaiseScoreChanged(int newScore)
        {
            OnScoreChanged?.Invoke(newScore);
        }

        #endregion

        #region Debug Methods

        /// <summary>
        /// Clears all event subscriptions.
        /// Useful for cleanup during testing.
        /// </summary>
        [ContextMenu("Clear All Event Subscriptions")]
        public void ClearAllSubscriptions()
        {
            OnPlayerDamaged = null;
            OnPlayerHealed = null;
            OnPlayerDied = null;
            OnPlayerRespawned = null;

            OnSpeedChanged = null;
            OnPositionChanged = null;

            OnPlayerAttack = null;
            OnEnemyKilled = null;

            OnUpgradeCollected = null;
            OnPowerUpCollected = null;

            OnScoreChanged = null;

            Debug.Log("[PlayerEvents] All event subscriptions cleared.");
        }

        /// <summary>
        /// Logs the number of subscribers for each event.
        /// Useful for debugging event subscription issues.
        /// </summary>
        [ContextMenu("Log Subscriber Counts")]
        public void LogSubscriberCounts()
        {
            Debug.Log("=== PlayerEvents Subscriber Counts ===");
            Debug.Log($"OnPlayerDamaged: {OnPlayerDamaged?.GetInvocationList().Length ?? 0}");
            Debug.Log($"OnPlayerHealed: {OnPlayerHealed?.GetInvocationList().Length ?? 0}");
            Debug.Log($"OnPlayerDied: {OnPlayerDied?.GetInvocationList().Length ?? 0}");
            Debug.Log($"OnPlayerRespawned: {OnPlayerRespawned?.GetInvocationList().Length ?? 0}");
            Debug.Log($"OnSpeedChanged: {OnSpeedChanged?.GetInvocationList().Length ?? 0}");
            Debug.Log($"OnPositionChanged: {OnPositionChanged?.GetInvocationList().Length ?? 0}");
            Debug.Log($"OnPlayerAttack: {OnPlayerAttack?.GetInvocationList().Length ?? 0}");
            Debug.Log($"OnEnemyKilled: {OnEnemyKilled?.GetInvocationList().Length ?? 0}");
            Debug.Log($"OnUpgradeCollected: {OnUpgradeCollected?.GetInvocationList().Length ?? 0}");
            Debug.Log($"OnPowerUpCollected: {OnPowerUpCollected?.GetInvocationList().Length ?? 0}");
            Debug.Log($"OnScoreChanged: {OnScoreChanged?.GetInvocationList().Length ?? 0}");
        }

        /// <summary>
        /// Test method to raise a damage event.
        /// </summary>
        [ContextMenu("Test: Raise Damage Event (50/100)")]
        public void TestRaiseDamageEvent()
        {
            RaiseDamaged(50, 100);
            Debug.Log("[PlayerEvents] Test damage event raised: 50/100 HP");
        }

        /// <summary>
        /// Test method to raise a death event.
        /// </summary>
        [ContextMenu("Test: Raise Death Event")]
        public void TestRaiseDeathEvent()
        {
            RaiseDied();
            Debug.Log("[PlayerEvents] Test death event raised");
        }

        #endregion

        #region ScriptableObject Lifecycle

        private void OnEnable()
        {
            // Clear all subscriptions when asset is loaded
            // This prevents stale references from previous play sessions
            ClearAllSubscriptions();
        }

        private void OnDisable()
        {
            // Clear all subscriptions when asset is unloaded
            ClearAllSubscriptions();
        }

        #endregion
    }
}
