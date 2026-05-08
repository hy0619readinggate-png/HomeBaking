using DoDoEng.Common;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

namespace DoDoEng.Activity.C1_A06
{
    public class RollerCoaster : MonoBehaviour
    {
        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Transform[] cartsTR = null;
        [SerializeField] private SplineContainer pathSC = null;
        [Header("★ Config")]
        [SerializeField] private float speed = 0.1f; // deltaT / sec

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
        private void OnEnable()
        {
            StartCoroutine(coDrive());
        }

        // Unity Coroutine
        IEnumerator coDrive()
        {
            using (LOG.Coroutine($"coDrive()", this))
            {
                // 현재 카트 위치를 Spline에 매칭
                var cartT = new float[cartsTR.Length];
                for (var i = 0; i < cartT.Length; i++)
                {
                    var pos = cartsTR[i].localPosition;
                    SplineUtility.GetNearestPoint(pathSC.Spline, pos, out var nearest, out var t);

                    cartsTR[i].localPosition = nearest;
                    cartT[i] = t;
                }

                while (cartT.Last() < 1)
                {
                    var deltaT = speed * Time.deltaTime;

                    for (var i = 0; i < cartT.Length; i++)
                    {
                        cartT[i] += deltaT;
                        pathSC.Evaluate(cartT[i], out var position, out var tangent, out _);

                        var angle = -Mathf.Atan2(tangent.x, tangent.y) - Mathf.PI / 2;

                        cartsTR[i].position = position;
                        cartsTR[i].rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
                    }

                    yield return null;
                }
                yield return null;
            }
        }
    }
}