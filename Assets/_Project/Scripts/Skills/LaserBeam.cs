using UnityEngine;

namespace RoombaRampage.Skills
{
    /// <summary>
    /// Visual representation of a laser beam using LineRenderer.
    /// Auto-destroys after duration.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class LaserBeam : MonoBehaviour
    {
        private LineRenderer lineRenderer;
        private float destroyTimer;
        private bool isInitialized;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        /// <summary>
        /// Initializes the laser beam visual.
        /// </summary>
        /// <param name="startPoint">Starting position of the laser</param>
        /// <param name="endPoint">Ending position of the laser</param>
        /// <param name="width">Width of the laser beam</param>
        /// <param name="color">Color of the laser</param>
        /// <param name="duration">How long the laser visual stays</param>
        public void Initialize(Vector3 startPoint, Vector3 endPoint, float width, Color color, float duration)
        {
            if (lineRenderer == null)
            {
                Debug.LogError("[LaserBeam] LineRenderer component missing!");
                Destroy(gameObject);
                return;
            }

            // Set up LineRenderer
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, endPoint);
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;

            // Set material properties if using a material
            if (lineRenderer.material != null)
            {
                lineRenderer.material.color = color;
            }

            destroyTimer = duration;
            isInitialized = true;
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
            if (destroyTimer <= 0.1f && lineRenderer != null)
            {
                float alpha = destroyTimer / 0.1f;
                Color startColor = lineRenderer.startColor;
                Color endColor = lineRenderer.endColor;
                lineRenderer.startColor = new Color(startColor.r, startColor.g, startColor.b, alpha);
                lineRenderer.endColor = new Color(endColor.r, endColor.g, endColor.b, alpha);
            }
        }
    }
}
