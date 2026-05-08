using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Activity.UI;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C3_A03;
using ProblemMGR = DoDoEng.Activity.C1_A10.C3_A03_ProblemMGR;

// Variation : C1_A10_FindTheMaze, C3_A03_BlockLand
namespace DoDoEng.Activity.C1_A10
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C3_A03_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C3_A03_LegoMaze;
        private enum State { Intro, Maze, ToProblem, Problem, Next, FadeOut, NextProblem, Outro, Reward }

        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();

        // Fields
        private FSM<State> fsm = null;
        private int[] mazeIds = null;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Intro,       E_Intro);
            fsm.AddState(State.Maze,        E_Maze,         X_Maze);
            fsm.AddState(State.ToProblem,   E_ToProblem,    X_ToProblem);
            fsm.AddState(State.Problem,     E_Problem,      X_Problem);
            fsm.AddState(State.FadeOut,     E_FadeOut,      X_FadeOut);
            fsm.AddState(State.Next,        E_Next);
            fsm.AddState(State.NextProblem, E_NextProblem);
            fsm.AddState(State.Outro,       E_Outro,        X_Outro);
            fsm.AddState(State.Reward,      E_Reward);
            #pragma warning restore format  
        }
        private void setupProblem(ProblemData pData)
        {
            // step2
            s2Problem.Setup(pData, friends[pMGR.PNO - 1], friends);
        }
        private Maze currentMaze => mazes[mazeIds[pMGR.PNO % mazeIds.Length] - 1];



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            if (ignoreRandomMapID)
                mazeIds = UtilArray.Sequential(1, mazes.Length);
            else mazeIds = UtilArray.Random(1, mazes.Length);

            mazes.ForEach(m => m.Init(cameraMGR, friends));
            s2Problem.Init(cameraMGR);

            var friendsIDs = UtilArray.Random(1, friends.Length);
            friends.ForEach((i, f) => f.Setup(friendsIDs[i]));

            outroGO.SetActive(false);

            ActivityUI.One.HideIndicator();
        }
        protected override async UniTask onPrepare(ActivityIndex actIDX)
        {
            await base.onPrepare(actIDX);

            // Load Unit Data
            await pMGR.Build(actIDX, CURRICULUM, TABLES);
            ActivityProgress.One.Setup(pMGR.BlanksCount);

            // setup
            currentMaze.PrepareMaze();
            ActivityUI.One.Setup(pMGR.PNO);
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

            if (fsm.CurrentState == State.Maze || fsm.CurrentState == State.Problem)
                CP(CheckPoint.UserFinish);

            stopBGM();
            fsm?.StopFSM();
        }
        protected override void onSpeaker()
        {
            base.onSpeaker();

            if (fsm.CurrentState == State.Problem)
            {
                AudioMGR.One.PlayNarration(pMGR.Current.ProblemCLIP);
            }
        }
        protected override void onDebugNextStep()
        {
            switch (fsm.CurrentState)
            {
                case State.Maze:
                case State.Problem:
                    fsm.PerformTransition(State.Next);
                    break;
            }
        }
        protected override void onDebugNext()
        {
            switch (fsm.CurrentState)
            {
                case State.Maze: fsm.PerformTransition(State.ToProblem); break;
                case State.Problem: fsm.PerformTransition(State.Next); break;
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

            fsm.PerformTransition(State.Maze);
        }
        IEnumerator E_Maze()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            yield return currentMaze.StartMaze();
            yield return null;

            fsm.PerformTransition(State.ToProblem);
        }
        IEnumerator X_Maze()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            AudioMGR.One.StopNarration();
            currentMaze.FinishMaze();
            yield return null;
        }
        IEnumerator E_ToProblem()
        {
            AudioMGR.One.PlayEffect(trasitionCLIP);
            yield return SystemUI.One.Fader.FadeOut();
            yield return null;

            setupProblem(pMGR.Current);
            yield return null;

            ActivityUI.One.ShowIndicator();
            UIActivityCommon.One.VisibleSpeakerButton = true;
            yield return SystemUI.One.Fader.FadeIn();
            yield return null;

            fsm.PerformTransition(State.Problem);
        }
        IEnumerator X_ToProblem()
        {
            yield return null;
        }
        IEnumerator E_Problem()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            yield return s2Problem.StartProblem();

            fsm.PerformTransition(State.FadeOut);
        }
        IEnumerator X_Problem()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            s2Problem.FinishProblem();
            yield return null;
        }
        IEnumerator E_FadeOut()
        {
            yield return SystemUI.One.Fader.FadeOut();
            yield return null;

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_FadeOut()
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
            currentMaze.PrepareMaze();
            ActivityUI.One.Setup(pMGR.PNO);
            yield return null;

            ActivityUI.One.HideIndicator();
            UIActivityCommon.One.VisibleSpeakerButton = false;
            yield return SystemUI.One.Fader.FadeIn();

            fsm.PerformTransition(State.Maze);
        }
        IEnumerator E_Outro()
        {
            CP(CheckPoint.Outro);
            yield return null;

            AudioMGR.One.PlayEffect(outroCLIP);
            outroGO.SetActive(true);
            yield return SystemUI.One.Fader.FadeIn();
            yield return new WaitForSeconds(outroDuration);

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
        [Header("★ Bindings - S1.Maze")]
        [SerializeField] private Maze[] mazes = null;
        [Header("★ Bindings - S2.Problem")]
        [SerializeField] private Problem s2Problem = null;
        [Header("★ Bindings - S3.Outro")]
        [SerializeField] private GameObject outroGO = null;
        [SerializeField] private float outroDuration = 3f;
        [Header("★ Bindings")]
        [SerializeField] private CameraMGR cameraMGR = null;
        [SerializeField] private Friend[] friends = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip trasitionCLIP = null;
        [SerializeField] private AudioClip outroCLIP = null;
        [Header("★ Timing")]
        [SerializeField] private float rewardDelay = 1f;
        [Header("★ Config")]
        [SerializeField] private bool ignoreRandomMapID = false;


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