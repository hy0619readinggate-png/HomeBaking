using beyondi.Util;
using DoDoEng.Common;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace DoDoEng.Activity.C1_A01
{
    [ExecuteAlways]
    public class TraceStrokeSegment : MonoBehaviour
    {
        // Properties
        public bool IsComplete => strokeRatio >= finishThreshold;
        public Vector3 GetCompletePosition()
        {
            var ptWorld = pathSC.EvaluatePosition(finishThreshold);
            return Camera.main.WorldToScreenPoint(ptWorld);
        }



        // Methods
        public void Init(float distThreshold)
        {
            LOG.Function(this, $"{distThreshold}");

            strokeRatio = 0f;
            drawStroke(strokeRatio);

            distanceThreshold = distThreshold;
        }
        public bool BeginStroke(Vector2 ptScreen)
        {
            LOG.Function(this, $"{ptScreen}");

            var ptLocal = UtilTransform.ScreenToLocal(ptScreen, rt, canvas);
            var dist = SplineUtility.GetNearestPoint(
                pathSC.Spline, new float3(ptLocal.x, ptLocal.y, 0),
                out var _, out var t);

            var valid = dist < distanceThreshold && t < startThreshold;
            if (valid)
            {
                strokeRatio = Mathf.Max(t, startThreshold);
                drawStroke(strokeRatio);

                return true;
            }
            else return false;
        }
        public bool DoStroke(Vector2 ptScreen)
        {
            //LOG.Function(this, $"{ptScreen}");

            var ptLocal = UtilTransform.ScreenToLocal(ptScreen, rt, canvas);
            var dist = SplineUtility.GetNearestPoint(
                pathSC.Spline, new float3(ptLocal.x, ptLocal.y, 0),
                out var _, out var t);

            var valid = dist < distanceThreshold;
            if (valid)
            {
                strokeRatio = Mathf.Max(t, startThreshold);
                drawStroke(strokeRatio);

                return true;
            }
            else return false;
        }
        public void CompleteStroke()
        {
            LOG.Function(this);

            strokeRatio = 1;
            drawStroke(strokeRatio);
        }
        public void ClearStroke()
        {
            LOG.Function(this);

            strokeRatio = 0;
            drawStroke(strokeRatio);
        }



        // Fields : caching
        private SplineContainer pathSC_ = null;
        private SplineContainer pathSC => pathSC_ ??= GetComponentInChildren<SplineContainer>(true);
        private TraceHandle handle_ = null;
        private TraceHandle handle => handle_ ??= GetComponentInChildren<TraceHandle>(true);
        private UILineRenderer maskLR_ = null;
        private UILineRenderer maskLR => maskLR_ ??= GetComponent<UILineRenderer>();
        private Mask mask_ = null;
        private Mask mask => mask_ ??= GetComponent<Mask>();
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private Canvas canvas_ = null;
        private Canvas canvas => canvas_ ??= rt.GetParentCanvas();

        // Fields
        private float distanceThreshold = 100;

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

        // Functions
        private void drawStroke(float t)
        {
            if (t < startThreshold) t = 0;
            if (t > finishThreshold) t = 1;

            maskLR.Points = evaluatePath(pathSC, t);

            var handleVisible = t >= startThreshold && t <= finishThreshold && t != 0 && t != 1;
            handle.gameObject.SetActive(handleVisible);
            handle.transform.position = pathSC.EvaluatePosition(t);
        }



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField][Range(0, 1)] private float strokeRatio = 1;
        [SerializeField] private float strokeWidth = 150f;
        [SerializeField][Range(0, 1)] private float startThreshold = 0.1f;
        [SerializeField][Range(0, 1)] private float finishThreshold = 0.9f;
        [Header("★ DEV")]
        [SerializeField] private bool showMask = false;
        [Button("Set Start")] private void setStart() { startThreshold = strokeRatio; }
        [Button("Set Finish")] private void setFinish() { finishThreshold = strokeRatio; }

        // Unity Messages
        private void Awake()
        {
            maskLR.Points = null;
            maskLR.LineThickness = strokeWidth;
            mask.showMaskGraphic = showMask;

            handle.gameObject.SetActive(false);
            handle.transform.position = pathSC.EvaluatePosition(strokeRatio);
        }
        private void Start()
        {
            if (Application.isPlaying)
            {
                strokeRatio = 0f;

                showMask = false;
                mask.showMaskGraphic = false;
            }
        }
        private void OnValidate()
        {
            if (canvas != null)
                drawStroke(strokeRatio);

            maskLR.LineThickness = strokeWidth;
            mask.showMaskGraphic = showMask;

        }
        private void OnDrawGizmos()
        {
            var ptStart = pathSC.EvaluatePosition(startThreshold);
            var ptFinish = pathSC.EvaluatePosition(finishThreshold);

            Gizmos.color = Color.red;
            Gizmos.DrawCube(ptStart, Vector3.one * 0.2f);
            Gizmos.DrawCube(ptFinish, Vector3.one * 0.2f);
        }
    }
}