using UnityEngine;
using System.Collections.Generic;

namespace RoombaRampage.Skills.Helpers
{
    /// <summary>
    /// Manages active status effects on an enemy.
    /// Attach to enemy GameObjects that can receive status effects.
    /// </summary>
    public class StatusEffectManager : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        private List<ActiveStatusEffect> activeEffects = new List<ActiveStatusEffect>();
        private Enemies.EnemyHealth enemyHealth;
        private Enemies.EnemyAI enemyAI;

        private void Awake()
        {
            enemyHealth = GetComponent<Enemies.EnemyHealth>();
            enemyAI = GetComponent<Enemies.EnemyAI>();
        }

        private void Update()
        {
            UpdateEffects();
        }

        /// <summary>
        /// Applies a status effect to this enemy.
        /// </summary>
        public void ApplyEffect(StatusEffectData effectData)
        {
            if (effectData.effectType == StatusEffectType.None)
                return;

            // Check if effect of same type already exists
            ActiveStatusEffect existing = activeEffects.Find(e => e.data.effectType == effectData.effectType);

            if (existing != null)
            {
                // Refresh duration
                existing.remainingDuration = effectData.duration;

                if (showDebugInfo)
                    Debug.Log($"[StatusEffect] Refreshed {effectData.effectType} on {gameObject.name}");
            }
            else
            {
                // Add new effect
                ActiveStatusEffect newEffect = new ActiveStatusEffect(effectData);
                activeEffects.Add(newEffect);

                if (showDebugInfo)
                    Debug.Log($"[StatusEffect] Applied {effectData.effectType} to {gameObject.name} for {effectData.duration}s");
            }
        }

        /// <summary>
        /// Updates all active effects.
        /// </summary>
        private void UpdateEffects()
        {
            if (activeEffects.Count == 0)
                return;

            // Update each effect
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                ActiveStatusEffect effect = activeEffects[i];
                bool shouldRemove = effect.Update(enemyHealth, enemyAI);

                if (shouldRemove)
                {
                    if (showDebugInfo)
                        Debug.Log($"[StatusEffect] {effect.data.effectType} expired on {gameObject.name}");

                    activeEffects.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Gets the combined movement speed multiplier from all effects.
        /// </summary>
        public float GetMovementSpeedMultiplier()
        {
            float multiplier = 1f;

            foreach (ActiveStatusEffect effect in activeEffects)
            {
                float effectMultiplier = effect.GetMovementSpeedMultiplier();

                // Use the lowest multiplier (most severe effect)
                if (effectMultiplier < multiplier)
                    multiplier = effectMultiplier;
            }

            return multiplier;
        }

        /// <summary>
        /// Checks if the enemy can act (not stunned).
        /// </summary>
        public bool CanAct()
        {
            foreach (ActiveStatusEffect effect in activeEffects)
            {
                if (!effect.CanAct())
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if a specific effect type is active.
        /// </summary>
        public bool HasEffect(StatusEffectType effectType)
        {
            return activeEffects.Exists(e => e.data.effectType == effectType);
        }

        /// <summary>
        /// Gets the remaining duration of a specific effect type.
        /// </summary>
        public float GetEffectDuration(StatusEffectType effectType)
        {
            ActiveStatusEffect effect = activeEffects.Find(e => e.data.effectType == effectType);
            return effect != null ? effect.remainingDuration : 0f;
        }

        /// <summary>
        /// Removes all active effects.
        /// </summary>
        public void ClearEffects()
        {
            activeEffects.Clear();

            if (showDebugInfo)
                Debug.Log($"[StatusEffect] Cleared all effects on {gameObject.name}");
        }

        /// <summary>
        /// Removes a specific effect type.
        /// </summary>
        public void RemoveEffect(StatusEffectType effectType)
        {
            activeEffects.RemoveAll(e => e.data.effectType == effectType);

            if (showDebugInfo)
                Debug.Log($"[StatusEffect] Removed {effectType} from {gameObject.name}");
        }

        #region Debug

        private void OnGUI()
        {
            if (!showDebugInfo || activeEffects.Count == 0)
                return;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2.5f);

            if (screenPos.z > 0)
            {
                Vector2 labelSize = new Vector2(200f, 20f * activeEffects.Count);
                Rect labelRect = new Rect(screenPos.x - labelSize.x / 2f, Screen.height - screenPos.y, labelSize.x, labelSize.y);

                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.fontSize = 10;
                style.alignment = TextAnchor.MiddleCenter;

                string effectText = "Effects:\n";
                foreach (ActiveStatusEffect effect in activeEffects)
                {
                    effectText += $"{effect.data.effectType} ({effect.remainingDuration:F1}s)\n";
                }

                GUI.Label(labelRect, effectText, style);
            }
        }

        #endregion
    }
}
