using beyondi.Coroutine;
using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Activity.UI;
using DoDoEng.Common;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

using ActivityData = DoDoEng.Common.ActivityData_C1_A03;
using ProblemMGR = DoDoEng.Activity.C1_A03.C1_A03_ProblemMGR;

namespace DoDoEng.Activity.C1_A03
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C1_A03_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C1_A03_PopcornCar;
        private enum State
        {
            Intro,
            ProblemIn, Problem,
            Solve, Correct, Wrong,
            RepairAct, Next,
            Outro, Reward
        }

        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();

        // Fields
        private FSM<State> fsm = null;
        private ExamplePart submitedExam = null;
        private Coroutine crPlaySound = null;
        private PlayableDirector feedbackTL = null;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Intro,       E_Intro,        X_Intro);
            fsm.AddState(State.ProblemIn,   E_ProblemIn,    X_ProblemIn);
            fsm.AddState(State.Problem,     E_Problem,      X_Problem);
            fsm.AddState(State.Solve,       E_Solve,        X_Solve);
            fsm.AddState(State.Correct,     E_Correct,      X_Correct);
            fsm.AddState(State.Wrong,       E_Wrong,        X_Wrong);
            fsm.AddState(State.RepairAct,   E_RepairAct,    X_RepairAct);
            fsm.AddState(State.Next,        E_Next);
            fsm.AddState(State.Outro,       E_Outro,        X_Outro);
            fsm.AddState(State.Reward,      E_Reward);
            #pragma warning restore format
        }
        private void setupProblem(ProblemData pData)
        {
            // step2
            s2ExampleParts.ForEach((i, p) => p.Setup(pData.Examples[i], pMGR.PNO));

            var answerIDX = Array.FindIndex(pData.Examples, e => e.IsAnswer);
            s2DragAffordance.Setup(pMGR.PNO, answerIDX);
        }

        // Event Handlers
        protected override void onSpeaker()
        {
            base.onSpeaker();

            if (fsm.CurrentState == State.Solve)
            {
                crPlaySound = StartCoroutine(coPlayWordSound(pMGR.Current.PhoneticCLIP));
            }
        }



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            // init 
            stepPanelCG.SetActiveOnly(0);

            evaluateTimeline(introTL);

            // step2
            s2ExampleParts.ForEach(p => p.Init(s2ExamplePartFloatParentTR, examplePartParam));
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

            if (fsm.CurrentState == State.Problem)
                CP(CheckPoint.UserFinish);

            stopBGM();
            AudioMGR.One.StopAmbient(true);
            fsm?.StopFSM();
        }
        protected override void onDebugNext()
        {
            switch (fsm.CurrentState)
            {
                #pragma warning disable format
                case State.Intro:       fsm.PerformTransition(State.ProblemIn);    break;
                case State.ProblemIn:   fsm.PerformTransition(State.Problem);       break;
                case State.Problem:     fsm.PerformTransition(State.Solve);         break;
                case State.Solve:
                    submitedExam = s2ExampleParts.First(e => e.IsAnswer);
                    fsm.PerformTransition(State.Correct);
                    break;
                case State.Correct:     fsm.PerformTransition(State.RepairAct);     break;
                case State.Wrong:       fsm.PerformTransition(State.Problem);       break;
                case State.RepairAct:   fsm.PerformTransition(State.Next);          break;
                case State.Outro:       fsm.PerformTransition(State.Reward);        break;
                #pragma warning restore format
            }
        }
        protected override void onDebugNextProblem()
        {
            base.onDebugNextProblem();

            switch (fsm.CurrentState)
            {
                case State.Problem:
                case State.Solve:
                case State.Correct:
                case State.Wrong:
                case State.RepairAct:
                    fsm.PerformTransition(State.Next);
                    break;
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

            fsm.PerformTransition(State.ProblemIn);
        }
        IEnumerator X_Intro()
        {
            yield return stopTimeline(introTL);
            yield return null;
        }
        IEnumerator E_ProblemIn()
        {
            setupProblem(pMGR.Current);
            yield return null;

            UIActivityCommon.One.VisibleSpeakerButton = true;
            yield return null;

            yield return playTimeline(problemInTL);
            yield return null;

            fsm.PerformTransition(State.Problem);
        }
        IEnumerator X_ProblemIn()
        {
            yield return stopTimeline(problemInTL);
            yield return null;
        }
        IEnumerator E_Problem()
        {
            crPlaySound = StartCoroutine(coPlayWordSound(pMGR.Current.PhoneticCLIP));
            yield return crPlaySound;

            fsm.PerformTransition(State.Solve);
        }
        IEnumerator X_Problem()
        {
            this.StopCoroutineSafe(ref crPlaySound);
            stepPanelCG[1].blocksRaycasts = true;
            yield return null;

            AudioMGR.One.StopNarration();
            yield return null;
        }
        IEnumerator E_Solve()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            UIActivityCommon.One.EnableSpeakerButton = true;
            yield return null;

            s2ExampleParts.ForEach(e => e.EnableInteraction(true));
            s2DragAffordance.EnableAff = true;
            yield return null;

            var wait = new WaitForSubmit(this, s2ExampleParts);
            yield return wait;

            submitedExam = wait.Submited as ExamplePart;
            if (submitedExam.IsAnswer)
                fsm.PerformTransition(State.Correct);
            else fsm.PerformTransition(State.Wrong);
        }
        IEnumerator X_Solve()
        {
            this.StopCoroutineSafe(ref crPlaySound);
            yield return null;

            UIActivityCommon.One.EnableSpeakerButton = false;
            stepPanelCG[1].blocksRaycasts = true;
            yield return null;

            s2ExampleParts.ForEach(e => e.EnableInteraction(false));
            s2DragAffordance.EnableAff = false;
            yield return null;

            CP(CheckPoint.UserFinish);
            yield return null;

            AudioMGR.One.StopNarration();
            yield return null;
        }
        IEnumerator E_Correct()
        {
            AudioMGR.One.PlayEffect(dragCorrectCLIP);
            s2CorrectVFX.Correct(pMGR.PNO);
            yield return null;

            var target = s2PopcornCar.ClearBroken(pMGR.PNO);
            var scale = exampleCorrectScale[pMGR.PNO - 1];
            submitedExam.MoveTo(target, scale, correctMoveDuration);
            yield return null;

            feedbackTL = UtilArray.ExtractOne(correctTL);
            yield return playTimeline(feedbackTL, 0);

            yield return new WaitForSeconds(dragCorrectDelay);
            yield return null;

            fsm.PerformTransition(State.RepairAct);
        }
        IEnumerator X_Correct()
        {
            s2CorrectVFX.Hide();
            yield return null;

            yield return stopTimeline(feedbackTL);
            yield return null;
        }
        IEnumerator E_Wrong()
        {
            ActivityProgress.One.Wrong();

            yield return playTimeline(wrongTL);

            fsm.PerformTransition(State.Problem);
        }
        IEnumerator X_Wrong()
        {
            yield return stopTimeline(wrongTL);
            yield return null;
        }
        IEnumerator E_RepairAct()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            UIActivityCommon.One.VisibleSpeakerButton = false;
            yield return null;

            yield return repareMGR.StartRepair(pMGR.PNO);
            yield return null;

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_RepairAct()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            // 팝업 닫기 전에 미리 자동차 고쳐놓음
            s2PopcornCar.Setup(pMGR.PNO);
            s2ExampleParts.ForEach(e => e.ReturnToOrizinNow());
            s2Wagon.SetActive(false);
            evaluateTimeline(problemInTL);
            yield return null;

            yield return repareMGR.FinishRepair(pMGR.PNO != 3);
            yield return null;
        }
        IEnumerator E_Next()
        {
            if (pMGR.Next())
                fsm.PerformTransition(State.ProblemIn);
            else fsm.PerformTransition(State.Outro);
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
        [Header("★ Bindings - S1.Intro")]
        [Header("★ Bindings - S2.Repair")]
        [SerializeField] private GameObject s2Wagon = null;
        [SerializeField] private Transform s2ExamplePartFloatParentTR = null;
        [SerializeField] private ExamplePart[] s2ExampleParts = null;
        [SerializeField] private DragAff s2DragAffordance = null;
        [SerializeField] private PopcornCar s2PopcornCar = null;
        [SerializeField] private CorrectVFX s2CorrectVFX = null;
        [SerializeField] private RepairMGR repareMGR = null;
        [Header("★ Bindings - S3.Outro")]
        [Header("★ Sound")]
        [SerializeField] private AudioClip dragCorrectCLIP = null;
        [Header("★ Sound - Ambient")]
        [SerializeField] private AudioClip ambientCLIP = null;
        [Range(0, 100)]
        [SerializeField] int ambientVolume = 100;
        [Header("★ Configs")]
        [SerializeField] private float dragCorrectDelay = 1f;
        [SerializeField] private float correctMoveDuration = 0.7f;
        [SerializeField] private ExamplePartParam examplePartParam = null;
        [SerializeField] private float[] exampleCorrectScale = null;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector introTL = null;
        [SerializeField] private PlayableDirector problemInTL = null;
        [SerializeField] private PlayableDirector[] correctTL = null;
        [SerializeField] private PlayableDirector wrongTL = null;
        [SerializeField] private PlayableDirector outroTL = null;
        [Header("★ Timing")]
        [SerializeField] private float rewardDelay = 1f;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            initFSM();

            // For Test

        }
        protected override void Start()
        {

        }

        // Unity Coroutine
        IEnumerator coPlayWordSound(AudioClip clip)
        {
            using (LOG.Coroutine("coPlayWordSound()", this))
            {
                UIActivityCommon.One.EnableSpeakerButton = false;
                stepPanelCG[1].blocksRaycasts = false;
                yield return null;

                AudioMGR.One.PlayNarration(clip);
                yield return new WaitForSeconds(clip.length);

                AudioMGR.One.PlayNarration(clip);
                yield return new WaitForSeconds(clip.length);

                UIActivityCommon.One.EnableSpeakerButton = true;
                stepPanelCG[1].blocksRaycasts = true;
                yield return null;
            }
        }
    }


    [System.Serializable]
    public class ExamplePartParam
    {
        public float returnJumpPower = 2f;
        public float returnJumpDuration = 0.3f;

        public AudioClip pickupClip = null;
        public AudioClip returnCLIP = null;
    }
}