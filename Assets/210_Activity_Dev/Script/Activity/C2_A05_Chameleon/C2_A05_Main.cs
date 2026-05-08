using beyondi.Coroutine;
using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

using ActivityData = DoDoEng.Common.ActivityData_C2_A05;
using ProblemMGR = DoDoEng.Activity.C2_A05.C2_A05_ProblemMGR;

namespace DoDoEng.Activity.C2_A05
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C2_A05_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C2_A05_Chameleon;
        private enum State
        {
            Intro,
            ProblemIn, ProblemNarr, BeeIn,
            Solve, Eat, Correct, Wrong, Next, NextProblem,
            Outro, Reward
        }



        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();
        private TimelineSignal chameleonOutSIG_ = null;
        private TimelineSignal chameleonOutSIG => chameleonOutSIG_ ??= chameleonOutTL.GetComponent<TimelineSignal>();

        // Fields
        private FSM<State> fsm = null;
        private Bee submitBee = null;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Intro,       E_Intro);
            fsm.AddState(State.ProblemIn,   E_ProblemIn,    X_ProblemIn);
            fsm.AddState(State.BeeIn,       E_BeeIn,        X_BeeIn);
            fsm.AddState(State.ProblemNarr, E_ProblemNarr,  X_ProblemNarr);
            fsm.AddState(State.Solve,       E_Solve,        X_Solve);
            fsm.AddState(State.Eat,         E_Eat,          X_Eat);
            fsm.AddState(State.Wrong,       E_Wrong,        X_Wrong);
            fsm.AddState(State.Correct,     E_Correct,      X_Correct);
            fsm.AddState(State.Next,        E_Next);
            fsm.AddState(State.NextProblem, E_NextProblem,  X_NextProblem);
            fsm.AddState(State.Outro,       E_Outro,        X_Outro);
            fsm.AddState(State.Reward,      E_Reward);
            #pragma warning restore format  
        }
        private void setupProblem(ProblemData pData)
        {
            problemSign.Setup(pData);

            beeMGR.Setup(pData.Examples);
        }

        // Functions
        private void chagneBG(int seq)
        {
            for (int i = 0; i < backgroundTR.childCount; i++)
                backgroundTR.GetChild(i).gameObject.SetActive(i == seq - 1);
        }

        // Event Handlers
        private void timelineSignal_OnSignal(string signal)
        {
            LOG.Info($"timelineSignal_OnSignal() | {signal}", this);

            if (signal == "Activity-ExtraAnimation")
            {
                var clip = UtilArray.ExtractOne(chameleonOutCLIP);
                AudioMGR.One.PlayEffect(clip);
            }
        }



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            evaluateTimeline(chameleonInTL);

            beeAff.Init(beeMGR);

            chagneBG(1);
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
                case State.ProblemIn: fsm.PerformTransition(State.BeeIn); break;
                case State.BeeIn: fsm.PerformTransition(State.ProblemNarr); break;
                case State.ProblemNarr: fsm.PerformTransition(State.Solve); break;
                case State.Solve:
                    submitBee = beeMGR.AnswerBee;
                    fsm.PerformTransition(State.Eat);
                    break;
                case State.Wrong: fsm.PerformTransition(State.BeeIn); break;
                case State.Correct: fsm.PerformTransition(State.Next); break;
                case State.NextProblem: fsm.PerformTransition(State.ProblemIn); break;
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
            yield return null;

            fsm.PerformTransition(State.ProblemIn);
        }
        IEnumerator E_ProblemIn()
        {
            setupProblem(pMGR.Current);

            yield return playTimeline(chameleonInTL);

            fsm.PerformTransition(State.ProblemNarr);
        }
        IEnumerator X_ProblemIn()
        {
            yield return stopTimeline(chameleonInTL);
        }
        IEnumerator E_ProblemNarr()
        {
            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.WordCLIP);
            yield return new WaitForSeconds(0.5f);

            fsm.PerformTransition(State.BeeIn);
        }
        IEnumerator X_ProblemNarr()
        {
            AudioMGR.One.StopNarration();
            yield return null;

            yield return stopTimeline(chameleonInTL);
        }
        IEnumerator E_BeeIn()
        {
            yield return beeMGR.StartIn();

            fsm.PerformTransition(State.Solve);
        }
        IEnumerator X_BeeIn()
        {
            beeMGR.FinishIn();
            yield return null;
        }
        IEnumerator E_Solve()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            beeMGR.EnableInteraction(true);
            problemSign.EnableInteraction(true);
            var wait = new WaitForSubmit(this, beeMGR.AlLBees);
            yield return wait;

            AudioMGR.One.PlayEffect(SfxMoment.Activity_Click);
            beeMGR.EnableInteraction(false);
            yield return null;

            AudioMGR.One.PlayEffect(SfxMoment.Common_Click);
            yield return null;

            submitBee = wait.Submited as Bee;
            yield return null;

            fsm.PerformTransition(State.Eat);
        }
        IEnumerator X_Solve()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            beeMGR.EnableInteraction(false);
            problemSign.EnableInteraction(false);
            AudioMGR.One.StopNarration();
            yield return null;
        }
        IEnumerator E_Eat()
        {
            chameleon.Eat(submitBee.ID);
            yield return new WaitForSeconds(beeTakenDelay * 0.7f);

            AudioMGR.One.PlayEffect(eatCLIP);
            yield return new WaitForSeconds(beeTakenDelay * 0.3f);

            beeMGR.TakeAndOut(submitBee.ID);
            yield return null;

            var correct = submitBee.IsAnswer;
            if (correct)
                fsm.PerformTransition(State.Correct);
            else fsm.PerformTransition(State.Wrong);
        }
        IEnumerator X_Eat()
        {
            CP(CheckPoint.UserFinish);
            yield return null;
        }
        IEnumerator E_Correct()
        {
            AudioMGR.One.PlayEffect(SfxMoment.Activity_Correct);
            yield return null;

            problemSign.ShowAnswer();
            yield return new WaitForSeconds(showAnswerDelay);

            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.WordCLIP);

            yield return chameleon.Correct();
            yield return new WaitForSeconds(correctDelay);

            yield return playTimeline(chameleonOutTL);
            yield return null;

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_Correct()
        {
            yield return stopTimeline(chameleonOutTL);
            yield return null;
        }
        IEnumerator E_Wrong()
        {
            ActivityProgress.One.Wrong();
            AudioMGR.One.PlayEffect(SfxMoment.Activity_Wrong);
            yield return null;

            yield return chameleon.Wrong();
            yield return null;

            yield return new WaitForSeconds(wrongDelay);

            fsm.PerformTransition(State.ProblemNarr);
        }
        IEnumerator X_Wrong()
        {
            yield return null;
        }
        IEnumerator E_Next()
        {
            yield return null;

            if (pMGR.Next())
                fsm.PerformTransition(State.NextProblem);
            else fsm.PerformTransition(State.Outro);
        }
        IEnumerator E_NextProblem()
        {
            // 배경 교체
            chagneBG(pMGR.PNO);

            yield return playTimeline(nextProblemTL);
            yield return null;

            fsm.PerformTransition(State.ProblemIn);
        }
        IEnumerator X_NextProblem()
        {
            // 배경 교체
            chagneBG(pMGR.PNO);

            yield return stopTimeline(nextProblemTL);
            yield return null;
        }
        IEnumerator E_Outro()
        {
            CP(CheckPoint.Outro);
            yield return null;

            yield return new WaitForSeconds(outroDelay);

            fsm.PerformTransition(State.Reward);
        }
        IEnumerator X_Outro()
        {
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
        [Header("★ Bindings")]
        [SerializeField] private ProblemSign problemSign = null;
        [SerializeField] private Chameleon chameleon = null;
        [SerializeField] private BeeMGR beeMGR = null;
        [SerializeField] private BeeAff beeAff = null;
        [SerializeField] private Transform backgroundTR = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip eatCLIP = null;
        [SerializeField] private AudioClip[] chameleonOutCLIP = null;
        [Header("★ Sound - Ambient")]
        [SerializeField] private AudioClip ambientCLIP = null;
        [Range(0, 100)]
        [SerializeField] int ambientVolume = 100;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector chameleonInTL = null;
        [SerializeField] private PlayableDirector chameleonOutTL = null;
        [SerializeField] private PlayableDirector nextProblemTL = null;
        [Header("★ Timing")]
        [SerializeField] private float beeTakenDelay = 0.7f;
        [SerializeField] private float showAnswerDelay = 2f;
        [SerializeField] private float correctDelay = 1f;
        [SerializeField] private float wrongDelay = 1f;
        [SerializeField] private float outroDelay = 1f;
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

            chameleonOutSIG.OnSignal += timelineSignal_OnSignal;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            chameleonOutSIG.OnSignal -= timelineSignal_OnSignal;
        }
    }
}