using UnityEngine;

namespace RoombaRampage.Enemies
{
    /// <summary>
    /// Simple AI that moves enemy toward player on XZ plane.
    /// Finds player via tag, uses Rigidbody for movement.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyAI : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Configuration")]
        [Tooltip("Enemy data containing movement stats")]
        [SerializeField] private EnemyData enemyData;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        #endregion

        #region Private Fields

        private Rigidbody rb;
        private Transform playerTransform;
        private float nextUpdateTime;
        private Vector3 moveDirection;
        private bool isActive = true;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            ConfigureRigidbody();

            if (enemyData == null)
            {
                Debug.LogError("[EnemyAI] No EnemyData assigned! AI needs EnemyData to function.", this);
                enabled = false;
                return;
            }
        }

        private void Start()
        {
            FindPlayer();
        }

        private void FixedUpdate()
        {
            if (!isActive || playerTransform == null) return;

            // Periodic AI updates for performance
            if (Time.time >= nextUpdateTime)
            {
                UpdateAI();
                nextUpdateTime = Time.time + enemyData.aiUpdateInterval;
            }

            // Move toward player
            MoveTowardPlayer();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Enables AI movement.
        /// </summary>
        public void EnableAI()
        {
            isActive = true;
        }

        /// <summary>
        /// Disables AI movement.
        /// </summary>
        public void DisableAI()
        {
            isActive = false;
            rb.linearVelocity = Vector3.zero;
        }

        /// <summary>
        /// Sets enemy data (useful for pooled enemies).
        /// </summary>
        /// <param name="data">Enemy data to use</param>
        public void SetEnemyData(EnemyData data)
        {
            enemyData = data;
        }

        /// <summary>
        /// Manually set player target (optional, overrides tag-based finding).
        /// </summary>
        /// <param name="player">Player transform</param>
        public void SetPlayer(Transform player)
        {
            playerTransform = player;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Finds player GameObject via "Player" tag.
        /// </summary>
        private void FindPlayer()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
            {
                playerTransform = playerObj.transform;

                if (showDebugInfo)
                {
                    Debug.Log($"[EnemyAI] {gameObject.name} found player: {playerObj.name}");
                }
            }
            else
            {
                Debug.LogWarning("[EnemyAI] No GameObject with 'Player' tag found! Enemy will not move.");
            }
        }

        /// <summary>
        /// Updates AI decision-making.
        /// </summary>
        private void UpdateAI()
        {
            if (playerTransform == null)
            {
                // Try to find player again
                FindPlayer();
                return;
            }

            // Check if player is in detection range
            if (!enemyData.IsPlayerInRange(transform.position, playerTransform.position))
            {
                moveDirection = Vector3.zero;
                return;
            }

            // Calculate direction to player (XZ plane)
            Vector3 directionToPlayer = playerTransform.position - transform.position;
            directionToPlayer.y = 0f; // Flatten to XZ plane
            float distanceToPlayer = directionToPlayer.magnitude;

            // Check if should stop (reached stopping distance)
            if (enemyData.ShouldStop(distanceToPlayer))
            {
                moveDirection = Vector3.zero;
            }
            else
            {
                moveDirection = directionToPlayer.normalized;
            }
        }

        /// <summary>
        /// Moves enemy toward player using Rigidbody.
        /// </summary>
        private void MoveTowardPlayer()
        {
            if (moveDirection.sqrMagnitude < 0.01f)
            {
                // Stop moving
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
                return;
            }

            // Set velocity on XZ plane
            Vector3 targetVelocity = moveDirection * enemyData.moveSpeed;
            targetVelocity.y = rb.linearVelocity.y; // Preserve Y velocity
            rb.linearVelocity = targetVelocity;

            // Rotate toward movement direction
            RotateToward(moveDirection);
        }

        /// <summary>
        /// Rotates enemy to face the target direction.
        /// </summary>
        /// <param name="direction">Direction to face (XZ plane)</param>
        private void RotateToward(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.01f) return;

            // Calculate target rotation
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Smoothly rotate toward target
            float rotationSpeed = enemyData.rotationSpeed * Time.fixedDeltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed);
        }

        /// <summary>
        /// Configures Rigidbody for enemy movement.
        /// </summary>
        private void ConfigureRigidbody()
        {
            if (rb == null) return;

            rb.isKinematic = false;
            rb.useGravity = false; // No gravity for top-down
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            if (!showDebugInfo || !Application.isPlaying) return;

            // Draw line to player
            if (playerTransform != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, playerTransform.position);
            }

            // Draw movement direction
            if (moveDirection.sqrMagnitude > 0.01f)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(transform.position, moveDirection * 2f);
            }

            // Draw detection range
            if (enemyData != null && !enemyData.HasInfiniteRange())
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
                DrawCircleXZ(transform.position, enemyData.detectionRange);
            }

            // Draw stopping distance
            if (enemyData != null && enemyData.stoppingDistance > 0f)
            {
                Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
                DrawCircleXZ(transform.position, enemyData.stoppingDistance);
            }
        }

        /// <summary>
        /// Draws a circle on the XZ plane for gizmos.
        /// </summary>
        private void DrawCircleXZ(Vector3 center, float radius, int segments = 32)
        {
            float angleStep = 360f / segments;

            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * angleStep * Mathf.Deg2Rad;
                float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

                Vector3 point1 = center + new Vector3(Mathf.Cos(angle1) * radius, 0f, Mathf.Sin(angle1) * radius);
                Vector3 point2 = center + new Vector3(Mathf.Cos(angle2) * radius, 0f, Mathf.Sin(angle2) * radius);

                Gizmos.DrawLine(point1, point2);
            }
        }

        #endregion
    }
}
