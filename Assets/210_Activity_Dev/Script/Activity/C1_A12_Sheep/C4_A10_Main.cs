using beyondi.Coroutine;
using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.C1_A05;
using DoDoEng.Activity.Framework;
using DoDoEng.Activity.UI;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using ActivityData = DoDoEng.Common.ActivityData_C4_A10;
using ProblemMGR = DoDoEng.Activity.C1_A12.C4_A10_ProblemMGR;

namespace DoDoEng.Activity.C1_A12
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C4_A10_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C4_A10_Sheep;
        private enum State
        {
            Intro,
            Enter, Problem,
            Solve, Center, Success,
            Next,
            Reset,
            Outro, Reward
        }

        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();
        private TimelineSignal success1_1SIG_ = null;
        private TimelineSignal success1_1SIG => success1_1SIG_ ??= successLeftTL[0].GetComponent<TimelineSignal>();
        private TimelineSignal success1_2SIG_ = null;
        private TimelineSignal success1_2SIG => success1_2SIG_ ??= successLeftTL[1].GetComponent<TimelineSignal>();
        private TimelineSignal success1_3SIG_ = null;
        private TimelineSignal success1_3SIG => success1_3SIG_ ??= successLeftTL[2].GetComponent<TimelineSignal>();
        private TimelineSignal success2_1SIG_ = null;
        private TimelineSignal success2_1SIG => success2_1SIG_ ??= successRightTL[0].GetComponent<TimelineSignal>();
        private TimelineSignal success2_2SIG_ = null;
        private TimelineSignal success2_2SIG => success2_2SIG_ ??= successRightTL[1].GetComponent<TimelineSignal>();
        private TimelineSignal success2_3SIG_ = null;
        private TimelineSignal success2_3SIG => success2_3SIG_ ??= successRightTL[2].GetComponent<TimelineSignal>();

        // Fields
        private FSM<State> fsm = null;
        private PlayableDirector currentTL = null;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Intro,      E_Intro,   X_Intro);
            fsm.AddState(State.Enter,      E_Enter,   X_Enter);
            fsm.AddState(State.Problem,    E_Problem, X_Problem);
            fsm.AddState(State.Solve,      E_Solve,   X_Solve);
            fsm.AddState(State.Center,     E_Center,  X_Center);
            fsm.AddState(State.Success,    E_Success, X_Success);
            fsm.AddState(State.Next,       E_Next);
            fsm.AddState(State.Reset,      E_Reset,   X_Reset);
            fsm.AddState(State.Outro,      E_Outro,   X_Outro);
            fsm.AddState(State.Reward,     E_Reward);
            #pragma warning restore format  
        }
        private void setupProblem(ProblemData pData)
        {
            papa.Setup();
            var sheepCount = pMGR.Current.SheepTypes.Length;
            sheepMGR.InitSheeps(pMGR.Current.SheepTypes);

            foreach (var (ex, i) in pData.Examples.Select((ex, i) => (ex, i)))
                barns[i].Setup(ex, sheepCount);

            var papaTR = new Transform[] { papa.transform };
            var barnTR = barns
                            .Where(b => b.IsAnswer)
                            .Take(1)
                            .Select(c => c.transform)
                            .ToArray();
            affDrag.Setup(papaTR, barnTR);
        }

        // Functions
        private PlayableDirector getRandomSuccessTL(int barn)
        {
            if(barn == 1)
                return UtilArray.ExtractOne(successLeftTL);
            else return UtilArray.ExtractOne(successRightTL);
        }

        // Event Handlers
        private void barnLBTN_onClick()
        {
            LOG.Info($"barnLBTN_onClick()", this);

            if (fsm.CurrentState == State.Solve)
                AudioMGR.One.PlayNarration(pMGR.Current.Examples[0].WordCLIP);
        }
        private void barnRBTN_onClick()
        {
            LOG.Info($"barnRBTN_onClick()", this);

            if (fsm.CurrentState == State.Solve)
                AudioMGR.One.PlayNarration(pMGR.Current.Examples[1].WordCLIP);
        }
        private void successSIG_OnSignal(string signal)
        {
            LOG.Info($"successSIG_OnSignal() | {signal}", this);

            if (signal == "Activity-ProblemSound")
                AudioMGR.One.PlayNarration(pMGR.Current.WordCLIP);
        }



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            UIActivityCommon.One.VisibleSpeakerButton = true;

            barns.AutoFillID();
            aff.Enabler = () => sheepMGR.IsRemainFreeSheep();
            aff.gameObject.SetActive(true);
            affDrag.Enabler = () => !sheepMGR.IsRemainFreeSheep();
            affDrag.gameObject.SetActive(true);

            evaluateTimeline(enterTL);
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
        protected override void onSpeaker()
        {
            base.onSpeaker();

            if (fsm.CurrentState == State.Solve)
            {
                AudioMGR.One.PlayNarration(pMGR.Current.WordCLIP);
            }
        }
        protected override void onDebugNext()
        {
            switch (fsm.CurrentState)
            {
                case State.Intro: fsm.PerformTransition(State.Enter); break;
                case State.Enter: fsm.PerformTransition(State.Problem); break;
                case State.Problem: fsm.PerformTransition(State.Solve); break;
                case State.Solve:
                    sheepMGR.ClearSheeps();
                    fsm.PerformTransition(State.Center);
                    break;
                case State.Center: fsm.PerformTransition(State.Success); break;
                case State.Success: fsm.PerformTransition(State.Next); break;
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

            fsm.PerformTransition(State.Enter);
        }
        IEnumerator X_Intro()
        {
            yield return null;
        }
        IEnumerator E_Enter()
        {
            yield return playTimeline(enterTL);
            yield return null;

            fsm.PerformTransition(State.Problem);
        }
        IEnumerator X_Enter()
        {
            yield return stopTimeline(enterTL);
            yield return null;
        }
        IEnumerator E_Problem()
        {
            barns.ForEach(b => b.Open());
            currentTL = UtilArray.ExtractOne(problemTL);
            yield return playTimeline(currentTL);
            yield return null;

            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.WordCLIP);
            yield return null;

            fsm.PerformTransition(State.Solve);
        }
        IEnumerator X_Problem()
        {
            yield return stopTimeline(currentTL);
            yield return null;

            AudioMGR.One.StopNarration();
            yield return null;
        }
        IEnumerator E_Solve()
        {
            UIActivityCommon.One.EnableSpeakerButton = true;

            CP(CheckPoint.UserStart);
            yield return null;

            PlayerController.One.EnableInteraction(true);

            var barn = barns.Single(b => b.IsAnswer);
            yield return new WaitForComplete(this, barn);

            fsm.PerformTransition(State.Center);
        }
        IEnumerator X_Solve()
        {
            UIActivityCommon.One.EnableSpeakerButton = false;

            CP(CheckPoint.UserFinish);
            yield return null;

            PlayerController.One.EnableInteraction(false);
        }
        IEnumerator E_Center()
        {
            yield return papa.MoveToCenterAndWait();
            yield return null;

            fsm.PerformTransition(State.Success);
        }
        IEnumerator X_Center()
        {
            papa.MoveToCenter();
            yield return null;
        }
        IEnumerator E_Success()
        {
            var barn = barns.Single(b => b.IsAnswer);
            currentTL = getRandomSuccessTL(barn.ID);
            yield return playTimeline(currentTL);

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_Success()
        {
            sheepMGR.ClearSheeps();
            yield return null;

            yield return stopTimeline(currentTL);
        }
        IEnumerator E_Next()
        {
            yield return null;

            if (pMGR.Next())
                fsm.PerformTransition(State.Reset);
            else fsm.PerformTransition(State.Outro);
        }
        IEnumerator E_Reset()
        {
            setupProblem(pMGR.Current);

            yield return playTimeline(resetTL);
            yield return null;

            fsm.PerformTransition(State.Enter);
        }
        IEnumerator X_Reset()
        {
            yield return stopTimeline(resetTL);
        }
        IEnumerator E_Outro()
        {
            CP(CheckPoint.Outro);
            yield return null;

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
        [SerializeField] private SheepMGR sheepMGR = null;
        [SerializeField] private Papa papa = null;
        [SerializeField] private Barn[] barns = null;
        [SerializeField] private Button barnLBTN = null;
        [SerializeField] private Button barnRBTN = null;
        [SerializeField] private AffBase aff = null;
        [SerializeField] private AffDrag affDrag = null;
        [Header("★ Sound - Ambient")]
        [SerializeField] private AudioClip ambientCLIP = null;
        [Range(0, 100)]
        [SerializeField] int ambientVolume = 100;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector enterTL = null;
        [SerializeField] private PlayableDirector[] problemTL = null;
        [SerializeField] private PlayableDirector[] successLeftTL = null;
        [SerializeField] private PlayableDirector[] successRightTL = null;
        [SerializeField] private PlayableDirector resetTL = null;
        [Header("★ Timing")]
        [SerializeField] private float rewardDelay = 1f;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            initFSM();

            barnLBTN.onClick.AddListener(barnLBTN_onClick);
            barnRBTN.onClick.AddListener(barnRBTN_onClick);
        }
        protected override void Start()
        {

        }
        protected override void OnEnable()
        {
            base.OnEnable();

            success1_1SIG.OnSignal += successSIG_OnSignal;
            success1_2SIG.OnSignal += successSIG_OnSignal;
            success1_3SIG.OnSignal += successSIG_OnSignal;
            success2_1SIG.OnSignal += successSIG_OnSignal;
            success2_2SIG.OnSignal += successSIG_OnSignal;
            success2_3SIG.OnSignal += successSIG_OnSignal;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            success1_1SIG.OnSignal -= successSIG_OnSignal;
            success1_2SIG.OnSignal -= successSIG_OnSignal;
            success1_3SIG.OnSignal -= successSIG_OnSignal;
            success2_1SIG.OnSignal -= successSIG_OnSignal;
            success2_2SIG.OnSignal -= successSIG_OnSignal;
            success2_3SIG.OnSignal -= successSIG_OnSignal;
        }
    }
}