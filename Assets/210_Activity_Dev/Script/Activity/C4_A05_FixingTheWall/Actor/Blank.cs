using beyondi.Util;
using DoDoEng.Common;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C4_A05
{
    public class Blank : MonoBehaviour,
        IDropHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        // Properties
        public bool IsCorrect { get; private set; } = false;
        public float PosX => transform.position.x;

        // Methods
        public void Setup(string text)
        {
            LOG.Info($"Setup() | {text}", this);

            correctText = text;
            cg.blocksRaycasts = true;
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();

        // Fields
        private string correctText = null;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI chunkTXT;
        [SerializeField] private GameObject blockGO;
        [Header("★ Sound")]
        [SerializeField] private AudioClip[] correctCLIP = null;

        // Unity Messages
        private void Awake()
        {
            chunkTXT.text = "";
            blockGO.SetActive(false);
        }
        private void Start()
        {
        }



        // Interface : IDropHandler, IPointerEnterHandler, IPointerExitHandler
        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            LOG.Info($"OnDrop() | {eventData.pointerDrag.name}", this);

            var block = eventData.pointerDrag.GetComponent<Block>();
            if (block != null)
            {
                if (block.Text == correctText)
                {
                    block.IsComplete = true;

                    chunkTXT.text = block.Text;
                    blockGO.SetActive(true);

                    var clip = UtilArray.ExtractOne(correctCLIP);
                    AudioMGR.One.PlayEffect(clip);
                    anim.SetTrigger("Show");

                    IsCorrect = true;
                    cg.blocksRaycasts = false;
                }
                else block.SetPositionTo(transform.position);

                eventData.Use();
            }
        }
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (Block.CurrentDrag != null)
                anim.SetTrigger("Over");
        }
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            //if (Block.CurrentDrag != null)
            anim.SetTrigger("Idle");
        }
    }
}