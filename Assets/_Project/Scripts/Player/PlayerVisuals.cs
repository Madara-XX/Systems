using UnityEngine;
using System.Collections;

namespace RoombaRampage.Player
{
    /// <summary>
    /// Manages player visual representation, 3D model/material handling, and visual feedback effects.
    /// Handles damage flash, death effects, and other visual feedback.
    /// Listens to PlayerEvents for triggering visual responses.
    /// Works with MeshRenderer for 3D models.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    public class PlayerVisuals : MonoBehaviour
    {
        #region Serialized Fields

        [Header("References")]
        [SerializeField] private MeshRenderer meshRenderer;
        [Tooltip("Optional: Event channel for listening to player events")]
        [SerializeField] private PlayerEvents playerEvents;

        [Header("Damage Flash Effect")]
        [SerializeField] private Color damageFlashColor = new Color(1f, 0.3f, 0.3f, 1f);
        [SerializeField] private float flashDuration = 0.1f;
        [SerializeField] private int flashCount = 3;

        [Header("Invulnerability Effect")]
        [SerializeField] private bool enableInvulnerabilityFlash = true;
        [SerializeField] private float invulnerabilityFlashInterval = 0.1f;

        [Header("Death Effect")]
        [SerializeField] private float deathFadeDuration = 0.5f;
        [SerializeField] private bool disableOnDeath = true;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        #endregion

        #region Private Fields

        // Cached components
        private PlayerHealth playerHealth;

        // Material instance for color changes
        private Material materialInstance;

        // Original material color
        private Color originalColor;

        // Effect coroutines
        private Coroutine damageFlashCoroutine;
        private Coroutine invulnerabilityCoroutine;
        private Coroutine deathCoroutine;

        // State
        private bool isFlashing;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Cache components
            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }

            playerHealth = GetComponent<PlayerHealth>();

            // Create material instance and store original color
            if (meshRenderer != null)
            {
                // Create a unique material instance to avoid modifying shared material
                materialInstance = meshRenderer.material;
                originalColor = materialInstance.color;
            }
        }

        private void OnEnable()
        {
            // Subscribe to player events
            if (playerEvents != null)
            {
                playerEvents.OnPlayerDamaged += OnPlayerDamaged;
                playerEvents.OnPlayerDied += OnPlayerDied;
                playerEvents.OnPlayerRespawned += OnPlayerRespawned;
            }

            // Subscribe to health invulnerability changes if PlayerHealth exists
            if (playerHealth != null)
            {
                StartInvulnerabilityMonitoring();
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from player events
            if (playerEvents != null)
            {
                playerEvents.OnPlayerDamaged -= OnPlayerDamaged;
                playerEvents.OnPlayerDied -= OnPlayerDied;
                playerEvents.OnPlayerRespawned -= OnPlayerRespawned;
            }

            // Stop all coroutines
            StopAllCoroutines();
            damageFlashCoroutine = null;
            invulnerabilityCoroutine = null;
            deathCoroutine = null;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles player damaged event.
        /// </summary>
        private void OnPlayerDamaged(int currentHealth, int maxHealth)
        {
            PlayDamageFlash();

            if (showDebugInfo)
            {
                Debug.Log($"[PlayerVisuals] Player damaged: {currentHealth}/{maxHealth}");
            }
        }

        /// <summary>
        /// Handles player death event.
        /// </summary>
        private void OnPlayerDied()
        {
            PlayDeathEffect();

            if (showDebugInfo)
            {
                Debug.Log("[PlayerVisuals] Player died - playing death effect");
            }
        }

        /// <summary>
        /// Handles player respawn event.
        /// </summary>
        private void OnPlayerRespawned()
        {
            ResetVisuals();

            if (showDebugInfo)
            {
                Debug.Log("[PlayerVisuals] Player respawned - resetting visuals");
            }
        }

        #endregion

        #region Damage Flash Effect

        /// <summary>
        /// Plays the damage flash effect.
        /// Flashes material color between original color and damage color.
        /// </summary>
        public void PlayDamageFlash()
        {
            if (materialInstance == null) return;

            // Stop existing flash if any
            if (damageFlashCoroutine != null)
            {
                StopCoroutine(damageFlashCoroutine);
            }

            // Start new flash
            damageFlashCoroutine = StartCoroutine(DamageFlashCoroutine());
        }

        /// <summary>
        /// Coroutine for damage flash effect.
        /// </summary>
        private IEnumerator DamageFlashCoroutine()
        {
            isFlashing = true;

            for (int i = 0; i < flashCount; i++)
            {
                // Flash to damage color
                materialInstance.color = damageFlashColor;
                yield return new WaitForSeconds(flashDuration);

                // Flash back to original color
                materialInstance.color = originalColor;
                yield return new WaitForSeconds(flashDuration);
            }

            isFlashing = false;
            damageFlashCoroutine = null;
        }

        #endregion

        #region Invulnerability Effect

        /// <summary>
        /// Starts monitoring invulnerability state for visual feedback.
        /// </summary>
        private void StartInvulnerabilityMonitoring()
        {
            if (!enableInvulnerabilityFlash) return;

            if (invulnerabilityCoroutine != null)
            {
                StopCoroutine(invulnerabilityCoroutine);
            }

            invulnerabilityCoroutine = StartCoroutine(InvulnerabilityMonitoringCoroutine());
        }

        /// <summary>
        /// Coroutine for monitoring invulnerability and applying visual effect.
        /// </summary>
        private IEnumerator InvulnerabilityMonitoringCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(invulnerabilityFlashInterval);

                // Check if player is invulnerable
                if (playerHealth != null && playerHealth.IsInvulnerable && !isFlashing)
                {
                    // Toggle mesh renderer visibility
                    meshRenderer.enabled = !meshRenderer.enabled;
                }
                else
                {
                    // Ensure mesh renderer is visible when not invulnerable
                    if (!meshRenderer.enabled)
                    {
                        meshRenderer.enabled = true;
                    }
                }
            }
        }

        #endregion

        #region Death Effect

        /// <summary>
        /// Plays the death effect.
        /// Fades out material over time.
        /// </summary>
        public void PlayDeathEffect()
        {
            if (materialInstance == null) return;

            // Stop existing death effect if any
            if (deathCoroutine != null)
            {
                StopCoroutine(deathCoroutine);
            }

            // Start new death effect
            deathCoroutine = StartCoroutine(DeathEffectCoroutine());
        }

        /// <summary>
        /// Coroutine for death fade effect.
        /// </summary>
        private IEnumerator DeathEffectCoroutine()
        {
            float elapsedTime = 0f;
            Color startColor = materialInstance.color;
            Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

            // Enable transparent rendering mode if needed
            EnableTransparentMode();

            while (elapsedTime < deathFadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / deathFadeDuration;

                materialInstance.color = Color.Lerp(startColor, targetColor, t);

                yield return null;
            }

            // Ensure fully transparent
            materialInstance.color = targetColor;

            // Disable mesh renderer if configured
            if (disableOnDeath)
            {
                meshRenderer.enabled = false;
            }

            deathCoroutine = null;
        }

        /// <summary>
        /// Enables transparent rendering mode for fade effects.
        /// </summary>
        private void EnableTransparentMode()
        {
            if (materialInstance == null) return;

            // Set render mode to transparent for URP materials
            materialInstance.SetFloat("_Surface", 1); // 0 = Opaque, 1 = Transparent
            materialInstance.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            materialInstance.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            materialInstance.SetInt("_ZWrite", 0);
            materialInstance.DisableKeyword("_ALPHATEST_ON");
            materialInstance.EnableKeyword("_ALPHABLEND_ON");
            materialInstance.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            materialInstance.renderQueue = 3000;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the player material.
        /// </summary>
        /// <param name="material">New material to apply</param>
        public void SetMaterial(Material material)
        {
            if (meshRenderer != null && material != null)
            {
                materialInstance = material;
                meshRenderer.material = materialInstance;
                originalColor = materialInstance.color;
            }
        }

        /// <summary>
        /// Sets the material color.
        /// </summary>
        /// <param name="color">New color</param>
        public void SetColor(Color color)
        {
            if (materialInstance != null)
            {
                originalColor = color;
                materialInstance.color = color;
            }
        }

        /// <summary>
        /// Resets visuals to original state.
        /// Used for respawn.
        /// </summary>
        public void ResetVisuals()
        {
            // Stop all active coroutines
            if (damageFlashCoroutine != null)
            {
                StopCoroutine(damageFlashCoroutine);
                damageFlashCoroutine = null;
            }

            if (deathCoroutine != null)
            {
                StopCoroutine(deathCoroutine);
                deathCoroutine = null;
            }

            // Reset mesh renderer and material
            if (meshRenderer != null)
            {
                meshRenderer.enabled = true;
            }

            if (materialInstance != null)
            {
                materialInstance.color = originalColor;
            }

            isFlashing = false;

            // Restart invulnerability monitoring
            StartInvulnerabilityMonitoring();
        }

        /// <summary>
        /// Enables or disables the mesh renderer.
        /// </summary>
        /// <param name="visible">Visibility state</param>
        public void SetVisible(bool visible)
        {
            if (meshRenderer != null)
            {
                meshRenderer.enabled = visible;
            }
        }

        #endregion

        #region Debug Methods

        /// <summary>
        /// Test method to trigger damage flash.
        /// </summary>
        [ContextMenu("Test: Play Damage Flash")]
        private void TestPlayDamageFlash()
        {
            PlayDamageFlash();
        }

        /// <summary>
        /// Test method to trigger death effect.
        /// </summary>
        [ContextMenu("Test: Play Death Effect")]
        private void TestPlayDeathEffect()
        {
            PlayDeathEffect();
        }

        /// <summary>
        /// Test method to reset visuals.
        /// </summary>
        [ContextMenu("Test: Reset Visuals")]
        private void TestResetVisuals()
        {
            ResetVisuals();
        }

        #endregion

        #region Validation

        private void OnValidate()
        {
            // Ensure MeshRenderer is assigned
            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }

            // Validate timing values
            flashDuration = Mathf.Max(0.01f, flashDuration);
            flashCount = Mathf.Max(1, flashCount);
            invulnerabilityFlashInterval = Mathf.Max(0.01f, invulnerabilityFlashInterval);
            deathFadeDuration = Mathf.Max(0.1f, deathFadeDuration);
        }

        private void Reset()
        {
            // Auto-assign components when component is added
            meshRenderer = GetComponent<MeshRenderer>();
        }

        #endregion
    }
}
