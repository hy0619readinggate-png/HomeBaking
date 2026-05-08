using beyondi.Coroutine;
using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DoDoEng.Activity.Framework;
using DoDoEng.Activity.UI;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

using ActivityData = DoDoEng.Common.ActivityData_C1_A06;
using ProblemMGR = DoDoEng.Activity.C1_A06.C1_A06_ProblemMGR;

namespace DoDoEng.Activity.C1_A06
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C1_A06_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C1_A06_MySeat;
        private enum State
        {
            Intro,
            ShowRide,
            Problem, Enter, Solve, Correct, Wrong, Next,
            PlayRide, NextRide,
            Outro, Reward
        }

        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();

        // Fields
        private FSM<State> fsm = null;
        private int[] rideIDs;
        private int rideIdx = 0;

        // Fields
        private Seat submitSeat = null;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);

            fsm.AddState(State.Intro,    E_Intro,    X_Intro);
            fsm.AddState(State.ShowRide, E_ShowRide, X_ShowRide);
            fsm.AddState(State.Problem,  E_Problem,  X_Problem);
            fsm.AddState(State.Enter,    E_Enter,    X_Enter);
            fsm.AddState(State.Solve,    E_Solve,    X_Solve);
            fsm.AddState(State.Correct,  E_Correct,  X_Correct);
            fsm.AddState(State.Wrong,    E_Wrong,    X_Wrong);
            fsm.AddState(State.Next,     E_Next);
            fsm.AddState(State.PlayRide, E_PlayRide, X_PlayRide);
            fsm.AddState(State.NextRide, E_NextRide);
            fsm.AddState(State.Outro,    E_Outro,    X_Outro);
            fsm.AddState(State.Reward,   E_Reward);
            #pragma warning restore format  
        }

        // Functions
        private int currentRideID => rideIDs[rideIdx];
        private Ride currentRide => rides[currentRideID - 1];
        private RidePlay currentRidePlay => ridesPlay[currentRideID - 1];
        private bool nextRide()
        {
            rideIdx++;
            return rideIdx < rideIDs.Length;
        }



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            evaluateTimeline(introTL);

            stepPanelCG.SetActiveAll(true);
            stepPanelCG.SetActiveOnly(0);

            UIActivityCommon.One.VisibleSpeakerButton = false;
        }
        protected override async UniTask onPrepare(ActivityIndex actIDX)
        {
            await base.onPrepare(actIDX);

            // Load Unit Data
            await pMGR.Build(actIDX, CURRICULUM, TABLES);
            ActivityProgress.One.Setup(pMGR.BlanksCount);

            // Select Ride
            rideIdx = 0;
            rideIDs = UtilArray.Random(1, 2);
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
                AudioMGR.One.PlayNarration(pMGR.Current.PhoneticCLIP);
            }
        }
        protected override void onDebugNext()
        {
            switch (fsm.CurrentState)
            {
                case State.Intro: fsm.PerformTransition(State.ShowRide); break;
                case State.ShowRide: fsm.PerformTransition(State.Problem); break;
                case State.Enter: fsm.PerformTransition(State.Problem); break;
                case State.Problem: fsm.PerformTransition(State.Solve); break;
                case State.Solve:
                    submitSeat = currentRide.AnswerSeat;
                    fsm.PerformTransition(State.Correct);
                    break;
                case State.Correct: fsm.PerformTransition(State.Next); break;
                case State.Wrong: fsm.PerformTransition(State.Enter); break;
                case State.PlayRide: fsm.PerformTransition(State.NextRide); break;
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

            fsm.PerformTransition(State.ShowRide);
        }
        IEnumerator X_Intro()
        {
            yield return stopTimeline(introTL);
            yield return null;
        }
        IEnumerator E_ShowRide()
        {
            UIActivityCommon.One.VisibleSpeakerButton = true;

            currentRide.Initialize(currentRideID == 2);
            currentRide.Setup(pMGR.Current);

            yield return playTimeline(showRideTL[currentRideID - 1]);
            yield return null;

            fsm.PerformTransition(State.Problem);
        }
        IEnumerator X_ShowRide()
        {
            yield return stopTimeline(showRideTL[currentRideID - 1]);
            yield return null;
        }
        IEnumerator E_Enter()
        {
            currentRide.Setup(pMGR.Current);
            yield return playTimeline(enterTL[currentRideID - 1]);
            yield return null;

            fsm.PerformTransition(State.Problem);
        }
        IEnumerator X_Enter()
        {
            yield return stopTimeline(enterTL[currentRideID - 1]);
            yield return null;
        }
        IEnumerator E_Problem()
        {
            currentRide.ShowBalloons();

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

            UIActivityCommon.One.EnableSpeakerButton = true;

            currentRide.EnableInteraction(true);
            var seats = currentRide.GetExamSeats();
            var wait = new WaitForSubmit(this, seats);
            yield return wait;

            submitSeat = wait.Submited as Seat;
            if (submitSeat.IsAnswer)
                fsm.PerformTransition(State.Correct);
            else fsm.PerformTransition(State.Wrong);
        }
        IEnumerator X_Solve()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            UIActivityCommon.One.EnableSpeakerButton = false;
            AudioMGR.One.StopNarration();

            currentRide.EnableInteraction(false);
        }
        IEnumerator E_Correct()
        {
            AudioMGR.One.PlayEffect(correctCLIP);
            currentRide.HideWrongBalloon();
            currentRide.SitDownCharacter(submitSeat);
            currentRide.CheerUp(submitSeat);
            yield return new WaitForSeconds(correctDuration);

            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.PhoneticCLIP);
            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.PhoneticCLIP);
            //yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.TextCLIP);
            yield return null;

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_Correct()
        {
            submitSeat.Balloon.Hide();
            currentRide.Idle();
            yield return null;

            AudioMGR.One.StopNarration();
            yield return null;
        }
        IEnumerator E_Wrong()
        {
            ActivityProgress.One.Wrong();

            currentRide.HideAllBalloon();
            yield return null;

            yield return submitSeat.StartTryWrongShake(currentRide.CurrentCharacterID);

            DOVirtual.DelayedCall(supriseDelay, () =>
            {
                AudioMGR.One.PlayEffect(surpriseCLIP);
                currentRide.Sad();
            });

            yield return submitSeat.ThrowAway(currentRide.CurrentCharacterID);
            yield return null;

            fsm.PerformTransition(State.Enter);
        }
        IEnumerator X_Wrong()
        {
            submitSeat.FinishTryWrongShake();
            yield return null;
        }
        IEnumerator E_Next()
        {
            yield return null;

            if (pMGR.Next())
            {
                if (pMGR.PNO % 3 != 1) // 3문제를 다 풀지 않았으면
                {
                    currentRide.NextCharacter();
                    fsm.PerformTransition(State.Enter);
                }
                else fsm.PerformTransition(State.PlayRide);
            }
            else fsm.PerformTransition(State.PlayRide);

        }
        IEnumerator E_PlayRide()
        {
            var characterIDs = currentRide.GetCharacterIDs();
            currentRidePlay.Setup(characterIDs);

            yield return playTimeline(playRideTL[currentRideID - 1]);
            yield return null;

            fsm.PerformTransition(State.NextRide);
        }
        IEnumerator X_PlayRide()
        {
            yield return stopTimeline(playRideTL[currentRideID - 1]);
            yield return null;
        }
        IEnumerator E_NextRide()
        {
            yield return null;

            if (nextRide())
                fsm.PerformTransition(State.ShowRide);
            else fsm.PerformTransition(State.Outro);
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
        [Header("★ Bindings - Panels")]
        [SerializeField] private CanvasGroup[] stepPanelCG = null;
        [Header("★ Bindings - S2.Quiz")]
        [SerializeField] private Ride[] rides = null;
        [Header("★ Bindings - S3.Outro")]
        [SerializeField] private RidePlay[] ridesPlay = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip correctCLIP = null;
        [SerializeField] private AudioClip surpriseCLIP = null;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector introTL = null;
        [SerializeField] private PlayableDirector[] enterTL = null;
        [SerializeField] private PlayableDirector[] showRideTL = null;
        [SerializeField] private PlayableDirector[] playRideTL = null;
        [Header("★ Timing")]
        [SerializeField] private float correctDuration = 2f;
        [SerializeField] private float rewardDelay = 1f;
        [SerializeField] private float supriseDelay = 0.5f;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            initFSM();
        }
        protected override void Start()
        {
        }
    }
}