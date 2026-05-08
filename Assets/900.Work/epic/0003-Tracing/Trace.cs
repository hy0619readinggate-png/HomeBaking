using beyondi.Coroutine;
using beyondi.FSM;
using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Splines;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace DoDoEng
{
    public class Trace : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, ICompletable
    {
        // Definitions
        private enum State { Trace, Feedback, Next, Fin }

        // Methods
        public void Setup(TraceAlphabet pb)
        {
            LOG.Info($"Setup()", this);

            Util.RemoveAllChildren(prefabParentTR);

            TA = Instantiate(pb, prefabParentTR);
            TA.StrokesGO.ForEach(go => go.SetActive(false));

            strokeCount = TA.StrokesGO.Length;

            var sCount = TA.StrokesGO.Length;
            var pCount = TA.PathsSC.Length;
            LOG.Assert(
                TA.StrokesGO.Length == TA.PathsSC.Length,
                $"Count of Strokes and Paths must be same. ({sCount} {pCount})", this);
        }
        public void StartTrace()
        {
            LOG.Info($"StartTrace()", this);

            isComplete = false;
            strokeCurrent = 0;

            fsm.StartFSM(State.Trace);
        }
        public void FinishTrace()
        {
            LOG.Info($"FinishTrace()", this);

            fsm.StopFSM();
        }



        // Fields : caching
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private Image img_ = null;
        private Image img => img_ ??= GetComponent<Image>();
        private Canvas canvas_ = null;
        private Canvas canvas => canvas_ ??= GetComponentInParent<Canvas>();

        // Fields
        private FSM<State> fsm = null;
        private bool isComplete = false;

        // Fields
        private TraceAlphabet TA = null;
        private int strokeCount = 0;
        private int strokeCurrent = 0;
        private bool isTracing = false;
        private SplineContainer pathSC = null;
        private GameObject strokeGO = null;
        private Transform originStrokeParentTR = null;
        private Vector3 strokeCompletePosition;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Trace,     E_Trace,      X_Trace);
            fsm.AddState(State.Feedback,  E_Feedback,   X_Feedback);
            fsm.AddState(State.Next,      E_Next);
            fsm.AddState(State.Fin,       E_Fin);
            #pragma warning restore format
        }

        // Functions
        private Vector2[] evaluatePath(SplineContainer spline, float toT)
        {
            var interval = 1f / 10; // 전체를 30개로 나눔
            toT = Mathf.Min(toT, 1);

            var path = new List<Vector2>();
            for (var t = 0f; t < toT; t += interval)
            {
                var ptWorld = spline.EvaluatePosition(t);
                var ptLocal = UtilTransform.WorldToLocal(ptWorld, rt, canvas);
                path.Add(ptLocal);
            }

            // 마지막 점 추가
            {
                var ptWorld = spline.EvaluatePosition(toT);
                var ptLocal = UtilTransform.WorldToLocal(ptWorld, rt, canvas);
                path.Add(ptLocal);
            }

            return path.ToArray();
        }
        private void locateHandle(SplineContainer spline, float t)
        {
            spline.Evaluate(t, out var ptWorld, out var tangent, out _);

            var angle = -Mathf.Atan2(tangent.x, tangent.y);

            handleTR.localPosition = UtilTransform.WorldToLocal(ptWorld, rt, canvas);
            handleTR.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
        }
        private void abortTrace(float fromT)
        {
            isTracing = false;
            StartCoroutine(coResetTrace(fromT, 0.1f));
        }


        // FSM
        IEnumerator E_Trace()
        {
            strokeGO = TA.StrokesGO[strokeCurrent];
            pathSC = TA.PathsSC[strokeCurrent];

            aff.transform.SetParent(TA.transform);
            aff.transform.SetSiblingIndex(TA.OuterLineTR.transform.GetSiblingIndex());
            aff.Setup(pathSC);
            aff.StartAff();

            maskLR.transform.SetParent(TA.transform);
            maskLR.transform.SetSiblingIndex(TA.OuterLineTR.transform.GetSiblingIndex());
            maskLR.Points = null;

            originStrokeParentTR = strokeGO.transform.parent;
            strokeGO.transform.SetParent(maskLR.transform);
            strokeGO.SetActive(true);

            locateHandle(pathSC, 0);
            handleTR.gameObject.SetActive(true);

            img.raycastTarget = true;
            yield return null;
        }
        IEnumerator X_Trace()
        {
            strokeGO.transform.SetParent(originStrokeParentTR);
            strokeGO = null;
            pathSC = null;

            maskLR.transform.SetParent(transform);
            maskLR.Points = null;

            aff.FinishAff();
            aff.transform.SetParent(transform);

            handleTR.gameObject.SetActive(false);

            isTracing = false;
            img.raycastTarget = false;
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
                fsm.PerformTransition(State.Trace);
            else fsm.PerformTransition(State.Fin);
            yield return null;
        }
        IEnumerator E_Fin()
        {
            isComplete = true;
            yield return null;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Transform prefabParentTR = null;
        [SerializeField] private StrokeAff aff = null;
        [SerializeField] private UILineRenderer maskLR = null;
        [SerializeField] private Transform handleTR = null;
        [SerializeField] private ParticleSystem strokeFX = null;
        [Header("★ Config")]
        [SerializeField] private float feedbackDelay = 1f;
        [SerializeField] private float distThreshold = 20;
        [SerializeField] private float strokeThreshold = 0.9f;

        // Unity Messages
        private void Awake()
        {
            handleTR.gameObject.SetActive(false);
            strokeFX.gameObject.SetActive(false);
            img.raycastTarget = false;

            initFSM();
        }
        private void Start()
        {
            // -----------------------------------------------------------
            // For Test
            TA = GetComponentInChildren<TraceAlphabet>();
            TA.StrokesGO.ForEach(go => go.SetActive(false));

            strokeCount = TA.StrokesGO.Length;

            StartTrace();
            // -----------------------------------------------------------
        }

        // Unity Coroutine
        IEnumerator coResetTrace(float fromT, float duration)
        {
            using (LOG.Coroutine($"coResetTrace()", this))
            {
                var step = 30;
                var deltaT = fromT / step;
                var interval = duration / step;

                for (var t = fromT; t > 0; t -= deltaT)
                {
                    maskLR.Points = evaluatePath(pathSC, t);
                    locateHandle(pathSC, t);

                    yield return new WaitForSeconds(interval);
                }

                maskLR.Points = null;
                yield return null;
            }
        }



        // Interface
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            var pos = UtilTransform.ScreenToLocal(eventData.position, rt, canvas);
            var dist = SplineUtility.GetNearestPoint(
                pathSC.Spline, new float3(pos.x, pos.y, 0),
                out _, out var t);

            if (dist < distThreshold && t < 0.1f)
                isTracing = true;
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!isTracing || pathSC == null)
                return;

            var pos = UtilTransform.ScreenToLocal(eventData.position, rt, canvas);
            var dist = SplineUtility.GetNearestPoint(
                pathSC.Spline, new float3(pos.x, pos.y, 0),
                out var nearest, out var t);

            maskLR.Points = evaluatePath(pathSC, t);
            locateHandle(pathSC, t);

            if (dist > distThreshold)
                abortTrace(t);

            if (t > strokeThreshold)
            {
                strokeCompletePosition = UtilTransform.LocalToScreen(nearest, rt, canvas);
                fsm.PerformTransition(State.Feedback);
            }
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (!isTracing)
                return;

            var pos = UtilTransform.ScreenToLocal(eventData.position, rt, canvas);
            var dist = SplineUtility.GetNearestPoint(
                pathSC.Spline, new float3(pos.x, pos.y, 0),
                out _, out var t);

            abortTrace(t);
        }

        // Interface
        bool ICompletable.IsComplete => isComplete;
    }
}