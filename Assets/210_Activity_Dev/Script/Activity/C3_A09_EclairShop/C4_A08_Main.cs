using beyondi.FSM;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Activity.UI;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

using ActivityData = DoDoEng.Common.ActivityData_C4_A08;
using ProblemMGR = DoDoEng.Activity.C3_A09.C4_A08_ProblemMGR;

namespace DoDoEng.Activity.C3_A09
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C4_A08_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C4_A08_ToyShop;
        private enum State
        {
            Intro,
            CustomerIn, ProblemIn, Solve, ProblemOut, CustomerOut, Next,
            Outro, Reward
        }

        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();
        private TimelineSignal problemOutSIG_ = null;
        private TimelineSignal problemOutSIG => problemOutSIG_ ??= problemOutTL.GetComponent<TimelineSignal>();
        private TimelineSignal outroSIG_ = null;
        private TimelineSignal outroSIG => outroSIG_ ??= customerOutTL.GetComponent<TimelineSignal>();

        // Fields
        private FSM<State> fsm = null;
        private Coroutine crPlaySound = null;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Intro,       E_Intro,        X_Intro);
            fsm.AddState(State.CustomerIn,  E_CustomerIn,   X_CustomerIn);
            fsm.AddState(State.ProblemIn,   E_ProblemIn,    X_ProblemIn);
            fsm.AddState(State.Solve,       E_Solve,        X_Solve);
            fsm.AddState(State.ProblemOut,  E_ProblemOut,   X_ProblemOut);
            fsm.AddState(State.CustomerOut, E_CustomerOut,  X_CustomerOut);
            fsm.AddState(State.Next,        E_Next);
            fsm.AddState(State.Outro,       E_Outro,        X_Outro);
            fsm.AddState(State.Reward,      E_Reward);
            #pragma warning restore format  
        }
        private void setupProblem(ProblemData pData)
        {
            breadMGR.Setup(pData.Examples);
            tray.Setup(pData);
            Customer.One.Setup(pData.CustomerID);
        }

        // Event Handlers
        protected override void onSpeaker()
        {
            base.onSpeaker();

            if (fsm.CurrentState == State.Solve)
            {
                crPlaySound = StartCoroutine(coPlayWordSound(pMGR.Current.SentenceCLIP));
            }
        }
        private void problemOutSIG_OnSignal(string signal)
        {
            LOG.Info($"problemOutSIG_OnSignal() | {signal}", this);

            if (signal == "Activity-ExtraAnimation")
                tray.HideTrayWord();
        }
        private void outroSIG_OnSignal(string signal)
        {
            LOG.Info($"outroSIG_OnSignal() | {signal}", this);

            if (signal == "Activity-ExtraAnimation")
                AudioMGR.One.PlayEffect(customerOutroClip[pMGR.Current.CustomerID - 1]);
        }



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            vfxCompleteGO.SetActive(false);

            evaluateTimeline(customerInTL);
        }
        protected override async UniTask onPrepare(ActivityIndex actIDX)
        {
            await base.onPrepare(actIDX);

            // Load Unit Data
            await pMGR.Build(actIDX, CURRICULUM, TABLES);
            ActivityProgress.One.Setup(pMGR.BlanksCount);

            intro.Setup(pMGR.IntroDatas);
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
        protected override void onDebugNext()
        {
            switch (fsm.CurrentState)
            {
                case State.Intro: fsm.PerformTransition(State.CustomerIn); break;
                case State.CustomerIn: fsm.PerformTransition(State.ProblemIn); break;
                case State.ProblemIn: fsm.PerformTransition(State.Solve); break;
                case State.Solve: fsm.PerformTransition(State.ProblemOut); break;
                case State.ProblemOut: fsm.PerformTransition(State.CustomerOut); break;
                case State.CustomerOut: fsm.PerformTransition(State.Next); break;
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

            yield return null;

            UIActivityCommon.One.VisibleSpeakerButton = true;

            fsm.PerformTransition(State.CustomerIn);
        }
        IEnumerator X_Intro()
        {
            UIActivityCommon.One.VisibleSpeakerButton = true;

            if (pMGR.IsIntroDataExist)
                intro.FinishPlay();

            AudioMGR.One.StopNarration();
            yield return null;
        }
        IEnumerator E_CustomerIn()
        {
            setupProblem(pMGR.Current);
            yield return playTimeline(customerInTL);

            fsm.PerformTransition(State.ProblemIn);
        }
        IEnumerator X_CustomerIn()
        {
            yield return stopTimeline(customerInTL);
        }
        IEnumerator E_ProblemIn()
        {
            Customer.One.Idle();
            yield return playTimeline(problemInTL);

            yield return breadMGR.StartShow();

            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.SentenceCLIP);

            fsm.PerformTransition(State.Solve);
        }
        IEnumerator X_ProblemIn()
        {
            AudioMGR.One.StopNarration();
            yield return null;

            yield return stopTimeline(problemInTL);

            breadMGR.FinishShow();
            yield return null;
        }
        IEnumerator E_Solve()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            UIActivityCommon.One.EnableSpeakerButton = true;
            breadMGR.EnableInteraction(true);
            yield return null;

            yield return tray.StartWaitComplete();

            fsm.PerformTransition(State.ProblemOut);
        }
        IEnumerator X_Solve()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            AudioMGR.One.StopNarration();
            yield return null;

            UIActivityCommon.One.EnableSpeakerButton = false;
            breadMGR.EnableInteraction(false);
            yield return null;

            tray.FinishWaitComplete();
            yield return null;
        }
        IEnumerator E_ProblemOut()
        {
            yield return breadMGR.StartHide(true);

            yield return playTimeline(problemOutTL);

            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.SentenceCLIP);

            fsm.PerformTransition(State.CustomerOut);
        }
        IEnumerator X_ProblemOut()
        {
            AudioMGR.One.StopNarration();
            yield return null;

            tray.HideTrayWord();
            yield return stopTimeline(problemOutTL);

            breadMGR.FinishHide();
            yield return null;
        }
        IEnumerator E_CustomerOut()
        {
            yield return playTimeline(customerOutTL);
            yield return new WaitForSeconds(1);

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_CustomerOut()
        {
            yield return stopTimeline(customerOutTL);
        }
        IEnumerator E_Next()
        {
            yield return null;

            if (pMGR.Next())
                fsm.PerformTransition(State.CustomerIn);
            else fsm.PerformTransition(State.Outro);
        }
        IEnumerator E_Outro()
        {
            CP(CheckPoint.Outro);
            yield return null;

            vfxCompleteGO.SetActive(true);
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
        [SerializeField] private Intro intro = null;
        [SerializeField] private BreadMGR breadMGR = null;
        [SerializeField] private Tray tray = null;
        [SerializeField] private GameObject vfxCompleteGO = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip[] customerOutroClip = null;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector customerInTL = null;
        [SerializeField] private PlayableDirector problemInTL = null;
        [SerializeField] private PlayableDirector problemOutTL = null;
        [SerializeField] private PlayableDirector customerOutTL = null;
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

            problemOutSIG.OnSignal += problemOutSIG_OnSignal;
            outroSIG.OnSignal += outroSIG_OnSignal;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            problemOutSIG.OnSignal -= problemOutSIG_OnSignal;
            outroSIG.OnSignal -= outroSIG_OnSignal;
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