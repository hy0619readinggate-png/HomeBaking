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

using ActivityData = DoDoEng.Common.ActivityData_C2_A03;
using ProblemMGR = DoDoEng.Activity.C2_A03.C2_A03_ProblemMGR;

namespace DoDoEng.Activity.C2_A03
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C2_A03_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C2_A03_ToriAndDolphin;
        private enum State
        {
            Intro,
            Problem,
            Solve, Correct1, Correct2, Wrong,
            Next, Move,
            Outro, Reward
        }

        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();
        private TimelineSignal introSIG_ = null;
        private TimelineSignal introSIG => introSIG_ ??= introTL.GetComponent<TimelineSignal>();
        private TimelineSignal moveSIG_ = null;
        private TimelineSignal moveSIG => moveSIG_ ??= moveTL.GetComponent<TimelineSignal>();
        private TimelineSignal wrongSIG1_ = null;
        private TimelineSignal wrongSIG1 => wrongSIG1_ ??= wrongTL[0].GetComponent<TimelineSignal>();
        private TimelineSignal wrongSIG2_ = null;
        private TimelineSignal wrongSIG2 => wrongSIG2_ ??= wrongTL[1].GetComponent<TimelineSignal>();
        private TimelineSignal wrongSIG3_ = null;
        private TimelineSignal wrongSIG3 => wrongSIG3_ ??= wrongTL[2].GetComponent<TimelineSignal>();

        // Fields
        private FSM<State> fsm = null;
        private Dolphin submitDolphin = null;
        private Coroutine crPlayNarration = null;
        private PlayableDirector correct1TL = null;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Intro,     E_Intro,     X_Intro);
            fsm.AddState(State.Problem,   E_Problem,   X_Problem);
            fsm.AddState(State.Solve,     E_Solve,     X_Solve);
            fsm.AddState(State.Correct1,  E_Correct1,  X_Correct1);
            fsm.AddState(State.Correct2,  E_Correct2,  X_Correct2);
            fsm.AddState(State.Wrong,     E_Wrong,     X_Wrong);
            fsm.AddState(State.Next,      E_Next);
            fsm.AddState(State.Move,      E_Move,      X_Move);
            fsm.AddState(State.Outro,     E_Outro,     X_Outro);
            fsm.AddState(State.Reward,    E_Reward);
            #pragma warning restore format  
        }
        private void setupProblem(ProblemData pData)
        {
            dolphineInWorld.SetSkin(pData.DolphinType);

            foreach (var (dolphin, i) in dolphins.Select((i, j) => (i, j)))
                dolphin.Setup(pData.Examples[i], pData.DolphinType);

            for (var i = 0; i < affs.Length; i++)
            {
                affs[i].EnableAff = pData.Examples[i].IsAnswer;
            }
        }

        // Event Handlers
        private void timelineSignal_OnSignal(string signal)
        {
            LOG.Info($"timelineSignal_OnSignal() | {signal}", this);

            if (signal == "Activity-SetupProblem")
                setupProblem(pMGR.Current);

            if (signal == "Activity-ExtraAnimation")
            {
                seagull.Setup(pMGR.Current);
                boat.Setup(pMGR.Current);
            }

            if (signal == "Activity-ProblemSound")
            {
                AudioMGR.One.PlayNarration(submitDolphin.PhonicsCLIP);
            }
        }



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            evaluateTimeline(introTL);

            stepPanelCG.SetActiveOnly(0);
            dolphins.AutoFillID();
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
                case State.Solve:
                    submitDolphin = dolphins.Single(d => d.IsAnswer);
                    fsm.PerformTransition(State.Correct1);
                    break;
                case State.Correct1: fsm.PerformTransition(State.Correct2); break;
                case State.Correct2: fsm.PerformTransition(State.Next); break;
                case State.Wrong: fsm.PerformTransition(State.Problem); break;
                case State.Next: fsm.PerformTransition(State.Problem); break;
                case State.Move: fsm.PerformTransition(State.Solve); break;
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
            yield return playTimeline(introTL);
            yield return null;

            fsm.PerformTransition(State.Problem);
        }
        IEnumerator X_Intro()
        {
            yield return stopTimeline(introTL);
            yield return null;
        }
        IEnumerator E_Problem()
        {
            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.WordCLIP);
            yield return new WaitForSeconds(0.5f);

            fsm.PerformTransition(State.Solve);
        }
        IEnumerator X_Problem()
        {
            yield return null;
        }
        IEnumerator E_Solve()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            seagull.EnableInteraction(true);
            dolphins.ForEach(d => d.EnableInteraction(true));
            yield return null;

            var wait = new WaitForSubmit(this, dolphins);
            yield return wait;

            submitDolphin = wait.Submited as Dolphin;
            if (submitDolphin.IsAnswer)
                fsm.PerformTransition(State.Correct1);
            else fsm.PerformTransition(State.Wrong);
        }
        IEnumerator X_Solve()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            seagull.EnableInteraction(false);
            dolphins.ForEach(d => d.EnableInteraction(false));
            yield return null;
        }
        IEnumerator E_Correct1()
        {
            correct1TL = submitDolphin.ID switch
            {
                1 => UtilArray.ExtractOne(correct1_1TL),
                2 => UtilArray.ExtractOne(correct1_2TL),
                3 => UtilArray.ExtractOne(correct1_3TL),
                _ => null
            };
            yield return playTimeline(correct1TL);

            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.PhonicsCLIP);
            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.PhonicsCLIP);
            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.WordCLIP);

            fsm.PerformTransition(State.Correct2);
        } 
        IEnumerator X_Correct1()
        {
            yield return stopTimeline(correct1TL);
        }
        IEnumerator E_Correct2()
        {
            var tl = correct2TL[submitDolphin.ID - 1];
            yield return playTimeline(tl);

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_Correct2()
        {
            this.StopCoroutineSafe(ref crPlayNarration);
            AudioMGR.One.StopNarration();

            var tl = correct2TL[submitDolphin.ID - 1];
            yield return stopTimeline(tl);
        }
        IEnumerator E_Wrong()
        {
            ActivityProgress.One.Wrong();
            
            var tl = wrongTL[submitDolphin.ID - 1];
            yield return playTimeline(tl);

            fsm.PerformTransition(State.Problem);
        }
        IEnumerator X_Wrong()
        {
            AudioMGR.One.StopNarration();
            var tl = wrongTL[submitDolphin.ID - 1];
            yield return stopTimeline(tl);
        }
        IEnumerator E_Next()
        {
            yield return null;

            if (pMGR.Next())
                fsm.PerformTransition(State.Move);
            else fsm.PerformTransition(State.Outro);
        }
        IEnumerator E_Move()
        {
            yield return playTimeline(moveTL);
            yield return null;

            fsm.PerformTransition(State.Problem);
        }
        IEnumerator X_Move()
        {
            yield return stopTimeline(moveTL);
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
        [SerializeField] private Seagull seagull = null;
        [SerializeField] private Boat boat = null;
        [SerializeField] private Dolphin[] dolphins = null;
        [SerializeField] private DolphinAni dolphineInWorld = null;
        [SerializeField] private AffBase[] affs = null;
        [Header("★ Sound - Ambient")]
        [SerializeField] private AudioClip ambientCLIP = null;
        [Range(0, 100)]
        [SerializeField] int ambientVolume = 100;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector introTL = null;
        [SerializeField] private PlayableDirector[] correct1_1TL = null;
        [SerializeField] private PlayableDirector[] correct1_2TL = null;
        [SerializeField] private PlayableDirector[] correct1_3TL = null;
        [SerializeField] private PlayableDirector[] correct2TL = null;
        [SerializeField] private PlayableDirector[] wrongTL = null;
        [SerializeField] private PlayableDirector moveTL = null;
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

            introSIG.OnSignal += timelineSignal_OnSignal;
            moveSIG.OnSignal += timelineSignal_OnSignal;
            wrongSIG1.OnSignal += timelineSignal_OnSignal;
            wrongSIG2.OnSignal += timelineSignal_OnSignal;
            wrongSIG3.OnSignal += timelineSignal_OnSignal;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            introSIG.OnSignal -= timelineSignal_OnSignal;
            moveSIG.OnSignal -= timelineSignal_OnSignal;
        }

        // Coroutine
        IEnumerator coPlayNarration()
        {
            using (LOG.Coroutine($"", this))
            {
                yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.PhonicsCLIP);
                yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.PhonicsCLIP);
                yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.WordCLIP);
            }
        }
    }
}