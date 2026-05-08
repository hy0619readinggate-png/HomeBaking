using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

using ActivityData = DoDoEng.Common.ActivityData_C2_A07;
using ProblemMGR = DoDoEng.Activity.C2_A07.C2_A07_ProblemMGR;

namespace DoDoEng.Activity.C2_A07
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C2_A07_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C2_A07_Baggage;
        private enum State
        {
            Intro,
            Problem, Solve, Correct, Wrong, Next,
            ChangeEx,
            Exit, NextSet,
            Outro, Reward
        }



        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();
        private TimelineSignal changeLuggage2SIG_ = null;
        private TimelineSignal changeLuggage2SIG => changeLuggage2SIG_ ??= problem2TL.GetComponent<TimelineSignal>();
        private TimelineSignal changeLuggage3SIG_ = null;
        private TimelineSignal changeLuggage3SIG => changeLuggage3SIG_ ??= problem3TL.GetComponent<TimelineSignal>();

        // Fields
        private FSM<State> fsm = null;

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
            fsm.AddState(State.Next,        Next);
            fsm.AddState(State.ChangeEx,    E_ChangeEx,     X_ChangeEx);
            fsm.AddState(State.Exit,        E_Exit,         X_Exit);
            fsm.AddState(State.NextSet,     NextSet);
            fsm.AddState(State.Outro,       E_Outro,        X_Outro);
            fsm.AddState(State.Reward,      E_Reward);
#pragma warning restore format
        }
        private void setupProblem(ProblemData pData)
        {
            problemBoard.Setup(pData);
            setupLuggages(pData);
            setupCarts();
        }
        private void setupLuggages(ProblemData pData)
        {
            var cases = UtilArray.Random(1, 3);
            for (var i = 0; i < pData.Examples.Length; i++)
            {
                var exam = pData.Examples[i];
                luggages[i].Setup(exam, cases[i]);
            }
        }
        private void setupCarts()
        {
            foreach (Cart cart in cartGroup.Carts)
                cart.Setup();
        }
        private void setupAff()
        {
            var luggageTRs = luggages
                                .Where(l => l.IsAnswer)
                                .Select(l => l.transform)
                                .Take(1)
                                .ToArray();
            var answerTRs = cartGroup
                                .EmptyCarts
                                .Take(1)
                                .ToArray();

            affordance.Setup(luggageTRs, answerTRs);
        }

        // Event Handlers
        private void changeLuggageSIG_OnSignal(string signal)
        {
            LOG.Info($"problemSIG_OnSignal() | {signal}", this);

            if (signal == "Activity-SetupProblem")
                setupLuggages(pMGR.Current);
        }



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            evaluateTimeline(intro2TL);

            stepPanelCG.SetActiveOnly(0);
        }
        protected override async UniTask onPrepare(ActivityIndex actIDX)
        {
            await base.onPrepare(actIDX);

            // Load Unit Data
            await pMGR.Build(actIDX, CURRICULUM, TABLES);
            ActivityProgress.One.Setup(pMGR.BlanksCount);

            // setup
            //setupProblem(pMGR.Current);
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
                case State.Correct: fsm.PerformTransition(State.Next); break;
                case State.ChangeEx: fsm.PerformTransition(State.Problem); break;
                case State.Exit: fsm.PerformTransition(State.NextSet); break;
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
            setupProblem(pMGR.Current);

            var tl = pMGR.Current.CartCount == 2 ? intro2TL : intro3TL;
            yield return playTimeline(tl);

            fsm.PerformTransition(State.Problem);
        }
        IEnumerator X_Intro()
        {
            var tl = pMGR.Current.CartCount == 2 ? intro2TL : intro3TL;
            yield return stopTimeline(tl);
        }
        IEnumerator E_Problem()
        {
            setupAff();
            edmond.Idle();

            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.PhoneticCLIP);
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

            foreach (Luggage luggage in luggages)
                luggage.EnableInteraction(true);
            luggageCG.blocksRaycasts = true;
            cartGroup.EnableInteraction(true);

            yield return cartGroup.StartWaitLuggages();
            yield return new WaitForSeconds(0.5f);

            var correct = cartGroup.CurrentDropedCart.IsFilled;
            fsm.PerformTransition(correct ? State.Correct : State.Wrong);
        }
        IEnumerator X_Solve()
        {
            CP(CheckPoint.UserFinish);

            foreach (Luggage luggage in luggages)
                luggage.EnableInteraction(false);
            luggageCG.blocksRaycasts = false;
            cartGroup.EnableInteraction(false);

            yield return null;
        }
        IEnumerator E_Correct()
        {
            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.PhoneticCLIP);
            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.PhoneticCLIP);
            var wordClip = pMGR.Current.Examples.First(e => e.IsAnswer).WordCLIP;
            yield return AudioMGR.One.PlayNarrationAndWait(wordClip);

            yield return edmond.Correct();
            edmond.Idle();

            yield return new WaitForSeconds(1f);

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_Correct()
        {
            yield return null;
        }
        IEnumerator E_Wrong()
        {
            ActivityProgress.One.Wrong();
            cartGroup.CurrentDropedCart.DroppedLuggage.Return();
            yield return null;

            yield return edmond.Wrong();
            edmond.Idle();

            yield return null;

            fsm.PerformTransition(State.Solve);
        }
        IEnumerator X_Wrong()
        {
            yield return null;
        }
        IEnumerator Next()
        {
            yield return null;

            if (pMGR.Current.LastCart)
                fsm.PerformTransition(State.Exit);
            else if (pMGR.Next())
                fsm.PerformTransition(State.ChangeEx);
            else fsm.PerformTransition(State.Exit);
        }
        IEnumerator E_ChangeEx()
        {
            var tl = pMGR.Current.CartCount == 2 ? problem2TL : problem3TL;
            yield return playTimeline(tl);

            fsm.PerformTransition(State.Problem);
        }
        IEnumerator X_ChangeEx()
        {
            var tl = pMGR.Current.CartCount == 2 ? problem2TL : problem3TL;
            yield return stopTimeline(tl);
        }
        IEnumerator E_Exit()
        {
            var tl = pMGR.Current.CartCount == 2 ? exit2TL : exit3TL;
            yield return playTimeline(tl);

            fsm.PerformTransition(State.NextSet);
        }
        IEnumerator X_Exit()
        {
            var tl = pMGR.Current.CartCount == 2 ? exit2TL : exit3TL;
            yield return stopTimeline(tl);
        }
        IEnumerator NextSet()
        {
            yield return null;

            if (pMGR.Next())
                fsm.PerformTransition(State.Intro);
            else fsm.PerformTransition(State.Outro);
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
        [SerializeField] private Problem problemBoard = null;
        [SerializeField] private CanvasGroup luggageCG = null;
        [SerializeField] private Luggage[] luggages = null;
        [SerializeField] private CartGroup cartGroup = null;
        [SerializeField] private Edmond edmond = null;
        [SerializeField] private AffDrag affordance = null;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector intro2TL = null;
        [SerializeField] private PlayableDirector intro3TL = null;
        [SerializeField] private PlayableDirector problem2TL = null;
        [SerializeField] private PlayableDirector problem3TL = null;
        [SerializeField] private PlayableDirector exit2TL = null;
        [SerializeField] private PlayableDirector exit3TL = null;
        [SerializeField] private PlayableDirector outroTL = null;
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

            changeLuggage2SIG.OnSignal += changeLuggageSIG_OnSignal;
            changeLuggage3SIG.OnSignal += changeLuggageSIG_OnSignal;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            changeLuggage2SIG.OnSignal -= changeLuggageSIG_OnSignal;
            changeLuggage3SIG.OnSignal -= changeLuggageSIG_OnSignal;
        }
    }
}