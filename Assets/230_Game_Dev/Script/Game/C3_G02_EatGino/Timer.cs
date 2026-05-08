using DoDoEng.Common;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace DoDoEng.Game.C3_G02
{
    public class Timer : MonoBehaviour
    {
        // Definitions
        // Properties
        public bool IsTimePaused
        {
            get { return isTimePaused; }
            set { isTimePaused = value; }
        }

        // Methods
        public void Setup(float time)
        {
            maxTime = time;
        }
        public void Ready()
        {
            anim.SetTrigger(hashKey_Show);
        }
        public void Run()
        {
            timerCoroutine = StartCoroutine(coTimer());
        }
        public void Pause(bool isPaused)
        {
            isTimePaused = isPaused;
        }
        public void Stop()
        {
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }

            anim.SetTrigger(hashKey_Hide);
        }
        public void Clock(float clockTime)
        {
            if (clockCoroutine == null)
                clockCoroutine = StartCoroutine(coClock(clockTime));
        }
        public void Reset()
        {
            maxTime = 0;
            playTime = 0;
        }

        // Event
        public event Action OnTimeOver = null;



        // Fields : caching
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();

        // Fields : Anim
        private readonly int hashKey_Show = Animator.StringToHash("Show");
        private readonly int hashKey_Hide = Animator.StringToHash("Hide");
        private readonly int hashKey_TimeOver = Animator.StringToHash("TimeOver");
        private readonly int hashKey_FX = Animator.StringToHash("FX");

        // Fields
        private float maxTime = 0;
        private float playTime = 0;

        private bool isTimePaused = false;
        private Coroutine timerCoroutine = null;
        private Coroutine clockCoroutine = null;
        // Functions
        private void setTimerText()
        {
            int minutes = Mathf.FloorToInt(playTime / 60F);
            int seconds = Mathf.FloorToInt(playTime - minutes * 60);

            timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI timeText = null;
        [SerializeField] private AudioClip timeoverFX = null;
        [SerializeField] private AudioClip clockFX = null;

        // Unity Messages
        private void Awake()
        {

        }
        private void Start()
        {

        }

        // Unity Coroutine
        IEnumerator coTimer()
        {
            isTimePaused = false;

            bool loop = true;
            while (loop)
            {
                playTime = maxTime;

                while (playTime > 0)
                {
                    if (!isTimePaused)
                    {
                        playTime -= Time.deltaTime;
                        setTimerText();
                        yield return null;
                    }
                    else yield return null;
                }

                timeText.text = "00:00";

                AudioMGR.One.PlayEffect(timeoverFX);
                anim.SetTrigger(hashKey_TimeOver);
                yield return new WaitForSeconds(1.5f);

                OnTimeOver?.Invoke();
                yield return new WaitForSeconds(1f);

                anim.SetTrigger(hashKey_Hide);
                yield return null;
            }

            yield return null;

            StopCoroutine(coTimer());
            yield return null;
        }
        IEnumerator coClock(float clockTime)
        {
            yield return null;
            isTimePaused = true;

            float duration = 1.0f;
            float offset = clockTime / duration;
            float targetTime = playTime + clockTime;

            if (targetTime > maxTime) targetTime = maxTime;

            anim.SetTrigger(hashKey_FX);
            AudioMGR.One.PlayEffect(clockFX);

            yield return new WaitForSeconds(1.2f);

            while (playTime < targetTime)
            {
                playTime += offset * Time.deltaTime;
                setTimerText();
                yield return null;
            }

            AudioMGR.One.StopEffectLL();
            yield return new WaitForSeconds(1.0f);
            isTimePaused = false;
            anim.SetTrigger(hashKey_Show);

            yield return null;
            clockCoroutine = null;
        }
    }
}