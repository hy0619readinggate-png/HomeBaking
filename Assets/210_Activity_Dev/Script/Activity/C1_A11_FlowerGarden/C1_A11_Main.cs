using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using ActivityData = DoDoEng.Common.ActivityData_C1_A11;
using ProblemMGR = DoDoEng.Activity.C1_A11.C1_A11_ProblemMGR;

namespace DoDoEng.Activity.C1_A11
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C1_A11_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C1_A11_FlowerGarden;
        private enum State
        {
            S1Intro, S1Problem, S1Solve, Next, ToNextField,
            S2Outro, Reward
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
            fsm.AddState(State.S1Intro,     E_S1Intro,      X_S1Intro);
            fsm.AddState(State.S1Problem,   E_S1Problem,    X_S1Problem);
            fsm.AddState(State.S1Solve,     E_S1Solve,      X_S1Solve);
            fsm.AddState(State.Next,        E_Next);
            fsm.AddState(State.ToNextField, E_ToNextField,  X_ToNextField);
            fsm.AddState(State.S2Outro,     E_S2Outro,      X_S2Outro);
            fsm.AddState(State.Reward,      E_Reward);
            #pragma warning restore format  
        }
        private void setupProblem(int pNO, ProblemData pData)
        {
            flowerMGR.ForEach((i, mgr) => mgr.enabled = i == pNO - 1);
            cFlowerMGR.Setup(pNO, pData.Examples, pData.PhoneticCLIP);
        }
        private void enableSpeakers(bool enable)
        {
            speakerBTN.ForEach(btn => btn.interactable = enable);
        }

        // Functions
        private FlowerMGR cFlowerMGR => flowerMGR[pMGR.PNO - 1];
        private Animator cSpeakerANIM => speakerANIM[pMGR.PNO - 1];

        // Event Handlers
        private void speakerBTN_onClick()
        {
            LOG.Info($"speakerBTN_onClick()", this);

            StartCoroutine(coPlayNarration());
        }
        private void flowerMGR_OnMonsterFinish()
        {
            LOG.Info($"flowerMGR_OnMonsterFinish()", this);

            StartCoroutine(coPlayNarration());
        }



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            evaluateTimeline(introTL);

            stepPanelCG.SetActiveOnly(0);
            enableSpeakers(false);
        }
        protected override async UniTask onPrepare(ActivityIndex actIDX)
        {
            await base.onPrepare(actIDX);

            // Load Unit Data
            await pMGR.Build(actIDX, CURRICULUM, TABLES);
            ActivityProgress.One.Setup(pMGR.BlanksCount);

            // setup
            setupProblem(pMGR.PNO, pMGR.Current);
        }
        protected override void onStartActivity()
        {
            base.onStartActivity();

            playBGM();
            fsm.StartFSM(State.S1Intro, RunnerParam.SkipStateTo);
        }
        protected override void onFinishActivity()
        {
            base.onFinishActivity();

            if (fsm.CurrentState == State.S1Solve)
                CP(CheckPoint.UserFinish);

            stopBGM();
            fsm?.StopFSM();
        }
        protected override void onDebugNext()
        {
            switch (fsm.CurrentState)
            {
                case State.S1Intro: fsm.PerformTransition(State.S1Problem); break;
                case State.S1Problem: fsm.PerformTransition(State.S1Solve); break;
                case State.S1Solve: fsm.PerformTransition(State.Next); break;
                case State.ToNextField: fsm.PerformTransition(State.S1Intro); break;
                case State.S2Outro: fsm.PerformTransition(State.Reward); break;
            }
        }
        protected override void onDebugForceFinish()
        {
            base.onDebugForceFinish();

            fsm.PerformTransition(State.Reward);
        }



        // FSM
        IEnumerator E_S1Intro()
        {
            CP(CheckPoint.Start);
            yield return playTimeline(introTL);
            yield return null;

            fsm.PerformTransition(State.S1Problem);
        }
        IEnumerator X_S1Intro()
        {
            yield return stopTimeline(introTL);
            yield return null;
        }
        IEnumerator E_S1Problem()
        {
            Gino.One.Idle();
            yield return null;

            cSpeakerANIM.SetBool("Playing", true);
            yield return null;

            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.PhoneticCLIP); ;
            yield return null;

            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.PhoneticCLIP); ;
            yield return null;

            cSpeakerANIM.SetBool("Playing", false);
            yield return null;

            fsm.PerformTransition(State.S1Solve);
        }
        IEnumerator X_S1Problem()
        {
            cSpeakerANIM.SetBool("Playing", false);
            yield return null;

            AudioMGR.One.StopNarration();
            yield return null;
        }
        IEnumerator E_S1Solve()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            enableSpeakers(true);
            cFlowerMGR.EnableInteraction(true);
            yield return null;

            yield return cFlowerMGR.StartWaitCollectAll();
            yield return null;

            enableSpeakers(false);
            cFlowerMGR.EnableInteraction(false);
            yield return null;

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_S1Solve()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            enableSpeakers(false);
            cFlowerMGR.EnableInteraction(false);
            yield return null;

            cFlowerMGR.StopWaitCollectAll();
            Gino.One.Idle();
            yield return null;
        }
        IEnumerator E_Next()
        {
            yield return null;

            if (pMGR.Next())
                fsm.PerformTransition(State.ToNextField);
            else fsm.PerformTransition(State.S2Outro);
        }
        IEnumerator E_ToNextField()
        {
            setupProblem(pMGR.PNO, pMGR.Current);
            yield return null;

            yield return playTimelines(nextFieldTL);
            yield return null;

            fsm.PerformTransition(State.S1Problem);
        }
        IEnumerator X_ToNextField()
        {
            yield return stopTimelines(nextFieldTL);
            yield return null;
        }
        IEnumerator E_S2Outro()
        {
            CP(CheckPoint.Outro);
            yield return null;

            yield return playTimeline(outroTL);
            yield return null;

            fsm.PerformTransition(State.Reward);
        }
        IEnumerator X_S2Outro()
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
        [Header("★ Bindings - S1.Intro")]
        [SerializeField] private FlowerMGR[] flowerMGR = null;
        [SerializeField] private Button[] speakerBTN = null;
        [SerializeField] private Animator[] speakerANIM = null;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector introTL = null;
        [SerializeField] private PlayableDirector nextFieldTL = null;
        [SerializeField] private PlayableDirector outroTL = null;
        [Header("★ Timing")]
        [SerializeField] private float rewardDelay = 1f;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            speakerBTN.ForEach(btn => btn.onClick.AddListener(speakerBTN_onClick));
            initFSM();
        }
        protected override void Start()
        {
        }
        protected override void OnEnable()
        {
            base.OnEnable();

            flowerMGR.ForEach(mgr => mgr.OnMonsterFinish += flowerMGR_OnMonsterFinish);
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            flowerMGR.ForEach(mgr => mgr.OnMonsterFinish -= flowerMGR_OnMonsterFinish);
        }

        // Unity Coroutine
        IEnumerator coPlayNarration()
        {
            using (LOG.Coroutine($"coPlayNarration()", this))
            {
                stepPanelCG[0].blocksRaycasts = false;
                cSpeakerANIM.SetBool("Playing", true);
                yield return null;

                yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.PhoneticCLIP);
                yield return null;

                yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.PhoneticCLIP);
                yield return null;

                stepPanelCG[0].blocksRaycasts = true;
                cSpeakerANIM.SetBool("Playing", false);
                yield return null;
            }
        }
    }
}