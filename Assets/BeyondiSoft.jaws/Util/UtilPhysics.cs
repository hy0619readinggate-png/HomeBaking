using UnityEngine;

namespace beyondi.Cube
{
    public static class UtilPhysics
    {
        public static Vector3 ComputeVelocityToTarget(Vector3 currentPos, Vector3 targetPos, float initialAngle)
        {
            // From https://chanuklee0227.blogspot.com/2017/02/unity.html 20230314 druidchoi

            var gravity = Physics.gravity.magnitude;
            var angle = initialAngle * Mathf.Deg2Rad;

            var planarTarget = new Vector3(targetPos.x, 0, targetPos.z);
            var planarPosition = new Vector3(currentPos.x, 0, currentPos.z);

            var distance = Vector3.Distance(planarTarget, planarPosition);
            var yOffset = currentPos.y - targetPos.y;

            var initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));
            var velocity = new Vector3(0f, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));

            var angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPosition) * (targetPos.x > currentPos.x ? 1 : -1);
            var finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;

            return finalVelocity;
        }
    }
}