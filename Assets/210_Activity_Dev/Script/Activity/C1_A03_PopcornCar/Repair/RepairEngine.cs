using beyondi.Coroutine;
using beyondi.FSM;
using beyondi.Util;
using DoDoEng.Common;
using NaughtyAttributes;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Activity.C1_A03
{
    public class RepairEngine : RepairActBase
    {
        // Definitions
        private enum State
        {
            Act, Clear,
            InsertEngin,
            Fin
        }



        // Fields : caching
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Canvas canvas_ = null;
        private Canvas canvas => canvas_ ??= GetComponentInParent<Canvas>();
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private Smoke[] smokes_ = null;
        private Smoke[] smokes => smokes_ ??= GetComponentsInChildren<Smoke>();

        // Fields
        private FSM<State> fsm = null;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Act,         E_Act,          X_Act);
            fsm.AddState(State.Clear,       E_Clear,        X_Clear);
            fsm.AddState(State.InsertEngin, E_InsertEngine,  X_InsertEngine);
            fsm.AddState(State.Fin,         E_Fin);
            #pragma warning restore format
        }



        // Overrides
        protected override void onStartPepair()
        {
            base.onStartPepair();

            isComplete = false;
            cg.blocksRaycasts = true;

            fsm.StartFSM(State.Act);
        }
        protected override void onFinishPepair()
        {
            base.onFinishPepair();

            cg.blocksRaycasts = false;

            fsm.StopFSM();
        }


        // FSM
        protected IEnumerator E_Act()
        {
            gun.gameObject.SetActive(true);
            yield return null;

            gun.Affordance();
            yield return null;

            cg.blocksRaycasts = true;
            yield return null;

            var wait = new WaitForCompleteAll(this, smokes);
            yield return wait;
            yield return null;

            fsm.PerformTransition(State.Clear);
        }
        protected IEnumerator X_Act()
        {
            AudioMGR.One.StopEffectLL();
            cg.blocksRaycasts = false;
            yield return null;

            gun.gameObject.SetActive(false);
            yield return null;
        }
        protected IEnumerator E_Clear()
        {
            smokes.ForEach(s => s.Clear());
            yield return null;

            fsm.PerformTransition(State.InsertEngin);
        }
        protected IEnumerator X_Clear()
        {
            yield return null;
        }
        protected IEnumerator E_InsertEngine()
        {
            anim.SetTrigger("install");
            yield return new WaitForSeconds(1.5f);
            yield return null;

            AudioMGR.One.PlayEffect(clearClip);
            completeVFX.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            yield return null;

            fsm.PerformTransition(State.Fin);
        }
        protected IEnumerator X_InsertEngine()
        {
            yield return null;
        }
        protected IEnumerator E_Fin()
        {
            isComplete = true;
            yield return null;
        }



        // Unity Inspectors : button
        [Button("(DEV)Start", EButtonEnableMode.Playmode)]
        private void devSetup()
        {
            StartRepair();
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private DryGun gun = null;
        [SerializeField] private ParticleSystem completeVFX = null;
        [Header("★ Audios")]
        [SerializeField] private AudioClip clearClip = null;
        [Header("★ Config")]
        [SerializeField] private GunParam gunParam = null;
        [SerializeField] private SmokeParam smokeParam = null;




        // Unity Messages
        private void Awake()
        {
            initFSM();

            cg.blocksRaycasts = false;

            gun.Setup(gunParam);
            smokes.ForEach(s => s.Setup(smokeParam, gun));

            gun.gameObject.SetActive(false);
            completeVFX.gameObject.SetActive(false);
        }
        private void Start()
        {
        }
    }

    [System.Serializable]
    public class GunParam
    {
        public float affDuration = 2f;

        public AudioClip gunOnClip = null;
    }
    [System.Serializable]
    public class SmokeParam
    {
        public float startLife = 100;
        public float blowRatioPerSec = 30;
    }
}