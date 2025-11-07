using UnityEngine;

namespace RoombaRampage.Skills
{
    /// <summary>
    /// Visual representation of a lightning strike using LineRenderer.
    /// Creates jagged lightning effect and auto-destroys after duration.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class LightningStrike : MonoBehaviour
    {
        [Header("Lightning Properties")]
        [Tooltip("Number of segments in the lightning bolt (more = more jagged)")]
        [Range(5, 30)]
        [SerializeField] private int segments = 10;

        [Tooltip("Maximum random displacement for each segment")]
        [Range(0.1f, 3f)]
        [SerializeField] private float displacement = 1f;

        private LineRenderer lineRenderer;
        private float destroyTimer;
        private bool isInitialized;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        /// <summary>
        /// Initializes the lightning strike visual.
        /// </summary>
        /// <param name="startPoint">Starting position (sky)</param>
        /// <param name="endPoint">Ending position (ground/enemy)</param>
        /// <param name="width">Width of the lightning bolt</param>
        /// <param name="color">Color of the lightning</param>
        /// <param name="duration">How long the lightning visual stays</param>
        public void Initialize(Vector3 startPoint, Vector3 endPoint, float width, Color color, float duration)
        {
            if (lineRenderer == null)
            {
                Debug.LogError("[LightningStrike] LineRenderer component missing!");
                Destroy(gameObject);
                return;
            }

            // Generate jagged lightning path
            Vector3[] positions = GenerateLightningPath(startPoint, endPoint);

            // Set up LineRenderer
            lineRenderer.positionCount = positions.Length;
            lineRenderer.SetPositions(positions);
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width * 0.5f; // Taper at the end
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;

            // Set material properties if using a material
            if (lineRenderer.material != null)
            {
                lineRenderer.material.color = color;

                // Enable emission for glow effect
                if (lineRenderer.material.HasProperty("_EmissionColor"))
                {
                    lineRenderer.material.SetColor("_EmissionColor", color * 2f);
                }
            }

            destroyTimer = duration;
            isInitialized = true;
        }

        /// <summary>
        /// Generates a jagged lightning path from start to end.
        /// </summary>
        private Vector3[] GenerateLightningPath(Vector3 start, Vector3 end)
        {
            Vector3[] positions = new Vector3[segments + 1];
            positions[0] = start;
            positions[segments] = end;

            Vector3 direction = end - start;
            float totalDistance = direction.magnitude;

            // Create intermediate points with random displacement
            for (int i = 1; i < segments; i++)
            {
                float t = (float)i / segments;
                Vector3 straightPoint = Vector3.Lerp(start, end, t);

                // Calculate maximum displacement (more at middle, less at ends)
                float displacementScale = Mathf.Sin(t * Mathf.PI); // Bell curve
                float maxDisplacement = displacement * displacementScale;

                // Add random offset perpendicular to the direction
                Vector3 perpendicular1 = Vector3.Cross(direction, Vector3.up).normalized;
                Vector3 perpendicular2 = Vector3.Cross(direction, perpendicular1).normalized;

                Vector3 randomOffset =
                    perpendicular1 * Random.Range(-maxDisplacement, maxDisplacement) +
                    perpendicular2 * Random.Range(-maxDisplacement, maxDisplacement);

                positions[i] = straightPoint + randomOffset;
            }

            return positions;
        }

        private void Update()
        {
            if (!isInitialized) return;

            destroyTimer -= Time.deltaTime;

            if (destroyTimer <= 0f)
            {
                Destroy(gameObject);
            }

            // Optional: Fade out effect
            if (destroyTimer <= 0.2f && lineRenderer != null)
            {
                float alpha = destroyTimer / 0.2f;
                Color startColor = lineRenderer.startColor;
                Color endColor = lineRenderer.endColor;
                lineRenderer.startColor = new Color(startColor.r, startColor.g, startColor.b, alpha);
                lineRenderer.endColor = new Color(endColor.r, endColor.g, endColor.b, alpha);
            }
        }

        /// <summary>
        /// Regenerates the lightning path (for flickering effect).
        /// Call this periodically to make lightning flicker.
        /// </summary>
        public void Regenerate(Vector3 start, Vector3 end)
        {
            if (lineRenderer == null) return;

            Vector3[] positions = GenerateLightningPath(start, end);
            lineRenderer.SetPositions(positions);
        }
    }
}
