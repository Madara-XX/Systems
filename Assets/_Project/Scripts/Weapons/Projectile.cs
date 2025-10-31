using UnityEngine;

namespace RoombaRampage.Weapons
{
    /// <summary>
    /// Projectile component for weapon bullets.
    /// Moves forward on XZ plane, deals damage to enemies, despawns after lifetime.
    /// Designed to work with object pooling.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class Projectile : MonoBehaviour
    {
        #region Private Fields

        private float damage;
        private float speed;
        private float lifetime;
        private Vector3 direction;
        private Rigidbody rb;
        private float spawnTime;
        private bool isInitialized;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            ConfigureRigidbody();
        }

        private void FixedUpdate()
        {
            if (!isInitialized) return;

            // Move projectile forward
            rb.linearVelocity = direction * speed;

            // Check lifetime and despawn
            if (Time.time - spawnTime >= lifetime)
            {
                Despawn();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isInitialized) return;

            // Check if hit enemy
            if (other.CompareTag("Enemy"))
            {
                // Try to deal damage to enemy
                var enemyHealth = other.GetComponent<Enemies.EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage);
                }

                // Despawn projectile
                Despawn();
            }
            // Optional: Hit environment/obstacles
            else if (other.CompareTag("Obstacle") || other.CompareTag("Wall"))
            {
                Despawn();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the projectile with firing parameters.
        /// Called by ProjectilePool or WeaponController.
        /// </summary>
        /// <param name="fireDirection">Direction to fire (normalized, XZ plane)</param>
        /// <param name="projectileSpeed">Speed of projectile</param>
        /// <param name="projectileDamage">Damage dealt on hit</param>
        /// <param name="projectileLifetime">Time before auto-despawn</param>
        public void Initialize(Vector3 fireDirection, float projectileSpeed, float projectileDamage, float projectileLifetime)
        {
            direction = fireDirection.normalized;
            speed = projectileSpeed;
            damage = projectileDamage;
            lifetime = projectileLifetime;
            spawnTime = Time.time;
            isInitialized = true;

            // Ensure projectile stays on XZ plane
            direction.y = 0f;
            direction.Normalize();

            // Rotate projectile to face direction
            if (direction.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        /// <summary>
        /// Despawns the projectile (returns to pool or destroys).
        /// </summary>
        public void Despawn()
        {
            isInitialized = false;
            rb.linearVelocity = Vector3.zero;

            // Try to return to pool
            if (ProjectilePool.Instance != null)
            {
                ProjectilePool.Instance.ReturnProjectile(this);
            }
            else
            {
                // Fallback: destroy if no pool exists
                Destroy(gameObject);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Configures Rigidbody for projectile physics.
        /// </summary>
        private void ConfigureRigidbody()
        {
            if (rb == null) return;

            // Set to kinematic or use velocity-based movement
            rb.isKinematic = false;
            rb.useGravity = false; // No gravity for top-down shooter
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // Prevent fast projectiles from tunneling

            // Set drag to zero for consistent speed
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            if (!isInitialized || !Application.isPlaying) return;

            // Draw velocity direction
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, direction * 2f);
        }

        #endregion
    }
}
