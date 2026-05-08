using beyondi.Behaviour;
using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

namespace DoDoEng.Activity.C1_A11
{
    public class Gino : BYDSingleton<Gino>
    {
        // Methods
        public void Init(FlowerNormalParam param)
        {
            LOG.Info($"Init() | {param}", this);

            flowers.ForEach(f => f.Init(param));
        }
        public void Idle()
        {
            LOG.Info($"Idle()", this);

            ginoAni.PlayAnimationLoopT2(
                GinoAnimation.Idle1,
                GinoAnimation.Idle2,
                GinoAnimation.Idle3);
        }
        public void SurpriseAndRun()
        {
            LOG.Info($"SurpriseAndRun()", this);

            this.StopCoroutineSafe(ref crStartPanic);
            this.StopCoroutineSafe(ref crStopPanic);

            moveTw.Kill();

            crStartPanic = StartCoroutine(coStartPanic());
        }
        public Coroutine RelaxAndReturn()
        {
            LOG.Info($"RelaxAndReturn()", this);

            this.StopCoroutineSafe(ref crStartPanic);
            this.StopCoroutineSafe(ref crStopPanic);

            moveTw.Kill();

            crStopPanic = StartCoroutine(coStopPanic());
            return crStopPanic;
        }

        // Methods
        public void LocateBehindWagon()
        {
            LOG.Info($"LocateBehindWagon()", this);

            var wagonSiblingIndex = wagonTR.GetSiblingIndex();
            ginoAni.transform.SetSiblingIndex(wagonSiblingIndex);

        }
        public void LocateFrontOfWagon()
        {
            LOG.Info($"LocateFrontOfWagon()", this);

            ginoAni.transform.SetSiblingIndex(defaultSiblingIndex);
        }
        public Vector3 ReserveFlowerPosition()
        {
            LOG.Info($"ReserveFlowerPosition()", this);

            reservedIndex++;
            var flower = flowers[flowers.Length - reservedIndex - 1];
            return flower.transform.position + jumpPosDelta;
        }
        public void AddFlower(int typeIdx, int colorIdx, string alphabet)
        {
            LOG.Info($"AddFlower() | {typeIdx}, {colorIdx}, {alphabet}", this);

            LOG.Info($"moveTw {moveTw}", this);

            if (!isPanic)
            {
                ginoAni.PlayAnimation(GinoAnimation.Correct);

                var clip = UtilArray.ExtractOne(correctCLIP);
                AudioMGR.One.PlayEffectLL(clip);
            }

            collectedIndex++;
            var flower = flowers[flowers.Length - collectedIndex - 1];
            flower.Show(typeIdx, colorIdx, alphabet);
        }



        // Fields : caching
        private CollectFlower[] flowers_ = null;
        private CollectFlower[] flowers => flowers_ ??= GetComponentsInChildren<CollectFlower>(true);

        // Fields
        private float originScaleX = 1;
        private bool isPanic = false;

        private int reservedIndex = -1;
        private int collectedIndex = -1;
        private int defaultSiblingIndex = 0;

        private Coroutine crStartPanic = null;
        private Coroutine crStopPanic = null;
        private Tween moveTw = null;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GinoAni ginoAni = null;
        [SerializeField] private RectTransform[] runRegion = null;
        [SerializeField] private Transform idleTR = null;
        [SerializeField] private Transform wagonTR = null;
        [SerializeField] private GameObject runSFXGO = null;
        [Header("★ Audios")]
        [SerializeField] private AudioClip surpriseCLIP = null;
        [SerializeField] private AudioClip runCLIP = null;
        [SerializeField] private AudioClip jumpCLIP = null;
        [SerializeField] private AudioClip sighCLIP = null;
        [SerializeField] private AudioClip[] correctCLIP = null;
        [Header("★ Config")]
        [SerializeField] private Vector3 jumpPosDelta = Vector3.down;
        [SerializeField] private float panicRunSpeed = 9;
        [SerializeField] private float panicJumpProb = 0.3f;


        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            originScaleX = ginoAni.ScaleX;

            reservedIndex = -1;
            collectedIndex = -1;

            defaultSiblingIndex = ginoAni.transform.GetSiblingIndex();

            flowers.ForEach(f => f.Clear());

            runSFXGO.SetActive(false);
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coStartPanic()
        {
            using (LOG.Coroutine($"coStartPanic()", this))
            {
                isPanic = true;

                AudioMGR.One.PlayEffect(surpriseCLIP);
                yield return ginoAni.PlayAnimationAndWait(
                    GinoAnimation.Surprise,
                    GinoAnimation.Run);

                AudioMGR.One.PlayEffectLL(runCLIP, true);
                runSFXGO.SetActive(true);
                yield return null;

                var queue = new Queue<RectTransform>();
                var regions = UtilArray.Shuffled(runRegion);
                regions.ForEach(r => queue.Enqueue(r));

                while (true)
                {
                    var rt = queue.Peek();
                    var destX = UtilRandom.RandomPositionIn(rt).x;
                    yield return coRunTo(destX);
                    yield return null;
                    if (UtilRandom.RandomSuccess(panicJumpProb))
                        ginoAni.PlayAnimation(GinoAnimation.Surprise, GinoAnimation.Run);

                    queue.Enqueue(queue.Dequeue());
                    yield return null;
                }
            }
        }
        IEnumerator coStopPanic()
        {
            using (LOG.Coroutine($"coStopPanic()", this))
            {
                ginoAni.PlayAnimationLoop(GinoAnimation.Run);

                yield return coRunTo(idleTR.position.x);

                AudioMGR.One.StopEffectLL(true);
                runSFXGO.SetActive(false);
                yield return null;

                AudioMGR.One.PlayEffect(sighCLIP);
                yield return null;

                ginoAni.ScaleX = originScaleX;
                yield return ginoAni.PlayAnimationAndWait(GinoAnimation.Relax, false);

                isPanic = false;

                ginoAni.PlayAnimationLoopT2(
                    GinoAnimation.Idle1,
                    GinoAnimation.Idle2,
                    GinoAnimation.Idle3);
            }
        }

        IEnumerator coRunTo(float destX)
        {
            using (LOG.Coroutine($"coRunTo()", this))
            {
                var duration = Math.Abs(destX - ginoAni.transform.position.x) / panicRunSpeed;
                var scaleX = destX > ginoAni.transform.position.x ? 1 : -1;
                ginoAni.ScaleX = originScaleX * scaleX;
                moveTw = ginoAni.transform
                    .DOMoveX(destX, duration)
                    .SetEase(Ease.Linear);
                yield return moveTw.WaitForCompletion();
            }
        }
    }
}