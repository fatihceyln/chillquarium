using UnityEngine;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// Swims back and forth inside the swim area: drifts to random heights and turns at the side edges.
    /// A baby chases the nearest food pellet and eats it.
    /// </summary>
    public sealed class FishSwimmer : MonoBehaviour
    {
        // Keeps the fish from flipping back and forth when food is right above or below it.
        private const float TurnDeadZone = 0.05f;

        [SerializeField] private Fish fish;
        [SerializeField] private SwimArea swimArea;
        [SerializeField] private Feeder feeder;
        [SerializeField] private Transform visual;
        [SerializeField, Min(0.01f)] private float swimSpeed = 1.5f;
        [SerializeField, Min(0.01f)] private float driftSpeed = 0.4f;
        [SerializeField, Min(0.01f)] private float chaseSpeed = 3f;
        [SerializeField, Min(0.01f)] private float eatDistance = 0.25f;

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
            // Adults ignore food: it only speeds up growth.
            if (!fish.IsAdult && feeder.TryGetNearestPellet(transform.position, out var pellet))
            {
                ChaseFood(pellet);
                return;
            }

            Wander();
        }

        private void Wander()
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

        private void ChaseFood(FoodPellet pellet)
        {
            var target = pellet.transform.position;
            var horizontalGap = target.x - transform.position.x;

            if (Mathf.Abs(horizontalGap) > TurnDeadZone && Mathf.Sign(horizontalGap) != direction)
            {
                Turn();
            }

            transform.position = Vector3.MoveTowards(transform.position, target, chaseSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) <= eatDistance)
            {
                feeder.Eat(pellet, fish);
            }
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
