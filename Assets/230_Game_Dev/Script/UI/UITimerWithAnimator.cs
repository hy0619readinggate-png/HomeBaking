using beyondi.Util;
using DoDoEng.Common;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace DoDoEng.Game.Common
{
    public class UITimerWithAnimator : MonoBehaviour
    {
        // Methods
        public void DisplayTime(float duration)
        {
            LOG.Function(this, $"{duration}");

            anim.SetTrigger("Reset");
            displayTime(duration, 1);
        }
        public void StartTimer(float duration)
        {
            LOG.Function(this, $"{duration}");

            isRunning = true;
            isPaused = false;
            crTimer = StartCoroutine(coTimer(duration));
        }
        public void StopTimer()
        {
            LOG.Function(this);

            isRunning = false;
            isPaused = true;
            this.StopCoroutineSafe(ref crTimer);
        }
        public void ResumeTimer()
        {
            LOG.Function(this);

            isPaused = false;
        }
        public void PauseTimer()
        {
            LOG.Function(this);

            isPaused = true;
        }

        // Methods
        public void DEV_Skip(float duration)
        {
            LOG.Function(this, $"{duration}");

            if (isRunning)
                remain -= duration;
        }

        // Events
        public event Action OnTimeOut;



        // Fields
        private Coroutine crTimer = null;
        private bool isRunning = false;
        private bool isPaused = true;
        private float remain = 0;

        // Functions
        private void displayTime(float time, float ratio)
        {
            var m = (int)time / 60;
            var s = (int)time % 60;
            timeTXT.text = $"{m:d2}:{s:d2}";

            anim.SetFloat("Blend", 1 - ratio);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI timeTXT = null;
        [SerializeField] private Animator anim = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip startCLIP = null;
        [SerializeField] private AudioClip timeOutCLIP = null;


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
                AudioMGR.One.PlayEffect(startCLIP);
                anim.SetTrigger("Start");

                remain = duration;
                while (remain > 0)
                {
                    displayTime(remain, remain / duration);
                    yield return null;

                    if (!isPaused)
                        remain -= Time.deltaTime;
                }

                AudioMGR.One.PlayEffect(timeOutCLIP);
                anim.SetTrigger("Timeout");
                OnTimeOut?.Invoke();

                isRunning = false;
                isPaused = true;
            }
        }
    }
}