using DoDoEng.Common;
using NaughtyAttributes;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UI;

namespace DoDoEng.Activity.C1_A01
{
    public class DotLocator : MonoBehaviour
    {
        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private SplineContainer pathSC = null;
        [Header("★ Locate")]
        [SerializeField] private float padStart = 0;
        [SerializeField] private float spaceFirst = 120f;
        [SerializeField] private float space = 100f;
        [Button("Locate")]
        private void locate()
        {
            var dots = GetComponentsInChildren<Dot>(true);

            if (pathSC == null)
            {
                LOG.Warning("No path is specified", this);
                return;
            }
            if (dots.Length < 2)
            {
                LOG.Warning("Child Dot must be exists", this);
                return;
            }

            var d = padStart;

            dots.First().Locate(pathSC, d);
            d += spaceFirst;

            foreach (var dot in dots.Skip(1))
            {
                dot.Locate(pathSC, d);
                d += space;
            }
        }
        [Button("Locate Evenly")]
        private void locateEvenly()
        {
            var dots = GetComponentsInChildren<Dot>(true);

            if (pathSC == null)
            {
                LOG.Warning("No path is specified", this);
                return;
            }
            if (dots.Length < 2)
            {
                LOG.Warning("Child Dot must be exists", this);
                return;
            }


            var length = SplineUtility.CalculateLength(pathSC.Spline, Matrix4x4.identity);
            var space = length / (dots.Length - 1);

            var d = 0f;
            foreach (var dot in dots)
            {
                dot.Locate(pathSC, d);
                d += space;
            }
        }

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}