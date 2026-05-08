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

using ActivityData = DoDoEng.Common.ActivityData_C3_A01;
using ProblemMGR = DoDoEng.Activity.C2_A01.C3_A01_ProblemMGR;

// Variation : C2_A01_BeveridgeTruck, C3_A01_HalloweenTruck
namespace DoDoEng.Activity.C2_A01
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C3_A01_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C3_A01_HollowinTruck;
        private enum State
        {
            Permission, 
            Intro, CustomerIn, Problem,
            Wait, Record, MyVoice, Correct, Wrong, Next,
            ChangeProblem,
            Outro, Reward
        }
        private const int CustomerCount = 4;

        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();
        private TimelineSignal problemSIG_ = null;
        private TimelineSignal problemSIG => problemSIG_ ??= changeProblemTL.GetComponent<TimelineSignal>();
        private VoiceRecognizer recognizer_ = null;
        private VoiceRecognizer recognizer => recognizer_ ??= gameObject.AddComponent<VoiceRecognizer>();

        // Fields
        private FSM<State> fsm = null;
        private int[] customerIDs = null;
        private Coroutine crPlayWord = null;
        private int recordTryCount = 0;
        private PlayableDirector currentTL = null;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Permission,      E_Permission);
            fsm.AddState(State.Intro,           E_Intro,            X_Intro);
            fsm.AddState(State.CustomerIn,      E_CustomerIn,       X_CustomerIn);
            fsm.AddState(State.Problem,         E_Problem,          X_Problem);
            fsm.AddState(State.ChangeProblem,   E_ChangeProblem,    X_ChangeProblem);
            fsm.AddState(State.Wait,            E_Wait,             X_Wait);
            fsm.AddState(State.Record,          E_Record,           X_Record);
            fsm.AddState(State.MyVoice,         E_MyVoice,          X_MyVoice);
            fsm.AddState(State.Correct,         E_Correct,          X_Correct);
            fsm.AddState(State.Wrong,           E_Wrong,            X_Wrong);
            fsm.AddState(State.Next,            E_Next);
            fsm.AddState(State.Outro,           E_Outro,            X_Outro);
            fsm.AddState(State.Reward,          E_Reward);

            #pragma warning restore format  
        }
        private void setupProblem(ProblemData pData)
        {
            problemSign.Setup(pData);
        }
        private int activeCustomerID => customerIDs[pMGR.PNO - 1];

        // Functions
        private int[] allCustomerInTrackIndices => customerInTrackIndices.Cast<int>().ToArray();
        private int[] getCustomerInTrackIndices(int customerIdx) => Enumerable.Range(0, customerInTrackIndices.GetLength(1))
                                                                                .Select(i => customerInTrackIndices[customerIdx, i])
                                                                                .ToArray();
        private int[] allChangeProblemTrackIndices => changeProblemTrackIndices.Cast<int>().ToArray();
        private int[] getChangeProblemTrackIndices(int customerIdx) => Enumerable.Range(0, changeProblemTrackIndices.GetLength(1))
                                                                                .Select(i => changeProblemTrackIndices[customerIdx, i])
                                                                                .ToArray();
        private PlayableDirector getPickupTL()
        {
            return activeCustomerID switch
            {
                0 => UtilArray.ExtractOne(pickup1TL),
                1 => UtilArray.ExtractOne(pickup2TL),
                2 => UtilArray.ExtractOne(pickup3TL),
                3 => UtilArray.ExtractOne(pickup4TL),
                _ => UtilArray.ExtractOne(pickup1TL)
            };
        }
        private PlayableDirector getWrongAndOutTL()
        {
            return activeCustomerID switch
            {
                0 => UtilArray.ExtractOne(wrongAndOut1TL),
                1 => UtilArray.ExtractOne(wrongAndOut2TL),
                2 => UtilArray.ExtractOne(wrongAndOut3TL),
                3 => UtilArray.ExtractOne(wrongAndOut4TL),
                _ => UtilArray.ExtractOne(wrongAndOut1TL)
            };
        }

        // Functions
        private void unmuteActiveCustomer(int customerIdx)
        {
            customerInTL.MuteTrack(allCustomerInTrackIndices);
            customerInTL.UnmuteTrack(getCustomerInTrackIndices(customerIdx));
            changeProblemTL.MuteTrack(allChangeProblemTrackIndices);
            changeProblemTL.UnmuteTrack(getChangeProblemTrackIndices(customerIdx));
        }
        private void muteAllCustomers()
        {
            customerInTL.MuteTrack(allCustomerInTrackIndices);
            changeProblemTL.MuteTrack(allChangeProblemTrackIndices);
        }
        
        // Event Handlers
        private void problemSign_OnClick()
        {
            LOG.Info($"problemSign_OnClick()", this);

            crPlayWord = StartCoroutine(coPlayWord());
        }
        private void problemSIG_OnSignal(string signal)
        {
            LOG.Info($"problemSIG_OnSignal() | {signal}", this);

            if (signal == "Activity-SetupProblem")
                setupProblem(pMGR.Current);
        }



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            evaluateTimeline(introTL);

            stepPanelCG.SetActiveOnly(0);
            aff.EnableAff = false;
        }
        protected override async UniTask onPrepare(ActivityIndex actIDX)
        {
            await base.onPrepare(actIDX);

            // Load Unit Data
            await pMGR.Build(actIDX, CURRICULUM, TABLES);
            ActivityProgress.One.Setup(pMGR.BlanksCount);

            // setup
            customerIDs = UtilArray.Random(0, CustomerCount - 1, pMGR.Count);
            setupProblem(pMGR.Current);
            muteAllCustomers();
        }
        protected override void onStartActivity()
        {
            base.onStartActivity();

            playBGM();
            AudioMGR.One.PlayAmbient(ambientCLIP, ambientVolume);

            fsm.StartFSM(State.Permission, RunnerParam.SkipStateTo);
        }
        protected override void onFinishActivity()
        {
            base.onFinishActivity();

            if (fsm.CurrentState == State.Record)
                CP(CheckPoint.UserFinish);

            stopBGM();
            AudioMGR.One.StopAmbient(true);

            fsm?.StopFSM();
        }
        protected override void onDebugNext()
        {
            switch (fsm.CurrentState)
            {
                case State.Intro: fsm.PerformTransition(State.CustomerIn); break;
                case State.CustomerIn: fsm.PerformTransition(State.Problem); break;
                case State.Problem: fsm.PerformTransition(State.Wait); break;
                case State.Wait: fsm.PerformTransition(State.Record); break;
                case State.Record: fsm.PerformTransition(State.Correct); break;
                case State.Correct: fsm.PerformTransition(State.Next); break;
                case State.Wrong: fsm.PerformTransition(State.Problem); break;
                case State.ChangeProblem: fsm.PerformTransition(State.Problem); break;
                case State.Outro: fsm.PerformTransition(State.Reward); break;
            }
        }
        protected override void onDebugForceFinish()
        {
            base.onDebugForceFinish();

            fsm.PerformTransition(State.Reward);
        }



        // FSM
        IEnumerator E_Permission()
        {
            var micPermitted = false;
            yield return permissionChecker.CheckMicrophonePermissionForced().ToCoroutine(ok => micPermitted = ok);

            if (!micPermitted)
                error();
            else fsm.PerformTransition(State.Intro);
        }
        IEnumerator E_Intro()
        {
            CP(CheckPoint.Start);

            customerMGR.Setup(activeCustomerID);
            unmuteActiveCustomer(activeCustomerID);
            yield return null;

            yield return playTimeline(introTL);
            yield return null;

            fsm.PerformTransition(State.CustomerIn);
        }
        IEnumerator X_Intro()
        {
            yield return stopTimeline(introTL);
            yield return null;
        }
        IEnumerator E_CustomerIn()
        {
            recordTryCount = 0;
            yield return playTimeline(customerInTL);
            yield return null;

            fsm.PerformTransition(State.Problem);
        }
        IEnumerator X_CustomerIn()
        {
            yield return stopTimeline(customerInTL);
            yield return null;

            muteAllCustomers();
            yield return null;
        }
        IEnumerator E_Problem()
        {
            blanc.Idle();
            customerMGR.Idle();
            yield return null;

            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.WordCLIP);
            yield return null;

            fsm.PerformTransition(State.Wait);
        }
        IEnumerator X_Problem()
        {
            AudioMGR.One.StopNarration();
            yield return null;
        }
        IEnumerator E_Wait()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            aff.EnableAff = true;
            recordButton.Show();
            yield return null;

            problemSign.EnableInteraction(true);
            recordButton.EnableInteraction(true);
            yield return null;

            var wait = new WaitForSubmit(this, recordButton);
            yield return wait;

            fsm.PerformTransition(State.Record);
        }
        IEnumerator X_Wait()
        {
            aff.EnableAff = false;
            AudioMGR.One.StopNarration();
            yield return null;

            problemSign.EnableInteraction(false);
            recordButton.EnableInteraction(false);
            yield return null;

            this.StopCoroutineSafe(ref crPlayWord);
            yield return null;
        }
        IEnumerator E_Record()
        {
            var duration = (int)Math.Ceiling(pMGR.Current.WordCLIP.length + recordDelay);
            recordButton.Recording();

            yield return recognizer.StartRecognize(pMGR.Current.WordCLIP, pMGR.Current.WordSTR, duration);

            if (recognizer.IsSuccess(recognizeCutOffResultScore))
                fsm.PerformTransition(State.MyVoice);
            else fsm.PerformTransition(State.Wrong);
        }
        IEnumerator X_Record()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            recognizer.StopRecognize();
            recordButton.StopAndHide();
            yield return null;
        }
        IEnumerator E_MyVoice()
        {
            yield return AudioMGR.One.PlayMyVocieAndWait(recognizer.RecordedClip);
            yield return null;

            fsm.PerformTransition(State.Correct);
        }
        IEnumerator X_MyVoice()
        {
            AudioMGR.One.StopNarration();
            yield return null;
        }
        IEnumerator E_Correct()
        {
            currentTL = getPickupTL();
            yield return playTimeline(currentTL);

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_Correct()
        {
            if (currentTL != null)
            {
                var tl = currentTL;
                currentTL = null;
                yield return stopTimeline(tl);
            }
        }
        IEnumerator E_Wrong()
        {
            ActivityProgress.One.Wrong();

            if (++recordTryCount >= tryCountForForceNext)
            {
                currentTL = getWrongAndOutTL();
                yield return playTimeline(currentTL);

                fsm.PerformTransition(State.Next);
            }
            else
            {
                blanc.Wrong();
                customerMGR.Wrong();
                yield return new WaitForSeconds(wrongDelay);

                fsm.PerformTransition(State.Problem);
            }
        }
        IEnumerator X_Wrong()
        {
            blanc.Idle();
            customerMGR.Idle();
            yield return null;

            if (currentTL != null)
            {
                var tl = currentTL;
                currentTL = null;
                yield return stopTimeline(tl);
            }
        }
        IEnumerator E_Next()
        {
            yield return null;

            if (pMGR.Next())
                fsm.PerformTransition(State.ChangeProblem);
            else fsm.PerformTransition(State.Outro);
        }
        IEnumerator E_ChangeProblem()
        {
            recordTryCount = 0;
            customerMGR.Setup(activeCustomerID);
            unmuteActiveCustomer(activeCustomerID);

            yield return playTimeline(changeProblemTL);
            yield return null;

            fsm.PerformTransition(State.Problem);
        }
        IEnumerator X_ChangeProblem()
        {
            yield return stopTimeline(changeProblemTL);
            yield return null;

            muteAllCustomers();
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
        [SerializeField] private ProblemSign problemSign = null;
        [SerializeField] private Character blanc = null;
        [SerializeField] private CustomerMGR customerMGR = null;
        [SerializeField] private RecordButton recordButton = null;
        [SerializeField] private PermissionChecker permissionChecker = null;
        [SerializeField] private AffGameObject aff = null;
        [Header("★ Sound - Ambient")]
        [SerializeField] private AudioClip ambientCLIP = null;
        [Range(0, 100)]
        [SerializeField] int ambientVolume = 100;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector introTL = null;
        [SerializeField] private PlayableDirector customerInTL = null;
        [SerializeField] private PlayableDirector changeProblemTL = null;
        [SerializeField] private PlayableDirector[] pickup1TL = null;
        [SerializeField] private PlayableDirector[] pickup2TL = null;
        [SerializeField] private PlayableDirector[] pickup3TL = null;
        [SerializeField] private PlayableDirector[] pickup4TL = null;
        [SerializeField] private PlayableDirector[] wrongAndOut1TL = null;
        [SerializeField] private PlayableDirector[] wrongAndOut2TL = null;
        [SerializeField] private PlayableDirector[] wrongAndOut3TL = null;
        [SerializeField] private PlayableDirector[] wrongAndOut4TL = null;
        [SerializeField] private PlayableDirector outroTL = null;
        [Header("★ Timing")]
        [SerializeField] private float wrongDelay = 2f;
        [SerializeField] private float recordDelay = 0f;
        [SerializeField] private float rewardDelay = 1f;
        [Header("★ Config")]
        [SerializeField] private int[,] customerInTrackIndices = { { 7, 8, 9 }, { 10, 11, 12 }, { 13, 14, 15 }, { 16, 17, 18 } };
        [SerializeField] private int[,] changeProblemTrackIndices = { { 8, 9, 10 }, { 11, 12, 13 }, { 14, 15, 16 }, { 17, 18, 19 } };
        [SerializeField] private int recognizeCutOffResultScore = 50;
        [SerializeField] private int tryCountForForceNext = 3;

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

            problemSign.OnClick += problemSign_OnClick;
            problemSIG.OnSignal += problemSIG_OnSignal;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            problemSign.OnClick -= problemSign_OnClick;
            problemSIG.OnSignal -= problemSIG_OnSignal;
        }

        // Unity Coroutine
        IEnumerator coPlayWord()
        {
            using (LOG.Coroutine($"coPlayNarration()", this))
            {
                problemSign.EnableInteraction(false);
                yield return null;

                yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.WordCLIP);

                problemSign.EnableInteraction(true);
                yield return null;
            }
        }
    }
}