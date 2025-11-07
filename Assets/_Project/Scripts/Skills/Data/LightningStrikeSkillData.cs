using UnityEngine;
using System.Collections.Generic;

namespace RoombaRampage.Skills
{
    /// <summary>
    /// Lightning strike skill that hits random enemies in a zone with lightning bolts.
    /// Deals instant damage and can apply status effects (burn, slow, etc.).
    /// </summary>
    [CreateAssetMenu(fileName = "LightningStrikeSkill", menuName = "RoombaRampage/Skills/Lightning Strike Skill", order = 2)]
    public class LightningStrikeSkillData : SkillData
    {
        [Header("Lightning Settings")]
        [Tooltip("Prefab for the lightning strike visual effect (LineRenderer based)")]
        public GameObject lightningPrefab;

        [Tooltip("Base damage per lightning strike")]
        [Range(10f, 200f)]
        public float baseDamage = 50f;

        [Tooltip("Number of enemies to strike at level 1")]
        [Range(1, 10)]
        public int baseStrikeCount = 1;

        [Tooltip("Additional strikes gained per level")]
        [Range(0, 5)]
        public int strikesPerLevel = 1;

        [Tooltip("Maximum range to search for enemies")]
        [Range(10f, 100f)]
        public float strikeRange = 40f;

        [Tooltip("Duration the lightning visual stays active (seconds)")]
        [Range(0.1f, 2f)]
        public float lightningDuration = 0.5f;

        [Tooltip("Width of the lightning bolt")]
        [Range(0.1f, 1f)]
        public float lightningWidth = 0.3f;

        [Tooltip("Color of the lightning")]
        public Color lightningColor = new Color(0.5f, 0.5f, 1f, 1f); // Light blue

        [Tooltip("Layer mask for what the lightning can hit")]
        public LayerMask hitLayer = -1; // Default: Everything

        [Header("Status Effect")]
        [Tooltip("Status effect to apply on hit (None, Burn, Slow, Stun, etc.)")]
        public Helpers.StatusEffectData statusEffect = new Helpers.StatusEffectData();

        [Header("Visual Effect")]
        [Tooltip("Height above enemy where lightning originates (skybox height)")]
        [Range(10f, 100f)]
        public float lightningOriginHeight = 30f;

        [Tooltip("Random offset for strike position (makes it less precise)")]
        [Range(0f, 5f)]
        public float strikeRandomOffset = 0.5f;

        /// <summary>
        /// Strikes random enemies with lightning in the defined zone.
        /// </summary>
        public override void Activate(GameObject caster, int level)
        {
            if (caster == null || lightningPrefab == null)
            {
                Debug.LogWarning($"[LightningStrike] Cannot activate - caster or lightningPrefab is null");
                return;
            }

            int strikeCount = GetStrikeCount(level);
            Vector3 casterPosition = caster.transform.position;

            // Find random enemies in range
            List<GameObject> enemies = Helpers.EnemyTargeting.FindEnemiesInRadius(casterPosition, strikeRange, hitLayer);

            if (enemies.Count == 0)
            {
                // No enemies to strike
                return;
            }

            // Strike up to strikeCount random enemies
            int actualStrikes = Mathf.Min(strikeCount, enemies.Count);

            // Shuffle enemies to randomize targets
            ShuffleList(enemies);

            for (int i = 0; i < actualStrikes; i++)
            {
                StrikeEnemy(enemies[i], level);
            }
        }

        /// <summary>
        /// Strikes a single enemy with lightning.
        /// </summary>
        private void StrikeEnemy(GameObject enemy, int level)
        {
            if (enemy == null)
                return;

            // Get enemy position with random offset
            Vector3 enemyPosition = enemy.transform.position;
            Vector3 strikePosition = enemyPosition + new Vector3(
                Random.Range(-strikeRandomOffset, strikeRandomOffset),
                0f,
                Random.Range(-strikeRandomOffset, strikeRandomOffset)
            );

            // Lightning originates from above
            Vector3 startPosition = strikePosition + Vector3.up * lightningOriginHeight;
            Vector3 endPosition = strikePosition;

            // Create visual effect
            GameObject lightningObj = Instantiate(lightningPrefab, startPosition, Quaternion.identity);
            LightningStrike lightning = lightningObj.GetComponent<LightningStrike>();

            if (lightning == null)
            {
                Debug.LogError($"[LightningStrike] LightningPrefab must have a LightningStrike component!");
                Destroy(lightningObj);
                return;
            }

            // Initialize lightning visual
            lightning.Initialize(startPosition, endPosition, lightningWidth, lightningColor, lightningDuration);

            // Deal damage to enemy
            var enemyHealth = enemy.GetComponent<Enemies.EnemyHealth>();
            if (enemyHealth != null && !enemyHealth.IsDead)
            {
                enemyHealth.TakeDamage(GetDamage(level));

                // Apply status effect if configured
                if (statusEffect.effectType != Helpers.StatusEffectType.None)
                {
                    var statusEffectManager = enemy.GetComponent<Helpers.StatusEffectManager>();

                    // Add StatusEffectManager if not present
                    if (statusEffectManager == null)
                    {
                        statusEffectManager = enemy.AddComponent<Helpers.StatusEffectManager>();
                    }

                    statusEffectManager.ApplyEffect(statusEffect);
                }
            }
        }

        /// <summary>
        /// Shuffles a list in place (Fisher-Yates shuffle).
        /// </summary>
        private void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }

        /// <summary>
        /// Gets the number of lightning strikes at a specific level.
        /// </summary>
        public int GetStrikeCount(int level)
        {
            // Ensure level is at least 1
            level = Mathf.Max(1, level);
            return baseStrikeCount + (strikesPerLevel * (level - 1));
        }

        /// <summary>
        /// Gets the damage at a specific level.
        /// </summary>
        public float GetDamage(int level)
        {
            return baseDamage;
        }

        public override string GetLevelDescription(int level)
        {
            int strikeCount = GetStrikeCount(level);
            string effectText = statusEffect.effectType != Helpers.StatusEffectType.None
                ? $" • {statusEffect.effectType}"
                : "";

            return $"{description}\n\nLevel {level}: {strikeCount} strike{(strikeCount > 1 ? "s" : "")} • {baseDamage} damage{effectText}";
        }

        #region Validation

        protected override void OnValidate()
        {
            base.OnValidate();

            baseDamage = Mathf.Max(1f, baseDamage);
            baseStrikeCount = Mathf.Max(1, baseStrikeCount);
            strikesPerLevel = Mathf.Max(0, strikesPerLevel);
            strikeRange = Mathf.Max(1f, strikeRange);
            lightningDuration = Mathf.Max(0.1f, lightningDuration);
            lightningWidth = Mathf.Max(0.01f, lightningWidth);
            lightningOriginHeight = Mathf.Max(1f, lightningOriginHeight);
            strikeRandomOffset = Mathf.Max(0f, strikeRandomOffset);
        }

        #endregion
    }
}
