using beyondi.FSM;
using beyondi.Util;
using DoDoEng.Common;
using NaughtyAttributes;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Splines;
using UnityEngine.UIElements;

namespace DoDoEng.Activity.C1_A03
{
    public class RepairDoor : RepairActBase,
        IPointerDownHandler, IPointerUpHandler,
        IDragHandler
    {
        // Definitions
        private enum State { Act, Delay, Fin }

        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Canvas canvas_ = null;
        private Canvas canvas => canvas_ ??= GetComponentInParent<Canvas>();
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private AffMecanim aff_ = null;
        private AffMecanim aff => aff_ ??= GetComponent<AffMecanim>();

        // Fields
        private FSM<State> fsm = null;
        private bool isDragging = false;

        // Functions
        private void activeGunEffect(bool enable)
        {
            var emiation = gunVFX.emission;
            emiation.enabled = enable;
        }

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Act,         E_Act,          X_Act);
            fsm.AddState(State.Delay,       E_Delay);
            fsm.AddState(State.Fin,         E_Fin);
            #pragma warning restore format
        }

        // Overrides
        protected override void onStartPepair()
        {
            base.onStartPepair();

            isComplete = false;


            fsm.StartFSM(State.Act);
        }
        protected override void onFinishPepair()
        {
            base.onFinishPepair();

            cg.blocksRaycasts = false;
            isDragging = false;

            maskLine.DrawAll();
            activeGunEffect(false);

            fsm.StopFSM();
        }



        // FSM
        protected IEnumerator E_Act()
        {
            aff.StartAffordance();
            yield return null;

            cg.blocksRaycasts = true;
            yield return new WaitUntil(() => maskLine.Progress >= completeRatio);
            yield return null;

            fsm.PerformTransition(State.Delay);
        }
        protected IEnumerator X_Act()
        {
            cg.blocksRaycasts = false;
            isDragging = false;
            yield return null;

            AudioMGR.One.StopEffectLL();
            maskLine.DrawAll();
            activeGunEffect(false);
            yield return null;
        }
        protected IEnumerator E_Delay()
        {
            AudioMGR.One.PlayEffect(clearClip);
            yield return new WaitForSeconds(1f);
            yield return null;

            fsm.PerformTransition(State.Fin);
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
        [SerializeField] private MaskBuilder maskLine = null;
        [SerializeField] private SplineContainer splineSC = null;
        [SerializeField] private Transform gunTR = null;
        [SerializeField] private Animator gunAnim = null;
        [SerializeField] private ParticleSystem gunVFX = null;
        [Header("★ Audios")]
        [SerializeField] private AudioClip gunCLIP = null;
        [SerializeField] private AudioClip clearClip = null;
        [Header("★ Configs")]
        [SerializeField] private float distThreshold = 20;
        [SerializeField] private float completeRatio = 0.999f;

        // Unity Messages
        private void Awake()
        {
            initFSM();

            cg.blocksRaycasts = false;

            gunAnim.SetBool("Use", false);
            activeGunEffect(false);
        }
        private void Start()
        {
        }



        // Interface : IPointerDownHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            LOG.Info($"OnPointerDown()", this);

            if (!isComplete)
            {
                var pos = UtilTransform.ScreenToLocal(eventData.position, rt, canvas);
                var dist = SplineUtility.GetNearestPoint(
                    splineSC.Spline, new float3(pos.x, pos.y, 0),
                    out var nearest, out var t);


                if (dist < distThreshold)
                {

                    isDragging = true;
                    maskLine.Draw(UtilTransform.LocalToScreen(nearest, rt, canvas));

                    gunAnim.SetBool("Use", true);
                    activeGunEffect(true);

                    gunTR.localPosition = nearest;
                    gunVFX.transform.localPosition = nearest;

                    AudioMGR.One.PlayEffectLL(gunCLIP, true);
                }
            }
        }

        // Interface : IPointerDownHandler, IPointerUpHandler
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            LOG.Info($"OnPointerUp()", this);

            isDragging = false;

            gunAnim.SetBool("Use", false);
            activeGunEffect(isDragging);
            AudioMGR.One.StopEffectLL();
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            //LOG.Info($"OnDrag()", this);

            if (isDragging)
            {
                var pos = UtilTransform.ScreenToLocal(eventData.position, rt, canvas);
                var dist = SplineUtility.GetNearestPoint(
                    splineSC.Spline, new float3(pos.x, pos.y, 0),
                    out var nearest, out var t);

                if (dist < distThreshold)
                {
                    maskLine.Draw(UtilTransform.LocalToScreen(nearest, rt, canvas));
                    gunTR.localPosition = nearest;
                    gunVFX.transform.localPosition = nearest;
                }
                else
                {
                    isDragging = false;
                    AudioMGR.One.StopEffectLL();
                }

                activeGunEffect(isDragging);
            }
        }
    }
}