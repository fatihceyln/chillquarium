using UnityEngine;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// Swims in strokes, like a real fish: a stroke speeds the fish up while its body squeezes and springs back,
    /// then it glides and slows down, and just before it stops it strokes again.
    /// Each stroke picks a new heading at random. A baby steers toward the nearest food pellet instead and eats it.
    /// </summary>
    public sealed class FishSwimmer : MonoBehaviour
    {
        // Keeps the fish from flipping back and forth when food is right above or below it.
        private const float TurnDeadZone = 0.05f;

        [SerializeField] private Fish fish;
        [SerializeField] private Transform visual;
        [Tooltip("The sprite under Visual. Only it squeezes, so Visual keeps the growth size and the facing.")]
        [SerializeField] private Transform spriteTransform;

        [Header("Strokes")]
        [Tooltip("Top speed a stroke reaches, in units per second.")]
        [SerializeField, Min(0.01f)] private float strokeSpeed = 1.8f;
        [Tooltip("Top speed of a stroke while a baby chases food.")]
        [SerializeField, Min(0.01f)] private float chaseSpeed = 3f;
        [Tooltip("Each stroke's top speed varies by up to this share, so fish do not swim in step.")]
        [SerializeField, Range(0f, 0.5f)] private float strokeSpeedVariation = 0.2f;
        [Tooltip("Seconds a stroke takes to speed the fish up.")]
        [SerializeField, Min(0.05f)] private float strokeSeconds = 0.3f;
        [Tooltip("How quickly the fish slows down while gliding. Higher stops sooner.")]
        [SerializeField, Min(0.01f)] private float glideDrag = 1.2f;
        [Tooltip("The next stroke starts when the speed falls below this share of the last stroke's top speed.")]
        [SerializeField, Range(0.01f, 0.9f)] private float nextStrokeSpeedRatio = 0.15f;
        [Tooltip("Chance that a stroke turns the fish around.")]
        [SerializeField, Range(0f, 1f)] private float turnChance = 0.3f;
        [Tooltip("Largest up or down angle of a stroke, in degrees.")]
        [SerializeField, Range(0f, 80f)] private float maxStrokeAngle = 25f;
        [Tooltip("Within this distance of the swim area's edge, a stroke heads back inside.")]
        [SerializeField, Min(0f)] private float edgeMargin = 1f;

        [Header("Squeeze")]
        [Tooltip("How much narrower the fish gets in the middle of a stroke (0.2 = 20%).")]
        [SerializeField, Range(0f, 1f)] private float strokeSquashWidth = 0.2f;
        [Tooltip("How much taller the fish gets in the middle of a stroke (0.25 = 25%).")]
        [SerializeField, Range(0f, 1f)] private float strokeStretchHeight = 0.25f;

        [Header("Food")]
        [SerializeField, Min(0.01f)] private float eatDistance = 0.25f;

        private SwimArea swimArea;
        private Feeder feeder;
        private float facing = 1f;
        private Vector2 heading = Vector2.right;
        private float speed;
        private float strokeTopSpeed;
        private float strokeStartSpeed;
        private float strokeElapsed;
        private bool isStroking;

        private void Start()
        {
            if (Random.value < 0.5f)
            {
                Turn();
            }

            // Start somewhere in a glide, so fish spawned in the same frame (e.g. after loading) do not stroke in step.
            strokeTopSpeed = strokeSpeed;
            speed = Random.Range(strokeSpeed * nextStrokeSpeedRatio, strokeSpeed);
            heading = PickWanderHeading();
            FaceHeading();
        }

        private void Update()
        {
            Swim(Time.deltaTime);
        }

        public void Initialize(SwimArea swimArea, Feeder feeder)
        {
            this.swimArea = swimArea;
            this.feeder = feeder;
        }

        private void Swim(float deltaTime)
        {
            if (isStroking)
            {
                strokeElapsed += deltaTime;
                var progress = Mathf.Clamp01(strokeElapsed / strokeSeconds);

                // Ease out: most of the push comes early in the stroke.
                speed = Mathf.Lerp(strokeStartSpeed, strokeTopSpeed, 1f - ((1f - progress) * (1f - progress)));

                // The body squeezes and springs back once per stroke, most in the middle.
                Squeeze(Mathf.Sin(progress * Mathf.PI));
                isStroking = progress < 1f;
            }
            else
            {
                // Water drag: the glide loses the same share of its speed every second.
                speed *= Mathf.Exp(-glideDrag * deltaTime);

                if (speed <= strokeTopSpeed * nextStrokeSpeedRatio)
                {
                    StartStroke();
                }
            }

            // A chasing baby keeps steering at the food, so it does not glide past a sinking pellet.
            var isChasing = TryGetFood(out var pellet);

            if (isChasing)
            {
                heading = DirectionTo(pellet);
                FaceHeading();
            }

            MoveInsideSwimArea(deltaTime);

            if (isChasing && Vector2.Distance(transform.position, pellet.transform.position) <= eatDistance)
            {
                feeder.Eat(pellet);
                fish.Feed();
            }
        }

        private void StartStroke()
        {
            var isChasing = TryGetFood(out var pellet);
            heading = isChasing ? DirectionTo(pellet) : PickWanderHeading();
            FaceHeading();

            var topSpeed = isChasing ? chaseSpeed : strokeSpeed;
            strokeTopSpeed = topSpeed * Random.Range(1f - strokeSpeedVariation, 1f + strokeSpeedVariation);
            strokeStartSpeed = speed;
            strokeElapsed = 0f;
            isStroking = true;
        }

        // Adults ignore food: it only speeds up growth.
        private bool TryGetFood(out FoodPellet pellet)
        {
            pellet = null;
            return !fish.IsAdult && feeder.TryGetNearestPellet(transform.position, out pellet);
        }

        private Vector2 DirectionTo(FoodPellet pellet)
        {
            return ((Vector2)(pellet.transform.position - transform.position)).normalized;
        }

        private Vector2 PickWanderHeading()
        {
            var area = swimArea.Area;
            var position = (Vector2)transform.position;
            var newFacing = facing;

            // Near a side, head back inside; elsewhere, turn around by chance.
            if (position.x >= area.xMax - edgeMargin)
            {
                newFacing = -1f;
            }
            else if (position.x <= area.xMin + edgeMargin)
            {
                newFacing = 1f;
            }
            else if (Random.value < turnChance)
            {
                newFacing = -facing;
            }

            var angle = Random.Range(-maxStrokeAngle, maxStrokeAngle);

            // Near the surface or the sand, angle back toward the middle.
            if (position.y >= area.yMax - edgeMargin)
            {
                angle = -Mathf.Abs(angle);
            }
            else if (position.y <= area.yMin + edgeMargin)
            {
                angle = Mathf.Abs(angle);
            }

            var radians = angle * Mathf.Deg2Rad;
            return new Vector2(newFacing * Mathf.Cos(radians), Mathf.Sin(radians));
        }

        private void FaceHeading()
        {
            if (Mathf.Abs(heading.x) > TurnDeadZone && Mathf.Sign(heading.x) != facing)
            {
                Turn();
            }
        }

        private void MoveInsideSwimArea(float deltaTime)
        {
            var area = swimArea.Area;
            var position = (Vector2)transform.position + (heading * speed * deltaTime);
            transform.position = new Vector3(
                Mathf.Clamp(position.x, area.xMin, area.xMax),
                Mathf.Clamp(position.y, area.yMin, area.yMax),
                transform.position.z);
        }

        private void Squeeze(float amount)
        {
            spriteTransform.localScale = new Vector3(1f - (strokeSquashWidth * amount), 1f + (strokeStretchHeight * amount), 1f);
        }

        private void Turn()
        {
            facing = -facing;

            // Only the sign changes, so the size set by growth is kept.
            var scale = visual.localScale;
            scale.x = Mathf.Abs(scale.x) * facing;
            visual.localScale = scale;
        }
    }
}
