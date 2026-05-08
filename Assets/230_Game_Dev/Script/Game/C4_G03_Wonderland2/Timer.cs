using beyondi.Util;
using DoDoEng.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DoDoEng.Game.C4_G03
{
    public class Timer : MonoBehaviour
    {

        // Methods
        public void Setup(float time)
        {
            maxTime = time;
        }
        public void SetChance(int chance)
        {
            challenge = chance;
        }
        public void Ready()
        {
            _ANI.SetFloat(hashKey_Blend, 0f);
        }
        public void Show()
        {
            _ANI.SetTrigger(hashKey_Show);
        }
        public void Run()
        {
            timerCoroutine = StartCoroutine(coTimer());
        }
        public void Stop()
        {
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                timerCoroutine = null;
            }

            _ANI.SetTrigger(hashKey_Hide);
        }



        // Event
        public event Action OnTimerRing = null;
        public event Action OnTimeOver = null;
        public event Action OnPlayerTeeter = null;



        // Fields
        private readonly int hashKey_Show = Animator.StringToHash("Show");
        private readonly int hashKey_Hide = Animator.StringToHash("Hide");
        private readonly int hashKey_TimeOver = Animator.StringToHash("TimeOver");
        private readonly int hashKey_GameOver = Animator.StringToHash("GameOver");
        private readonly int hashKey_Blend = Animator.StringToHash("Blend");

        private float maxTime = 0;
        private int challenge = 0;
        private int timeInterval = 5;
        private Coroutine timerCoroutine = null;

        // Functions
        private List<int> SetRndTimeStamp()
        {
            List<int> rndTimeStamp = new();
            int minTimeInterval = (int)maxTime / timeInterval;

            for (int i = 0; i < timeInterval; i++)
            {
                int stampNum = UtilArray.RandomOne(minTimeInterval * i, minTimeInterval * (i + 1));
                rndTimeStamp.Add(stampNum);
            }

            return rndTimeStamp;
        }
        private void limitBlockMoving(bool canMove)
        {
            var blockList = FindObjectsOfType<Block>();

            foreach (var b in blockList)
            {
                if (b.StartParent != null) b.ResetBlockPosition();
                b.enabled = canMove;
            }
        }



        // Unity Inspectors
        [SerializeField] private Animator _ANI;
        [SerializeField] private AudioClip _SlideSFX = null;

        // Unity Coroutines
        IEnumerator coTimer()
        {
            var rndTimeStamp = SetRndTimeStamp();
            bool loop = true;
            while (loop)
            {
                float time = 0;
                int idx = 0;

                limitBlockMoving(true);

                while (time < maxTime)
                {
                    time += Time.deltaTime;
                    yield return null;

                    _ANI.SetFloat(hashKey_Blend, time / maxTime);

                    if (idx < timeInterval)
                    {
                        if (((int)time).Equals(rndTimeStamp[idx]))
                        {
                            OnPlayerTeeter?.Invoke();
                            idx++;
                        }
                    }
                }

                if(challenge > 1)
                {
                    _ANI.SetTrigger(hashKey_TimeOver);
                    limitBlockMoving(false);
                    OnTimerRing?.Invoke();
                    yield return new WaitForSeconds(1.5f);

                    AudioMGR.One.PlayEffect(_SlideSFX);
                    yield return new WaitForSeconds(1f);


                    OnTimeOver?.Invoke();
                    yield return new WaitForSeconds(1f);


                    _ANI.SetFloat(hashKey_Blend, 0f);
                    yield return new WaitForSeconds(1.5f);

                    challenge--;
                }
                else
                {
                    _ANI.SetTrigger(hashKey_GameOver);
                    limitBlockMoving(false);
                    OnTimerRing?.Invoke();
                    yield return new WaitForSeconds(1.5f);

                    OnTimeOver?.Invoke();
                    yield return new WaitForSeconds(1f);

                    loop = false;
                }
            }


            yield return null;
        }
    }
}