using UnityEngine;
using RoombaRampage.Player;

namespace RoombaRampage.Weapons
{
    /// <summary>
    /// Weapon controller that handles firing projectiles based on player input.
    /// Attach to player GameObject alongside PlayerInput and PlayerController.
    /// Shoots in the direction the player is driving (velocity direction).
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(PlayerController))]
    public class WeaponController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Weapon Configuration")]
        [Tooltip("Current weapon data (stats, projectile prefab, etc.)")]
        [SerializeField] private WeaponData currentWeapon;

        [Header("Firing Configuration")]
        [Tooltip("Position to spawn projectiles from (leave null to use player position)")]
        [SerializeField] private Transform firePoint;

        [Tooltip("Offset from player position if firePoint is null")]
        [SerializeField] private Vector3 firePointOffset = new Vector3(0f, 0.5f, 1f);

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        #endregion

        #region Private Fields

        private PlayerInput playerInput;
        private PlayerController playerController;
        private float lastFireTime;
        private bool canFire = true;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            playerController = GetComponent<PlayerController>();

            if (playerInput == null)
            {
                Debug.LogError("[WeaponController] PlayerInput component not found! WeaponController requires PlayerInput.");
                enabled = false;
                return;
            }

            if (playerController == null)
            {
                Debug.LogError("[WeaponController] PlayerController component not found! WeaponController requires PlayerController.");
                enabled = false;
                return;
            }

            // Initialize weapon data
            if (currentWeapon != null)
            {
                currentWeapon.Initialize();
            }
        }

        private void Update()
        {
            if (!canFire || currentWeapon == null) return;

            // Check for fire input (Attack button held down)
            if (playerInput.AttackHeld)
            {
                TryFire();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the current weapon.
        /// </summary>
        /// <param name="weapon">New weapon data to equip</param>
        public void SetWeapon(WeaponData weapon)
        {
            if (weapon == null)
            {
                Debug.LogWarning("[WeaponController] Cannot set null weapon!");
                return;
            }

            currentWeapon = weapon;
            currentWeapon.Initialize();

            if (showDebugInfo)
            {
                Debug.Log($"[WeaponController] Equipped weapon: {weapon.weaponName}");
            }
        }

        /// <summary>
        /// Enables weapon firing.
        /// </summary>
        public void EnableWeapon()
        {
            canFire = true;
        }

        /// <summary>
        /// Disables weapon firing.
        /// </summary>
        public void DisableWeapon()
        {
            canFire = false;
        }

        /// <summary>
        /// Gets the current weapon data.
        /// </summary>
        public WeaponData GetCurrentWeapon() => currentWeapon;

        /// <summary>
        /// Reloads the current weapon (if it uses ammo).
        /// </summary>
        public void Reload()
        {
            if (currentWeapon != null)
            {
                currentWeapon.Reload();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Attempts to fire the weapon if cooldown has elapsed.
        /// </summary>
        private void TryFire()
        {
            // Check cooldown
            if (Time.time - lastFireTime < currentWeapon.GetCooldownTime())
            {
                return;
            }

            // Check ammo
            if (!currentWeapon.CanFire())
            {
                if (showDebugInfo)
                {
                    Debug.Log("[WeaponController] Out of ammo!");
                }
                return;
            }

            // Fire weapon
            Fire();

            // Consume ammo
            currentWeapon.ConsumeAmmo();

            // Update fire time
            lastFireTime = Time.time;
        }

        /// <summary>
        /// Fires the weapon, spawning projectile(s).
        /// </summary>
        private void Fire()
        {
            if (currentWeapon.projectilePrefab == null)
            {
                Debug.LogError("[WeaponController] Weapon has no projectile prefab assigned!");
                return;
            }

            // Get fire position
            Vector3 spawnPosition = GetFirePosition();

            // Get fire direction (player's forward or look direction)
            Vector3 fireDirection = GetFireDirection();

            // Spawn projectiles
            int projectileCount = currentWeapon.projectileCount;

            if (projectileCount == 1)
            {
                // Single projectile
                SpawnProjectile(spawnPosition, fireDirection);
            }
            else
            {
                // Multiple projectiles with spread
                SpawnProjectilesWithSpread(spawnPosition, fireDirection, projectileCount);
            }

            if (showDebugInfo)
            {
                Debug.Log($"[WeaponController] Fired {projectileCount} projectile(s) in direction {fireDirection}");
            }
        }

        /// <summary>
        /// Spawns a single projectile.
        /// </summary>
        private void SpawnProjectile(Vector3 position, Vector3 direction)
        {
            if (ProjectilePool.Instance != null)
            {
                // Use pool
                ProjectilePool.Instance.GetProjectile(
                    currentWeapon.projectilePrefab,
                    position,
                    direction,
                    currentWeapon.projectileSpeed,
                    currentWeapon.damage,
                    currentWeapon.projectileLifetime
                );
            }
            else
            {
                // Fallback: direct instantiation
                GameObject projectileObj = Instantiate(currentWeapon.projectilePrefab, position, Quaternion.identity);
                Projectile projectile = projectileObj.GetComponent<Projectile>();

                if (projectile != null)
                {
                    projectile.Initialize(direction, currentWeapon.projectileSpeed, currentWeapon.damage, currentWeapon.projectileLifetime);
                }
            }
        }

        /// <summary>
        /// Spawns multiple projectiles with spread pattern.
        /// </summary>
        private void SpawnProjectilesWithSpread(Vector3 position, Vector3 baseDirection, int count)
        {
            float totalSpread = currentWeapon.spreadAngle;
            float angleStep = totalSpread / (count - 1);
            float startAngle = -totalSpread / 2f;

            for (int i = 0; i < count; i++)
            {
                float currentAngle = startAngle + (angleStep * i);
                Vector3 spreadDirection = RotateVectorAroundY(baseDirection, currentAngle);

                SpawnProjectile(position, spreadDirection);
            }
        }

        /// <summary>
        /// Gets the position to spawn projectiles from.
        /// </summary>
        private Vector3 GetFirePosition()
        {
            if (firePoint != null)
            {
                return firePoint.position;
            }

            // Use player position with offset
            return transform.position + transform.TransformDirection(firePointOffset);
        }

        /// <summary>
        /// Gets the direction to fire projectiles.
        /// Uses the direction the roomba is facing (rotation/forward).
        /// </summary>
        private Vector3 GetFireDirection()
        {
            // Use the transform's forward direction (where the roomba is facing)
            Vector3 fireDirection = transform.forward;
            fireDirection.y = 0f; // Keep on XZ plane
            return fireDirection.normalized;
        }

        /// <summary>
        /// Rotates a vector around the Y axis by the specified angle.
        /// </summary>
        private Vector3 RotateVectorAroundY(Vector3 vector, float angleDegrees)
        {
            Quaternion rotation = Quaternion.Euler(0f, angleDegrees, 0f);
            return rotation * vector;
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            if (!showDebugInfo || !Application.isPlaying) return;

            // Draw fire point
            Vector3 firePos = GetFirePosition();
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePos, 0.2f);

            // Draw fire direction
            if (playerInput != null)
            {
                Vector3 fireDir = GetFireDirection();
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(firePos, fireDir * 3f);

                // Draw spread cone if multiple projectiles
                if (currentWeapon != null && currentWeapon.projectileCount > 1)
                {
                    Gizmos.color = new Color(1f, 1f, 0f, 0.3f);

                    float halfSpread = currentWeapon.spreadAngle / 2f;
                    Vector3 leftDir = RotateVectorAroundY(fireDir, -halfSpread);
                    Vector3 rightDir = RotateVectorAroundY(fireDir, halfSpread);

                    Gizmos.DrawRay(firePos, leftDir * 3f);
                    Gizmos.DrawRay(firePos, rightDir * 3f);
                }
            }
        }

        private void OnGUI()
        {
            if (!showDebugInfo || currentWeapon == null) return;

            GUILayout.BeginArea(new Rect(10, 550, 400, 150));
            GUILayout.Label("=== Weapon Status ===");
            GUILayout.Label($"Weapon: {currentWeapon.weaponName}");
            GUILayout.Label($"Damage: {currentWeapon.damage}");
            GUILayout.Label($"Fire Rate: {currentWeapon.fireRate:F2}s");
            GUILayout.Label($"Can Fire: {canFire}");
            GUILayout.Label($"Time Since Last Fire: {Time.time - lastFireTime:F2}s");

            if (!currentWeapon.HasInfiniteAmmo())
            {
                GUILayout.Label($"Ammo: {currentWeapon.currentAmmo} / {currentWeapon.maxAmmo}");
            }
            else
            {
                GUILayout.Label("Ammo: Infinite");
            }

            GUILayout.EndArea();
        }

        #endregion
    }
}
