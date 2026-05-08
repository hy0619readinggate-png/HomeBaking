using beyondi.Behaviour;
using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C2_A12
{
    public class ExampleMGR : BYDSingleton<ExampleMGR>
    {
        // Methods
        public void Setup(ExampleData[] exams)
        {
            LOG.Info($"Setup()", this);

            this.exams = exams;
            examIndices = UtilArray.Random(0, exams.Length - 1);
        }
        public void StartSpawnExample()
        {
            LOG.Info($"StartSpawnExample()", this);

            crSpawn = StartCoroutine(coSpawn());
            startRail();
            cg.blocksRaycasts = true;
        }
        public void Pause()
        {
            LOG.Info($"Pause()", this);

            stopRail();
            cg.blocksRaycasts = false;
        }
        public void Resume()
        {
            LOG.Info($"Resume()", this);

            startRail();
            cg.blocksRaycasts = true;
        }
        public void FinishSpawnExample()
        {
            LOG.Info($"FinishSpawnExample()", this);

            this.StopCoroutineSafe(ref crSpawn);
            cg.blocksRaycasts = false;
        }
        public void ExcludeExam(ExampleData exam)
        {
            LOG.Info($"ExcludeExam() | {exam.Word}", this);

            var idx = exams.FindIndex(exam);
            examIndices = examIndices.Where(e => e != idx).ToArray();
        }


        // Fields : caching
        private ExamplePool pool_ = null;
        private ExamplePool pool => pool_ ??= GetComponent<ExamplePool>();
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private RectTransform vRail1_ = null;
        private RectTransform vRail1 => vRail1_ ??= virtualRailANIM.transform.GetChild(0).GetComponent<RectTransform>();
        private RectTransform vRail2_ = null;
        private RectTransform vRail2 => vRail2_ ??= virtualRailANIM.transform.GetChild(1).GetComponent<RectTransform>();

        // Fields
        private ExampleData[] exams = null;
        private int[] examIndices = null;
        private Coroutine crSpawn = null;

        // Functions
        private ExampleData nextExamData()
        {
            var queue = new Queue<int>(examIndices);
            var idx = queue.Dequeue();
            queue.Enqueue(idx);
            examIndices = queue.ToArray();

            return exams[idx];
        }
        private void spawnExample()
        {
            var parentTR = getBehindVirtualRail();

            var example = pool.Get();
            example.Setup(nextExamData(), pickupScale, this);
            example.SetPosition(
                parentTR,
                exampleDragTR,
                exampleStartTR.position,
                exampleFinishTR.position);
        }
        private RectTransform getBehindVirtualRail()
        {
            return vRail1.position.x < vRail2.position.x ? vRail1 : vRail2;
        }
        private void startRail()
        {

            DOTween
                .To(s => imageRailANIM.speed = s,
                    0, railSpeed,
                    railStartDuration)
                .SetEase(railStartEase);
            DOTween
                .To(s => virtualRailANIM.speed = s,
                    0, railSpeed,
                    railStartDuration)
                .SetEase(railStartEase);
        }
        private void stopRail()
        {
            DOTween
                .To(s => imageRailANIM.speed = s,
                    railSpeed, 0,
                    railStopDuration)
                .SetEase(railStopEase);
            DOTween
                .To(s => virtualRailANIM.speed = s,
                    railSpeed, 0,
                    railStopDuration)
                .SetEase(railStopEase);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Transform exampleStartTR = null;
        [SerializeField] private Transform exampleFinishTR = null;
        [SerializeField] private Transform exampleDragTR = null;
        [SerializeField] private Animator imageRailANIM = null;
        [SerializeField] private Animator virtualRailANIM = null;
        [Header("★ Config")]
        [SerializeField] private float spawnDistance = 300f;     // canvas unit (pixel)
        [SerializeField] private float railSpeed = 0.5f;         // Ratio (0 ~ 1)
        [SerializeField] private float railStartDuration = 1f;
        [SerializeField] private float railStopDuration = 1f;
        [SerializeField] private Ease railStartEase = Ease.InQuad;
        [SerializeField] private Ease railStopEase = Ease.OutQuad;
        [SerializeField] private float pickupScale = 1f;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            imageRailANIM.speed = 0;
            virtualRailANIM.speed = 0;
            cg.blocksRaycasts = false;
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coSpawn()
        {
            using (LOG.Coroutine($"coSpawn()", this))
            {
                var distance = spawnDistance;
                var prevX = 0f;

                while (true)
                {
                    var railRT = getBehindVirtualRail();
                    var X = railRT.anchoredPosition.x;
                    var deltaX = Mathf.Clamp(X - prevX, 0, 2000);

                    distance -= deltaX;
                    if (distance < 0)
                    {
                        spawnExample();
                        distance += spawnDistance;
                    }

                    prevX = X;
                    yield return null;
                }
            }
        }
    }
}