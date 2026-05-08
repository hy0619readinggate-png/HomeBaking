using beyondi.Coroutine;
using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

using ActivityData = DoDoEng.Common.ActivityData_C2_A06;
using ProblemMGR = DoDoEng.Activity.C2_A06.C2_A06_ProblemMGR;

namespace DoDoEng.Activity.C2_A06
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C2_A06_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C2_A06_DesertYoga;
        private enum State { Intro, Problem, Solve, Correct, Wrong, NextCarpet, Next, ToNextProblem, Outro, Reward }

        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();

        // Fields
        private FSM<State> fsm = null;
        private Carpet submitCarpet = null;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Intro,           E_Intro);
            fsm.AddState(State.Problem,         E_Problem,      X_Problem);
            fsm.AddState(State.Solve,           E_Solve,        X_Solve);
            fsm.AddState(State.Correct,         E_Correct,      X_Correct);
            fsm.AddState(State.Wrong,           E_Wrong,        X_Wrong);
            fsm.AddState(State.NextCarpet,      E_NextCarpet);
            fsm.AddState(State.Next,            E_Next);
            fsm.AddState(State.ToNextProblem,   E_ToNextProblem,  X_ToNextProblem);
            fsm.AddState(State.Outro,           E_Outro,        X_Outro);
            fsm.AddState(State.Reward,          E_Reward);
            #pragma warning restore format  
        }
        private void setupProblem(ProblemData pData)
        {
            teacher.Setup(pData.YogaPose);
            exampleCharacters.ForEach((i, e) => e.Setup(pData.CharacterExamples[i]));
            carpets.ForEach((i, c) => c.Setup(pData.Examples[i], pData.YogaPose));
        }
        private void setupAffordance()
        {
            var list = new List<int[]>();
            foreach (var (e, i) in exampleCharacters.Select((e, i) => (e, i)))
            {
                var arr = new int[2];
                arr[0] = i;
                arr[1] = carpets.ToList<Carpet>().FindIndex(c => c.Word == e.Word && !c.IsCorrect);

                list.Add(arr);
            }
            aff.Setup(list);
        }



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            evaluateTimeline(problemTL);

            stepPanelCG.SetActiveOnly(0);

            carpets.AutoFillID();
            exampleCharacters.ForEach(e => e.Init(exampleCharacterParam));
        }
        protected override async UniTask onPrepare(ActivityIndex actIDX)
        {
            await base.onPrepare(actIDX);

            // Load Unit Data
            await pMGR.Build(actIDX, CURRICULUM, TABLES);
            ActivityProgress.One.Setup(pMGR.BlanksCount);
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
                case State.Problem: fsm.PerformTransition(State.Solve); break;
                case State.Correct: fsm.PerformTransition(State.NextCarpet); break;
                case State.Wrong: fsm.PerformTransition(State.Solve); break;
                case State.ToNextProblem: fsm.PerformTransition(State.Problem); break;
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

            fsm.PerformTransition(State.Problem);
        }
        IEnumerator E_Problem()
        {
            setupProblem(pMGR.Current);
            yield return playTimeline(problemTL);

            fsm.PerformTransition(State.Solve);
        }
        IEnumerator X_Problem()
        {
            yield return stopTimeline(problemTL);

            AudioMGR.One.StopNarration();
            yield return null;
        }
        IEnumerator E_Solve()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            setupAffordance();
            yield return null;

            teacher.Idle();
            exampleCharacters.ForEach(e => e.Idle());
            exampleCharacters.ForEach(e => e.EnableInteraction(true));
            var waittCarets = carpets.Where(c => !c.IsSubmit || !c.IsCorrect).ToArray();
            var wait = new WaitForSubmit(this, waittCarets);
            yield return wait;

            submitCarpet = wait.Submited as Carpet;

            if (submitCarpet.IsCorrect)
                fsm.PerformTransition(State.Correct);
            else fsm.PerformTransition(State.Wrong);
        }
        IEnumerator X_Solve()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            exampleCharacters.ForEach(e => e.EnableInteraction(false));
            yield return null;
        }
        IEnumerator E_Correct()
        {
            teacher.Correct();
            yield return submitCarpet.StartCorrect();
            yield return null;

            fsm.PerformTransition(State.NextCarpet);

        }
        IEnumerator X_Correct()
        {
            submitCarpet.FinishCorrect();
            yield return null;
        }
        IEnumerator E_Wrong()
        {
            ActivityProgress.One.Wrong();
            teacher.Wrong();
            yield return submitCarpet.StartWrong();
            yield return null;

            fsm.PerformTransition(State.Solve);
        }
        IEnumerator X_Wrong()
        {
            submitCarpet.FinishWrong();
            yield return null;
        }
        IEnumerator E_NextCarpet()
        {
            yield return null;

            if (carpets.All(c => c.IsSubmit))
                fsm.PerformTransition(State.Next);
            else fsm.PerformTransition(State.Solve);
        }
        IEnumerator E_Next()
        {
            yield return null;

            if (pMGR.Next())
                fsm.PerformTransition(State.ToNextProblem);
            else fsm.PerformTransition(State.Outro);
        }
        IEnumerator E_ToNextProblem()
        {
            carpets.ForEach(c => c.Out());

            yield return playTimeline(nextProblemTL);
            yield return null;

            fsm.PerformTransition(State.Problem);
        }
        IEnumerator X_ToNextProblem()
        {
            yield return stopTimeline(nextProblemTL);
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
        [Header("★ Bindings")]
        [SerializeField] private Teacher teacher = null;
        [SerializeField] private Carpet[] carpets = null;
        [SerializeField] private DragAff aff = null;
        [SerializeField] private ExampleCharacter[] exampleCharacters = null;
        [SerializeField] private ExampleCharacterParam exampleCharacterParam = null;
        [Header("★ Sound - Ambient")]
        [SerializeField] private AudioClip ambientCLIP = null;
        [Range(0, 100)]
        [SerializeField] int ambientVolume = 100;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector problemTL = null;
        [SerializeField] private PlayableDirector nextProblemTL = null;
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

    [System.Serializable]
    public class ExampleCharacterParam
    {
        public float returnJumpPower = 2f;
        public float returnJumpDuration = 0.5f;

        public AudioClip dragCLIP = null;
        public AudioClip returnCLIP = null;
    }
}