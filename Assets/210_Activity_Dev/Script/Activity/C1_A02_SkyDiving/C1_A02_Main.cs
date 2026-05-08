using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using ActivityData = DoDoEng.Common.ActivityData_C1_A02;
using ProblemMGR = DoDoEng.Activity.C1_A02.C1_A02_ProblemMGR;

namespace DoDoEng.Activity.C1_A02
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C1_A02_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C1_A02_SkyDiving;
        private enum State
        {
            S1Intro,
            S2Solve, S3Landing, Next, S2ToS1,
            S3Outro, Reward
        }

        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();
        private TimelineSignal s1toS2SIG_ = null;
        private TimelineSignal s1toS2SIG => s1toS2SIG_ ??= s1toS2TL.GetComponent<TimelineSignal>();

        // Fields
        private FSM<State> fsm = null;
        private Coroutine crFlagSound = null;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.S1Intro,     E_S1Intro,      X_S1Intro);
            fsm.AddState(State.S2Solve,     E_S2Solve,      X_S2Solve);
            fsm.AddState(State.S3Landing,   E_S2Landding,   X_S2Landing);
            fsm.AddState(State.Next,        E_Next);
            fsm.AddState(State.S2ToS1,      E_S2ToS1,       X_S2ToS1);
            fsm.AddState(State.S3Outro,     E_S3Outro,      X_S3Outro);
            fsm.AddState(State.Reward,      E_Reward);
            #pragma warning restore format  
        }
        private void setupProblem(ProblemData pData)
        {
            // step1
            s1Character.Setup(pData.Text, pData.CharacterID);

            // step2
            s2FlagTXT.text = pData.Text;
            s2Character.Setup(pData.TextAnswer, pData.CharacterID);
            s2ParachuteGroup.Setup(pData.Examples, pData.CharacterID);
            var answerIDX = Array.FindIndex(pData.Examples, e => e.IsAnswer);
            aff.Setup(answerIDX);

            // step3
            s3FlagTXT.text = pData.TextAnswer;
        }

        // Event Handlers
        private void s2Character_OnWrong()
        {
            vfxWrongGO.SetActive(true);
            DOVirtual.DelayedCall(1f, () => vfxWrongGO.SetActive(false));
        }
        private void s1toS2SIG_OnSignal(string signal)
        {
            LOG.Info($"s1toS2SIG_OnSignal() | {signal}", this);

            if (signal == "Activity-ExtraAnimation")
                s2ParachuteGroup.Show();
        }
        private void onS2Flag_OnClick()
        {
            LOG.Info($"onS2Flag_OnClick()", this);

            crFlagSound = StartCoroutine(coFlagSound());

        }



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            // init
            evaluateTimeline(introTL);

            stepPanelCG.SetActiveOnly(0);

            s2FlagBTN.interactable = false;
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
            fsm.StartFSM(State.S1Intro, RunnerParam.SkipStateTo);
        }
        protected override void onFinishActivity()
        {
            base.onFinishActivity();

            if (fsm.CurrentState == State.S2Solve)
                CP(CheckPoint.UserFinish);

            stopBGM();
            AudioMGR.One.StopAmbient(true);
            fsm?.StopFSM();
        }
        protected override void onDebugNext()
        {
            switch (fsm.CurrentState)
            {
                case State.S1Intro:
                    setupProblem(pMGR.Current);
                    fsm.PerformTransition(State.S2Solve);
                    break;
                case State.S2Solve: fsm.PerformTransition(State.S3Landing); break;
                case State.S3Landing: fsm.PerformTransition(State.Next); break;
                case State.S2ToS1:
                    setupProblem(pMGR.Current);
                    fsm.PerformTransition(State.S2Solve);
                    break;
                case State.S3Outro: fsm.PerformTransition(State.Reward); break;
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
            s1Character.ShowInitial();
            yield return null;

            yield return playTimelines(
                new[] { introTL, s1toS2TL },
                (idx) => setupProblem(pMGR.Current));
            yield return null;

            fsm.PerformTransition(State.S2Solve);
        }
        IEnumerator X_S1Intro()
        {
            yield return stopTimelines(introTL, s1toS2TL);
            yield return null;
        }
        IEnumerator E_S2Solve()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            AudioMGR.One.PlayNarration(pMGR.Current.SoundCLIP);
            yield return null;

            this.StopCoroutineSafe(ref crFlagSound);
            yield return null;

            s2ParachuteGroup.EnableInteraction(true);
            s2FlagBTN.interactable = true;
            yield return null;

            yield return s2Character.StartWaitAnswer();
            yield return null;

            fsm.PerformTransition(State.S3Landing);
        }
        IEnumerator X_S2Solve()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            AudioMGR.One.StopNarration();
            yield return null;

            s2ParachuteGroup.EnableInteraction(false);
            s2FlagBTN.interactable = false;
            yield return null;

            s2Character.StopWaitAnswer();
            yield return null;
        }
        IEnumerator E_S2Landding()
        {
            AudioMGR.One.PlayNarration(pMGR.Current.SoundAnswerCLIP);
            AudioMGR.One.PlayEffect(correctCLIP);
            yield return new WaitForSeconds(0.4f);

            vfxCorrectGO.SetActive(true);
            s2ParachuteGroup.Correct();
            s2FlagANIM.SetTrigger("Hide");
            yield return s2Character.Landing();
            yield return null;

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_S2Landing()
        {
            AudioMGR.One.StopNarration();
            vfxCorrectGO.SetActive(false);
            yield return null;
        }
        IEnumerator E_Next()
        {
            yield return null;

            if (pMGR.Next())
                fsm.PerformTransition(State.S2ToS1);
            else fsm.PerformTransition(State.S3Outro);
        }
        IEnumerator E_S2ToS1()
        {
            yield return playTimelines(
                new[] { s2toS1TL, s1toS2TL },
                (idx) => setupProblem(pMGR.Current));
            yield return null;

            fsm.PerformTransition(State.S2Solve);
        }
        IEnumerator X_S2ToS1()
        {
            yield return stopTimelines(s2toS1TL, s1toS2TL);
            yield return null;
        }
        IEnumerator E_S3Outro()
        {
            CP(CheckPoint.Outro);
            yield return null;

            yield return playTimeline(outroTL);
            yield return null;

            fsm.PerformTransition(State.Reward);
        }
        IEnumerator X_S3Outro()
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
        [SerializeField] private S1Character s1Character = null;
        [Header("★ Bindings - S2.Problem")]
        [SerializeField] private S2Character s2Character = null;
        [SerializeField] private ParachuteGroup s2ParachuteGroup = null;
        [SerializeField] private Button s2FlagBTN = null;
        [SerializeField] private TextMeshProUGUI s2FlagTXT = null;
        [SerializeField] private Animator s2FlagANIM = null;
        [SerializeField] private GameObject vfxCorrectGO = null;
        [SerializeField] private GameObject vfxWrongGO = null;
        [SerializeField] private Affordance aff = null;
        [Header("★ Bindings - S3.Outro")]
        [SerializeField] private TextMeshProUGUI s3FlagTXT = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip correctCLIP = null;
        [Header("★ Sound - Ambient")]
        [SerializeField] private AudioClip ambientCLIP = null;
        [Range(0, 100)]
        [SerializeField] int ambientVolume = 100;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector introTL = null;
        [SerializeField] private PlayableDirector s1toS2TL = null;
        [SerializeField] private PlayableDirector s2toS1TL = null;
        [SerializeField] private PlayableDirector outroTL = null;
        [Header("★ Timing")]
        [SerializeField] private float rewardDelay = 1f;


        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            initFSM();

            s2FlagBTN.onClick.AddListener(onS2Flag_OnClick);
        }
        protected override void Start()
        {

        }
        protected override void OnEnable()
        {
            base.OnEnable();

            s2Character.OnWrong += s2Character_OnWrong;
            s1toS2SIG.OnSignal += s1toS2SIG_OnSignal;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            s2Character.OnWrong -= s2Character_OnWrong;
            s1toS2SIG.OnSignal -= s1toS2SIG_OnSignal;
        }


        // 
        IEnumerator coFlagSound()
        {
            using (LOG.Coroutine($"coFlagSound()", this))
            {
                s2ParachuteGroup.EnableInteraction(false);
                s2FlagBTN.interactable = false;

                yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.SoundCLIP);

                s2ParachuteGroup.EnableInteraction(true);
                s2FlagBTN.interactable = true;
            }

        }
    }
}