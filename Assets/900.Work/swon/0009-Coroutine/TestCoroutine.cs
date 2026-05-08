using beyondi.Coroutine;
using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng
{
    public class TestCoroutine : MonoBehaviour
    {
        // Fields
        private Coroutine affCR = null;
        private bool isRunning = false;
        private Button selectedBTN = null;

        // Functions
        private void finishAff()
        {
            if (isRunning)
            {
                StartCoroutine(coFinishAff());
                isRunning = false;
            }
        }

        // Event Handlers
        private void startAffBTN_onClick()
        {
            selectedBTN = startDotweenBTN;
            affCR = StartCoroutine(coStartAff());

            isRunning = true;
        }
        private void finishAffBTN_onClick()
        {
            this.StopCoroutineSafe(ref affCR);
            DOTween.Kill(affTargetGO.transform);
            finishAff();
        }
        private void startAffAnimBTN_onClick()
        {
            selectedBTN = startAnimBTN;
            affCR = StartCoroutine(coStartAff());

            isRunning = true;
        }
        private void finishAffAnimBTN_onClick()
        {
            this.StopCoroutineSafe(ref affCR);
            //affAnim.SetTrigger("hidden");
            finishAff();
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Button startDotweenBTN = null;
        [SerializeField] private Button finishDotweenBTN = null;
        [SerializeField] private Button startAnimBTN = null;
        [SerializeField] private Button finishAnimBTN = null;
        [SerializeField] private GameObject affTargetGO = null;
        [SerializeField] private Animator affAnim = null;

        // Unity Messages
        private void Awake()
        {
            startDotweenBTN.onClick.AddListener(startAffBTN_onClick);
            finishDotweenBTN.onClick.AddListener(finishAffBTN_onClick);
            startAnimBTN.onClick.AddListener(startAffAnimBTN_onClick);
            finishAnimBTN.onClick.AddListener(finishAffAnimBTN_onClick);
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coStartAff()
        {
            using (LOG.Coroutine($"coStartAff()", this))
            {
                LOG.Info($"Call onStartAff()", this);
                yield return onStartAff();
                yield return null;

                LOG.Info($"Call StartCoroutine(onStartAff())", this);
                yield return StartCoroutine(onStartAff());

                finishAff();
            }
        }
        IEnumerator coFinishAff()
        {
            yield return onFinishAff();
        }
        IEnumerator onStartAff()
        {
            using (LOG.Coroutine($"onStartAff()", this))
            {
                var startPos = new Vector3(-5, 0, 0);
                var endPos = new Vector3(5, 0, 0); ;

                if (selectedBTN == startDotweenBTN)
                {
                    affTargetGO.transform.position = startPos;
                    affTargetGO.SetActive(true);
                    yield return new WaitForSeconds(1f);

                    var duration = Vector2.Distance(startPos, endPos) / 10;
                    yield return affTargetGO.transform.DOMove(endPos, duration)
                        .SetEase(Ease.Linear)
                        .WaitForCompletion();

                    yield return DOVirtual.DelayedCall(3f, () => { LOG.Info($"DelayedCall", this); }).WaitForCompletion();
                    affTargetGO.transform.position = endPos;
                    yield return new WaitForSeconds(1f);
                }
                else
                {
                    affAnim.SetTrigger("shown");
                    yield return new WaitForSeconds(1f);

                    yield return affAnim.PlayAndWait("affordance");

                    yield return new WaitForSeconds(1f);
                }

                LOG.Important($"Coroutine in Coroutine Complete", this);

            }
        }
        IEnumerator onFinishAff()
        {
            using (LOG.Coroutine($"onFinishAff()", this))
            {
                affTargetGO.SetActive(false);
                yield return null;
            }
        }
        IEnumerator coTest()
        {
            using (LOG.Coroutine($"coTest()", this))
            {
                while (true)
                {
                    LOG.Info($"Test", this);

                    yield return new WaitForSeconds(1);
                }
            }

        }
    }
}