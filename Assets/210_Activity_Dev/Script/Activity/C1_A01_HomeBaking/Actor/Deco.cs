using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;
using XDPaint;
using XDPaint.Controllers;
using XDPaint.Core.PaintObject.Data;

namespace DoDoEng.Activity.C1_A01
{
    [RequireComponent(typeof(PaintManager))]
    public class Deco : MonoBehaviour
    {
        // Properties
        public Texture2D UserImage
        {
            get
            {
                var texture = paintMGR.GetResultTexture();

                LOG.Info($"{texture} - {texture.width}, {texture.height}, {alphabetRect}", this);
                var tex = texture.Crop((int)alphabetRect.x, (int)alphabetRect.y,
                    (int)alphabetRect.width, (int)alphabetRect.height);

                return tex;
            }

        }

        // Methods
        public void Setup(GameObject pb)
        {
            LOG.Info($"Setup()", this);

            Util.RemoveAllChildren(prefabParentTR);

            var go = Instantiate(pb, prefabParentTR);
            var alphabet = go.GetComponent<DecoAlphabet>();
            alphabet.CrumbGO.SetActive(false);
            alphabet.EatAlphabetGO.SetActive(false);

            paintMGR.Init();
            undoBTN.gameObject.SetActive(false);

            alphabetRect = getRectInRect(alphabet.transform, prefabParentTR);
        }
        public void StartDeco()
        {
            LOG.Info($"StartDeco()", this);

            enableInteraction(true);

            undoBTN.gameObject.SetActive(false);
        }
        public void FinishDeco()
        {
            LOG.Info($"FinishDeco()", this);

            enableInteraction(false);
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction()", this);

            enableInteraction(enable);
        }
        public void ChangeTool(ToolType toolType, BrushInfo toolInfo, int itemID)
        {
            LOG.Info($"ChangeTool() | {toolType}, {toolInfo}", this);

            brushInfo = toolInfo;

            var brush = PaintController.Instance.Brush;
            brush.SourceTexture = brushInfo.SourceTexture;
            brush.FilterMode = brushInfo.FilterMode;
            brush.SetColor(brushInfo.Color);
            brush.Size = brushInfo.Size * brushInfo.Scale;
            brush.Hardness = brushInfo.Hardness;
            brush.RenderAngle = brushInfo.Angle;
            PaintController.Instance.Brush = brush;

            if (paintMGR.ToolsManager.CurrentTool != null)
                paintMGR.ToolsManager.CurrentTool.BaseSettings.CanPaintLines = toolType == ToolType.Cream;

            selectedItemID = itemID;
        }



        // Fields : caching
        private PaintManager paintMGR_ = null;
        private PaintManager paintMGR => paintMGR_ ??= GetComponent<PaintManager>();
        private Button undoBTN_ = null;
        private Button undoBTN => undoBTN_ ??= undoButtonGO.GetComponentInChildren<Button>(true);
        private Image maskImage_ = null;
        private Image maskImage => maskImage_ ??= GetComponent<Image>();

        // Fields
        private Rect alphabetRect;
        private BrushInfo brushInfo = null;
        private Tween showUndoButtonTween = null;
        private int selectedItemID = -1;

        // Functions
        private void enableInteraction(bool enable)
        {
            InputController.Instance.enabled = enable;

            undoBTN.gameObject.SetActive(enable);
        }
        private Rect getRectInRect(Transform childTR, Transform parentTR)
        {
            var parentRect = parentTR.GetComponent<RectTransform>().rect;
            var childRect = childTR.GetComponent<RectTransform>().rect;
            var x = childRect.x - parentRect.x;
            var y = childRect.y - parentRect.y;
            var width = childRect.width;
            var height = childRect.height;
            return new Rect(x, y, width, height);
        }

        // Functions
        private void roundImageAlpha()
        {
            var texSrc = maskImage.mainTexture as Texture2D;
            var texMask = new Texture2D(texSrc.width, texSrc.height, TextureFormat.ARGB32, false);

            var pixels = texSrc.GetPixels();
            for (var i = 0; i < pixels.Length; i++)
                pixels[i].a = pixels[i].a > maskImageAlphaMin ? pixels[i].a : 0;

            texMask.SetPixels(pixels);
            texMask.Apply();

            var rect = new Rect(0, 0, texMask.width, texMask.height);
            var pivot = new Vector2(0.5f, 0.5f);
            maskImage.sprite = Sprite.Create(texMask, rect, pivot);
        }

        // Event Handlers
        private void paintMGR_OnUndoStatusChanged(bool enable)
        {
            var undoActionCount = paintMGR.StatesController.GetUndoActionsCount();

            if (undoActionCount > 0)
                DOVirtual.DelayedCall(0.05f, () => undoBTN.gameObject.SetActive(true));
            else undoBTN.gameObject.SetActive(false);
        }
        private void paintObject_OnPointerDown(PointerData pointData)
        {
            AudioMGR.One.PlayEffectLL(decoCLIP[selectedItemID - 1]);

            var brush = PaintController.Instance.Brush;
            brush.Size = brushInfo.Size * brushInfo.Scale;
            brush.RenderAngle = brushInfo.Angle;
            PaintController.Instance.Brush = brush;

            showUndoButtonTween.Kill();
            undoButtonGO.gameObject.SetActive(false);
        }
        private void paintObject_OnPointerUp(PointerUpData pointData)
        {
            showUndoButtonTween.Kill();
            showUndoButtonTween = DOVirtual.DelayedCall(
                undoButtonDelay,
                () =>
                {
                    undoButtonGO.gameObject.SetActive(true);
                });
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Transform prefabParentTR = null;
        [SerializeField] private GameObject undoButtonGO = null;
        [Header("★ Audios")]
        [SerializeField] private AudioClip[] decoCLIP = null;
        [Header("★ Config")]
        private float undoButtonDelay = 0.7f;
        private float maskImageAlphaMin = 0.4f;

        // Unity Messages
        private void Awake()
        {
            enableInteraction(false);

            undoBTN.gameObject.SetActive(false);
            undoBTN.onClick.AddListener(() => paintMGR.StatesController.Undo());

            roundImageAlpha();
        }
        private void Start()
        {

        }
        private void OnDestroy()
        {

        }
        private void OnEnable()
        {
            paintMGR.StatesController.OnUndoStatusChanged += paintMGR_OnUndoStatusChanged;
            paintMGR.PaintObject.OnPointerDown += paintObject_OnPointerDown;
            paintMGR.PaintObject.OnPointerUp += paintObject_OnPointerUp;
        }
        private void OnDisable()
        {
            paintMGR.StatesController.OnUndoStatusChanged -= paintMGR_OnUndoStatusChanged;
            paintMGR.PaintObject.OnPointerDown -= paintObject_OnPointerDown;
            paintMGR.PaintObject.OnPointerUp -= paintObject_OnPointerUp;
        }
    }
}