using beyondi.Coroutine;
using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using ActivityData = DoDoEng.Common.ActivityData_C1_A01;
using ProblemMGR = DoDoEng.Activity.C1_A01.C1_A01_ProblemMGR;

namespace DoDoEng.Activity.C1_A01
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C1_A01_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C1_A01_HomeBaking;
        private enum State
        {
            S1Intro,
            S1Recipe, S1ToS2,
            S2Trace, S2ToS3,
            S3Baking, S3ToS4,
            S4Topping, Next, S4ToS2, S4ToS5,
            S5Eat,
            S5Outro, Reward
        }



        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();

        // Fields
        private FSM<State> fsm = null;
        private Texture2D[] userImages = null;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.S1Intro,     E_S1Intro,      X_S1Intro);
            fsm.AddState(State.S1Recipe,    E_S1Recipe,     X_S1Recipe);
            fsm.AddState(State.S1ToS2,      E_S1ToS2,       X_S1ToS2);
            fsm.AddState(State.S2Trace,     E_S2Trace,      X_S2Trace);
            fsm.AddState(State.S2ToS3,      E_S2ToS3,       X_S2ToS3);
            fsm.AddState(State.S3Baking,    E_S3Baking,     X_S3Baking);
            fsm.AddState(State.S3ToS4,      E_S3ToS4,       X_S3ToS4);
            fsm.AddState(State.S4Topping,   E_S4Topping,    X_S4Topping);
            fsm.AddState(State.Next,        E_Next);
            fsm.AddState(State.S4ToS2,      E_S4ToS2,       X_S4ToS2);
            fsm.AddState(State.S4ToS5,      E_S4ToS5,       X_S4ToS5);
            fsm.AddState(State.S5Eat,       E_S5Eat,        X_S5Eat);
            fsm.AddState(State.S5Outro,     E_S5Outro,      X_S5Outro);
            fsm.AddState(State.Reward,      E_Reward);
            #pragma warning restore format
        }
        private void setupTrace(ProblemData pData)
        {
            LOG.Info($"setupTrace() | {pData}", this);

            // step2
            s2Trace.Setup(pData.TracingPB);
        }
        private void setupProblem(ProblemData pData)
        {
            // step2
            s4Deco.Setup(pData.DecoPB);
        }

        // Event Handlers
        private void s5Eat_OnEat()
        {
            LOG.Info($"s5Eat_OnEat()", this);

            if (s5Affordance.activeSelf)
                s5Affordance.SetActive(false);
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

            s2TraceCompleteGO.gameObject.SetActive(false);
            s4Deco.EnableInteraction(false);
            s4Palette.EnableInteraction(false);
            s5CameraBTN.interactable = false;
        }
        protected override async UniTask onPrepare(ActivityIndex actIDX)
        {
            await base.onPrepare(actIDX);

            // Load Unit Data
            await pMGR.Build(actIDX, CURRICULUM, TABLES);
            ActivityProgress.One.Setup(pMGR.BlanksCount);

            // init
            userImages = new Texture2D[pMGR.Count];
            s1Recipe.Setup(pMGR.Current.Text);
            s4DoneBTN.EnableInteraction(false);
            s5Eat.Setup(pMGR.Problems[0].DecoPB, pMGR.Problems[1].DecoPB);
            s5Eat.EnableInteraction(false);

            // setup
            setupTrace(pMGR.Current);
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

            stopBGM();
            fsm?.StopFSM();
        }
        protected override void onDebugNext()
        {
            switch (fsm.CurrentState)
            {
                #pragma warning disable format
                case State.S1Intro:     fsm.PerformTransition(State.S1Recipe);      break;
                case State.S1Recipe:    fsm.PerformTransition(State.S1ToS2);        break;
                case State.S1ToS2:      fsm.PerformTransition(State.S2Trace);       break;
                case State.S2Trace:     fsm.PerformTransition(State.S2ToS3);        break;
                case State.S2ToS3:      fsm.PerformTransition(State.S3Baking);      break;
                case State.S3Baking:    fsm.PerformTransition(State.S3ToS4);        break;
                case State.S3ToS4:      fsm.PerformTransition(State.S4Topping);     break;
                case State.S4Topping:   fsm.PerformTransition(State.Next);          break;
                case State.S4ToS2:      fsm.PerformTransition(State.S2Trace);       break;
                case State.S4ToS5:      fsm.PerformTransition(State.S5Eat);         break;
                case State.S5Eat:       fsm.PerformTransition(State.S5Outro);       break;
                #pragma warning restore format
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

            fsm.PerformTransition(State.S1Recipe);
        }
        IEnumerator X_S1Intro()
        {
            yield return stopTimeline(introTL);
            yield return null;
        }
        IEnumerator E_S1Recipe()
        {
            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.Text2CLIP);
            yield return null;

            yield return new WaitForSeconds(0.5f);
            yield return null;

            fsm.PerformTransition(State.S1ToS2);
        }
        IEnumerator X_S1Recipe()
        {
            AudioMGR.One.StopNarration();
            yield return null;
        }
        IEnumerator E_S1ToS2()
        {
            s2PlateFace.Normal();
            yield return null;

            yield return playTimeline(s1toS2TL);
            yield return null;

            fsm.PerformTransition(State.S2Trace);
        }
        IEnumerator X_S1ToS2()
        {
            yield return stopTimeline(s1toS2TL);
            yield return null;
        }
        IEnumerator E_S2Trace()
        {
            CP(CheckPoint.UserStart);

            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.Text1CLIP);
            yield return null;

            s2Trace.StartTrace();
            yield return new WaitForComplete(this, s2Trace);
            yield return new WaitForSeconds(0.3f);

            s2PlateFace.Smile();
            s2TraceCompleteGO.SetActive(true);
            AudioMGR.One.PlayNarration(pMGR.Current.Text1CLIP);
            yield return new WaitForSeconds(2f);

            fsm.PerformTransition(State.S2ToS3);
        }
        IEnumerator X_S2Trace()
        {
            CP(CheckPoint.UserFinish);

            s2PlateFace.Smile();
            s2TraceCompleteGO.SetActive(false);
            AudioMGR.One.StopNarration();
            yield return null;

            s2Trace.FinishTrace();
            yield return null;
        }
        IEnumerator E_S2ToS3()
        {
            s3BakeMGR.Idle();
            yield return playTimeline(s2toS3TL);
            yield return null;

            fsm.PerformTransition(State.S3Baking);
        }
        IEnumerator X_S2ToS3()
        {
            yield return stopTimeline(s2toS3TL);
            yield return null;
        }
        IEnumerator E_S3Baking()
        {
            yield return s3BakeMGR.StartBake();
            yield return null;

            fsm.PerformTransition(State.S3ToS4);
        }
        IEnumerator X_S3Baking()
        {
            s3BakeMGR.FinishBake();
            yield return null;
        }
        IEnumerator E_S3ToS4()
        {
            setupProblem(pMGR.Current);

            s4Palette.SelectDefault();
            yield return null;

            yield return playTimeline(s3toS4TL);
            yield return null;

            fsm.PerformTransition(State.S4Topping);
        }
        IEnumerator X_S3ToS4()
        {
            yield return stopTimeline(s3toS4TL);
            yield return null;
        }
        IEnumerator E_S4Topping()
        {
            CP(CheckPoint.UserStart);

            AudioMGR.One.PlayNarration(pMGR.Current.Text1CLIP);
            yield return null;

            s4DoneBTN.EnableInteraction(true);
            s4Palette.EnableInteraction(true);
            yield return null;

            s4Deco.StartDeco();

            var wait = new WaitForSubmit(this, s4DoneBTN);
            yield return wait;

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_S4Topping()
        {
            CP(CheckPoint.UserFinish);

            AudioMGR.One.StopNarration();
            AudioMGR.One.PlayEffectLL(s2DoneCLIP);
            yield return null;

            s4DoneBTN.EnableInteraction(false);
            s4Palette.EnableInteraction(false);
            yield return null;

            s4Deco.FinishDeco();
            yield return null;

            userImages[pMGR.PNO - 1] = s4Deco.UserImage;
            yield return null;

            yield return new WaitForSeconds(0.5f);
            yield return null;
        }
        IEnumerator E_Next()
        {
            if (pMGR.Next())
                fsm.PerformTransition(State.S4ToS2);
            else fsm.PerformTransition(State.S4ToS5);
            yield return null;
        }
        IEnumerator E_S4ToS2()
        {
            setupTrace(pMGR.Current);

            s2PlateFace.Normal();
            yield return null;

            yield return playTimeline(s4toS2TL);
            yield return null;

            fsm.PerformTransition(State.S2Trace);
        }
        IEnumerator X_S4ToS2()
        {
            yield return stopTimeline(s4toS2TL);
            yield return null;
        }
        IEnumerator E_S4ToS5()
        {
            CP(CheckPoint.Outro);
            yield return null;

            s5DecoUpperIMG.texture = userImages[0];
            s5DecoLowerIMG.texture = userImages[1];
            yield return null;

            yield return playTimeline(s4toS5TL);
            yield return null;

            fsm.PerformTransition(State.S5Eat);
        }
        IEnumerator X_S4ToS5()
        {
            yield return stopTimeline(s4toS5TL);
            yield return null;
        }
        IEnumerator E_S5Eat()
        {
            CP(CheckPoint.UserStart);

            AudioMGR.One.PlayNarration(pMGR.Current.Text2CLIP);
            yield return null;

            s5Affordance.SetActive(true);
            s5Eat.EnableInteraction(true);
            s5CameraBTN.interactable = true;
            yield return null;

            yield return new WaitForComplete(this, s5Eat);
            yield return null;

            fsm.PerformTransition(State.S5Outro);
        }
        IEnumerator X_S5Eat()
        {
            CP(CheckPoint.UserFinish);

            AudioMGR.One.StopNarration();
            yield return null;

            s5Affordance.SetActive(false);
            s5Eat.EnableInteraction(false);
            s5CameraBTN.interactable = false;
            yield return null;

            s5Eat.EatAll();
            yield return null;
        }
        IEnumerator E_S5Outro()
        {
            yield return playTimeline(outroTL);
            yield return null;

            fsm.PerformTransition(State.Reward);
        }
        IEnumerator X_S5Outro()
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
        [SerializeField] private Recipe s1Recipe = null;
        [Header("★ Bindings - S2.Trace")]
        [SerializeField] private Trace s2Trace = null;
        [SerializeField] private PlateFace s2PlateFace = null;
        [SerializeField] private GameObject s2TraceCompleteGO = null;
        [Header("★ Bindings - S3.Baking")]
        [SerializeField] private BakeMGR s3BakeMGR = null;
        [Header("★ Bindings - S4.Topping")]
        [SerializeField] private Deco s4Deco = null;
        [SerializeField] private Palette s4Palette = null;
        [SerializeField] private SubmitButton s4DoneBTN = null;
        [Header("★ Bindings - S5.Outro")]
        [SerializeField] private Eat s5Eat = null;
        [SerializeField] private RawImage s5DecoUpperIMG = null;
        [SerializeField] private RawImage s5DecoLowerIMG = null;
        [SerializeField] private GameObject s5Affordance = null;
        [SerializeField] private Button s5CameraBTN = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip s2DoneCLIP = null;
        [SerializeField] private AudioClip s5CameraCLIP = null;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector introTL = null;
        [SerializeField] private PlayableDirector s1toS2TL = null;
        [SerializeField] private PlayableDirector s2toS3TL = null;
        [SerializeField] private PlayableDirector s3toS4TL = null;
        [SerializeField] private PlayableDirector s4toS5TL = null;
        [SerializeField] private PlayableDirector s4toS2TL = null;
        [SerializeField] private PlayableDirector outroTL = null;
        [Header("★ Timing")]
        [SerializeField] private float rewardDelay = 1f;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            initFSM();

            s5CameraBTN.onClick.AddListener(() => AudioMGR.One.PlayEffectLL(s5CameraCLIP));
        }
        protected override void Start()
        {

        }
        protected override void OnEnable()
        {
            base.OnEnable();

            s5Eat.OnEat += s5Eat_OnEat;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            s5Eat.OnEat -= s5Eat_OnEat;
        }
    }
}