using UnityEngine;

namespace RoombaRampage.Weapons
{
    /// <summary>
    /// ScriptableObject containing all data for a weapon type.
    /// Create instances via Assets > Create > RoombaRampage > Weapon Data.
    /// </summary>
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "RoombaRampage/Weapon Data", order = 1)]
    public class WeaponData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Display name of the weapon")]
        public string weaponName = "New Weapon";

        [Tooltip("Description of the weapon")]
        [TextArea(2, 4)]
        public string description = "A weapon description.";

        [Header("Combat Stats")]
        [Tooltip("Damage dealt per projectile")]
        [Min(0f)]
        public float damage = 10f;

        [Tooltip("Time between shots (in seconds)")]
        [Min(0.01f)]
        public float fireRate = 0.5f;

        [Tooltip("Speed of fired projectiles")]
        [Min(0f)]
        public float projectileSpeed = 20f;

        [Tooltip("Lifetime of projectiles before despawn (seconds)")]
        [Min(0.1f)]
        public float projectileLifetime = 5f;

        [Header("Projectile Configuration")]
        [Tooltip("Prefab to spawn as projectile (must have Projectile component)")]
        public GameObject projectilePrefab;

        [Tooltip("Number of projectiles fired per shot")]
        [Min(1)]
        public int projectileCount = 1;

        [Tooltip("Spread angle for multiple projectiles (degrees)")]
        [Range(0f, 180f)]
        public float spreadAngle = 15f;

        [Header("Ammo (Optional)")]
        [Tooltip("Maximum ammo capacity (-1 for infinite)")]
        public int maxAmmo = -1;

        [Tooltip("Current ammo (set at runtime)")]
        [HideInInspector]
        public int currentAmmo;

        [Header("Audio/VFX References")]
        [Tooltip("Sound effect when firing (not implemented yet)")]
        public AudioClip fireSound;

        [Tooltip("Muzzle flash particle effect (not implemented yet)")]
        public GameObject muzzleFlashPrefab;

        [Tooltip("Impact effect when projectile hits (not implemented yet)")]
        public GameObject impactEffectPrefab;

        #region Runtime Helpers

        /// <summary>
        /// Returns the cooldown time between shots.
        /// </summary>
        public float GetCooldownTime() => fireRate;

        /// <summary>
        /// Checks if weapon has infinite ammo.
        /// </summary>
        public bool HasInfiniteAmmo() => maxAmmo < 0;

        /// <summary>
        /// Checks if weapon can fire (has ammo).
        /// </summary>
        public bool CanFire() => HasInfiniteAmmo() || currentAmmo > 0;

        /// <summary>
        /// Consumes ammo on fire. Returns true if ammo was consumed.
        /// </summary>
        public bool ConsumeAmmo()
        {
            if (HasInfiniteAmmo()) return true;

            if (currentAmmo > 0)
            {
                currentAmmo--;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reloads weapon to max ammo.
        /// </summary>
        public void Reload()
        {
            if (!HasInfiniteAmmo())
            {
                currentAmmo = maxAmmo;
            }
        }

        /// <summary>
        /// Initializes weapon data at start (resets ammo).
        /// </summary>
        public void Initialize()
        {
            if (!HasInfiniteAmmo())
            {
                currentAmmo = maxAmmo;
            }
        }

        #endregion

        #region Validation

        private void OnValidate()
        {
            // Ensure sensible defaults
            if (damage < 0f) damage = 0f;
            if (fireRate < 0.01f) fireRate = 0.01f;
            if (projectileSpeed < 0f) projectileSpeed = 0f;
            if (projectileLifetime < 0.1f) projectileLifetime = 0.1f;
            if (projectileCount < 1) projectileCount = 1;
        }

        #endregion
    }
}
