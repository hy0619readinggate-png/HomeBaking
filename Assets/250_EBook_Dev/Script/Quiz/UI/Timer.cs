using beyondi.Util;
using DoDoEng.Common;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace DoDoEng.EBook.Quiz
{
    public class Timer : MonoBehaviour
    {
        // Properties
        public bool IsRunning { get; private set; }

        // Methods
        public void ResetTimer()
        {
            LOG.Info($"ResetTimer()", this);

        }
        public void StartTimer(float duration)
        {
            LOG.Info($"StartTimer() | {duration}", this);

            crTimer = StartCoroutine(coTimer(duration));
        }
        public void StopTimer()
        {
            LOG.Info($"StopTimer()", this);

            this.StopCoroutineSafe(ref crTimer);
        }
        public void DEV_SkipToEnd()
        {
            LOG.Info($"DEV_SkipToEnd()", this);

            remain = 10;
        }

        // Events
        public event Action OnTimeOver;



        // Fields
        private Coroutine crTimer = null;
        private Color originColor;
        private float remain;

        // Functions
        private void updateState(float second)
        {
            var m = (int)second / 60;
            var s = (int)second % 60;

            timeTXT.text = $"{m:D2}:{s:D2}";
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI timeTXT = null;
        [Header("★ Config")]
        [SerializeField] private Color redColor = Color.red;
        [SerializeField] private int redAtSecond = 30;

        // Unity Messages
        private void Awake()
        {
            originColor = timeTXT.color;
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coTimer(float duration)
        {
            using (LOG.Coroutine($"coTimer() | {duration}", this))
            {
                IsRunning = true;

                remain = duration;
                while (remain > 0)
                {
                    updateState(remain);
                    timeTXT.color = remain < redAtSecond ? redColor : originColor;
                    yield return null;

                    remain -= Time.deltaTime;
                }

                IsRunning = false;
                OnTimeOver?.Invoke();
            }
        }
    }
}