using DoDoEng.Common;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C2_A04
{
    public class DropArea : MonoBehaviour,
        IDropHandler,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject answerEffect;
        [SerializeField] private AudioClip correctCLIP;
        [SerializeField] private TextMeshProUGUI wrongStingrayText;
        [SerializeField] private GameObject overGO;

        // Unity Messages
        private void Awake()
        {
            overGO.SetActive(false);
        }
        private void Start()
        {
        }



        // Interface : IDropHandler, IPointerEnterHandler, IPointerExitHandler
        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            LOG.Info($"OnDrop() | {eventData.pointerDrag.name}", this);

            var stingray = eventData.pointerDrag.GetComponent<StingrayExam>();
            if (stingray != null)
            {
                overGO.SetActive(false);

                if (stingray.IsAnswer)
                {
                    answerEffect.SetActive(true);
                    AudioMGR.One.PlayEffect(correctCLIP);
                }

                wrongStingrayText.text = stingray.Word;

                eventData.Use();
            }
        }
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (StingrayExam.CurrentDrag != null)
                overGO.SetActive(true);
        }
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (StingrayExam.CurrentDrag != null)
                overGO.SetActive(false);
        }
    }
}