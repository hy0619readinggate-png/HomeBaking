using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Activity.UI;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

using ActivityData = DoDoEng.Common.ActivityData_C4_A07;
using ProblemMGR = DoDoEng.Activity.C4_A07.C4_A07_ProblemMGR;

namespace DoDoEng.Activity.C4_A07
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C4_A07_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C4_A07_BrokenIcyRoad;
        private enum State
        {
            Intro,
            ToProblem, Problem, Solve, Correct, Next,
            Outro, Reward
        }

        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();
        private TimelineSignal introSIG_ = null;
        private TimelineSignal introSIG => introSIG_ ??= introTL.GetComponent<TimelineSignal>();

        // Fields
        private FSM<State> fsm = null;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Intro,       E_Intro,        X_Intro);
            fsm.AddState(State.ToProblem,   E_ToProblem,    X_ToProblem);
            fsm.AddState(State.Problem,     E_Problem,      X_Problem);
            fsm.AddState(State.Solve,       E_Solve,        X_Solve);
            fsm.AddState(State.Correct,     E_Correct,      X_Correct);
            fsm.AddState(State.Next,        E_Next); 
            fsm.AddState(State.Outro,       E_Outro,        X_Outro);
            fsm.AddState(State.Reward,      E_Reward);
            #pragma warning restore format  
        }
        private void setupProblem()
        {
            problemZones.ForEach((i, z) => z.Setup(pMGR.Problems[i]));
        }
        private void setupAff()
        {
            var exampleTRs = cZone.Examples
                            .Where(e => e.IsAnswer)
                            .Select(e => e.transform)
                            .ToArray();
            var problemTRs = new Transform[] { cZone.Problem.transform };

            aff.Setup(exampleTRs, problemTRs);
        }

        // Functions
        private ProblemZone cZone => problemZones[pMGR.PNO - 1];

        // Event Handlers
        private void introSIG_OnSignal(string signal)
        {
            LOG.Info($"introSIG_OnSignal() | {signal}", this);

            if (signal == "Activity-SetupProblem")
                setupProblem();
        }



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            UIActivityCommon.One.VisibleSpeakerButton = true;
        }
        protected override async UniTask onPrepare(ActivityIndex actIDX)
        {
            await base.onPrepare(actIDX);

            // Load Unit Data
            await pMGR.Build(actIDX, CURRICULUM, TABLES);
            ActivityProgress.One.Setup(pMGR.BlanksCount);

            // devBOX(swon) - onInitActivity에서 실행시 오류 : Assertion failed on expression: 'm_DidAwake'
            evaluateTimeline(introTL);
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
                cZone.PlaySound();
            }
        }
        protected override void onDebugNext()
        {
            switch (fsm.CurrentState)
            {
                case State.Intro: fsm.PerformTransition(State.ToProblem); break;
                case State.ToProblem: fsm.PerformTransition(State.Problem); break;
                case State.Problem: fsm.PerformTransition(State.Solve); break;
                case State.Solve: fsm.PerformTransition(State.Correct); break;
                case State.Correct: fsm.PerformTransition(State.Next); break;
            }
        }



        // FSM
        IEnumerator E_Intro()
        {
            CP(CheckPoint.Start);
            yield return playTimeline(introTL);
            yield return null;

            fsm.PerformTransition(State.ToProblem);
        }
        IEnumerator X_Intro()
        {
            yield return stopTimeline(introTL);
            yield return null;
        }
        IEnumerator E_ToProblem()
        {
            yield return playTimeline(toProblemTL[pMGR.PNO - 1]);
            yield return null;

            fsm.PerformTransition(State.Problem);
        }
        IEnumerator X_ToProblem()
        {
            yield return stopTimeline(toProblemTL[pMGR.PNO - 1]);
            yield return null;
        }
        IEnumerator E_Problem()
        {
            setupAff();
            yield return cZone.ShowExamples();
            yield return null;

            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.SentenceCLIP);
            yield return null;

            fsm.PerformTransition(State.Solve);
        }
        IEnumerator X_Problem()
        {
            AudioMGR.One.StopNarration();
            yield return null;

            cZone.ShownExamples();
            yield return null;
        }
        IEnumerator E_Solve()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            UIActivityCommon.One.EnableSpeakerButton = true;
            cZone.EnableInteraction(true);
            yield return cZone.StartWaitForComplete();

            fsm.PerformTransition(State.Correct);
        }
        IEnumerator X_Solve()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            UIActivityCommon.One.EnableSpeakerButton = false;
            AudioMGR.One.StopNarration();
            cZone.EnableInteraction(false);
            cZone.FinishWaitForComplete();
            yield return null;
        }
        IEnumerator E_Correct()
        {
            cZone.ClearExamples();
            yield return new WaitForSeconds(1);

            var clip = UtilArray.ExtractOne(correctClip);
            AudioMGR.One.PlayEffect(clip);
            yield return playTimeline(correct1TL[pMGR.PNO - 1]);

            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.SentenceCLIP);

            yield return playTimeline(correct2TL[pMGR.PNO - 1]);
            yield return null;

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_Correct()
        {
            AudioMGR.One.StopNarration();

            yield return stopTimeline(correct1TL[pMGR.PNO - 1]);
            yield return null;

            yield return stopTimeline(correct2TL[pMGR.PNO - 1]);
            yield return null;
        }
        IEnumerator E_Next()
        {
            yield return null;
            if (pMGR.Next())
                fsm.PerformTransition(State.ToProblem);
            else fsm.PerformTransition(State.Outro);
        }
        IEnumerator E_Outro()
        {
            CP(CheckPoint.Outro);
            yield return null;

            UIActivityCommon.One.VisibleSpeakerButton = false;
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
        [Header("★ Bindings")]
        [SerializeField] private ProblemZone[] problemZones = null;
        [SerializeField] private AffDrag aff = null;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector introTL = null;
        [SerializeField] private PlayableDirector[] toProblemTL = null;
        [SerializeField] private PlayableDirector[] correct1TL = null;
        [SerializeField] private PlayableDirector[] correct2TL = null;
        [SerializeField] private PlayableDirector outroTL = null;
        [Header("★ Audios")]
        [SerializeField] private AudioClip[] correctClip = null;
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

            introSIG.OnSignal += introSIG_OnSignal;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            introSIG.OnSignal -= introSIG_OnSignal;
        }
    }
}