using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Activity.C4_A04
{
    public class Counter : MonoBehaviour
    {
        // Properties
        public bool IsComplete { get; private set; } = false;

        // Methods
        public void StartCountDown()
        {
            LOG.Info($"StartCountDown()", this);

            gameObject.SetActive(true);
            IsComplete = false;

            crCountDown = StartCoroutine(coCountDown());
        }
        public void FinishCountDown()
        {
            LOG.Info($"FinishCountDown()", this);

            this.StopCoroutineSafe(ref crCountDown);

            transform.SetActiveAllChildren(false);
            gameObject.SetActive(false);
        }



        // Fields
        private Coroutine crCountDown = null;



        // Unity Inspectors
        [Header("¡Ú Bindings")]
        [SerializeField] private AudioClip timerCLIP = null;

        // Unity Messages
        private void Awake()
        {
            transform.SetActiveAllChildren(false);
            gameObject.SetActive(false);
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coCountDown()
        {
            using (LOG.Coroutine($"coCountDown()", this))
            {
                for (int i = transform.childCount; i > 0; i--)
                {
                    transform.SetChildActiveOnly(i - 1);
                    AudioMGR.One.PlayEffect(timerCLIP);

                    yield return new WaitForSeconds(1f);
                }
                transform.SetActiveAllChildren(false);

                IsComplete = true;
            }
        }
    }
}