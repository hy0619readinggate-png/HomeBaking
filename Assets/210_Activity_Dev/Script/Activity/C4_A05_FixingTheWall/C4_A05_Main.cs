using beyondi.FSM;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Activity.UI;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

using ActivityData = DoDoEng.Common.ActivityData_C4_A05;
using ProblemMGR = DoDoEng.Activity.C4_A05.C4_A05_ProblemMGR;

namespace DoDoEng.Activity.C4_A05
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C4_A05_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C4_A05_FixingTheWall;
        private enum State
        {
            Intro,
            ShowBlock, Solve, Complete, Next, ProblemChange,
            Outro, Reward
        }

        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();

        // Fields
        private FSM<State> fsm = null;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Intro,           E_Intro,            X_Intro);
            fsm.AddState(State.ShowBlock,       E_ShowBlock,        X_ShowBlock);
            fsm.AddState(State.Solve,           E_Solve,            X_Solve);
            fsm.AddState(State.Complete,        E_Complete,         X_Complete);
            fsm.AddState(State.Next,            E_Next);
            fsm.AddState(State.ProblemChange,   E_ProblemChange,    X_ProblemChange);
            fsm.AddState(State.Outro,           E_Outro,            X_Outro);
            fsm.AddState(State.Reward,          E_Reward);
            #pragma warning restore format  
        }
        private void setupProblem(ProblemData pData)
        {
            wallMGR.Setup(pData, pMGR.PNO);
            blockMGR.Setup(pData);
        }



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            evaluateTimeline(introTL);
            blockMGR.Init();
        }
        protected override async UniTask onPrepare(ActivityIndex actIDX)
        {
            await base.onPrepare(actIDX);

            // Load Unit Data
            await pMGR.Build(actIDX, CURRICULUM, TABLES);
            ActivityProgress.One.Setup(pMGR.BlanksCount);

            intro.Setup(pMGR.IntroDatas);

            // setup
            setupProblem(pMGR.Current);
        }
        protected override void onStartActivity()
        {
            base.onStartActivity();

            playBGM();
            fsm.StartFSM(State.Intro, RunnerParam.SkipStateTo);
        }
        protected override void onFinishActivity()
        {
            base.onFinishActivity();

            if (fsm.CurrentState == State.Solve)
                CP(CheckPoint.UserFinish);

            stopBGM();
            fsm?.StopFSM();
        }
        protected override void onSpeaker()
        {
            base.onSpeaker();

            if (fsm.CurrentState == State.Solve)
            {
                AudioMGR.One.PlayNarration(pMGR.Current.SentenceCLIP);
            }
        }
        protected override void onDebugNext()
        {
            switch (fsm.CurrentState)
            {
                case State.Intro: fsm.PerformTransition(State.ShowBlock); break;
                case State.ShowBlock: fsm.PerformTransition(State.Solve); break;
                case State.Solve: fsm.PerformTransition(State.Complete); break;
                case State.Complete: fsm.PerformTransition(State.Next); break;
                //case State.ProblemChange: fsm.PerformTransition(State.ShowBlock); break;
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

            if (pMGR.IsIntroDataExist)
                yield return intro.StartPlay();

            UIActivityCommon.One.VisibleSpeakerButton = true;
            yield return playTimeline(introTL);
            yield return null;

            fsm.PerformTransition(State.ShowBlock);
        }
        IEnumerator X_Intro()
        {
            UIActivityCommon.One.VisibleSpeakerButton = true;

            if (pMGR.IsIntroDataExist)
                intro.FinishPlay();

            yield return stopTimeline(introTL);
            yield return null;
        }
        IEnumerator E_ShowBlock()
        {
            gino.Hide();
            blockMGR.FixBlocksLayout(true);
            yield return null;

            yield return blockMGR.StartShow();

            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.SentenceCLIP);
            yield return null;

            fsm.PerformTransition(State.Solve);
        }
        IEnumerator X_ShowBlock()
        {
            AudioMGR.One.StopNarration();
            yield return null;

            blockMGR.FixBlocksLayout(false);
            yield return null;

            blockMGR.FinishShow();
            yield return null;
        }
        IEnumerator E_Solve()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            UIActivityCommon.One.EnableSpeakerButton = true;
            yield return null;

            gino.StartAppear();
            blockMGR.EnableInteraction(true);
            yield return null;

            yield return wallMGR.StartWaitComplete();

            fsm.PerformTransition(State.Complete);
        }
        IEnumerator X_Solve()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            UIActivityCommon.One.EnableSpeakerButton = false;
            yield return null;

            gino.StopAppear();
            blockMGR.EnableInteraction(false);
            blockMGR.HideBlocks();
            yield return null;

            wallMGR.FinishWaitComplete();
            yield return null;
        }
        IEnumerator E_Complete()
        {
            yield return gino.StartCorrect();
            yield return wallMGR.StartCorrect();
            yield return null;

            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.SentenceCLIP);
            yield return null;

            yield return gino.StartOut();
            yield return null;

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_Complete()
        {
            gino.FinishCorrect();
            wallMGR.FinishCorrect();
            yield return null;

            AudioMGR.One.StopNarration();
            yield return null;

            gino.FinishOut();
            yield return null;
        }
        IEnumerator E_Next()
        {
            yield return null;

            if (pMGR.Next())
                fsm.PerformTransition(State.ProblemChange);
            else fsm.PerformTransition(State.Outro);
        }
        IEnumerator E_ProblemChange()
        {
            setupProblem(pMGR.Current);
            yield return null;

            yield return wallMGR.StartChangeWall();
            yield return null;

            fsm.PerformTransition(State.ShowBlock);
        }
        IEnumerator X_ProblemChange()
        {
            wallMGR.FinishChangeWall();
            yield return null;
        }
        IEnumerator E_Outro()
        {
            CP(CheckPoint.Outro);
            yield return null;

            yield return playTimeline(outroTL, 0);
            wallMGR.Outro();
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
        [Header("★ Bindings")]
        [SerializeField] private Intro intro = null;
        [SerializeField] private Gino gino = null;
        [SerializeField] private WallMGR wallMGR = null;
        [SerializeField] private BlockMGR blockMGR = null;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector introTL = null;
        [SerializeField] private PlayableDirector outroTL = null;
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
        }
        protected override void OnDisable()
        {
            base.OnDisable();
        }
    }
}