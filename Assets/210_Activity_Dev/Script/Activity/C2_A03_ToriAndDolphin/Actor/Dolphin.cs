using beyondi.Coroutine;
using beyondi.Util;
using DoDoEng.Common;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C2_A03
{
    public class Dolphin : MonoBehaviour, IPointerDownHandler, ISubmitable, IID
    {
        // Properties
        public int ID { get; set; }
        public bool IsAnswer { get; private set; }
        public bool IsSubmit { get; private set; }
        public AudioClip PhonicsCLIP { get; private set; }

        // Methods
        public void Setup(ExampleData exam, int dolphinType)
        {
            LOG.Info($"Setup() | {exam} {dolphinType}", this);

            IsSubmit = false;
            IsAnswer = exam.IsAnswer;
            PhonicsCLIP = exam.PhonicsCLIP;

            phonicsTXT.text = exam.Phonics;
            dolphinAni.SetSkin(dolphinType);
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
            if (enable)
                IsSubmit = false;
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();




        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI phonicsTXT = null;
        [SerializeField] private DolphinAni dolphinAni = null;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;
        }
        private void Start()
        {
        }



        // Interface : IPointerDownHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            LOG.Info($"OnPointerDown()", this);

            //AudioMGR.One.PlayNarration(PhonicsCLIP);
            IsSubmit = true;
        }
    }
}