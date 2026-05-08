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
using UnityEngine.UI;
using ActivityData = DoDoEng.Common.ActivityData_C1_A05;
using ProblemMGR = DoDoEng.Activity.C1_A05.C1_A05_ProblemMGR;

namespace DoDoEng.Activity.C1_A05
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C1_A05_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C1_A05_Weightlifting;
        private enum State
        {
            Intro,
            Problem, Solve, Next,
            Lift, LiftNOut,
            Outro, Reward
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
            fsm.AddState(State.Intro,       E_Intro,        X_Intro);
            fsm.AddState(State.Problem,     E_Problem,      X_Problem);
            fsm.AddState(State.Solve,       E_Solve,        X_Solve);
            fsm.AddState(State.Next,        E_Next);
            fsm.AddState(State.Lift,        E_Lift,         X_Lift);
            fsm.AddState(State.LiftNOut,    E_LiftNOut,     X_LiftNOut);
            fsm.AddState(State.Outro,       E_Outro,        X_Outro);
            fsm.AddState(State.Reward,      E_Reward);
            #pragma warning restore format  
        }
        private void setupProblem(ProblemData pData)
        {
            bar.Setup(pMGR.PNO, leoni);
            currentPlateGroup.Setup(pData.Examples);
            setupAff();
        }

        // Functions
        private PlateGroup currentPlateGroup => plateGroups[(pMGR.PNO - 1) % 2];
        private void setupAff()
        {
            var plateTRs = currentPlateGroup.AnswerPlateTRs.ToArray();
            var clampTRs = bar.AvailableClampTRs.Take(1).ToArray();
            aff.Setup(plateTRs, clampTRs);
        }

        // Event Handlers
        private void speakerBTN_onClick()
        {
            LOG.Info($"speakerBTN_onClick()", this);

            StartCoroutine(coPlayNarration());
        }
        private void bar_OnCorrect()
        {
            LOG.Info($"bar_OnCorrect()", this);

            setupAff();
        }



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            evaluateTimeline(introTL);

            stepPanelCG.SetActiveOnly(0);
            speakerBTN.interactable = false;
            plateGroups.ForEach(g => g.Init());
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
                case State.Intro: fsm.PerformTransition(State.Problem); break;
                case State.Problem: fsm.PerformTransition(State.Solve); break;
                case State.Solve: fsm.PerformTransition(State.Next); break;
                case State.Lift: fsm.PerformTransition(State.Problem); break;
                case State.LiftNOut: fsm.PerformTransition(State.Intro); break;
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

            if (pMGR.PNO != 1)
            {
                evaluateTimeline(introTL);
                dimAnim.SetTrigger("FadeOut");
                yield return new WaitForAnimationToFinish(dimAnim, "FadeOut");

                dimAnim.gameObject.SetActive(false);
                yield return null;
            }

            setupProblem(pMGR.Current);
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
            speakerANIM.SetBool("Playing", true);
            yield return null;

            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.SoundCLIP);
            yield return null;

            speakerANIM.SetBool("Playing", false);
            yield return null;

            fsm.PerformTransition(State.Solve);
        }
        IEnumerator X_Problem()
        {
            speakerANIM.SetBool("Playing", false);
            yield return null;

            AudioMGR.One.StopNarration();
            yield return null;
        }
        IEnumerator E_Solve()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            StartCoroutine(coLeoniIdle());

            speakerBTN.interactable = true;
            currentPlateGroup.EnableInteraction(true);
            yield return null;

            yield return bar.StartWaitPlates(pMGR.PNO);
            yield return null;

            speakerBTN.interactable = false;
            currentPlateGroup.EnableInteraction(false);
            yield return new WaitForSeconds(solveDelay);

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_Solve()
        {
            bar.StopWaitPlates();
            yield return null;

            speakerBTN.interactable = false;
            currentPlateGroup.EnableInteraction(false);
            currentPlateGroup.Hide();

            CP(CheckPoint.UserFinish);
            yield return null;
        }
        IEnumerator E_Next()
        {
            yield return null;
            if (pMGR.Next())
            {
                if (pMGR.Current.IsFirstInSet)
                    fsm.PerformTransition(State.LiftNOut);
                else fsm.PerformTransition(State.Lift);
            }
            else fsm.PerformTransition(State.Outro);
        }
        IEnumerator E_Lift()
        {
            setupProblem(pMGR.Current);

            yield return playTimeline(liftTL);
            yield return null;

            fsm.PerformTransition(State.Problem);
        }
        IEnumerator X_Lift()
        {
            yield return stopTimeline(liftTL);
            yield return null;
        }
        IEnumerator E_LiftNOut()
        {
            yield return playTimeline(toIntroTL);
            yield return null;

            fsm.PerformTransition(State.Intro);
        }
        IEnumerator X_LiftNOut()
        {
            yield return stopTimeline(toIntroTL);
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
        [Header("★ Bindings - Activity")]
        [SerializeField] private Animator dimAnim = null;
        [SerializeField] private Button speakerBTN = null;
        [SerializeField] private Animator speakerANIM = null;
        [SerializeField] private Leoni leoni = null;
        [SerializeField] private PlateGroup[] plateGroups = null;
        [SerializeField] private Bar bar = null;
        [SerializeField] private AffDrag aff = null;
        [Header("★ Sound - Ambient")]
        [SerializeField] private AudioClip ambientCLIP = null;
        [Range(0, 100)]
        [SerializeField] int ambientVolume = 100;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector introTL = null;
        [SerializeField] private PlayableDirector liftTL = null;
        [SerializeField] private PlayableDirector toIntroTL = null;
        [SerializeField] private PlayableDirector outroTL = null;
        [Header("★ Timing")]
        [SerializeField] private float solveDelay = 1.5f;
        [SerializeField] private float rewardDelay = 1f;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            speakerBTN.onClick.AddListener(speakerBTN_onClick);
            initFSM();
        }
        protected override void Start()
        {

        }
        protected override void OnEnable()
        {
            base.OnEnable();

            bar.OnCorrect += bar_OnCorrect;
        }protected override void OnDisable()
        {
            base.OnDisable();

            bar.OnCorrect -= bar_OnCorrect;
        }

        // Unity Coroutine
        IEnumerator coPlayNarration()
        {
            using (LOG.Coroutine($"coPlayNarration()", this))
            {
                stepPanelCG[0].blocksRaycasts = false;
                speakerANIM.SetBool("Playing", true);
                yield return null;

                yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.SoundCLIP);
                yield return null;

                stepPanelCG[0].blocksRaycasts = true;
                speakerANIM.SetBool("Playing", false);
                yield return null;
            }
        }
        IEnumerator coLeoniIdle()
        {
            using (LOG.Coroutine($"coLeoniIdle()", this))
            {
                yield return leoni.PlayAnimationAndWait(LeoniAnimation.Idle1);
                yield return leoni.PlayAnimationAndWait(LeoniAnimation.Idle1);
                leoni.PlayAnimationLoop(LeoniAnimation.Idle2);
            }
        }
    }
}