using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DoDoEng.EBook.Quiz
{
    public class T3ExampleFloat : MonoBehaviour
    {
        // Properties
        public T3Example CurrentExample => exam;

        // Methods
        public void Pickup(T3Example exam, PointerEventData eventData)
        {
            LOG.Info($"Pickup()", this);

            cancelReturn();

            AudioMGR.One.PlayEffect(pickupCLIP);
            floatRT.gameObject.SetActive(true);
            examIMG.sprite = exam.Sprite;

            this.exam = exam;

            locate(eventData);
        }
        public void Locate(PointerEventData eventData)
        {
            //LOG.Info($"Locate()", this);
            locate(eventData);
        }
        public void Drop()
        {
            LOG.Info($"Drop()", this);

            exam = null;
            floatRT.gameObject.SetActive(false);
        }
        public void ReturnTo()
        {
            LOG.Info($"ReturnTo()", this);

            var returnTo = exam.transform.position;
            tween = floatRT
                        .DOMove(returnTo, returnDuration)
                        .OnComplete(() =>
                        {
                            floatRT.gameObject.SetActive(false);
                            exam.Return();
                        });

            AudioMGR.One.PlayEffect(returnCLIP);
        }
        public IEnumerator Move(Sprite sprite, Vector3 from, Vector3 to, float duration)
        {
            floatRT.gameObject.SetActive(true);
            examIMG.sprite = sprite;

            var jumpPowerDir = from.x < to.x ? +1 : -1;
            floatRT.position = from;
            yield return floatRT.DOJump(to, jumpPowerDir * 1, 1, duration).WaitForCompletion();

            floatRT.gameObject.SetActive(false);
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= floatRT.gameObject.AddComponent<CanvasGroup>();

        // Fields
        private Tween tween = null;
        private T3Example exam = null;

        // Functions
        private void locate(PointerEventData eventData)
        {
            var cam = eventData.pressEventCamera;
            var pos = eventData.position;

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(floatRT, pos, cam, out var ptWorld))
                floatRT.position = ptWorld;
        }
        private void cancelReturn()
        {
            tween.Complete(true);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private RectTransform floatRT = null;
        [SerializeField] private Image examIMG = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip pickupCLIP = null;
        [SerializeField] private AudioClip returnCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float returnDuration = 0.5f;

        // Unity Messages
        private void Awake()
        {
            floatRT.gameObject.SetActive(false);
            cg.blocksRaycasts = false;
        }
    }
}