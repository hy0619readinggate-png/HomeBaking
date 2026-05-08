using beyondi.Coroutine;
using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

using ActivityData = DoDoEng.Common.ActivityData_C4_A06;
using ProblemMGR = DoDoEng.Activity.C2_A04.C4_A06_ProblemMGR;

namespace DoDoEng.Activity.C2_A04
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C4_A06_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C4_A06_ToUnderground;
        private enum State
        {
            Intro,
            Problem, Solve, Correct, Wrong,
            Bubble, Return, Next,
            ToTreasure, Treasure,
            Outro, Reward
        }

        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();

        // Fields
        private FSM<State> fsm = null;
        private Coroutine crPlayWord;
        private Coroutine crPlayBackgroundLoop;
        private StingrayExam submitedExam;
        private PlayableDirector currentTL = null;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Intro,       E_Intro,        X_Intro);
            fsm.AddState(State.Problem,     E_Problem,      X_Problem);
            fsm.AddState(State.Solve,       E_Solve,        X_Solve);
            fsm.AddState(State.Correct,     E_Correct,      X_Correct);
            fsm.AddState(State.Wrong,       E_Wrong,        X_Wrong);
            fsm.AddState(State.Bubble,      E_Bubble,       X_Bubble);
            fsm.AddState(State.Return,      E_Return,       X_Return);
            fsm.AddState(State.Next,        E_Next);
            fsm.AddState(State.ToTreasure,  E_ToTreasure,   X_ToTreasure);
            fsm.AddState(State.Treasure,    E_Treasure,     X_Treasure);
            fsm.AddState(State.Outro,       E_Outro,        X_Outro);
            fsm.AddState(State.Reward,      E_Reward);
            #pragma warning restore format  
        }
        private void setupProblem(ProblemData pData)
        {
            problem.Setup(pData.WordSPR);
            stingrayGroup.Setup(pData.Examples);
            var answerIDX = Array.FindIndex(pData.Examples, e => e.IsAnswer);
            affordanceSolve.Setup(answerIDX);
        }

        // Event Handlers
        private void problemButton_OnClick()
        {
            LOG.Info($"problemButton_OnClick()", this);

            crPlayWord = StartCoroutine(coPlayWord());
        }



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            evaluateTimeline(introTL);

            stepPanelCG.SetActiveOnly(0);

            affordanceSolve.gameObject.SetActive(false);
            affordanceTreasure.gameObject.SetActive(false);
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

            fsm.StartFSM(State.Intro, RunnerParam.SkipStateTo);
        }
        protected override void onFinishActivity()
        {
            base.onFinishActivity();

            if (fsm.CurrentState == State.Solve)
                CP(CheckPoint.UserFinish);

            stopBGM();
            AudioMGR.One.StopAmbient(true);

            fsm?.StopFSM();
        }
        protected override void onDebugNext()
        {
            switch (fsm.CurrentState)
            {
                case State.Intro: fsm.PerformTransition(State.Problem); break;
                case State.Problem: fsm.PerformTransition(State.Solve); break;
                case State.Solve: fsm.PerformTransition(State.Correct); break;
                case State.Correct: fsm.PerformTransition(State.Bubble); break;
                case State.Bubble: fsm.PerformTransition(State.Return); break;
                case State.Return: fsm.PerformTransition(State.Next); break;
                case State.ToTreasure: fsm.PerformTransition(State.Treasure); break;
                case State.Treasure: fsm.PerformTransition(State.Outro); break;
                case State.Outro: fsm.PerformTransition(State.Reward); break;
            }
        }
        protected override void onDebugForceFinish()
        {
            base.onDebugForceFinish();

            fsm.PerformTransition(State.Reward);
        }



        // FSM
        IEnumerator E_Intro()
        {
            CP(CheckPoint.Start);
            yield return playTimeline(introTL, 0);

            fsm.PerformTransition(State.Problem);
        }
        IEnumerator X_Intro()
        {
            yield return stopTimeline(introTL);
        }
        IEnumerator E_Problem()
        {
            setupProblem(pMGR.Current);

            yield return playTimeline(problemTL, 0);
            yield return null;

            AudioMGR.One.PlayNarrationAndWait(pMGR.Current.WordCLIP);
            yield return stingrayGroup.Show();
            yield return null;

            fsm.PerformTransition(State.Solve);
        }
        IEnumerator X_Problem()
        {
            yield return stopTimeline(problemTL);
            yield return null;

            AudioMGR.One.StopNarration();
            stingrayGroup.Idle();
            yield return null;

            this.StopCoroutineSafe(ref crPlayWord);
            yield return null;
        }
        IEnumerator E_Solve()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            affordanceSolve.gameObject.SetActive(true);
            problem.EnableInteraction(true);
            stingrayGroup.EnableInteraction(true);
            yield return null;

            var ramainExamples = stingrayGroup.Exams.Where(e => !e.IsSubmit).ToArray();
            var wait = new WaitForSubmit(this, ramainExamples);
            yield return wait;

            submitedExam = wait.Submited as StingrayExam;
            if (submitedExam.IsAnswer)
                fsm.PerformTransition(State.Correct);
            else fsm.PerformTransition(State.Wrong);
        }
        IEnumerator X_Solve()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            affordanceSolve.gameObject.SetActive(false);
            problem.EnableInteraction(false);
            stingrayGroup.EnableInteraction(false);
            yield return null;
        }
        IEnumerator E_Correct()
        {
            stingrayGroup.Hide();
            yield return null;

            AudioMGR.One.PlayEffect(correctCLIP);
            currentTL = UtilArray.ExtractOne(answerTL);
            yield return playTimeline(currentTL, 0);

            fsm.PerformTransition(State.Bubble);
        }
        IEnumerator X_Correct()
        {
            yield return stopTimeline(currentTL);
            yield return null;
        }
        IEnumerator E_Wrong()
        {
            ActivityProgress.One.Wrong();
            currentTL = UtilArray.ExtractOne(wrongTL);
            yield return playTimeline(currentTL, 0);
            yield return null;

            fsm.PerformTransition(State.Solve);
        }
        IEnumerator X_Wrong()
        {
            yield return stopTimeline(currentTL);
            yield return null;
        }
        IEnumerator E_Bubble()
        {
            characterMover.EnableInteraction(true);
            crPlayBackgroundLoop = StartCoroutine(coPlayBackgroundLoop());
            yield return bubbleMGR.StartWaitPopAll(pMGR.Current);

            fsm.PerformTransition(State.Return);
        }
        IEnumerator X_Bubble()
        {
            characterMover.EnableInteraction(false);
            bubbleMGR.FinishWaitPopAll();
            yield return null;
        }
        IEnumerator E_Return()
        {
            yield return new WaitForSeconds(returnCenterPreDelay);
            yield return characterMover.ReturnToCenter(pMGR.PNO != pMGR.Count);
            yield return crPlayBackgroundLoop;

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_Return()
        {
            characterMover.CenterNow();
            this.StopCoroutineSafe(ref crPlayBackgroundLoop);
            yield return null;
        }
        IEnumerator E_Next()
        {
            if (pMGR.Next())
                fsm.PerformTransition(State.Problem);
            else fsm.PerformTransition(State.ToTreasure);
            yield return null;
        }
        IEnumerator E_ToTreasure()
        {
            yield return playTimeline(toTreasureTL);
            yield return null;

            fsm.PerformTransition(State.Treasure);
        }
        IEnumerator X_ToTreasure()
        {
            yield return stopTimeline(toTreasureTL);
            yield return null;
        }
        IEnumerator E_Treasure()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            affordanceTreasure.gameObject.SetActive(true);
            affordanceTreasure.StartAffordance();
            treasure.EnableInteraction(true);
            var wait = new WaitForSubmit(this, treasure);
            yield return wait;

            fsm.PerformTransition(State.Outro);
        }
        IEnumerator X_Treasure()
        {
            CP(CheckPoint.UserFinish);

            affordanceTreasure.gameObject.SetActive(false);
            treasure.EnableInteraction(false);
            yield return null;
        }
        IEnumerator E_Outro()
        {
            CP(CheckPoint.Outro);
            yield return null;

            yield return playTimeline(outroTL);
            yield return null;

            fsm.PerformTransition(State.Reward);
        }
        IEnumerator X_Outro()
        {
            yield return stopTimeline(outroTL);
            yield return null;
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
        [Header("★ Bindings - Activity")]
        [SerializeField] private Problem problem = null;
        [SerializeField] private StingrayGroup stingrayGroup = null;
        [SerializeField] private BubbleMGR bubbleMGR = null;
        [SerializeField] private CharacterMover characterMover = null;
        [SerializeField] private SubmitButton treasure = null;
        [SerializeField] private Affordance affordanceSolve = null;
        [SerializeField] private AffGameObject affordanceTreasure = null;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector introTL = null;
        [SerializeField] private PlayableDirector problemTL = null;
        [SerializeField] private PlayableDirector[] answerTL = null;
        [SerializeField] private PlayableDirector bubbleTL = null;
        [SerializeField] private PlayableDirector[] wrongTL = null;
        [SerializeField] private PlayableDirector toTreasureTL = null;
        [SerializeField] private PlayableDirector outroTL = null;
        [Header("★ Timing")]
        [SerializeField] private float rewardDelay = 1f;
        [SerializeField] private float returnCenterPreDelay = 1f;
        [Header("★ Sound")]
        [SerializeField] private AudioClip correctCLIP = null;
        [Header("★ Sound - Ambient")]
        [SerializeField] private AudioClip ambientCLIP = null;
        [Range(0, 100)]
        [SerializeField] int ambientVolume = 100;



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

            problem.OnClick += problemButton_OnClick;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            problem.OnClick -= problemButton_OnClick;
        }

        // Unity Coroutine
        IEnumerator coPlayWord()
        {
            using (LOG.Coroutine($"coPlayWord()", this))
            {
                problem.EnableInteraction(false);
                yield return null;

                yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.WordCLIP);
                yield return null;

                problem.EnableInteraction(true);
                yield return null;
            }
        }
        IEnumerator coPlayBackgroundLoop()
        {
            using (LOG.Coroutine($"coPlayBackgroundLoop()", this))
            {
                while (!characterMover.IsComplete)
                    yield return playTimeline(bubbleTL, 0);
            }
        }
    }
}