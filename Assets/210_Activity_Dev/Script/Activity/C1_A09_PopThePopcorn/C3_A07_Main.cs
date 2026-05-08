using beyondi.Coroutine;
using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Activity.UI;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

using ActivityData = DoDoEng.Common.ActivityData_C3_A07;
using ProblemMGR = DoDoEng.Activity.C1_A09.C3_A07_ProblemMGR;

namespace DoDoEng.Activity.C1_A09
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C3_A07_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C3_A07_PopThePopcorn2;
        private enum State
        {
            S1Intro,
            S1User, S1LoadCorn, S1CloseCap, S1ToS2,
            S2Setup, S2Problem, S2User, S2Next, S2Complete,
            Next, S2ToS1,
            S3Outro, Reward
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
            fsm.AddState(State.S1Intro,         E_S1Intro,          X_S1Intro);
            fsm.AddState(State.S1User,          E_S1User,           X_S1User);
            fsm.AddState(State.S1LoadCorn,      E_S1LoadCorn,       X_S1Loadcorn);
            fsm.AddState(State.S1CloseCap,      E_S1Close);
            fsm.AddState(State.S1ToS2,          E_S1ToS2,           X_S1ToS2);
            fsm.AddState(State.S2Setup,         E_S2Setup);
            fsm.AddState(State.S2Problem,       E_S2Problem,        X_S2Problem);
            fsm.AddState(State.S2User,          E_S2User,           X_S2User);
            fsm.AddState(State.S2Next,          E_S2Next);
            fsm.AddState(State.S2Complete,      E_S2Complete,       X_S2Complete);
            fsm.AddState(State.Next,            E_Next);
            fsm.AddState(State.S2ToS1,          E_S2ToS1,           X_S2ToS1);
            fsm.AddState(State.S3Outro,         E_S3Outro,          X_S3Outro);
            fsm.AddState(State.Reward,          E_Reward);
            #pragma warning restore format  
        }



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            // init
            stepPanelCG.SetActiveOnly(0);

            // step1
            introMovingGO.SetActive(false);
            s1BagAffGO.SetActive(false);
            s1CapAffGO.SetActive(false);

            // step2
            progress.SetProgress(0);
            characters.SwitchTo(1);
            answerPS.gameObject.SetActive(false);
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

            if (fsm.CurrentState == State.S2User)
                CP(CheckPoint.UserFinish);

            stopBGM();
            AudioMGR.One.StopAmbient(true);
            fsm?.StopFSM();
        }
        protected override void onDebugNext()
        {
            switch (fsm.CurrentState)
            {
                case State.S1Intro: fsm.PerformTransition(State.S1User); break;
                case State.S1User:
                    if (s1Corns.IsEmpty)
                        fsm.PerformTransition(State.S1LoadCorn);
                    else fsm.PerformTransition(State.S1CloseCap);
                    break;
                //case State.S1LoadCorn: fsm.PerformTransition(State.S1User); break;
                case State.S1ToS2: fsm.PerformTransition(State.S2Setup); break;
                case State.S2Problem: fsm.PerformTransition(State.S2User); break;
                case State.S2User: fsm.PerformTransition(State.S2Complete); break;
                case State.S2ToS1: fsm.PerformTransition(State.S1User); break;
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
            introMovingGO.SetActive(true);
            yield return playTimeline(introTL);
            yield return null;

            fsm.PerformTransition(State.S1User);
        }
        IEnumerator X_S1Intro()
        {
            yield return stopTimeline(introTL);
            yield return null;
        }
        IEnumerator E_S1User()
        {
            s1BagAffGO.SetActive(s1Corns.IsEmpty);
            s1CapAffGO.SetActive(!s1Corns.IsEmpty);
            s1Bag.EnableInteraction(!s1Corns.IsFull);
            s1Cap.EnableInteraction(!s1Corns.IsEmpty);
            var wait = new WaitForSubmit(this, s1Bag, s1Cap);
            yield return wait;

            var submit = wait.Submited as SubmitButton;
            if (submit == s1Bag)
                fsm.PerformTransition(State.S1LoadCorn);
            else fsm.PerformTransition(State.S1CloseCap);
        }
        IEnumerator X_S1User()
        {
            AudioMGR.One.StopEffectLL();

            s1BagAffGO.SetActive(false);
            s1CapAffGO.SetActive(false);
            s1Bag.EnableInteraction(false);
            s1Cap.EnableInteraction(false);
            yield return null;
        }
        IEnumerator E_S1LoadCorn()
        {
            if (s1Corns.IsEmpty)
                s1CapAni.PlayAnimation(MachineCapAnimation.Open);
            s1ScoopGO.SetActive(true);
            s1BagANIM.SetTrigger("Click");
            yield return new WaitForSeconds(scoopAniDuration - 0.4f);

            s1Corns.AddCorn();
            yield return new WaitForSeconds(0.4f);
            yield return new WaitForSeconds(loadCornDelay);

            fsm.PerformTransition(State.S1User);
        }
        IEnumerator X_S1Loadcorn()
        {
            s1ScoopGO.gameObject.SetActive(false);
            yield return null;
        }
        IEnumerator E_S1Close()
        {
            s1CapAni.PlayAnimation(MachineCapAnimation.Close);
            yield return new WaitForSeconds(1);

            fsm.PerformTransition(State.S1ToS2);
        }
        IEnumerator E_S1ToS2()
        {
            wordBox.Setup(pMGR.Current);
            wordBox.ShowLocation();
            characters.SwitchTo(pMGR.PNO);

            var tl = s1ToS2TL[pMGR.PNO - 1];
            yield return playTimeline(tl);
            yield return null;

            fsm.PerformTransition(State.S2Setup);
        }
        IEnumerator X_S1ToS2()
        {
            var tl = s1ToS2TL[pMGR.PNO - 1];
            yield return stopTimeline(tl);
            yield return null;
        }
        IEnumerator E_S2Setup()
        {
            var loadedCount = s1Corns.LoadedCorns;
            var extraCount = pMGR.Config.GetExtraPopcornCount(loadedCount);
            popcornMGR.Setup(pMGR.Current, extraCount, characters.Current);
            s1Corns.Empty();
            yield return null;

            fsm.PerformTransition(State.S2Problem);
        }
        IEnumerator E_S2Problem()
        {
            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.WordCLIP);
            yield return new WaitForSeconds(0.5f);

            fsm.PerformTransition(State.S2User);
        }
        IEnumerator X_S2Problem()
        {
            AudioMGR.One.StopNarration();
            yield return null;
        }
        IEnumerator E_S2User()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            AudioMGR.One.PlayEffect(questionCLIP);
            yield return popcornMGR.StartPop();
            yield return null;

            fsm.PerformTransition(State.S2Next);
        }
        IEnumerator X_S2User()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            popcornMGR.StopPop();
            yield return null;
        }
        IEnumerator E_S2Next()
        {
            yield return null;

            if (popcornMGR.IsAllCollected)
                fsm.PerformTransition(State.S2Complete);
            else fsm.PerformTransition(State.S2Problem);
        }
        IEnumerator E_S2Complete()
        {
            progress.SetProgress(pMGR.PNO);
            yield return null;

            characters.ShowCompleteVfx(true);
            characters.PlayCompleteSFX();
            yield return new WaitForSeconds(0.5f);
            
            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.WordCLIP);
            yield return new WaitForSeconds(1);

            characters.ShowCompleteVfx(false);
            yield return null;

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_S2Complete()
        {
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
            var tl = s2ToS1TL[pMGR.PNO - 2];
            yield return playTimeline(tl);
            yield return null;

            fsm.PerformTransition(State.S1User);
        }
        IEnumerator X_S2ToS1()
        {
            var tl = s2ToS1TL[pMGR.PNO - 2];
            yield return stopTimeline(tl);
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
        [SerializeField] private GameObject introMovingGO = null;
        [SerializeField] private SubmitButton s1Bag = null;
        [SerializeField] private SubmitButton s1Cap = null;
        [SerializeField] private Animator s1BagANIM = null;
        [SerializeField] private GameObject s1BagAffGO = null;
        [SerializeField] private GameObject s1CapAffGO = null;
        [SerializeField] private GameObject s1ScoopGO = null;
        [SerializeField] private MachineCap s1CapAni = null;
        [SerializeField] private MachineCorn s1Corns = null;
        [Header("★ Bindings - S2.Collecting")]
        [SerializeField] private PopcornMGR popcornMGR = null;
        [SerializeField] private CharacterGroup characters = null;
        [SerializeField] private ParticleSystem answerPS = null;
        [SerializeField] private WordBox wordBox = null;
        [SerializeField] private Progress progress = null;
        [Header("★ Bindings - S3.Outro")]
        [Header("★ Sound")]
        [SerializeField] private AudioClip questionCLIP = null;
        [Header("★ Sound - Ambient")]
        [SerializeField] private AudioClip ambientCLIP = null;
        [Range(0, 100)]
        [SerializeField] int ambientVolume = 100;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector introTL = null;
        [SerializeField] private PlayableDirector[] s1ToS2TL = null;
        [SerializeField] private PlayableDirector[] s2ToS1TL = null;
        [SerializeField] private PlayableDirector outroTL = null;
        [Header("★ Timing")]
        [SerializeField] private float scoopAniDuration = 1.2f;
        [SerializeField] private float loadCornDelay = 1f;
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
        }
        protected override void OnDisable()
        {
            base.OnDisable();
        }
    }
}