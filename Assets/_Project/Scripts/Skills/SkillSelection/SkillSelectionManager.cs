using UnityEngine;
using System.Collections.Generic;

namespace RoombaRampage.Skills
{
    /// <summary>
    /// Manages skill selection logic during level-up.
    /// Coordinates between XPManager, SkillManager, and SkillSelectionUI.
    /// </summary>
    public class SkillSelectionManager : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Skill pool configuration (all available skills)")]
        [SerializeField] private SkillPoolData skillPool;

        [Tooltip("Skill selection UI controller")]
        [SerializeField] private UI.SkillSelectionUI selectionUI;

        [Header("Settings")]
        [Tooltip("Automatically pause game during selection")]
        [SerializeField] private bool autoPauseGame = true;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        private SkillManager playerSkillManager;
        private List<SkillOffer> currentOffers;
        private bool isSelectionActive = false;

        private void Start()
        {
            // Subscribe to XP level-up event
            if (Progression.XPManager.Instance != null)
            {
                Progression.XPManager.Instance.OnLevelUp.AddListener(OnPlayerLevelUp);

                if (showDebugLogs)
                    Debug.Log("[SkillSelection] Subscribed to XPManager.OnLevelUp");
            }
            else
            {
                Debug.LogError("[SkillSelection] XPManager.Instance not found! Skill selection will not work.");
            }

            // Find player's SkillManager
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerSkillManager = player.GetComponent<SkillManager>();

                if (playerSkillManager == null)
                {
                    Debug.LogError("[SkillSelection] Player doesn't have SkillManager! Adding one now.");
                    playerSkillManager = player.AddComponent<SkillManager>();
                }
            }
            else
            {
                Debug.LogError("[SkillSelection] Player GameObject not found! Make sure player has 'Player' tag.");
            }

            // Validate references
            if (skillPool == null)
            {
                Debug.LogError("[SkillSelection] SkillPool not assigned! Assign in Inspector.");
            }

            if (selectionUI == null)
            {
                Debug.LogError("[SkillSelection] SkillSelectionUI not assigned! Assign in Inspector.");
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (Progression.XPManager.Instance != null)
            {
                Progression.XPManager.Instance.OnLevelUp.RemoveListener(OnPlayerLevelUp);
            }
        }

        /// <summary>
        /// Called when player levels up.
        /// </summary>
        private void OnPlayerLevelUp(int newLevel)
        {
            if (isSelectionActive)
            {
                if (showDebugLogs)
                    Debug.LogWarning("[SkillSelection] Selection already active! Ignoring level-up.");
                return;
            }

            if (skillPool == null || selectionUI == null || playerSkillManager == null)
            {
                Debug.LogError("[SkillSelection] Missing references! Cannot show selection.");
                CompleteSelection();
                return;
            }

            if (showDebugLogs)
                Debug.Log($"[SkillSelection] Player reached level {newLevel}! Generating skill offers...");

            // Generate skill offers
            currentOffers = skillPool.GenerateSkillOffers(
                playerSkillManager,
                skillPool.skillOffersPerLevelUp
            );

            // Check if any skills available
            if (currentOffers == null || currentOffers.Count == 0)
            {
                if (showDebugLogs)
                    Debug.Log("[SkillSelection] No skills available (all maxed?). Skipping selection.");

                // Show "MAX POWER" message or just resume
                CompleteSelection();
                return;
            }

            // Show selection UI
            isSelectionActive = true;

            if (autoPauseGame)
            {
                Time.timeScale = 0f;
            }

            selectionUI.ShowSelection(currentOffers, OnSkillSelected);

            if (showDebugLogs)
                Debug.Log($"[SkillSelection] Showing {currentOffers.Count} skill offers");
        }

        /// <summary>
        /// Called when player selects a skill.
        /// </summary>
        /// <param name="index">Index of selected skill (0-2)</param>
        private void OnSkillSelected(int index)
        {
            if (!isSelectionActive)
            {
                Debug.LogWarning("[SkillSelection] Selection not active! Ignoring.");
                return;
            }

            if (index < 0 || index >= currentOffers.Count)
            {
                Debug.LogError($"[SkillSelection] Invalid selection index: {index}");
                return;
            }

            SkillOffer selected = currentOffers[index];

            if (showDebugLogs)
            {
                string type = selected.isNewSkill ? "NEW" : "UPGRADE";
                Debug.Log($"[SkillSelection] Player selected: {selected.skillData.skillName} ({type}, Level {selected.targetLevel})");
            }

            // Apply selected skill
            if (selected.isUpgrade)
            {
                // Level up existing skill
                bool success = playerSkillManager.LevelUpSkill(selected.skillData);

                if (!success && showDebugLogs)
                {
                    Debug.LogWarning($"[SkillSelection] Failed to level up {selected.skillData.skillName}");
                }
            }
            else
            {
                // Add new skill at level 1
                playerSkillManager.AddSkill(selected.skillData, selected.targetLevel);

                if (showDebugLogs)
                    Debug.Log($"[SkillSelection] Added new skill: {selected.skillData.skillName}");
            }

            // Hide UI and complete selection
            CompleteSelection();
        }

        /// <summary>
        /// Completes the selection process and resumes game.
        /// </summary>
        private void CompleteSelection()
        {
            isSelectionActive = false;
            currentOffers = null;

            // Hide UI
            if (selectionUI != null)
            {
                selectionUI.HideSelection();
            }

            // Resume game
            if (autoPauseGame)
            {
                Time.timeScale = 1f;
            }

            // Notify XPManager that level-up is complete
            if (Progression.XPManager.Instance != null)
            {
                Progression.XPManager.Instance.CompleteLevelUp();
            }

            if (showDebugLogs)
                Debug.Log("[SkillSelection] Selection completed. Game resumed.");
        }

        /// <summary>
        /// Cancels current selection (for testing or edge cases).
        /// </summary>
        public void CancelSelection()
        {
            if (showDebugLogs)
                Debug.Log("[SkillSelection] Selection cancelled.");

            CompleteSelection();
        }

        /// <summary>
        /// Force triggers a skill selection (for testing).
        /// </summary>
        [ContextMenu("Test: Trigger Skill Selection")]
        public void TestTriggerSelection()
        {
            OnPlayerLevelUp(1);
        }
    }
}
