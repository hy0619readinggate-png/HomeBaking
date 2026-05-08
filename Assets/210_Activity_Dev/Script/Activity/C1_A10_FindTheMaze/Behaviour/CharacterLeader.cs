using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C1_A10
{
    public class CharacterLeader : MonoBehaviour
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
        public void EnableRecordPath(bool enable)
        {
            LOG.Info($"EnableRecordPath() | {enable}", this);

            enableRecordPath = enable;
        }



        // Fields
        private List<Vector2> wayPoints = new List<Vector2>();
        private bool enableRecordPath = false;

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
        [Header("★ Config")]
        [SerializeField] private int maxRecords = 100;
        [SerializeField] private float minInterval = 0.2f;
        [Header("★ Dev")]
        [SerializeField] private bool drawGizmos = false;

        // Unity Messages
        private void Awake()
        {
            enableRecordPath = true;
        }
        private void Start()
        {
        }
        private void FixedUpdate()
        {
            if (enableRecordPath)
            {
                if (wayPoints.Count == 0 ||
                    Vector2.Distance(transform.position, wayPoints[0]) > minInterval)
                    wayPoints.Insert(0, transform.position);

                while (wayPoints.Count > maxRecords)
                    wayPoints.RemoveAt(wayPoints.Count - 1);
            }
        }
        private void OnDrawGizmos()
        {
            if (drawGizmos)
            {
                var points = wayPoints.Select(p => (Vector3)p).ToArray();
                Gizmos.color = Color.blue;
                Gizmos.DrawLineStrip(points, false);
            }
        }
    }
}