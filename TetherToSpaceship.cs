using UnityEngine;

namespace Space
{
    public class TetherToSpaceship : MonoBehaviour
    {
        [Header("Tether Settings")]
        public float tetherLength = 12.0f;      // Maximum distance before tether force applies
        public float tetherStrength = 5.0f;    // Strength of pull-back force
        public float quadOffset = 16f;        // Offset distance for quad placement

        [Header("References")]
        public Rigidbody player;
        public Rigidbody spaceship;
        public Transform bubbleQuad;
        [SerializeField] private Transform fuckOffPos;

        private Collider[] spaceshipColliders;

        void FixedUpdate()
        {
            // Dynamically fetch spaceship colliders
            spaceshipColliders = spaceship.GetComponentsInChildren<Collider>();

            if (spaceshipColliders.Length == 0)
            {
                Debug.LogWarning("No colliders found on the spaceship. Tether will not function.");
                return;
            }

            // Find the closest point on the spaceship
            Vector3 closestPoint = GetClosestPoint(player.position);

            // Calculate distance and apply tethering logic
            Vector3 distanceVector = player.position - closestPoint;
            float distance = distanceVector.magnitude;

            if (distance > tetherLength)
            {
                Vector3 directionToClosestPoint = distanceVector.normalized;
                float excessDistance = distance - tetherLength;

                // Apply proportional pull-back force
                Vector3 pullForce = directionToClosestPoint * (excessDistance * tetherStrength);
                player.AddForce(-pullForce, ForceMode.Acceleration);

                // Update the bubble quad
                UpdateBubbleQuad(closestPoint, directionToClosestPoint);
            }
            else
            {
                bubbleQuad.position = fuckOffPos.position;
            }
        }

        Vector3 GetClosestPoint(Vector3 playerPosition)
        {
            Vector3 closestPoint = Vector3.zero;
            float minDistance = float.MaxValue;

            foreach (var collider in spaceshipColliders)
            {
                Vector3 point = collider.ClosestPoint(playerPosition);
                float sqrDistance = (playerPosition - point).sqrMagnitude;

                if (sqrDistance < minDistance)
                {
                    minDistance = sqrDistance;
                    closestPoint = point;
                }
            }

            return closestPoint;
        }

        void UpdateBubbleQuad(Vector3 closestPoint, Vector3 directionToPlayer)
        {
            bubbleQuad.position = closestPoint + directionToPlayer * quadOffset;
            bubbleQuad.LookAt(player.position);
            bubbleQuad.Rotate(0, 180, 0); // Ensure the quad faces the player
        }

        void OnDrawGizmos()
        {
            if (spaceship)
            {
                // Draw tether boundary as a cyan wire sphere
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(spaceship.position, tetherLength);

                // (Optional) Visualize each collider's precise bounds using points
                Gizmos.color = Color.green;
                if (spaceshipColliders != null)
                {
                    foreach (var collider in spaceship.GetComponentsInChildren<Collider>())
                    {
                        // Visualize colliders more accurately by marking key points
                        Gizmos.DrawSphere(collider.bounds.center, 0.2f);
                    }
                }
            }
        }
    }
}
