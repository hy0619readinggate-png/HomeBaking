using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C1_A07
{
    public class Obstacle : MonoBehaviour
    {
        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
        private void Update()
        {
            var currentPosition = transform.position;
            var distanceX = Track.One.ObstacleSpeed * Time.deltaTime;
            transform.position = new Vector2(currentPosition.x - distanceX, currentPosition.y);
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            LOG.Info($"OnTriggerEnter2D() | {collision.gameObject.name}", this);

            Destroy(this.gameObject);
        }
    }
}