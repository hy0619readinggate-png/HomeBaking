using beyondi.Behaviour;
using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Game.C1_G03
{
    public class RoadFloatBlock : BYDSingleton<RoadFloatBlock>
    {
        // Properties
        public Vector3 ScreenPositionOfCenter { get; private set; } // 화면 좌표계에서의 중심점의 위치

        // Methods
        public void Show(RoadType roadType)
        {
            LOG.Function(this);

            roadImages.SetActiveOnly((int)roadType);
            blockCG.gameObject.SetActive(true);
        }
        public void Hide()
        {
            LOG.Function(this);

            blockCG.gameObject.SetActive(false);
        }
        public void Locate(PointerEventData eventData)
        {
            var cam = eventData.pressEventCamera;
            var pos = eventData.position;

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, pos, cam, out var ptWorld))
                rt.position = ptWorld;

            ScreenPositionOfCenter = cam.WorldToScreenPoint(centerTR.position);
        }



        // Fields : caching
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private CanvasGroup blockCG = null;
        [SerializeField] private GameObject[] roadImages = null;
        [SerializeField] private Transform centerTR = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            blockCG.blocksRaycasts = false;
            blockCG.gameObject.SetActive(false);
        }
        private void Start()
        {

        }
    }
}