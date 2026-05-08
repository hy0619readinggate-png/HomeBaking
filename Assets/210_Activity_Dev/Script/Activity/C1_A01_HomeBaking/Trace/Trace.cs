using beyondi.Coroutine;
using beyondi.FSM;
using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C1_A01
{
    public class Trace : MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        ICompletable
    {
        // Definitions
        private enum State { Trace, Feedback, Next, Fin }

        // Methods
        public void Setup(GameObject pb)
        {
            LOG.Info($"Setup()", this);

            Util.RemoveAllChildren(prefabParentTR);

            var go = Instantiate(pb, prefabParentTR);

            TA = go.GetComponent<TraceAlphabet>();
            TA.Init(distThreshold);
            foreach (var stroke in TA.Strokes)
                stroke.Segments.ForEach(s => s.gameObject.SetActive(true));

            strokeCount = TA.Strokes.Length;
        }
        public void StartTrace()
        {
            LOG.Info($"StartTrace()", this);

            isComplete = false;
            strokeCurrent = 0;
            segmentCurrent = 0;

            fsm.StartFSM(State.Trace);
        }
        public void FinishTrace()
        {
            LOG.Info($"FinishTrace()", this);

            fsm.StopFSM();
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        private FSM<State> fsm = null;
        private bool isComplete = false;

        // Fields
        private TraceAlphabet TA = null;
        private int strokeCount = 0;
        private int strokeCurrent = 0;
        private int segmentCurrent = 0;
        private bool isTracing = false;
        private Vector3 strokeCompletePosition;

        // Functions
        private TraceStroke cStroke => TA.Strokes[strokeCurrent];
        private TraceStrokeSegment cSegment => TA.Strokes[strokeCurrent].Segments[segmentCurrent];

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Trace,       E_Trace,      X_Trace);
            fsm.AddState(State.Feedback,    E_Feedback,   X_Feedback);
            fsm.AddState(State.Next,        E_Next);
            fsm.AddState(State.Fin,         E_Fin);
            #pragma warning restore format
        }

        private void abortTrace()
        {
            cStroke.Segments.ForEach(s => s.ClearStroke());
            segmentCurrent = 0;
            isTracing = false;
            AudioMGR.One.StopEffectLL();
        }


        // FSM
        IEnumerator E_Trace()
        {
            cStroke.Aff.StartAff();

            cg.blocksRaycasts = true;
            yield return null;
        }
        IEnumerator X_Trace()
        {
            AudioMGR.One.StopEffectLL();
            cStroke.Aff.FinishAff();

            cg.blocksRaycasts = false;
            yield return null;
        }
        IEnumerator E_Feedback()
        {
            strokeFX.PlayAtPosition(strokeCompletePosition);
            yield return new WaitForSeconds(feedbackDelay);

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_Feedback()
        {
            yield return null;
        }
        IEnumerator E_Next()
        {
            strokeCurrent++;
            if (strokeCurrent < strokeCount)
            {
                segmentCurrent = 0;

                fsm.PerformTransition(State.Trace);
            }
            else fsm.PerformTransition(State.Fin);
            yield return null;
        }
        IEnumerator E_Fin()
        {
            TA.ShowOutLine();
            isComplete = true;
            yield return null;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Transform prefabParentTR = null;
        [SerializeField] private ParticleSystem strokeFX = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip doughCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float feedbackDelay = 1f;
        [SerializeField] private float distThreshold = 100;

        // Unity Messages
        private void Awake()
        {
            strokeFX.gameObject.SetActive(false);

            cg.blocksRaycasts = false;

            initFSM();
        }
        private void Start()
        {
        }



        // Interface
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            LOG.Function(this, $"{eventData.position}");
        }
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            LOG.Function(this, $"{eventData.position}");
        }
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            //LOG.Function(this, $"{eventData.position}");
            var valid = cSegment.BeginStroke(eventData.position);

            LOG.Info($"OnBeginDrag() | {valid}", this);
            if (valid)
            {
                isTracing = true;

                AudioMGR.One.PlayEffectLL(doughCLIP);
            }
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (isTracing)
            {
                var valid = cSegment.DoStroke(eventData.position);
                if (valid)
                {
                    if (cSegment.IsComplete)
                    {
                        LOG.Info($"Segment Complete!!", this);
                        strokeCompletePosition = cSegment.GetCompletePosition();
                        segmentCurrent++;
                        if (segmentCurrent >= cStroke.SegmentCount)
                        {
                            isTracing = false;
                            fsm.PerformTransition(State.Feedback);
                        }
                    }
                }
                else abortTrace();
            }
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            //LOG.Function(this, $"{eventData.position}");

            if (isTracing)
                abortTrace();
        }

        // Interface
        bool ICompletable.IsComplete => isComplete;
    }
}