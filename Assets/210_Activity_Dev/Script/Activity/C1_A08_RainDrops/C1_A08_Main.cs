using beyondi.Coroutine;
using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Activity.UI;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using ActivityData = DoDoEng.Common.ActivityData_C1_A08;
using ProblemMGR = DoDoEng.Activity.C1_A08.C1_A08_ProblemMGR;

namespace DoDoEng.Activity.C1_A08
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C1_A08_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C1_A08_RainDrops;
        private enum State
        {
            Intro,
            S1Lever, S1Problem,
            S2Solve, S2Happy, Next, S2ToS1,
            S3Outro, Reward
        }

        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();
        private TimelineSignal problemSIG_ = null;
        private TimelineSignal problemSIG => problemSIG_ ??= problemTL.GetComponent<TimelineSignal>();

        // Fields
        private FSM<State> fsm = null;
        private Coroutine crPlaySound = null;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Intro,       E_Intro,        X_Intro);
            fsm.AddState(State.S1Lever,     E_S1Lever,      X_S1Lever);
            fsm.AddState(State.S1Problem,   E_S1Problem,    X_S1Problem);
            fsm.AddState(State.S2Solve,     E_S2Solve,      X_S2Solve);
            fsm.AddState(State.S2Happy,     E_S2Happy,      X_S2Happy);
            fsm.AddState(State.Next,        E_Next);
            fsm.AddState(State.S2ToS1,      E_S2ToS1,       X_S2ToS1);
            fsm.AddState(State.S3Outro,     E_S3Outro,      X_S3Outro);
            fsm.AddState(State.Reward,      E_Reward);
            #pragma warning restore format  
        }
        private void setupProblem(ProblemData pData)
        {
            // step1
            s1Pachinko.Setup(pData.AnswerSprite, pData.ExamSprite);
        }

        // Event Handlers
        private void timelineSignal_OnSignal(string signal)
        {
            if (signal == "Activity-ProblemSound")
                AudioMGR.One.PlayNarration(pMGR.Current.WordCLIP);
        }
        protected override void onSpeaker()
        {
            base.onSpeaker();

            if (fsm.CurrentState == State.S2Solve)
                crPlaySound = StartCoroutine(coPlayWordSound(pMGR.Current.WordCLIP));
        }



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            introTL.time = 0;
            introTL.Evaluate();

            stepPanelCG.SetActiveOnly(0);

            s1AffGO.SetActive(false);
            s2AffGO.SetActive(true);
            s2VFXHappyGO.SetActive(false);
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
            AudioMGR.One.PlayAmbient(ambient1CLIP, ambient1Volume);
            fsm.StartFSM(State.Intro, RunnerParam.SkipStateTo);
        }
        protected override void onFinishActivity()
        {
            base.onFinishActivity();

            if (fsm.CurrentState == State.S2Solve)
                CP(CheckPoint.UserFinish);

            stopBGM();
            AudioMGR.One.StopAmbient(true);
            fsm?.StopFSM();
        }
        protected override void onDebugNext()
        {
            switch (fsm.CurrentState)
            {
                case State.Intro: fsm.PerformTransition(State.S1Lever); break;
                case State.S1Lever: fsm.PerformTransition(State.S1Problem); break;
                case State.S1Problem: fsm.PerformTransition(State.S2Solve); break;
                case State.S2Solve: fsm.PerformTransition(State.S2Happy); break;
                case State.S2Happy: fsm.PerformTransition(State.Next); break;
                case State.S3Outro: fsm.PerformTransition(State.Reward); break;
            }
        }
        protected override void onDebugNextProblem()
        {
            switch (fsm.CurrentState)
            {
                case State.S1Problem:
                case State.S2Solve:
                    fsm.PerformTransition(State.Next);
                    break;
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
            yield return playTimeline(introTL);
            yield return null;

            fsm.PerformTransition(State.S1Lever);
        }
        IEnumerator X_Intro()
        {
            yield return stopTimeline(introTL);
            yield return null;
        }
        IEnumerator E_S1Lever()
        {
            s1AffGO.SetActive(true);
            yield return s1LeverBTN.StartWaitClick();
            yield return null;

            fsm.PerformTransition(State.S1Problem);
        }
        IEnumerator X_S1Lever()
        {
            s1AffGO.SetActive(false);
            s1LeverBTN.FinishWaitClick();
            yield return null;
        }
        IEnumerator E_S1Problem()
        {
            stepPanelCG.SetActiveOnly(0);
            s2Players.SwitchTo(pMGR.PNO);

            setupProblem(pMGR.Current);
            yield return playTimeline(problemTL);
            yield return null;

            fsm.PerformTransition(State.S2Solve);
        }
        IEnumerator X_S1Problem()
        {
            yield return stopTimeline(problemTL);
            yield return null;

            AudioMGR.One.StopNarration();
            yield return null;
        }
        IEnumerator E_S2Solve()
        {
            UIActivityCommon.One.VisibleSpeakerButton = true;
            UIActivityCommon.One.EnableSpeakerButton = true;

            CP(CheckPoint.UserStart);
            yield return null;

            stepPanelCG.SetActiveOnly(1);
            s2Players.SwitchTo(pMGR.PNO); // for skip
            yield return null;

            AudioMGR.One.PlayAmbient(ambient2CLIP, ambient2Volume);
            AffordanceMGR.One.StartAffNow();
            s2RainDropMGR.StartDrop(
                pMGR.Current.AnswerSprite,
                pMGR.Current.ExamSprite);
            s2PlayerController.EnableInteraction(true);
            s2Players.Current.StartPlay(pMGR.Current.WordCLIP, s2LimitLeftTR, s2LimitRightTR);
            yield return new WaitForComplete(this, s2Players.Current);

            fsm.PerformTransition(State.S2Happy);
        }
        IEnumerator X_S2Solve()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            this.StopCoroutineSafe(ref crPlaySound);
            AudioMGR.One.StopNarration();
            yield return null;

            UIActivityCommon.One.VisibleSpeakerButton = false;
            UIActivityCommon.One.EnableSpeakerButton = false;

            s2RainDropMGR.StopDrop();
            s2PlayerController.EnableInteraction(false);
            s2Players.Current.StopPlay();
        }
        IEnumerator E_S2Happy()
        {
            AudioMGR.One.PlayEffect(successCLIP);
            s2VFXHappyGO.SetActive(true);
            yield return new WaitForSeconds(0.5f);

            s2Players.Current.DoHappy(s2HappyZoneTR);
            yield return s2RainDropMGR.ClearDrop();
            yield return new WaitForSeconds(happyDuration);

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_S2Happy()
        {
            s2VFXHappyGO.SetActive(false);
            yield return null;
        }
        IEnumerator E_Next()
        {
            yield return null;

            if (pMGR.Next())
                fsm.PerformTransition(State.S2ToS1);
            else fsm.PerformTransition(State.S3Outro);
        }
        IEnumerator E_S2ToS1()
        {
            AudioMGR.One.PlayAmbient(ambient1CLIP, ambient1Volume);
            yield return playTimeline(s2ToS1TL);
            yield return null;

            fsm.PerformTransition(State.S1Lever);
        }
        IEnumerator X_S2ToS1()
        {
            yield return stopTimeline(s2ToS1TL);
            yield return null;
        }
        IEnumerator E_S3Outro()
        {
            CP(CheckPoint.Outro);
            yield return null;

            yield return playTimeline(s2ToS3TL);
            yield return null;

            fsm.PerformTransition(State.Reward);
        }
        IEnumerator X_S3Outro()
        {
            yield return stopTimeline(s2ToS3TL);
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
        [Header("★ Bindings - S1.Intro")]
        [SerializeField] private Pachinko s1Pachinko = null;
        [SerializeField] private LeverButton s1LeverBTN = null;
        [SerializeField] private GameObject s1AffGO = null;
        [Header("★ Bindings - S2.Catch")]
        [SerializeField] private RainDropMGR s2RainDropMGR = null;
        [SerializeField] private PlayerGroup s2Players = null;
        [SerializeField] private PlayerController s2PlayerController = null;
        [SerializeField] private GameObject s2AffGO = null;
        [SerializeField] private GameObject s2VFXHappyGO = null;
        [SerializeField] private Transform s2HappyZoneTR = null;
        [SerializeField] private Transform s2LimitLeftTR = null;
        [SerializeField] private Transform s2LimitRightTR = null;
        [Header("★ Bindings - S3.Outro")]   
        [Header("★ Sound")]
        [SerializeField] private AudioClip successCLIP = null;
        [Header("★ Sound - Ambient")]
        [SerializeField] private AudioClip ambient1CLIP = null;
        [SerializeField] private AudioClip ambient2CLIP = null;
        [Range(0, 100)]
        [SerializeField] int ambient1Volume = 100;
        [SerializeField] int ambient2Volume = 100;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector introTL = null;
        [SerializeField] private PlayableDirector problemTL = null;
        [SerializeField] private PlayableDirector s2ToS1TL = null;
        [SerializeField] private PlayableDirector s2ToS3TL = null;
        [Header("★ Timing")]
        [SerializeField] private float happyDuration = 1f;
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

            problemSIG.OnSignal += timelineSignal_OnSignal;
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            
            problemSIG.OnSignal -= timelineSignal_OnSignal;
        }

        // Unity Coroutine
        IEnumerator coPlayWordSound(AudioClip clip)
        {
            using (LOG.Coroutine("coPlayWordSound()", this))
            {
                UIActivityCommon.One.EnableSpeakerButton = false;
                yield return null;

                AudioMGR.One.PlayNarration(clip);
                yield return new WaitForSeconds(clip.length);

                UIActivityCommon.One.EnableSpeakerButton = true;
                yield return null;
            }
        }
    }
}