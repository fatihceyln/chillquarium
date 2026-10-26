using System;
using UnityEngine;

namespace Chillquarium.Gameplay
{
    /// <summary>
    /// One piece of food. Sinks to the bottom of the swim area and disappears if nobody eats it in time.
    /// </summary>
    public sealed class FoodPellet : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float sinkSpeed = 0.6f;
        [SerializeField, Min(0.1f)] private float lifetimeSeconds = 10f;

        private float bottomHeight;
        private float ageSeconds;

        public event Action<FoodPellet> Expired;

        private void Update()
        {
            var position = transform.position;
            position.y = Mathf.MoveTowards(position.y, bottomHeight, sinkSpeed * Time.deltaTime);
            transform.position = position;

            ageSeconds += Time.deltaTime;

            if (ageSeconds >= lifetimeSeconds)
            {
                Expired?.Invoke(this);
                Destroy(gameObject);
            }
        }

        public void Initialize(float bottomHeight)
        {
            this.bottomHeight = bottomHeight;
        }
    }
}
