using beyondi.Coroutine;
using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

using ActivityData = DoDoEng.Common.ActivityData_C2_A08;
using ProblemMGR = DoDoEng.Activity.C2_A08.C2_A08_ProblemMGR;

namespace DoDoEng.Activity.C2_A08
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C2_A08_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C2_A08_Campsite;
        private enum State
        {
            S1Intro,
            S1Problem, S1Solve, S1Correct, S1Wrong, S1ToS2,
            S2Collect, S2CollectFin, Next, S2ToS1,
            Collection, FadeOut, NextSet, Fade,
            Outro, Reward
        }

        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();

        // Fields
        private FSM<State> fsm = null;
        private S1Example submitedExam = null;
        private Coroutine crPlayWord = null;
        private int collectCount;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.S1Intro,         E_S1Intro,      X_S1Intro);
            fsm.AddState(State.S1Problem,       E_S1Problem,    X_S1Problem);
            fsm.AddState(State.S1Solve,         E_S1Solve,      X_S1Solve);
            fsm.AddState(State.S1Correct,       E_S1Correct,    X_S1Correct);
            fsm.AddState(State.S1Wrong,         E_S1Wrong,      X_S1Wrong);
            fsm.AddState(State.S1ToS2,          E_S1ToS2,       X_S1ToS2);
            fsm.AddState(State.S2Collect,       E_S2Collect,    X_S2Collect);
            fsm.AddState(State.S2CollectFin,    E_S2CollectFin, X_S2CollectFin);
            fsm.AddState(State.Next,            E_Next);
            fsm.AddState(State.S2ToS1,          E_S2ToS1,       X_S2ToS1);
            fsm.AddState(State.Collection,      E_Collection,   X_Collection);
            fsm.AddState(State.Fade,            E_Fade,         X_Fade);
            fsm.AddState(State.NextSet,         E_NextSet);
            fsm.AddState(State.Outro,           E_Outro);
            fsm.AddState(State.Reward,          E_Reward);
            #pragma warning restore format  
        }
        private void setupProblem(ProblemData pData)
        {
            s1ProbelmSign.Setup(pData);
            s1Examples.ForEach((i, e) => e.Setup(pData.Examples[i]));

            var answerIDX = Array.FindIndex(pData.Examples, e => e.IsAnswer);
            s1Aff.Setup(answerIDX);
        }
        private void setupAff()
        {
            if (pMGR.PNO == 1)
            {
                s2Aff.gameObject.SetActive(true);
                s2Aff.EnableAff = true;
                var fireFlyTRs = fireflyGroup.ActiveFireFlyTRs;
                var dropTRs = new Transform[] { s2AffDropPos };
                s2Aff.Setup(fireFlyTRs, dropTRs);
            }
            else
            {
                s2Aff.gameObject.SetActive(false);
                s2Aff.EnableAff = false;
            }
        }

        // Event Handlers
        private void s1ProbelmSign_OnClick()
        {
            LOG.Info($"s1ProbelmSign_OnClick()", this);

            crPlayWord = StartCoroutine(coPlayWord());
        }



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            evaluateTimeline(s1ProblemTL);

            stepPanelCG.SetActiveOnly(0);

            s1Examples.ForEach(e => e.EnableInteraction(false));
        }
        protected override async UniTask onPrepare(ActivityIndex actIDX)
        {
            await base.onPrepare(actIDX);

            // Load Unit Data
            await pMGR.Build(actIDX, CURRICULUM, TABLES);
            ActivityProgress.One.Setup(pMGR.BlanksCount);

            // setup
            setupProblem(pMGR.Current);
        }
        protected override void onStartActivity()
        {
            base.onStartActivity();

            playBGM();
            AudioMGR.One.PlayAmbient(ambientCLIP, ambientVolume);
            fsm.StartFSM(State.S1Intro, RunnerParam.SkipStateTo);
        }
        protected override void onFinishActivity()
        {
            base.onFinishActivity();

            if (fsm.CurrentState == State.S1Solve)
                CP(CheckPoint.UserFinish);

            stopBGM();
            AudioMGR.One.StopAmbient(true);
            fsm?.StopFSM();
        }
        protected override void onDebugNext()
        {
            switch (fsm.CurrentState)
            {
                case State.S1Intro: fsm.PerformTransition(State.S1Problem); break;
                case State.S1Problem: fsm.PerformTransition(State.S1Solve); break;
                case State.S1Solve:
                    submitedExam = s1Examples.First(e => e.IsAnswer);
                    fsm.PerformTransition(State.S1Correct);
                    break;
                case State.S1ToS2: fsm.PerformTransition(State.S2Collect); break;
                case State.S2Collect: fsm.PerformTransition(State.S2CollectFin); break;
                case State.S2CollectFin: fsm.PerformTransition(State.Next); break;
                case State.S2ToS1: fsm.PerformTransition(State.S1Problem); break;
                case State.Collection: fsm.PerformTransition(State.NextSet); break;
            }
        }
        protected override void onDebugForceFinish()
        {
            base.onDebugForceFinish();

            fsm.PerformTransition(State.Reward);
        }



        // FSM
        IEnumerator E_S1Intro()
        {
            CP(CheckPoint.Start);
            setupProblem(pMGR.Current);
            sideBottles.Init();
            outroBottles.Init();

            yield return null;

            yield return playTimeline(s1ProblemTL);
            yield return null;

            fsm.PerformTransition(State.S1Problem);
        }
        IEnumerator X_S1Intro()
        {
            yield return stopTimeline(s1ProblemTL);
            yield return null;
        }
        IEnumerator E_S1Problem()
        {
            AudioMGR.One.PlayEffect(problemAppearClip);
            s1Examples.ForEach(e => e.Appear());
            yield return new WaitForSeconds(1f);

            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.PhoneticCLIP);
            yield return null;

            fsm.PerformTransition(State.S1Solve);
        }
        IEnumerator X_S1Problem()
        {
            AudioMGR.One.StopNarration();
            yield return null;

            s1Examples.ForEach(e => e.Idle());
            yield return null;
        }
        IEnumerator E_S1Solve()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            s1ProbelmSign.EnableInteraction(true);
            s1Examples.ForEach(e => e.EnableInteraction(true));
            yield return null;

            var wait = new WaitForSubmit(this, s1Examples);
            yield return wait;

            submitedExam = wait.Submited as S1Example;
            if (submitedExam.IsAnswer)
                fsm.PerformTransition(State.S1Correct);
            else fsm.PerformTransition(State.S1Wrong);
        }
        IEnumerator X_S1Solve()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            s1ProbelmSign.EnableInteraction(false);
            s1Examples.ForEach(e => e.EnableInteraction(false));
            yield return null;

            this.StopCoroutineSafe(ref crPlayWord);
            yield return null;
        }
        IEnumerator E_S1Correct()
        {
            s1Characters.ForEach(c => c.Correct());
            yield return null;

            yield return submitedExam.StartCorrect();
            yield return new WaitForSeconds(0.5f);

            fsm.PerformTransition(State.S1ToS2);
        }
        IEnumerator X_S1Correct()
        {
            submitedExam.StopCorrect();
            yield return null;
        }
        IEnumerator E_S1Wrong()
        {
            ActivityProgress.One.Wrong();
            s1Characters.ForEach(c => c.Wrong());
            yield return null;
            
            yield return submitedExam.StartWrong();
            yield return new WaitForSeconds(0.7f);

            fsm.PerformTransition(State.S1Solve);
        }
        IEnumerator X_S1Wrong()
        {
            s1Characters.ForEach(c => c.Idle());
            yield return null;

            submitedExam.StopWrong();
            yield return null;
        }
        IEnumerator E_S1ToS2()
        {
            correctText.alpha = 0f;
            correctText.text = submitedExam.Word;
            yield return null;


            collectCount = UnityEngine.Random.Range(fireflyCountMin, fireflyCountMax + 1);
            yield return null;

            collectBottle.Setup(submitedExam.WordSPR, submitedExam.WordCLIP, collectCount);
            yield return null;

            fireflyGroup.Setup(collectCount);
            setupAff();
            yield return null;

            yield return playTimeline(s1ToS2, 0f);
            collectBottle.ShowNow();
            yield return null;

            fsm.PerformTransition(State.S2Collect);
        }
        IEnumerator X_S1ToS2()
        {
            collectBottle.ShowNow();
            yield return stopTimeline(s1ToS2);
            yield return null;
        }
        IEnumerator E_S2Collect()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            if (pMGR.PNO == 1)
                s2Aff.StartAffordance();
            fireflyGroup.EnableInteraction(true);
            yield return collectBottle.StartWaitCollectAll();

            fsm.PerformTransition(State.S2CollectFin);
        }
        IEnumerator X_S2Collect()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            fireflyGroup.EnableInteraction(false);
            fireflyGroup.Hide();
            collectBottle.StopWaitCollectAll();
            collectBottle.ShowFireflies();
            yield return null;
        }
        IEnumerator E_S2CollectFin()
        {
            if (!pMGR.Current.LastBottle)
                sideBottles.Setup(submitedExam.WordSPR, collectCount);
            outroBottles.Setup(submitedExam.WordSPR, collectCount);
            yield return null;

            yield return correctText.DOFade(1, 1).WaitForCompletion();

            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.PhoneticCLIP);
            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.PhoneticCLIP);
            yield return AudioMGR.One.PlayNarrationAndWait(submitedExam.WordCLIP);

            yield return collectBottle.StartCloseCork();

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_S2CollectFin()
        {
            DOTween.Kill(correctText);
            correctText.alpha = 1f;
            yield return null;

            AudioMGR.One.StopNarration();
            collectBottle.StopCloseCork();
            yield return null;
        }
        IEnumerator E_Next()
        {
            yield return null;

            if (pMGR.Current.LastBottle)
                fsm.PerformTransition(State.Collection);
            else if (pMGR.Next())
                fsm.PerformTransition(State.S2ToS1);
            else fsm.PerformTransition(State.Collection);
        }
        IEnumerator E_S2ToS1()
        {
            setupProblem(pMGR.Current);
            yield return null;

            yield return playTimeline(s2ToS1, 0f);

            fsm.PerformTransition(State.S1Problem);
        }
        IEnumerator X_S2ToS1()
        {
            yield return null;
        }
        IEnumerator E_Collection()
        {
            outroText.text = pMGR.Current.Text;
            outroBottles.Show();
            yield return null;

            yield return playTimelines(outroTL);
            yield return null;

            fsm.PerformTransition(State.NextSet);
        }
        IEnumerator X_Collection()
        {
            yield return stopTimeline(outroTL);
            yield return null;
        }
        IEnumerator E_NextSet()
        {
            yield return null;

            if (pMGR.Next())
                fsm.PerformTransition(State.Fade);
            else fsm.PerformTransition(State.Outro);
        }
        IEnumerator E_Fade()
        {
            yield return playTimeline(fadeOut);
            yield return null;

            yield return new WaitForSeconds(1f);

            yield return playTimeline(fadeIn);
            yield return null;

            fsm.PerformTransition(State.S1Intro);
        }
        IEnumerator X_Fade()
        {
            fadeOut.Stop();
            yield return stopTimeline(fadeIn);
            yield return null;
        }
        IEnumerator E_Outro()
        {
            CP(CheckPoint.Outro);
            yield return null;

            yield return new WaitForSeconds(1f);

            fsm.PerformTransition(State.Reward);
        }
        IEnumerator E_Reward()
        {
            CP(CheckPoint.Complete);
            yield return null;

            AudioMGR.One.PlayEffect(SfxMoment.Activity_Complete);
            yield return new WaitForSeconds(rewardDelay);

            complete();
            yield return null;

        }



        // Unity Inspectors
        [Header("★ Bindings - Panels")]
        [SerializeField] private CanvasGroup[] stepPanelCG = null;
        [Header("★ Bindings - S1.Problem")]
        [SerializeField] private ProblemSign s1ProbelmSign = null;
        [SerializeField] private S1Example[] s1Examples = null;
        [SerializeField] private Character[] s1Characters = null;
        [SerializeField] private SideBottles sideBottles = null;
        [SerializeField] private OutroBottles outroBottles = null;
        [SerializeField] private AffordanceS1 s1Aff = null;
        [Header("★ Bindings - S2.Collect")]
        [SerializeField] private TextMeshProUGUI correctText = null;
        [SerializeField] private CollectBottle collectBottle = null;
        [SerializeField] private FireflyGroup fireflyGroup = null;
        [SerializeField] private TextMeshProUGUI outroText = null;
        [SerializeField] private AffordanceS2 s2Aff = null;
        [SerializeField] private Transform s2AffDropPos = null;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector s1ProblemTL = null;
        [SerializeField] private PlayableDirector s1ToS2 = null;
        [SerializeField] private PlayableDirector s2ToS1 = null;
        [SerializeField] private PlayableDirector fadeIn = null;
        [SerializeField] private PlayableDirector fadeOut = null;
        [SerializeField] private PlayableDirector outroTL = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip problemAppearClip = null;
        [Header("★ Sound - Ambient")]
        [SerializeField] private AudioClip ambientCLIP = null;
        [Range(0, 100)]
        [SerializeField] int ambientVolume = 100;
        [Header("★ Configs")]
        [SerializeField] private int fireflyCountMin = 3;
        [SerializeField] private int fireflyCountMax = 5;
        [Header("★ Timing")]
        [SerializeField] private float rewardDelay = 1f;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            initFSM();
        }
        protected override void Start()
        {

        }
        protected override void OnEnable()
        {
            base.OnEnable();

            s1ProbelmSign.OnClick += s1ProbelmSign_OnClick;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            s1ProbelmSign.OnClick -= s1ProbelmSign_OnClick;
        }

        // Unity Coroutine
        IEnumerator coPlayWord()
        {
            using (LOG.Coroutine($"coPlayNarration()", this))
            {
                s1ProbelmSign.EnableInteraction(false);
                s1Examples.ForEach(e => e.EnableInteraction(false));
                yield return null;

                yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.PhoneticCLIP);

                s1ProbelmSign.EnableInteraction(true);
                s1Examples.ForEach(e => e.EnableInteraction(true));
                yield return null;
            }
        }
    }
}