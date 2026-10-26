using UnityEngine;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// Swims back and forth inside the swim area: drifts to random heights and turns at the side edges.
    /// </summary>
    public sealed class FishSwimmer : MonoBehaviour
    {
        [SerializeField] private SwimArea swimArea;
        [SerializeField] private Transform visual;
        [SerializeField, Min(0.01f)] private float swimSpeed = 1.5f;
        [SerializeField, Min(0.01f)] private float driftSpeed = 0.4f;

        private float direction = 1f;
        private float targetHeight;

        private void Start()
        {
            if (Random.value < 0.5f)
            {
                Turn();
            }

            targetHeight = RandomHeight();
        }

        private void Update()
        {
            var area = swimArea.Area;
            var position = transform.position;
            position.x += direction * swimSpeed * Time.deltaTime;
            position.y = Mathf.MoveTowards(position.y, targetHeight, driftSpeed * Time.deltaTime);

            if (position.y == targetHeight)
            {
                targetHeight = RandomHeight();
            }

            // Only turn when swimming outward, so a fish resting on the edge does not flip every frame.
            if ((direction > 0f && position.x >= area.xMax) || (direction < 0f && position.x <= area.xMin))
            {
                position.x = Mathf.Clamp(position.x, area.xMin, area.xMax);
                Turn();
            }

            transform.position = position;
        }

        private void Turn()
        {
            direction = -direction;

            // Only the sign changes, so a size set elsewhere (e.g. growth) is kept.
            var scale = visual.localScale;
            scale.x = Mathf.Abs(scale.x) * direction;
            visual.localScale = scale;
        }

        private float RandomHeight()
        {
            var area = swimArea.Area;
            return Random.Range(area.yMin, area.yMax);
        }
    }
}
