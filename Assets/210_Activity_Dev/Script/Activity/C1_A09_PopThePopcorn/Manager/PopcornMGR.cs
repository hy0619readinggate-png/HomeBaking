using beyondi.Util;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C1_A09
{
    public class PopcornMGR : MonoBehaviour
    {
        // Properties
        public bool IsAllCollected => exams.Where(p => p.IsAnswer).All(p => p.IsCollected);
        public bool IsNoCollected => exams.Where(p => p.IsAnswer).All(p => !p.IsCollected);

        // Methods
        public void Setup(ProblemData problem, int extraCornCount, Character currentCh)
        {
            LOG.Info($"Setup() | {problem} {extraCornCount}", this);

            character = currentCh;
            tryCount = 0;
            C3_A07 = problem.C3_A07;

            // C3_A07 only
            if (C3_A07)
            {
                C3_A07_Q.Clear();
                problem.Alphabets.ForEach(w => C3_A07_Q.Enqueue(w));
                character?.SetupPopcornBox(C3_A07, problem.EmptyTextCount);
            }

            character?.ClearPopcornBox();
            clearPopcorns();
            buildPopcorns(problem, extraCornCount);
        }
        public Coroutine StartPop()
        {
            LOG.Info($"StartPop()", this);

            tryCount++;

            crPopThePopcorn = StartCoroutine(coPopThePopcorn());
            return crPopThePopcorn;
        }
        public void StopPop()
        {
            LOG.Info($"StopPop()", this);

            Util.SetActiveAllChildren(popcornParentTR, false);
            StopCoroutine(crPopThePopcorn);
        }



        // Fields
        private Popcorn[] popcorns = null;
        private PopcornExam[] exams = null;
        private Character character = null;
        private int tryCount = 0;
        private bool isAff = false;

        // Fields
        private Coroutine crPopThePopcorn = null;

        // Fields : C3_A07 only
        private bool C3_A07 = false;
        private Queue<string> C3_A07_Q = new Queue<string>();

        // Functions
        private void buildPopcorns(ProblemData problem, int extraCornCount)
        {
            var list = new List<Popcorn>();

            // 추가 팝콘 생성
            for (var i = 0; i < extraCornCount; i++)
            {
                var popcorn = Instantiate(popcornExtraPB, popcornParentTR);
                popcorn.gameObject.SetActive(false);
                list.Add(popcorn);
            }

            // 팝콘 생성
            foreach (var exam in problem.Examples)
            {
                var pb = UtilArray.ExtractOne(popcornPB);
                var popcorn = Instantiate(pb, popcornParentTR);
                popcorn.Setup(exam, popcornAniParam);
                popcorn.gameObject.SetActive(false);
                popcorn.OnSubmit += popcorn_OnSubmit;
                popcorn.OnCollected += popcorn_OnCollected;
                list.Add(popcorn);
            }

            popcorns = list.ToArray();
            exams = popcorns.Where(p => p is PopcornExam).Cast<PopcornExam>().ToArray();
        }
        private void clearPopcorns()
        {
            if (popcorns != null)
            {
                foreach (var p in popcorns)
                {
                    var exam = p as PopcornExam;
                    if (exam)
                    {
                        exam.OnSubmit -= popcorn_OnSubmit;
                        exam.OnCollected -= popcorn_OnCollected;
                    }
                }
            }
            popcorns = null;
            Util.RemoveAllChildren(popcornParentTR);
        }
        private void startAff()
        {
            isAff = true;
            popcorns.ForEach(p => p.StartAff());

            AudioMGR.One.PlayEffect(affordanceCLIP);
        }
        private void stopAff()
        {
            if (isAff)
            {
                isAff = false;
                popcorns.ForEach(p => p.StopAff());
            }
        }
        private void playWrongFX(PopcornExam popcorn)
        {
            var ps = Instantiate(wrongPS, transform);
            var main = ps.main;
            main.stopAction = ParticleSystemStopAction.Destroy;

            ps.transform.position = popcorn.transform.position;
            ps.gameObject.SetActive(true);
        }

        // Event Handlers
        private void popcorn_OnSubmit(PopcornExam popcorn)
        {
            LOG.Info($"popcorn_OnSubmit() | {popcorn.gameObject.name}", this);

            stopAff();

            if (C3_A07)
            {
                if (popcorn.IsAnswer && popcorn.Text == C3_A07_Q.Peek())
                {
                    wordBox.Correct();
                    popcorn.DoCorrect();
                    C3_A07_Q.Dequeue();
                }
                else
                {
                    ActivityProgress.One.Wrong();
                    popcorn.DoWrong(false);
                    playWrongFX(popcorn);
                }
            }
            else
            {
                if (popcorn.IsAnswer)
                {
                    popcorn.DoCorrect();
                }
                else
                {
                    ActivityProgress.One.Wrong();
                    popcorn.DoWrong();
                    playWrongFX(popcorn);
                }
            }

            if (IsAllCollected)
                exams.ForEach(ex => ex.DisableInteraction());

            if (!popcorn.IsAnswer)
                character?.Wrong();
        }
        private void popcorn_OnCollected(PopcornExam popcorn)
        {
            LOG.Info($"popcorn_OnCollected() | {popcorn.gameObject.name}", this);

            AudioMGR.One.PlayEffect(getPopcornCLIP);
            character?.FillPopcornBox();

            character?.Correct();
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private PopcornSpawner[] spawners = null;
        [SerializeField] private Transform popcornParentTR = null;
        [SerializeField] private PopcornExam[] popcornPB = null;
        [SerializeField] private PopcornExtra popcornExtraPB = null;
        [SerializeField] private ParticleSystem wrongPS = null;
        [Header("★ C3_A07 Only ")]
        [SerializeField] private WordBox wordBox = null;
        [Header("★ Popcorn ")]
        [SerializeField] private PopcornAniParam popcornAniParam = null;
        [Header("★ Audio ")]
        [SerializeField] private AudioClip getPopcornCLIP = null;
        [SerializeField] private AudioClip affordanceCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float spawnRatio = 10; // 초당 생성 개수
        [SerializeField] private float affDelay = 0.5f;
        [SerializeField] private int affOrder = 3;

        // Unity Inspectors : button
        [Button("(DEV)Setup", EButtonEnableMode.Playmode)]
        private void devSetup()
        {
            var problemData = new ProblemData
            {
                Index = 1,
                Text = "A",
                Examples = new ExampleData[]
                   {
                       new ExampleData { Text= "A", IsAnswer = true},
                       new ExampleData { Text= "A", IsAnswer = true},
                       new ExampleData { Text= "A", IsAnswer = true},
                       new ExampleData { Text= "B"},
                       new ExampleData { Text= "C"},
                       new ExampleData { Text= "D"},
                   }
            };

            Setup(problemData, 15, null);
        }
        [Button("(DEV)Pop", EButtonEnableMode.Playmode)]
        private void devPop()
        {
            StartPop();
        }

        // Unity Messages
        private void Awake()
        {
            wrongPS.gameObject.SetActive(false);
        }
        private void Start()
        {

        }

        // Unity Coroutine
        IEnumerator coPopThePopcorn()
        {
            using (LOG.Coroutine($"coPopThePopcorn()", this))
            {
                UtilArray.Shuffle(popcorns);

                var spawnerQ = new Queue<PopcornSpawner>(spawners);
                var intervalBase = 1 / spawnRatio;
                foreach (var popcorn in popcorns.Where(p => !p.IsCollected))
                {
                    var spawner = spawnerQ.Peek();
                    spawner.Spawn(popcorn);

                    spawnerQ.Enqueue(spawnerQ.Dequeue());

                    var interval = intervalBase * (1 + Random.Range(-0.2f, +0.2f));
                    yield return new WaitForSeconds(interval);
                }

                if (tryCount == affOrder && IsNoCollected)
                {
                    yield return new WaitForSeconds(affDelay);
                    startAff();
                }

                yield return coWaitAllPopcornOnGround();
            }
        }
        IEnumerator coWaitAllPopcornOnGround()
        {
            while (popcorns.Any(p => p.gameObject.activeSelf))
                yield return null;
        }
    }

    [System.Serializable]
    public class PopcornAniParam
    {
        public AudioClip correctCLIP = null;
        public AudioClip wrongCLIP = null;
        public AudioClip collectCLIP = null;

        public float correctDuration = 0.5f;
        public float wrongDuration = 0.5f;
        public float collectDuration = 1f;
        public float collectScale = 0.2f;
        public float collectAlpha = 0.5f;
        public float collectJumpPower = 3f;
        public int collectRotateCount = 2;

        public Transform collectTargetTR = null;
    }
}