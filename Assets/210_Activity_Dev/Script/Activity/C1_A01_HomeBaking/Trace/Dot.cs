using UnityEngine;
using UnityEngine.Splines;

namespace DoDoEng.Activity.C1_A01
{
    public class Dot : MonoBehaviour
    {
        // Methods
        public void Locate(SplineContainer path, float distance)
        {
            pathSC = path;
            distanceFromStart = distance;

            locate();
        }



        // Functions
        private void locate()
        {
            if (pathSC != null)
            {
                var length = SplineUtility.CalculateLength(pathSC.Spline, Matrix4x4.identity);
                var t = distanceFromStart / length;

                var ptWorld = pathSC.EvaluatePosition(t);
                transform.position = ptWorld;
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private SplineContainer pathSC = null;
        [Header("★ Config")]
        [SerializeField] private float distanceFromStart = 100;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
        private void OnValidate()
        {
            locate();
        }
    }
}