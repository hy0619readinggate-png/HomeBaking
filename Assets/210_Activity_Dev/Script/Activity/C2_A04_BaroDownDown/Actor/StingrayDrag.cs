using DoDoEng.Common;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C2_A04
{
    public class StingrayDrag : MonoBehaviour
    {
        // Properties
        public RectTransform RT { get; private set; }

        // Methods
        public void Drag(string str, PointerEventData eventData)
        {
            LOG.Info($"Drag()", this);

            gameObject.SetActive(true);
            text.text = str;
            ani.PlayAnimationLoop(StingrayAnimation.Idle);
            RT = rt;

            LocateStingray(eventData);
        }
        public void Locate(PointerEventData eventData)
        {
            //LOG.Info($"Locate()", this);

            LocateStingray(eventData);
        }
        public void Hide()
        {
            LOG.Info($"Hide()", this);

            gameObject.SetActive(false);
        }



        // Fields : caching
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Functions
        private void LocateStingray(PointerEventData eventData)
        {
            var cam = eventData.pressEventCamera;
            var pos = eventData.position;

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, pos, cam, out var ptWorld))
                rt.position = ptWorld;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private StingrayAni ani;
        [SerializeField] private TextMeshProUGUI text;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;
            text.text = "";
            gameObject.SetActive(false);
        }
        private void Start()
        {
        }
    }
}