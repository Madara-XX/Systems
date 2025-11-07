using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace RoombaRampage.Skills.Helpers
{
    /// <summary>
    /// Reusable helper class for finding and targeting enemies.
    /// Used by skills that need enemy detection (lasers, missiles, etc.).
    /// </summary>
    public static class EnemyTargeting
    {
        /// <summary>
        /// Finds all enemies within a sphere.
        /// </summary>
        /// <param name="origin">Center point to search from</param>
        /// <param name="radius">Search radius</param>
        /// <param name="layerMask">Layer mask to filter colliders (default: Everything)</param>
        /// <param name="includeDeadEnemies">Whether to include dead enemies (default: false)</param>
        /// <returns>List of enemy GameObjects</returns>
        public static List<GameObject> FindEnemiesInRadius(Vector3 origin, float radius, LayerMask layerMask = default, bool includeDeadEnemies = false)
        {
            if (layerMask == default)
                layerMask = -1; // Everything

            List<GameObject> enemies = new List<GameObject>();
            Collider[] colliders = Physics.OverlapSphere(origin, radius, layerMask);

            foreach (Collider col in colliders)
            {
                if (col.CompareTag("Enemy"))
                {
                    // Check if enemy is alive
                    if (!includeDeadEnemies)
                    {
                        var enemyHealth = col.GetComponent<Enemies.EnemyHealth>();
                        if (enemyHealth != null && enemyHealth.IsDead)
                            continue;
                    }

                    enemies.Add(col.gameObject);
                }
            }

            return enemies;
        }

        /// <summary>
        /// Finds the nearest enemy to a position.
        /// </summary>
        /// <param name="origin">Position to search from</param>
        /// <param name="maxRange">Maximum search range (default: Infinity)</param>
        /// <param name="layerMask">Layer mask to filter colliders (default: Everything)</param>
        /// <param name="includeDeadEnemies">Whether to include dead enemies (default: false)</param>
        /// <returns>Nearest enemy GameObject, or null if none found</returns>
        public static GameObject FindNearestEnemy(Vector3 origin, float maxRange = Mathf.Infinity, LayerMask layerMask = default, bool includeDeadEnemies = false)
        {
            List<GameObject> enemies = FindEnemiesInRadius(origin, maxRange, layerMask, includeDeadEnemies);

            if (enemies.Count == 0)
                return null;

            GameObject nearest = null;
            float nearestDistance = Mathf.Infinity;

            foreach (GameObject enemy in enemies)
            {
                float distance = Vector3.Distance(origin, enemy.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = enemy;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Gets multiple nearest enemies sorted by distance.
        /// </summary>
        /// <param name="origin">Position to search from</param>
        /// <param name="count">Number of enemies to return</param>
        /// <param name="maxRange">Maximum search range (default: Infinity)</param>
        /// <param name="layerMask">Layer mask to filter colliders (default: Everything)</param>
        /// <param name="includeDeadEnemies">Whether to include dead enemies (default: false)</param>
        /// <returns>List of nearest enemies sorted by distance</returns>
        public static List<GameObject> FindNearestEnemies(Vector3 origin, int count, float maxRange = Mathf.Infinity, LayerMask layerMask = default, bool includeDeadEnemies = false)
        {
            List<GameObject> enemies = FindEnemiesInRadius(origin, maxRange, layerMask, includeDeadEnemies);

            if (enemies.Count == 0)
                return new List<GameObject>();

            // Sort by distance and take top N
            return enemies
                .OrderBy(enemy => Vector3.Distance(origin, enemy.transform.position))
                .Take(count)
                .ToList();
        }

        /// <summary>
        /// Gets a direction vector pointing from origin to target.
        /// </summary>
        /// <param name="origin">Start position</param>
        /// <param name="target">Target position</param>
        /// <returns>Normalized direction vector</returns>
        public static Vector3 GetDirectionToTarget(Vector3 origin, Vector3 target)
        {
            return (target - origin).normalized;
        }

        /// <summary>
        /// Gets random directions spread evenly around 360 degrees.
        /// Useful for skills that fire in multiple directions.
        /// </summary>
        /// <param name="count">Number of directions to generate</param>
        /// <param name="randomOffset">Random angle offset to add (0-360, default: 0)</param>
        /// <returns>List of direction vectors on XZ plane</returns>
        public static List<Vector3> GetRandomDirections(int count, float randomOffset = 0f)
        {
            List<Vector3> directions = new List<Vector3>();

            for (int i = 0; i < count; i++)
            {
                float angle = Random.Range(0f, 360f) + randomOffset;
                Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                directions.Add(direction);
            }

            return directions;
        }

        /// <summary>
        /// Gets evenly spaced directions around 360 degrees.
        /// </summary>
        /// <param name="count">Number of directions to generate</param>
        /// <param name="startAngle">Starting angle in degrees (default: 0)</param>
        /// <returns>List of direction vectors on XZ plane</returns>
        public static List<Vector3> GetEvenlySpacedDirections(int count, float startAngle = 0f)
        {
            List<Vector3> directions = new List<Vector3>();
            float angleStep = 360f / count;

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + (angleStep * i);
                Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                directions.Add(direction);
            }

            return directions;
        }

        /// <summary>
        /// Gets the distance to the nearest enemy.
        /// </summary>
        /// <param name="origin">Position to search from</param>
        /// <param name="maxRange">Maximum search range (default: Infinity)</param>
        /// <param name="layerMask">Layer mask to filter colliders (default: Everything)</param>
        /// <returns>Distance to nearest enemy, or Infinity if none found</returns>
        public static float GetNearestEnemyDistance(Vector3 origin, float maxRange = Mathf.Infinity, LayerMask layerMask = default)
        {
            GameObject nearest = FindNearestEnemy(origin, maxRange, layerMask);
            return nearest != null ? Vector3.Distance(origin, nearest.transform.position) : Mathf.Infinity;
        }

        /// <summary>
        /// Checks if there are any enemies within range.
        /// </summary>
        /// <param name="origin">Position to search from</param>
        /// <param name="range">Search range</param>
        /// <param name="layerMask">Layer mask to filter colliders (default: Everything)</param>
        /// <returns>True if at least one enemy is in range</returns>
        public static bool HasEnemiesInRange(Vector3 origin, float range, LayerMask layerMask = default)
        {
            return FindEnemiesInRadius(origin, range, layerMask).Count > 0;
        }
    }
}
