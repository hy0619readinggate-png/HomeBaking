using DoDoEng.Common;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XDPaint.Controllers;

namespace DoDoEng.Activity.C1_A04
{
    [RequireComponent(typeof(Image))]
    public class PaintLayerPickup : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        // Properties
        public static PaintLayerPickup CurrentLayer { get; private set; } = null;
        public Texture2D MaskTexture => maskTexture;

        // Methods
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }

        // Events
        public event Action<PaintLayerPickup> OnPointerDown;
        public event Action<PaintLayerPickup> OnPointerUp;



        // Fields : caching
        private Image image_ = null;
        private Image image => image_ ??= GetComponent<Image>();
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        private Texture2D maskTexture = null;

        // Functions
        private Texture2D genMaskTexture()
        {
            LOG.Info($"genMaskTexture()", this);

            var texSrc = image.mainTexture as Texture2D;
            var texMask = new Texture2D(texSrc.width, texSrc.height, TextureFormat.R8, false);

            // 알파채널을 R8의 R채널로 적용
            var pixels = texSrc.GetPixels();
            for (var i = 0; i < pixels.Length; i++)
                pixels[i].r = pixels[i].a;

            texMask.SetPixels(pixels);
            texMask.Apply();

            return texMask;
        }



        // Unity Messages
        private void Awake()
        {
            maskTexture = genMaskTexture();
        }
        private void OnEnable()
        {
            InputController.Instance.AddIgnoreForRaycasts(gameObject);
        }
        private void OnDisable()
        {
            InputController.Instance.RemoveIgnoreForRaycasts(gameObject);
        }



        // Implementation : IPointerDownHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (CurrentLayer != null)
                return;

            CurrentLayer = this;
            OnPointerDown?.Invoke(this);
        }

        // Implementation : IPointerUpHandler
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (CurrentLayer != this)
                return;

            CurrentLayer = null;
            OnPointerUp?.Invoke(this);
        }
    }
}

