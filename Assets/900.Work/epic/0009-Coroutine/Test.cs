using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.epic
{
    public class Test : MonoBehaviour
    {
        private Coroutine crA;
        private Coroutine crB;
        private Coroutine crC;
        private Coroutine crD;

        // Unity Messages
        private void Awake()
        {

        }
        private void Start()
        {
            crA = StartCoroutine(A());

            //DOVirtual.DelayedCall(2, () =>
            //{
            //    StopCoroutine(cr);
            //    StopCoroutine(crB);
            //    StopCoroutine(crC);
            //    StopCoroutine(crD);
            //}, false);
            StartCoroutine(S(2));
        }

        // Unity Coroutine
        IEnumerator A()
        {
            using (LOG.Coroutine($"A()", this))
            {
                crB = StartCoroutine(B());
                yield return crB;

                yield return B();
            }
        }
        IEnumerator B()
        {
            using (LOG.Coroutine($"B()", this))
            {
                crC = StartCoroutine(D());
                yield return crC;

                //yield return StartCoroutine(C());
                //yield return C();
            }
        }
        IEnumerator C()
        {
            using (LOG.Coroutine($"C()", this))
            {
                crD = StartCoroutine(D());
                yield return crD;

                //yield return StartCoroutine(D());
                //yield return D();
            }
        }
        IEnumerator D()
        {
            using (LOG.Coroutine($"D()", this))
            {
                for (var i = 0; i < 10; i++)
                {
                    print(i);
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }
        IEnumerator S(float d)
        {
            yield return new WaitForSeconds(d);
            this.StopCoroutineSafe(ref crA);
            this.StopCoroutineSafe(ref crB);
            this.StopCoroutineSafe(ref crC);
            this.StopCoroutineSafe(ref crD);
        }
    }
}