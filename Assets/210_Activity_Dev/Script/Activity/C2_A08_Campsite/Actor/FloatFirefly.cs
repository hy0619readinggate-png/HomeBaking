using DoDoEng.Common;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C2_A08
{
    public class FloatFirefly : MonoBehaviour
    {
        // Properties
        public RectTransform RT { get; private set; }

        // Methods
        public void Drag(PointerEventData eventData)
        {
            LOG.Info($"Drag()", this);

            gameObject.SetActive(true);
            fireflyAni.PlayAnimationLoop(FireflyAnimation.Drag);
            RT = rt;

            LocateFirefly(eventData);
        }
        public void Locate(PointerEventData eventData)
        {
            //LOG.Info($"Locate()", this);

            LocateFirefly(eventData);
        }
        public void Drop()
        {
            LOG.Info($"Drop()", this);

            gameObject.SetActive(false);
        }
        public void PlayFireflyAnimation(FireflyAnimation e)
        {
            LOG.Info($"PlayFireflyAnimation()", this);

            fireflyAni.PlayAnimationLoop(e);
        }



        // Fields : caching
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= rt.AddComponent<CanvasGroup>();

        // Functions
        private void LocateFirefly(PointerEventData eventData)
        {
            var cam = eventData.pressEventCamera;
            var pos = eventData.position;

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, pos, cam, out var ptWorld))
                rt.position = ptWorld;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private FireflyAni fireflyAni = null;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;
            gameObject.SetActive(false);
        }
        private void Start()
        {
        }
    }
}