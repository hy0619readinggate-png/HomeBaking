using beyondi.Util;
using DoDoEng.Common;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Game.C1_G01
{
    public enum TimerState { Green, Yellow, Red, TimeOut };

    public class Timer : MonoBehaviour
    {
        // Properties
        public bool IsRunning { get; private set; }

        // Methods
        public void ResetTimer()
        {
            LOG.Info($"ResetTimer()", this);

            clockANIM.SetBool("Go", false);
            hurryUpGO.SetActive(false);
            gaugeIMG.fillAmount = 1;
        }
        public void StartTimer(float duration)
        {
            LOG.Info($"StartTimer() | {duration}", this);

            crTimer = StartCoroutine(coTimer(duration));
        }
        public void StopTimer()
        {
            LOG.Info($"StopTimer()", this);

            AudioMGR.One.StopEffectLL(this);    // StatePlayAudio로 재생된 타이머 효과음 중지
            this.StopCoroutineSafe(ref crTimer);
        }

        // Events
        public event Action<TimerState> OnStateChanged;



        // Fields
        private float[] limits;

        // Fields
        private TimerState currentState = TimerState.Green;
        private Coroutine crTimer = null;

        // Functions
        private void updateState(float ratio)
        {
            if ((int)currentState >= limits.Length)
                return;

            if (ratio < limits[(int)currentState])
                changeState(currentState + 1);
        }
        private void changeState(TimerState state)
        {
            currentState = state;
            hurryUpGO.SetActive(currentState == TimerState.Red);

            OnStateChanged?.Invoke(currentState);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Image gaugeIMG = null;
        [SerializeField] private Animator clockANIM = null;
        [SerializeField] private GameObject hurryUpGO = null;
        [Header("★ Config")]
        [SerializeField] private float limitYellow = 0.6f;
        [SerializeField] private float limitRed = 0.3f;

        // Unity Messages
        private void Awake()
        {
            limits = new float[] { limitYellow, limitRed };

            clockANIM.SetBool("Go", false);
            hurryUpGO.SetActive(false);
            gaugeIMG.fillAmount = 1;
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coTimer(float duration)
        {
            using (LOG.Coroutine($"coTimer() | {duration}", this))
            {
                changeState(TimerState.Green);

                IsRunning = true;
                clockANIM.SetBool("Go", true);
                hurryUpGO.SetActive(false);

                var remain = duration;
                while (remain > 0)
                {
                    var ratio = remain / duration;
                    gaugeIMG.fillAmount = ratio;

                    updateState(remain / duration);
                    yield return null;

                    remain -= Time.deltaTime;
                }

                IsRunning = false;
                clockANIM.SetBool("Go", false);
                changeState(TimerState.TimeOut);
            }
        }
    }
}