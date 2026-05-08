using DoDoEng.Common;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DoDoEng.Activity.C2_A07
{
    public class FloatLuggage : MonoBehaviour
    {
        // Properties
        public RectTransform FloatRT { get; private set; }

        // Methods
        public void Pickup(ExampleData data, int caseType, PointerEventData eventData)
        {
            LOG.Info($"Pickup() | {data}", this);

            transform.SetAsLastSibling();

            image.sprite = data.WordSPR;
            for (var i = 0; i < cases.Length; i++)
                cases[i].SetActive(caseType - 1 == i);

            gameObject.SetActive(true);
            FloatRT = rt;

            locateLuggage(eventData);
        }
        public void Locate(PointerEventData eventData)
        {
            //LOG.Info($"Locate()", this);

            locateLuggage(eventData);
        }
        public void Drop()
        {
            LOG.Info($"Drop()", this);

            gameObject.SetActive(false);
        }



        // Fields : caching
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= rt.AddComponent<CanvasGroup>();

        // Functions
        private void locateLuggage(PointerEventData eventData)
        {
            var cam = eventData.pressEventCamera;
            var pos = eventData.position;

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, pos, cam, out var ptWorld))
                rt.position = ptWorld;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Image image = null;
        [SerializeField] private GameObject[] cases = null;

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