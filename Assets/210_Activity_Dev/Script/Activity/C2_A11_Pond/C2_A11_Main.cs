using beyondi.Coroutine;
using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

using ActivityData = DoDoEng.Common.ActivityData_C2_A11;
using ProblemMGR = DoDoEng.Activity.C2_A11.C2_A11_ProblemMGR;

namespace DoDoEng.Activity.C2_A11
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C2_A11_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C2_A11_Pond;
        private enum State { Intro, Problem, Solve, Correct, Wrong, Cross, Next, Outro, Reward }

        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();
        private TimelineSignal problem1SIG_ = null;
        private TimelineSignal problem1SIG => problem1SIG_ ??= problemTL[0].GetComponent<TimelineSignal>();
        private TimelineSignal problem2SIG_ = null;
        private TimelineSignal problem2SIG => problem2SIG_ ??= problemTL[1].GetComponent<TimelineSignal>();
        private TimelineSignal problem3SIG_ = null;
        private TimelineSignal problem3SIG => problem3SIG_ ??= problemTL[2].GetComponent<TimelineSignal>();

        // Fields
        private FSM<State> fsm = null;
        private Example submitedExam = null;
        private Coroutine crPlayWord = null;

        // Functions
        private PlayableDirector currentProblemTL => problemTL[pMGR.PNO - 1];
        private Bridge currentBridge => bridges[pMGR.PNO - 1];

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Intro,       E_Intro);
            fsm.AddState(State.Problem,     E_Problem,      X_Problem);
            fsm.AddState(State.Solve,       E_Solve,        X_Solve);
            fsm.AddState(State.Correct,     E_Correct,      X_Correct);
            fsm.AddState(State.Wrong,       E_Wrong,        X_Wrong);
            fsm.AddState(State.Cross,       E_Cross,        X_Cross);
            fsm.AddState(State.Next,        E_Next);
            fsm.AddState(State.Outro,       E_Outro,        X_Outro);
            fsm.AddState(State.Reward,      E_Reward);
            #pragma warning restore format  
        }
        private void setupProblem(ProblemData pData)
        {
            frog.Setup(pData);
            lotus.Setup(pData.WordSPR);

            examples.ForEach((i, e) => e.Setup(pData.Examples[i]));
            positionGroup.Setup(pMGR.PNO);
        }

        // Event Handlers
        private void problemSIG_OnSignal(string signal)
        {
            LOG.Info($"problemSIG_OnSignal() | {signal}", this);

            if (signal == "Activity-SetupProblem")
                setupProblem(pMGR.Current);
            else if (signal == "Activity-ExtraAnimation")
                AudioMGR.One.PlayEffect(bloomingCLIP);
        }
        private void lotus_OnClick()
        {
            LOG.Info($"lotus_OnClick", this);

            crPlayWord = StartCoroutine(coPlayWord());

        }
        private void frog_OnClick()
        {
            LOG.Info($"lotus_OnClick", this);

            crPlayWord = StartCoroutine(coPlayWord());
        }



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            evaluateTimeline(problemTL[0]);

            stepPanelCG.SetActiveOnly(0);

            frog.EnableInteraction(false);
            lotus.EnableInteraction(false);
            examples.ForEach(e => e.EnableInteraction(false));

            vfxCompleteGO.SetActive(false);
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
        protected override void onDebugNextStep()
        {
            switch (fsm.CurrentState)
            {
                case State.Problem:
                case State.Solve:
                    fsm.PerformTransition(State.Outro);
                    break;
            }
        }
        protected override void onDebugNext()
        {
            switch (fsm.CurrentState)
            {
                case State.Problem: fsm.PerformTransition(State.Solve); break;
                case State.Solve:
                    submitedExam = examples.FirstOrDefault(e => e.IsAnswer);
                    fsm.PerformTransition(State.Correct);
                    break;
                case State.Correct: fsm.PerformTransition(State.Cross); break;
                case State.Cross: fsm.PerformTransition(State.Next); break;
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
            tori.Idle();
            frog.ProblemIdle();
            yield return playTimeline(currentProblemTL);
            yield return null;

            var shuffled = UtilArray.Random(0, examples.Length - 1);
            examples.ForEach((i, e) => e.Show(shuffled[i] * exampleAppearDelay));
            yield return new WaitForSeconds(1.5f);

            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.WordCLIP);

            fsm.PerformTransition(State.Solve);
        }
        IEnumerator X_Problem()
        {
            yield return stopTimeline(currentProblemTL);
            yield return null;

            examples.ForEach(e => e.Idle());
            yield return null;

            AudioMGR.One.StopNarration();
            yield return null;
        }
        IEnumerator E_Solve()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            examples.ForEach(e => e.EnableInteraction(true));
            frog.EnableInteraction(true);
            lotus.EnableInteraction(true);
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

            examples.ForEach(e => e.EnableInteraction(false));
            frog.EnableInteraction(false);
            lotus.EnableInteraction(false);
            yield return null;

            this.StopCoroutineSafe(ref crPlayWord);
        }
        IEnumerator E_Correct()
        {
            submitedExam.Correct();
            examples.Where(e => !e.IsAnswer).ForEach(e => e.Hide());
            yield return null;

            yield return tori.Correct();
            yield return null;

            frog.Correct();
            yield return new WaitForSeconds(0.5f);

            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.WordCLIP);

            fsm.PerformTransition(State.Cross);
        }
        IEnumerator X_Correct()
        {
            examples.Where(e => !e.IsAnswer).ForEach(e => e.Hidden());
            yield return null;

            tori.Idle();
            yield return null;

            //frog.CorrectFin();
            AudioMGR.One.StopNarration();
            yield return null;
        }
        IEnumerator E_Wrong()
        {
            ActivityProgress.One.Wrong();

            frog.Wrong();
            submitedExam.Wrong();
            yield return null;

            yield return tori.Wrong();
            yield return new WaitForSeconds(0.5f);

            fsm.PerformTransition(State.Solve);
        }
        IEnumerator X_Wrong()
        {
            submitedExam.Hidden();
            yield return null;
        }
        IEnumerator E_Cross()
        {
            submitedExam.Hide();
            yield return new WaitForSeconds(0.5f);

            currentBridge.Show();
            yield return new WaitForSeconds(1f);

            yield return tori.StartCross(positionGroup.Positions, pMGR.Current);

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_Cross()
        {
            AudioMGR.One.StopNarration();
            yield return null;

            var lastTR = positionGroup.Positions[positionGroup.Positions.Length - 1];
            tori.StopCross(lastTR);
            yield return null;
        }
        IEnumerator E_Next()
        {
            yield return null;

            if (pMGR.Next())
                fsm.PerformTransition(State.Problem);
            else fsm.PerformTransition(State.Outro);
        }
        IEnumerator E_Outro()
        {
            CP(CheckPoint.Outro);
            yield return null;

            tori.OutroIdle();
            yield return playTimeline(outroTL);

            fsm.PerformTransition(State.Reward);
        }
        IEnumerator X_Outro()
        {
            yield return stopTimeline(outroTL);
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
        [SerializeField] private Tori tori = null;
        [SerializeField] private Example[] examples = null;
        [SerializeField] private Lotus lotus = null;
        [SerializeField] private Frog frog = null;
        [SerializeField] private PositionGroup positionGroup = null;
        [SerializeField] private Bridge[] bridges = null;
        [SerializeField] private GameObject vfxCompleteGO = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip bloomingCLIP = null;
        [Header("★ Sound - Ambient")]
        [SerializeField] private AudioClip ambientCLIP = null;
        [Range(0, 100)]
        [SerializeField] int ambientVolume = 100;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector[] problemTL = null;
        [SerializeField] private PlayableDirector outroTL = null;
        [Header("★ Timing")]
        [SerializeField] private float exampleAppearDelay = 0.5f;
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

            problem1SIG.OnSignal += problemSIG_OnSignal;
            problem2SIG.OnSignal += problemSIG_OnSignal;
            problem3SIG.OnSignal += problemSIG_OnSignal;

            frog.OnClick += frog_OnClick;
            lotus.OnClick += lotus_OnClick;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            problem1SIG.OnSignal -= problemSIG_OnSignal;
            problem2SIG.OnSignal -= problemSIG_OnSignal;
            problem3SIG.OnSignal -= problemSIG_OnSignal;

            frog.OnClick -= frog_OnClick;
            lotus.OnClick -= lotus_OnClick;
        }

        // Unity Coroutine
        IEnumerator coPlayWord()
        {
            using (LOG.Coroutine($"coPlayWord()", this))
            {
                frog.EnableInteraction(false);
                lotus.EnableInteraction(false);
                examples.ForEach(e => e.EnableInteraction(false));
                yield return null;

                yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.PhonicsCLIP);
                yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.PhonicsCLIP);
                yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.WordCLIP);

                frog.EnableInteraction(true);
                lotus.EnableInteraction(true);
                examples.ForEach(e => e.EnableInteraction(true));
                yield return null;
            }
        }
    }
}