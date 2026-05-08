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

using ActivityData = DoDoEng.Common.ActivityData_C2_A10;
using ProblemMGR = DoDoEng.Activity.C2_A10.C2_A10_ProblemMGR;

// Variation : C2_A10_Forest, C3_A08_ToTheDesert
namespace DoDoEng.Activity.C2_A10
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C2_A10_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C2_A10_Forest;
        private enum State
        {
            Intro,
            ProblemIn, Problem, Solve, Correct, Wrong, Next, GoOut,
            Outro, Reward
        }

        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();

        // Fields
        private FSM<State> fsm = null;
        private Example submitedExam = null;
        private int setNO = 0;

        // C2_A10
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Intro,       E_Intro,        X_Intro);
            fsm.AddState(State.ProblemIn,   E_ProblemIn,    X_ProblemIn);
            fsm.AddState(State.Problem,     E_Problem,      X_Problem);
            fsm.AddState(State.Solve,       E_Solve,        X_Solve);
            fsm.AddState(State.Correct,     E_Correct,      X_Correct);
            fsm.AddState(State.Wrong,       E_Wrong,        X_Wrong);
            fsm.AddState(State.Next,        E_Next);  
            fsm.AddState(State.GoOut,       E_GoOut,        X_GoOut);
            fsm.AddState(State.Outro,       E_Outro,        X_Outro);
            fsm.AddState(State.Reward,      E_Reward);
            #pragma warning restore format  
        }
        private void setupProblem(ProblemData pData)
        {
            problemBoard.Setup(pData);
            examples.ForEach((i, e) => e.Setup(pData.Examples[i]));

            var answerIDX = Array.FindIndex(pData.Examples, e => e.IsAnswer);
            aff.Setup(answerIDX);
        }

        // Functions
        private ProblemBoard problemBoard => setNO == 0 ? s1ProblemBoard : s2ProblemBoard;
        private Example[] examples => setNO == 0 ? s1Examples : s2Examples;
        private PlayableDirector problemTL => setNO == 0 ? s1ProblemTL : s2ProblemTL;
        private PlayableDirector[] correctTLs => setNO == 0 ? s1CorrectTL : s2CorrectTL;
        private PlayableDirector[] wrongTLs => setNO == 0 ? s1WrongTL : s2WrongTL;
        private PlayableDirector nextTL => setNO == 0 ? s1NextTL : s2NextTL;
        private PlayableDirector outroTL => setNO == 0 ? s1OutroTL : s2OutroTL;
        private Affordance aff => setNO == 0 ? s1Aff : s2Aff;



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            setNO = 0;

            evaluateTimeline(problemTL);

            stepPanelCG.SetActiveOnly(0);

            s1Examples.ForEach(e => e.EnableInteraction(false));
            s1Examples.AutoFillID();
            s2Examples.ForEach(e => e.EnableInteraction(false));
            s2Examples.AutoFillID();

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
                case State.ProblemIn: fsm.PerformTransition(State.Problem); break;
                case State.Problem: fsm.PerformTransition(State.Solve); break;
                case State.Solve:
                    submitedExam = examples.FirstOrDefault(e => e.IsAnswer);
                    fsm.PerformTransition(State.Correct);
                    break;
                case State.Correct: fsm.PerformTransition(State.GoOut); break;
                case State.Wrong: fsm.PerformTransition(State.Problem); break;
                case State.GoOut: fsm.PerformTransition(State.Next); break;
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
        IEnumerator X_Intro()
        {
            yield return null;
        }
        IEnumerator E_ProblemIn()
        {
            setupProblem(pMGR.Current);
            //yield return null;


            //evaluateTimeline(problemTL);
            //if (setNO > 0)
            //    yield return SystemUI.One.Fader.FadeIn();

            yield return playTimeline(problemTL);
            yield return null;

            fsm.PerformTransition(State.Problem);
        }
        IEnumerator X_ProblemIn()
        {
            yield return stopTimeline(problemTL);
            yield return null;
        }
        IEnumerator E_Problem()
        {
            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.WordCLIP);
            yield return null;

            fsm.PerformTransition(State.Solve);
        }
        IEnumerator X_Problem()
        {
            AudioMGR.One.StopNarration();
            yield return null;
        }
        IEnumerator E_Solve()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            problemBoard.EnableInteraction(true);
            examples.ForEach(e => e.EnableInteraction(true));
            yield return null;

            var wait = new WaitForSubmit(this, examples);
            yield return wait;

            submitedExam = wait.Submited as Example;
            if (submitedExam.IsAnswer)
                fsm.PerformTransition(State.Correct);
            else fsm.PerformTransition(State.Wrong);
        }
        IEnumerator X_Solve()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            problemBoard.EnableInteraction(false);
            examples.ForEach(e => e.EnableInteraction(false));
            yield return null;
        }
        IEnumerator E_Correct()
        {
            var tl = correctTLs[submitedExam.ID - 1];
            yield return playTimeline(tl);
            yield return null;

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_Correct()
        {
            var tl = correctTLs[submitedExam.ID - 1];
            yield return stopTimeline(tl);
            yield return null;
        }
        IEnumerator E_Wrong()
        {
            ActivityProgress.One.Wrong();
            var tl = wrongTLs[submitedExam.ID - 1];
            yield return playTimeline(tl);
            yield return null;

            fsm.PerformTransition(State.Problem);
        }
        IEnumerator X_Wrong()
        {
            var tl = wrongTLs[submitedExam.ID - 1];
            yield return stopTimeline(tl);
            yield return null;
        }
        IEnumerator E_Next()
        {
            yield return null;

            if (pMGR.Next())
            {
                if (pMGR.Current.IsFirstInSet)
                    fsm.PerformTransition(State.GoOut);
                else fsm.PerformTransition(State.ProblemIn);
            }
            else fsm.PerformTransition(State.Outro);
        }
        IEnumerator E_GoOut()
        {
            yield return playTimeline(nextTL);
            //yield return SystemUI.One.Fader.FadeOut();

            fsm.PerformTransition(State.ProblemIn);
        }
        IEnumerator X_GoOut()
        {
            yield return stopTimeline(nextTL);
            setNO++;

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
        [Header("★ Bindings - S1.Brush")]
        [SerializeField] private ProblemBoard s1ProblemBoard = null;
        [SerializeField] private Example[] s1Examples = null;
        [Header("★ Bindings - S2.WaterFall")]
        [SerializeField] private ProblemBoard s2ProblemBoard = null;
        [SerializeField] private Example[] s2Examples = null;
        [Header("★ TimeLine - S1")]
        [SerializeField] private PlayableDirector s1ProblemTL = null;
        [SerializeField] private PlayableDirector[] s1CorrectTL = null;
        [SerializeField] private PlayableDirector[] s1WrongTL = null;
        [SerializeField] private PlayableDirector s1NextTL = null;
        [SerializeField] private PlayableDirector s1OutroTL = null;
        [SerializeField] private Affordance s1Aff = null;
        [Header("★ TimeLine - S2")]
        [SerializeField] private PlayableDirector s2ProblemTL = null;
        [SerializeField] private PlayableDirector[] s2CorrectTL = null;
        [SerializeField] private PlayableDirector[] s2WrongTL = null;
        [SerializeField] private PlayableDirector s2NextTL = null;
        [SerializeField] private PlayableDirector s2OutroTL = null;
        [SerializeField] private Affordance s2Aff = null;
        [Header("★ Sound - Ambient")]
        [SerializeField] private AudioClip ambientCLIP = null;
        [Range(0, 100)]
        [SerializeField] int ambientVolume = 100;
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