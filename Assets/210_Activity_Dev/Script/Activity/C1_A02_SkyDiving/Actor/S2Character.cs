using beyondi.Util;
using DG.Tweening;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C1_A02
{
    public class S2Character : MonoBehaviour,
        IDropHandler
    {
        // Methods
        public void Setup(string answer, int characterID)
        {
            LOG.Info($"Setup() | {answer} {characterID}", this);

            txts.ForEach(t => t.text = answer);
            currentText = txts[characterID - 1];
            characters.SetActiveOnly(characterID - 1);

            currentCharacter = characters.Single(c => c.gameObject.activeSelf);
        }
        public Coroutine StartWaitAnswer()
        {
            LOG.Info($"StartWaitAnswer()", this);

            crWaitAnswer = StartCoroutine(coWaitAnswer());
            return crWaitAnswer;
        }
        public void StopWaitAnswer()
        {
            LOG.Info($"StopWaitAnswer()", this);

            this.StopCoroutineSafe(ref crWaitAnswer);
        }
        public Coroutine Landing()
        {
            return StartCoroutine(coLanding());
        }

        // Events
        public event Action OnWrong;



        // Fields
        private S2CharacterAni currentCharacter = null;
        private TextMeshProUGUI currentText = null;
        private Coroutine crWaitAnswer = null;
        private bool isAnswerSubmit = false;
        private bool isAnimating = false;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private S2CharacterAni[] characters = null;
        [SerializeField] private TextMeshProUGUI[] txts = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip wrongCLIP = null;
        [Header("★ Timing")]
        [SerializeField] private float wrongDurtaion = 1f;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coWaitAnswer()
        {
            isAnswerSubmit = false;
            yield return new WaitUntil(() => isAnswerSubmit);
        }
        IEnumerator coWrong()
        {
            isAnimating = true;
            yield return null;

            AudioMGR.One.PlayEffect(wrongCLIP);
            yield return null;

            currentCharacter.PlayAnimation(CharacterAnimation.Wrong);
            yield return new WaitForSeconds(wrongDurtaion);

            isAnimating = false;
            yield return null;
        }
        IEnumerator coLanding()
        {
            DOVirtual.DelayedCall(0.2f, () => currentText.gameObject.SetActive(true));
            yield return null;

            var ani = UtilRandom.RandomSuccess(0.5f) ? CharacterAnimation.Correct : CharacterAnimation.Correct2;
            yield return currentCharacter.PlayAnimationAndWait(ani, false);
            yield return null;

            currentText.gameObject.SetActive(false);
            yield return null;
        }



        // Interface : IDropHandler
        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            LOG.Info($"OnDrop() | {eventData.pointerDrag.name}", this);

            var parachute = eventData.pointerDrag.GetComponent<Parachute>();
            if (parachute != null)
            {
                if (!isAnimating)
                {
                    if (parachute.IsAnswer)
                    {
                        isAnswerSubmit = true;
                    }
                    else
                    {
                        ActivityProgress.One.Wrong();

                        StartCoroutine(coWrong());
                        parachute.Respawn();

                        OnWrong?.Invoke();
                    }

                    eventData.Use();
                }
            }
        }
    }
}