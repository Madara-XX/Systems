using UnityEngine;
using System.Collections.Generic;

namespace RoombaRampage.Skills
{
    /// <summary>
    /// Manages player skills, cooldowns, and activation.
    /// Attach to the player GameObject.
    /// </summary>
    public class SkillManager : MonoBehaviour
    {
        [Header("Skills")]
        [Tooltip("List of active skills")]
        [SerializeField] private List<SkillInstance> skills = new List<SkillInstance>();

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        private Dictionary<SkillData, float> cooldowns = new Dictionary<SkillData, float>();

        /// <summary>
        /// Represents an instance of a skill with its current level.
        /// </summary>
        [System.Serializable]
        public class SkillInstance
        {
            [Tooltip("The skill data")]
            public SkillData skillData;

            [Tooltip("Current level of the skill (minimum 1)")]
            [Range(1, 10)]
            public int level = 1;

            [Tooltip("Is this skill enabled?")]
            public bool isEnabled = true;

            /// <summary>
            /// Current cooldown remaining (set at runtime)
            /// </summary>
            [HideInInspector]
            public float currentCooldown;
        }

        private void Update()
        {
            UpdateCooldowns();
            TryActivateSkills();
        }

        /// <summary>
        /// Updates all skill cooldowns.
        /// </summary>
        private void UpdateCooldowns()
        {
            foreach (SkillInstance skill in skills)
            {
                if (skill.currentCooldown > 0f)
                {
                    skill.currentCooldown -= Time.deltaTime;
                }
            }
        }

        /// <summary>
        /// Attempts to activate all autofire skills that are off cooldown.
        /// </summary>
        private void TryActivateSkills()
        {
            foreach (SkillInstance skill in skills)
            {
                if (skill == null || skill.skillData == null || !skill.isEnabled)
                    continue;

                if (!skill.skillData.autoFire)
                    continue;

                if (skill.currentCooldown <= 0f)
                {
                    ActivateSkill(skill);
                }
            }
        }

        /// <summary>
        /// Activates a specific skill.
        /// </summary>
        public void ActivateSkill(SkillInstance skill)
        {
            if (skill == null || skill.skillData == null || !skill.isEnabled)
                return;

            if (skill.currentCooldown > 0f)
            {
                if (showDebugInfo)
                    Debug.Log($"[SkillManager] {skill.skillData.skillName} is on cooldown ({skill.currentCooldown:F1}s remaining)");
                return;
            }

            // Ensure level is at least 1
            int actualLevel = Mathf.Max(1, skill.level);

            // Activate the skill
            skill.skillData.Activate(gameObject, actualLevel);

            // Set cooldown
            skill.currentCooldown = skill.skillData.GetCooldown(actualLevel);

            if (showDebugInfo)
                Debug.Log($"[SkillManager] Activated {skill.skillData.skillName} (Level {actualLevel})");
        }

        /// <summary>
        /// Adds a new skill to the manager.
        /// </summary>
        public void AddSkill(SkillData skillData, int level = 1)
        {
            if (skillData == null) return;

            SkillInstance newSkill = new SkillInstance
            {
                skillData = skillData,
                level = Mathf.Clamp(level, 1, skillData.maxLevel),
                isEnabled = true,
                currentCooldown = 0f
            };

            skills.Add(newSkill);

            if (showDebugInfo)
                Debug.Log($"[SkillManager] Added skill: {skillData.skillName} (Level {level})");
        }

        /// <summary>
        /// Levels up a skill by 1.
        /// </summary>
        public bool LevelUpSkill(SkillData skillData)
        {
            SkillInstance skill = skills.Find(s => s.skillData == skillData);
            if (skill == null) return false;

            if (skill.level >= skill.skillData.maxLevel)
            {
                if (showDebugInfo)
                    Debug.LogWarning($"[SkillManager] {skillData.skillName} is already at max level");
                return false;
            }

            skill.level++;

            if (showDebugInfo)
                Debug.Log($"[SkillManager] {skillData.skillName} leveled up to {skill.level}");

            return true;
        }

        /// <summary>
        /// Removes a skill from the manager.
        /// </summary>
        public void RemoveSkill(SkillData skillData)
        {
            skills.RemoveAll(s => s.skillData == skillData);

            if (showDebugInfo)
                Debug.Log($"[SkillManager] Removed skill: {skillData.skillName}");
        }

        /// <summary>
        /// Gets the level of a specific skill.
        /// </summary>
        public int GetSkillLevel(SkillData skillData)
        {
            SkillInstance skill = skills.Find(s => s.skillData == skillData);
            return skill != null ? skill.level : 0;
        }

        /// <summary>
        /// Checks if a skill is on cooldown.
        /// </summary>
        public bool IsOnCooldown(SkillData skillData)
        {
            SkillInstance skill = skills.Find(s => s.skillData == skillData);
            return skill != null && skill.currentCooldown > 0f;
        }

        /// <summary>
        /// Gets remaining cooldown for a skill.
        /// </summary>
        public float GetCooldownRemaining(SkillData skillData)
        {
            SkillInstance skill = skills.Find(s => s.skillData == skillData);
            return skill != null ? skill.currentCooldown : 0f;
        }

        #region Debug

        private void OnGUI()
        {
            if (!showDebugInfo) return;

            GUILayout.BeginArea(new Rect(10, 250, 400, 300));
            GUILayout.Label("<b>Skills</b>");

            foreach (SkillInstance skill in skills)
            {
                if (skill == null || skill.skillData == null) continue;

                string status = skill.isEnabled ? "Active" : "Disabled";
                string cooldown = skill.currentCooldown > 0f ? $"CD: {skill.currentCooldown:F1}s" : "Ready";

                GUILayout.Label($"{skill.skillData.skillName} (Lv.{skill.level}) - {status} - {cooldown}");
            }

            GUILayout.EndArea();
        }

        #endregion
    }
}
