using beyondi.Util;
using DoDoEng.Common;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace DoDoEng.Game.Common
{
    public class UITimer : MonoBehaviour
    {
        // Methods
        public void DisplayTime(float duration)
        {
            LOG.Function(this, $"{duration}");

            displayTime(duration);
        }
        public void StartTimer(float duration)
        {
            LOG.Function(this, $"{duration}");

            crTimer = StartCoroutine(coTimer(duration));
        }
        public void StopTimer()
        {
            LOG.Function(this);

            this.StopCoroutineSafe(ref crTimer);
        }

        // Events
        public event Action OnTimeOut;



        // Fields
        private Coroutine crTimer = null;

        // Functions
        private void displayTime(float time)
        {
            var m = (int)time / 60;
            var s = (int)time % 60;
            timeTXT.text = $"{m:d2}:{s:d2}";
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI timeTXT = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coTimer(float duration)
        {
            using (LOG.Coroutine($"{nameof(coTimer)}() | {duration}", this))
            {
                var remain = duration;
                while (remain > 0)
                {
                    displayTime(remain);
                    yield return null;

                    remain -= Time.deltaTime;
                }

                OnTimeOut?.Invoke();
            }
        }
    }
}