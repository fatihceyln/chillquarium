using UnityEngine;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// The rectangle fish swim in, centered on this object.
    /// </summary>
    public sealed class SwimArea : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float width = 15f;
        [SerializeField, Min(0.1f)] private float height = 5.6f;

        public Rect Area => new Rect(transform.position.x - width / 2f, transform.position.y - height / 2f, width, height);

        // Only drawn while selected, so the Scene view stays clean.
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, new Vector3(width, height, 0f));
        }
    }
}
