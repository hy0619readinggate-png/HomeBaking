using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C3_A04
{
    public class Leader : MonoBehaviour
    {
        // Methods
        public Vector2? GetPosition(float distance)
        {
            return calculatePointAlongPath(distance);
        }
        public void ResetWayPoints()
        {
            LOG.Info($"ResetWayPoints()", this);

            wayPoints.Clear();
        }
        public void AddFollower(GameObject go)
        {
            LOG.Info($"AddFollower() | {go.name}", this);

            LOG.Assert(go != null, $"The gameObject must not be null.", this);

            var follower = go.GetComponent<Follower>();
            LOG.Assert(follower != null, $"The gameObject must have a Follower component.", this);

            followerCount++;
            var distance = leaderRadius + followerInterval * (followerCount - 1);
            var zOffset = followerZDelta * followerCount;
            follower.Follow(this, distance, zOffset);
        }
        public void ClearFollowers()
        {
            LOG.Info($"ClearFollowers()", this);

            followerCount = 0;
        }



        // Fields
        private List<Vector2> wayPoints = new List<Vector2>();
        private int followerCount = 0;

        // Functions
        private Vector2? calculatePointAlongPath(float distance)
        {
            if (wayPoints.Count == 0)
                return null;

            var totalDistance = 0f;
            var currentWaypoint = 0;
            while (currentWaypoint < wayPoints.Count - 1)
            {
                var curr = wayPoints[currentWaypoint];
                var next = wayPoints[currentWaypoint + 1];
                var dist = Vector3.Distance(curr, next);

                if (totalDistance + dist > distance)
                {
                    var remainingDistance = distance - totalDistance;
                    var t = remainingDistance / dist;
                    return Vector3.Lerp(curr, next, t);
                }

                totalDistance += dist;
                currentWaypoint++;
            }

            return null;
        }



        // Unity Inspectors
        [Header("★ Config - waypoint")]
        [SerializeField] private int wpMaxRecords = 100;
        [SerializeField] private float wpMinInterval = 0.2f;
        [Header("★ Config - follower")]
        [SerializeField] private float leaderRadius = 1.4f;
        [SerializeField] private float followerInterval = 1.4f;
        [SerializeField] private float followerZDelta = -1f;
        [Header("★ Dev")]
        [SerializeField] private bool drawGizmos = false;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
        private void FixedUpdate()
        {
            if (wayPoints.Count == 0 ||
                Vector2.Distance(transform.position, wayPoints[0]) > wpMinInterval)
                wayPoints.Insert(0, transform.position);

            while (wayPoints.Count > wpMaxRecords)
                wayPoints.RemoveAt(wayPoints.Count - 1);
        }
        private void OnDrawGizmos()
        {
            if (drawGizmos)
            {
                var points = wayPoints.Select(p => (Vector3)p).ToArray();
                Gizmos.color = Color.white;
                Gizmos.DrawLineStrip(points, false);
            }
        }
    }
}