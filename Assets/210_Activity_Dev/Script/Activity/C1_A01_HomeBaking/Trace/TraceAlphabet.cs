using beyondi.Util;
using DoDoEng.Common;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C1_A01
{
    public class TraceAlphabet : MonoBehaviour
    {
        // Properties
        public TraceStroke[] Strokes => strokes;


        // Methods
        public void Init(float distThreshold)
        {
            LOG.Function(this);

            foreach (var stroke in strokes)
            {
                stroke.Segments.ForEach(s => s.Init(distThreshold));
                stroke.Aff.gameObject.SetActive(true);
            }
            outLineCG = outerLineGO.AddComponent<CanvasGroup>();
            outLineCG.blocksRaycasts = false;
            outLineCG.alpha = 0;
        }
        public void ShowOutLine()
        {
            LOG.Function(this);

            outerLineGO.SetActive(true);
            outLineCG.DOFade(1, 0.5f);
        }



        // Fields
        private CanvasGroup outLineCG = null;


        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TraceStroke[] strokes = null;
        [SerializeField] private GameObject outerLineGO = null;
    }

    [Serializable]
    public class TraceStroke
    {
        // Properties
        public int SegmentCount => Segments?.Length ?? 0;

        public TraceStrokeSegment[] Segments = null;
        public TraceAff Aff = null;
    }
}