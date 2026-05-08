using beyondi.Coroutine;
using beyondi.FSM;
using beyondi.Util;
using DG.Tweening;
using NaughtyAttributes;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Activity.C1_A03
{
    public class RepairTire : RepairActBase
    {
        // Definitions
        private enum State
        {
            Problem, Act, Next,
            Fin
        }



        // Fields : caching
        private NutSet[] nutSets_ = null;
        private NutSet[] nutSets => nutSets_ ??= GetComponentsInChildren<NutSet>(true);

        // Fields
        private FSM<State> fsm = null;
        private int seq = 0;
        private NutSet currentNut = null;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Problem,     E_Problem,      X_Problem);
            fsm.AddState(State.Act,         E_Act,          X_Act);
            fsm.AddState(State.Next,        E_Next);
            fsm.AddState(State.Fin,         E_Fin);
            #pragma warning restore format
        }



        // Overrides
        protected override void onStartPepair()
        {
            base.onStartPepair();

            seq = 0;
            isComplete = false;

            fsm.StartFSM(State.Problem);
        }
        protected override void onFinishPepair()
        {
            base.onFinishPepair();

            fsm.StopFSM();
        }



        // FSM
        protected IEnumerator E_Problem()
        {
            currentNut = nutSets[seq];
            currentNut.Ready();
            yield return null;

            fsm.PerformTransition(State.Act);
        }
        protected IEnumerator X_Problem()
        {
            yield return null;
        }
        protected IEnumerator E_Act()
        {
            currentNut.Affordance(seq == 0 ? 0 : secondNutAffDelay);
            yield return null;

            currentNut.EnableInteraction(true);
            var wait = new WaitForComplete(this, currentNut);
            yield return wait;
            yield return null;

            fsm.PerformTransition(State.Next);
        }
        protected IEnumerator X_Act()
        {
            currentNut.EnableInteraction(false);
            currentNut.Finish();
            yield return null;
        }
        protected IEnumerator E_Next()
        {
            yield return new WaitForSeconds(1f);
            yield return null;

            if (++seq < nutSets.Length)
                fsm.PerformTransition(State.Problem);
            else fsm.PerformTransition(State.Fin);

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
        [Header("★ Config")]
        [SerializeField] private float secondNutAffDelay = 1.0f;
        [SerializeField] private NutParam nutParam = null;



        // Unity Messages
        private void Awake()
        {
            initFSM();

            nutSets.ForEach(n => n.Setup(nutParam));
        }
        private void Start()
        {
        }
    }

    [System.Serializable]
    public class NutParam
    {
        public AudioClip startClip = null;

        public ParticleSystem spinVFX = null;
        public ParticleSystem correctVFX = null;

        public float maxAngleInterval = 25f;
        public float completeRatio = 0.95f;

        public float affDuration = 2f;
    }
}