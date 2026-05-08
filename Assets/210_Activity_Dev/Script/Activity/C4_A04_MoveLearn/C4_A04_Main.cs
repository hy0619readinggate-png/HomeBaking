using beyondi.Coroutine;
using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.C1_A02;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using DoDoEng.Launcher;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

using ActivityData = DoDoEng.Common.ActivityData_C4_A04;
using ProblemMGR = DoDoEng.Activity.C4_A04.C4_A04_ProblemMGR;

namespace DoDoEng.Activity.C4_A04
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C4_A04_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C4_A04_MoveLearn;
        private enum State
        {
            CheckWebCam,
            Intro,
            SpaceshipIn, PlayVideo, PauseVideo,
            Countdown, TakePhoto, CheckToNext, ToDecorate,
            Decorate, Next, SpaceshipOut,
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
            fsm.AddState(State.CheckWebCam,     E_CheckWebCam);
            fsm.AddState(State.Intro,           E_Intro,            X_Intro);
            fsm.AddState(State.SpaceshipIn,     E_SpaceshipIn,      X_SpaceshipIn);
            fsm.AddState(State.PlayVideo,       E_PlayVideo,        X_PlayVideo);
            fsm.AddState(State.PauseVideo,      E_PauseVideo,       X_PauseVideo);
            fsm.AddState(State.Countdown,       E_Countdown,        X_Countdown);
            fsm.AddState(State.TakePhoto,       E_TakePhoto,        X_TakePhoto);
            fsm.AddState(State.CheckToNext,     E_CheckToNext,      X_CheckToNext);
            fsm.AddState(State.ToDecorate,      E_ToDecorate,       X_ToDecorate);
            fsm.AddState(State.Decorate,        E_Decorate,         X_Decorate);
            fsm.AddState(State.Next,            E_Next);
            fsm.AddState(State.SpaceshipOut,    E_SpaceshipOut,     X_SpaceshipOut);
            fsm.AddState(State.Outro,           E_Outro,            X_Outro);
            fsm.AddState(State.Reward,          E_Reward);
            #pragma warning restore format  
        }
        private void setupProblem(ProblemData pData)
        {
            // cTeacher.Setup(pData, cMediaPlayer);
            // cStudent.Setup(pData, cMediaPlayer, webCamScreen);
            decorate.Setup(cStudent);
        }

        // Functions
        private void loadVideo()
        {
            pMGR.Problems.ForEach((i, problem) =>
            {
                //var url = $"https://content.dev.gohidodo.com/media-convert-test/{problem.Movie}/{problem.Movie}.m3u8";
                var url = $"{API.One.MediaHost}/media-convert-test/{problem.Movie}/{problem.Movie}.m3u8";
                // var path = new MediaPath(url, MediaPathType.AbsolutePathOrURL);
                // mediaPlayers[i].OpenMedia(path, false);
            });
        }
        private SpaceshipTeacher cTeacher => teachers[pMGR.PNO - 1];
        private SpaceshipStudent cStudent => students[pMGR.PNO - 1];
        // private MediaPlayer cMediaPlayer => mediaPlayers[pMGR.PNO - 1];

        // Event Handlers
        private void teacherShip_OnPause()
        {
            LOG.Function(this);

            cTeacher.StopPlayVideo();

            fsm.PerformTransition(State.PauseVideo);
        }
        private void teacherShip_OnResume()
        {
            LOG.Function(this);

            fsm.PerformTransition(State.PlayVideo);
        }
        private void teacherShip_OnReplay()
        {
            LOG.Function(this);

            fsm.PerformTransition(State.PlayVideo);
        }
        private void introSIG_OnSignal(string signal)
        {
            LOG.Info($"introSIG_OnSignal() | {signal}", this);

            if (signal == "Activity-ExtraAnimation")
                AudioMGR.One.PlayAmbient(ambientCLIP, ambientVolume);
        }



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            evaluateTimeline(introTL);

            teachers.ForEach(t => t.Init());
            students.ForEach(s => s.Init());
        }
        protected override async UniTask onPrepare(ActivityIndex actIDX)
        {
            await base.onPrepare(actIDX);

            // Load Unit Data
            await pMGR.Build(actIDX, CURRICULUM, TABLES);
            ActivityProgress.One.Setup(pMGR.BlanksCount);

            // setup
            loadVideo();
        }
        protected override void onStartActivity()
        {
            base.onStartActivity();

            playBGM();
            fsm.StartFSM(State.CheckWebCam, RunnerParam.SkipStateTo);
        }
        protected override void onFinishActivity()
        {
            base.onFinishActivity();

            if (fsm.CurrentState == State.Countdown)
                CP(CheckPoint.UserFinish);

            webCamScreen.StopWebCam();
            stopBGM();
            AudioMGR.One.StopAmbient(true);

            fsm?.StopFSM();
        }
        protected override void onDebugNext()
        {
            switch (fsm.CurrentState)
            {
                case State.CheckWebCam: fsm.PerformTransition(State.Intro); break;
                case State.Intro: fsm.PerformTransition(State.SpaceshipIn); break;
                case State.SpaceshipIn: fsm.PerformTransition(State.PlayVideo); break;
                case State.PlayVideo: fsm.PerformTransition(State.Countdown); break;
                case State.Countdown: fsm.PerformTransition(State.TakePhoto); break;
                //case State.TakePhoto: fsm.PerformTransition(State.CheckPhoto); break;
                case State.CheckToNext: fsm.PerformTransition(State.ToDecorate); break;
                case State.ToDecorate: fsm.PerformTransition(State.Decorate); break;
                case State.Decorate: fsm.PerformTransition(State.Next); break;
                case State.SpaceshipOut: fsm.PerformTransition(State.SpaceshipIn); break;
                case State.Outro: fsm.PerformTransition(State.Reward); break;
            }
        }
        protected override void onDebugForceFinish()
        {
            base.onDebugForceFinish();

            fsm.PerformTransition(State.Reward);
        }



        // FSM
        IEnumerator E_CheckWebCam()
        {
            yield return permissionChecker.CheckPermission(Authorization.WebCam);

            if (webCamScreen.Init())
                webCamScreen.PlayWebCam();
            else yield return SystemUI.One.ShowPopupCameraDisconnected().ToCoroutine();

            yield return null;

            fsm.PerformTransition(State.Intro);
        }
        IEnumerator E_Intro()
        {
            CP(CheckPoint.Start);
            yield return playTimeline(introTL, 0);

            fsm.PerformTransition(State.SpaceshipIn);
        }
        IEnumerator X_Intro()
        {
            yield return stopTimeline(introTL);
        }
        IEnumerator E_SpaceshipIn()
        {
            setupProblem(pMGR.Current);
            yield return null;

            var spaceshipInTL = pMGR.PNO == 1 ? spaceship1InTL : spaceship2InTL;
            yield return playTimeline(spaceshipInTL);
            yield return null;

            fsm.PerformTransition(State.PlayVideo);
        }
        IEnumerator X_SpaceshipIn()
        {
            var spaceshipInTL = pMGR.PNO == 1 ? spaceship1InTL : spaceship2InTL;
            yield return stopTimeline(spaceshipInTL);
            yield return null;
        }
        IEnumerator E_PlayVideo()
        {
            cTeacher.EnableInteraction(true);
            cTeacher.EnableTextNarration(false);
            cStudent.ShowWebcam();
            yield return cTeacher.StartPlayVideo();
            yield return new WaitForSeconds(0.5f);

            fsm.PerformTransition(State.Countdown);
        }
        IEnumerator X_PlayVideo()
        {
            cTeacher.StopPlayVideo();
            yield return null;
        }
        IEnumerator E_PauseVideo()
        {
            cTeacher.EnableTextNarration(true);
            yield return cStudent.StartWaitTakePhoto();

            fsm.PerformTransition(State.TakePhoto);
        }
        IEnumerator X_PauseVideo()
        {
            cStudent.StopWaitTakePhoto();
            yield return null;
        }
        IEnumerator E_Countdown()
        {
            cTeacher.EnableTextNarration(true);
            yield return cStudent.StartWaitCountdown();
            yield return null;

            fsm.PerformTransition(State.TakePhoto);
        }
        IEnumerator X_Countdown()
        {
            cStudent.StopWaitCountdown();
            yield return null;
        }
        IEnumerator E_TakePhoto()
        {
            yield return cStudent.StartCapture();
            yield return null;

            fsm.PerformTransition(State.CheckToNext);
        }
        IEnumerator X_TakePhoto()
        {
            cStudent.FinishCapture();
            yield return null;
        }
        IEnumerator E_CheckToNext()
        {
            cStudent.EnableInteractionAtCheck(true);
            yield return null;

            var wait = new WaitForSubmit(this, cStudent.CheckButtons);
            yield return wait;

            var submited = wait.Submited as SpaceshipBTN;
            if (submited == cStudent.UndoBTN)
                fsm.PerformTransition(State.Countdown);
            else if (submited == cStudent.NextBTN)
                fsm.PerformTransition(State.ToDecorate);
        }
        IEnumerator X_CheckToNext()
        {
            cTeacher.EnableTextNarration(false);
            cStudent.EnableInteractionAtCheck(false);
            yield return null;
        }
        IEnumerator E_ToDecorate()
        {
            cTeacher.EnableInteraction(false);
            cStudent.ReadyToDecorate();
            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.WordCLIP);
            yield return null;

            var moveUpTL = pMGR.PNO == 1 ? MoveUp1TL : MoveUp2TL;
            yield return playTimeline(moveUpTL);
            yield return null;

            yield return decorate.StartMenuSetting();
            yield return null;

            fsm.PerformTransition(State.Decorate);
        }
        IEnumerator X_ToDecorate()
        {
            AudioMGR.One.StopNarration();
            yield return null;

            var moveUpTL = pMGR.PNO == 1 ? MoveUp1TL : MoveUp2TL;
            yield return stopTimeline(moveUpTL);
            yield return null;

            decorate.FinishMenuSetting();
            yield return null;
        }
        IEnumerator E_Decorate()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            cTeacher.EnableInteraction(true);
            yield return decorate.StartWaitDecorate();
            yield return null;

            cTeacher.EnableInteraction(false);
            decorate.StartHideMenu();
            vfxCompleteGO.SetActive(true);
            yield return new WaitForSeconds(1);
            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.WordCLIP);

            yield return new WaitForSeconds(completeDelay);

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_Decorate()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            cTeacher.EnableInteraction(false);
            decorate.StopWaitDecorate();
            yield return null;

            decorate.FinishHideMenu();
            vfxCompleteGO.SetActive(false);
            yield return null;
        }
        IEnumerator E_Next()
        {
            yield return null;

            if (pMGR.Next())
                fsm.PerformTransition(State.SpaceshipOut);
            else fsm.PerformTransition(State.Outro);
        }
        IEnumerator E_SpaceshipOut()
        {
            yield return playTimeline(spaceship1OutTL);
            yield return null;

            fsm.PerformTransition(State.SpaceshipIn);
        }
        IEnumerator X_SpaceshipOut()
        {
            yield return stopTimeline(spaceship1OutTL);
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
        [Header("★ Bindings")]
        [SerializeField] private SpaceshipTeacher[] teachers = null;
        [SerializeField] private SpaceshipStudent[] students = null;
        [SerializeField] private Decorate decorate = null;
        [SerializeField] private GameObject vfxCompleteGO = null;
        [SerializeField] private PermissionChecker permissionChecker = null;
        [SerializeField] private WebCamScreen webCamScreen = null;
        [Header("★ Sound - Ambient")]
        [SerializeField] private AudioClip ambientCLIP = null;
        [Range(0, 100)]
        [SerializeField] int ambientVolume = 100;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector introTL = null;
        [SerializeField] private PlayableDirector spaceship1InTL = null;
        [SerializeField] private PlayableDirector MoveUp1TL = null;
        [SerializeField] private PlayableDirector spaceship1OutTL = null;
        [SerializeField] private PlayableDirector spaceship2InTL = null;
        [SerializeField] private PlayableDirector MoveUp2TL = null;
        [SerializeField] private PlayableDirector outroTL = null;
        // [Header("★ Media Player")]
        // [SerializeField] private MediaPlayer[] mediaPlayers = null;
        [Header("★ Timing")]
        [SerializeField] private float completeDelay = 3f;
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

            teachers.ForEach(t => t.OnPause += teacherShip_OnPause);
            teachers.ForEach(t => t.OnResume += teacherShip_OnResume);
            teachers.ForEach(t => t.OnReplay += teacherShip_OnReplay);

            introSIG.OnSignal += introSIG_OnSignal;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            teachers.ForEach(t => t.OnPause -= teacherShip_OnPause);
            teachers.ForEach(t => t.OnResume -= teacherShip_OnResume);
            teachers.ForEach(t => t.OnReplay -= teacherShip_OnReplay);

            introSIG.OnSignal += introSIG_OnSignal;
        }
    }
}
