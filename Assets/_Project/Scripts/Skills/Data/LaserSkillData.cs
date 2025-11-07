using UnityEngine;
using System.Collections.Generic;

namespace RoombaRampage.Skills
{
    /// <summary>
    /// Targeting mode for laser beams.
    /// </summary>
    public enum LaserTargetingMode
    {
        Random,         // Fire in random directions
        NearestEnemy    // Target nearest enemies
    }

    /// <summary>
    /// Laser skill that fires beams in random directions or towards enemies.
    /// Pierces through multiple enemies and increases laser count on level up.
    /// </summary>
    [CreateAssetMenu(fileName = "LaserSkill", menuName = "RoombaRampage/Skills/Laser Skill", order = 1)]
    public class LaserSkillData : SkillData
    {
        [Header("Targeting")]
        [Tooltip("How lasers choose their direction")]
        public LaserTargetingMode targetingMode = LaserTargetingMode.Random;

        [Header("Laser Settings")]
        [Tooltip("Prefab for the laser beam (LineRenderer based)")]
        public GameObject laserPrefab;

        [Tooltip("Base damage per laser")]
        [Range(5f, 100f)]
        public float baseDamage = 20f;

        [Tooltip("Number of lasers to fire at level 1")]
        [Range(1, 5)]
        public int baseLaserCount = 1;

        [Tooltip("Additional lasers gained per level")]
        [Range(1, 3)]
        public int lasersPerLevel = 1;

        [Tooltip("Number of enemies each laser can pierce through")]
        [Range(1, 10)]
        public int pierceCount = 3;

        [Tooltip("Maximum range of the laser")]
        [Range(10f, 100f)]
        public float laserRange = 30f;

        [Tooltip("Duration the laser visual stays active (seconds)")]
        [Range(0.1f, 1f)]
        public float laserDuration = 0.3f;

        [Tooltip("Width of the laser beam")]
        [Range(0.05f, 0.5f)]
        public float laserWidth = 0.1f;

        [Tooltip("Color of the laser")]
        public Color laserColor = Color.red;

        [Tooltip("Layer mask for what the laser can hit")]
        public LayerMask hitLayer = -1; // Default: Everything

        /// <summary>
        /// Fires lasers based on targeting mode from the caster.
        /// </summary>
        public override void Activate(GameObject caster, int level)
        {
            if (caster == null || laserPrefab == null)
            {
                Debug.LogWarning($"[LaserSkill] Cannot activate - caster or laserPrefab is null");
                return;
            }

            int laserCount = GetLaserCount(level);
            Vector3 origin = caster.transform.position;

            // Get directions based on targeting mode
            List<Vector3> directions = GetLaserDirections(origin, laserCount);

            // Fire each laser
            for (int i = 0; i < directions.Count; i++)
            {
                FireLaser(origin, directions[i], level);
            }
        }

        /// <summary>
        /// Gets laser directions based on targeting mode.
        /// </summary>
        private List<Vector3> GetLaserDirections(Vector3 origin, int count)
        {
            List<Vector3> directions = new List<Vector3>();

            switch (targetingMode)
            {
                case LaserTargetingMode.Random:
                    // Random directions
                    directions = Helpers.EnemyTargeting.GetRandomDirections(count);
                    break;

                case LaserTargetingMode.NearestEnemy:
                    // Target nearest enemies
                    List<GameObject> nearestEnemies = Helpers.EnemyTargeting.FindNearestEnemies(origin, count, laserRange, hitLayer);

                    if (nearestEnemies.Count > 0)
                    {
                        // Aim at each nearest enemy
                        foreach (GameObject enemy in nearestEnemies)
                        {
                            Vector3 direction = Helpers.EnemyTargeting.GetDirectionToTarget(origin, enemy.transform.position);
                            directions.Add(direction);
                        }
                    }

                    // If not enough enemies, fill remaining with random directions
                    while (directions.Count < count)
                    {
                        float randomAngle = Random.Range(0f, 360f);
                        Vector3 direction = Quaternion.Euler(0f, randomAngle, 0f) * Vector3.forward;
                        directions.Add(direction);
                    }
                    break;
            }

            return directions;
        }

        /// <summary>
        /// Fires a single laser beam.
        /// </summary>
        private void FireLaser(Vector3 origin, Vector3 direction, int level)
        {
            // Instantiate laser visual
            GameObject laserObj = Instantiate(laserPrefab, origin, Quaternion.identity);
            LaserBeam laserBeam = laserObj.GetComponent<LaserBeam>();

            if (laserBeam == null)
            {
                Debug.LogError($"[LaserSkill] LaserPrefab must have a LaserBeam component!");
                Destroy(laserObj);
                return;
            }

            // Perform raycast to hit enemies
            RaycastHit[] hits = Physics.RaycastAll(origin, direction, laserRange, hitLayer);

            // Sort hits by distance (closest first)
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            // Process hits up to pierce count
            int enemiesHit = 0;
            Vector3 endPoint = origin + direction * laserRange; // Default to max range
            Vector3 lastEnemyHitPoint = endPoint;

            foreach (RaycastHit hit in hits)
            {
                // Check if it's an enemy
                if (hit.collider.CompareTag("Enemy"))
                {
                    // Try to damage the enemy
                    var enemyHealth = hit.collider.GetComponent<Enemies.EnemyHealth>();
                    if (enemyHealth != null && !enemyHealth.IsDead)
                    {
                        enemyHealth.TakeDamage(GetDamage(level));
                        enemiesHit++;

                        // Track the last enemy hit for visual end point
                        lastEnemyHitPoint = hit.point;

                        // Stop piercing after hitting pierce count enemies
                        if (enemiesHit >= pierceCount)
                        {
                            endPoint = lastEnemyHitPoint;
                            break;
                        }
                    }
                }
                else
                {
                    // Hit a non-enemy obstacle (wall, etc.) - stop laser here
                    endPoint = hit.point;
                    break;
                }
            }

            // If we hit enemies but didn't hit pierce limit or obstacle, extend to max range
            if (enemiesHit > 0 && enemiesHit < pierceCount)
            {
                endPoint = origin + direction * laserRange;
            }

            // Initialize laser visual
            laserBeam.Initialize(origin, endPoint, laserWidth, laserColor, laserDuration);
        }

        /// <summary>
        /// Gets the number of lasers to fire at a specific level.
        /// </summary>
        public int GetLaserCount(int level)
        {
            // Ensure level is at least 1
            level = Mathf.Max(1, level);
            return baseLaserCount + (lasersPerLevel * (level - 1));
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
            int laserCount = GetLaserCount(level);
            return $"{description}\n\nLevel {level}: {laserCount} laser{(laserCount > 1 ? "s" : "")} • {baseDamage} damage • {pierceCount} pierce";
        }

        #region Validation

        protected override void OnValidate()
        {
            base.OnValidate();

            baseDamage = Mathf.Max(1f, baseDamage);
            baseLaserCount = Mathf.Max(1, baseLaserCount);
            lasersPerLevel = Mathf.Max(0, lasersPerLevel);
            pierceCount = Mathf.Max(1, pierceCount);
            laserRange = Mathf.Max(1f, laserRange);
            laserDuration = Mathf.Max(0.1f, laserDuration);
            laserWidth = Mathf.Max(0.01f, laserWidth);
        }

        #endregion
    }
}
