using beyondi.Util;
using DoDoEng.Common;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using XDPaint;
using XDPaint.Controllers;
using XDPaint.Core.PaintObject.Data;

namespace DoDoEng.Activity.C1_A04
{
    public class PaintBoard : MonoBehaviour
    {
        // Properties
        public Texture2D UserImage => paintMGR.GetResultTexture();


        // Methods
        public void Setup(GameObject pb)
        {
            LOG.Info($"Setup()", this);

            Util.RemoveAllChildren(prefabParentTR);

            var go = Instantiate(pb, prefabParentTR);
            layerPickups = go.GetComponentsInChildren<PaintLayerPickup>();
            layerPickups.ForEach(p => p.OnPointerDown += layerPickup_OnPointerDown);
            layerPickups.ForEach(p => p.OnPointerUp += layerPickup_OnPointerUp);

            var lineGO = go.transform.GetChild(go.transform.childCount - 1);
            lineGO.gameObject.SetActive(false);

            var sprite = lineGO.GetComponent<Image>().mainTexture;
            var layer = paintMGR.LayersController.ActiveLayer;


            // paintMGR.StatesController.GetUndoActionsCount를 증가시킨다
            paintMGR.LayersController.AddNewLayer("line", sprite);
            paintMGR.LayersController.SetActiveLayer(layer);

        }
        public void EnableInteraction(bool enable)
        {
            PaintController.Instance.GetComponent<InputController>().enabled = enable;
            undoBTN.interactable = enable;
        }
        public void ChangeTool(ToolType toolType, BrushInfo toolInfo)
        {
            LOG.Info($"ChangeTool() | {toolType}, {toolInfo}", this);

            currentToolType = toolType;

            var brush = PaintController.Instance.Brush;
            brush.SourceTexture = toolInfo.SourceTexture;
            brush.FilterMode = toolInfo.FilterMode;
            brush.SetColor(toolInfo.Color);
            brush.Size = toolInfo.Size;
            brush.Hardness = toolInfo.Hardness;
            brush.RenderAngle = toolInfo.RenderAngle + (toolInfo.RandomRenderAngle ? UnityEngine.Random.Range(0, 360) : 0);
            PaintController.Instance.Brush = brush;
            //PaintController.Instance.Tool = toolInfo.PaintTool;

            if (paintMGR.ToolsManager.CurrentTool != null)
                paintMGR.ToolsManager.CurrentTool.BaseSettings.CanPaintLines = toolType != ToolType.Sticker;
        }

        // Events
        public event Action OnPaintStarted;
        public event Action OnPaintEnded;


        // Fields : caching
        private PaintManager paintMGR_ = null;
        private PaintManager paintMGR => paintMGR_ ??= GetComponent<PaintManager>();

        // Fields
        private PaintLayerPickup[] layerPickups = null;
        private ToolType currentToolType = ToolType.NA;
        private bool maskLayerUsed = false;

        // Event Handlers
        private void layerPickup_OnPointerDown(PaintLayerPickup sender)
        {
            LOG.Info($"layerPickup_OnPointerDown() | {sender.gameObject.name}", this);

            if (currentToolType == ToolType.NA)
                return;

            if (maskLayerUsed)
                return;

            if (currentToolType == ToolType.Eraser)
                return;

            paintMGR.StatesController.EnableGrouping(true);
            var layer = paintMGR.LayersController.AddNewLayer("current");
            layer.AddMask(sender.MaskTexture, RenderTextureFormat.R8);
            layer.MaskEnabled = true;

            maskLayerUsed = true;
        }
        private void layerPickup_OnPointerUp(PaintLayerPickup sender)
        {
            LOG.Info($"layerPickup_OnPointerUp() | {sender.gameObject.name}", this);

            if (maskLayerUsed)
                StartCoroutine(coMergeLayer());
        }
        private void paintMGR_OnUndoStatusChanged(bool enable)
        {
            var undoActionCount = paintMGR.StatesController.GetUndoActionsCount();

            undoBTN.gameObject.SetActive(undoActionCount > 4);
        }
        private void paintObject_OnDrawPoint(DrawPointData drawPointData)
        {
            LOG.Info($"paintObject_OnDrawPoint()", this);

            var clips = currentToolType switch
            {
                ToolType.Brush => brushCLIP,
                ToolType.Pencil => penCLIP,
                ToolType.Eraser => eraserCLIP,
                ToolType.Sticker => stickerCLIP,
                _ => brushCLIP
            };
            var clip = UtilArray.ExtractOne(clips);
            AudioMGR.One.PlayEffectLL(clip, currentToolType != ToolType.Sticker);

            OnPaintStarted?.Invoke();
        }
        private void paintObject_OnPointerUp(PointerUpData paintData)
        {
            LOG.Info($"paintObject_OnPointerUp()", this);

            if (currentToolType != ToolType.Sticker)
                AudioMGR.One.StopEffectLL();

            OnPaintEnded?.Invoke();
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Transform prefabParentTR = null;
        [SerializeField] private Button undoBTN = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip[] stickerCLIP = null;
        [SerializeField] private AudioClip[] penCLIP = null;
        [SerializeField] private AudioClip[] eraserCLIP = null;
        [SerializeField] private AudioClip[] brushCLIP = null;

        // Unity Messages
        private void Awake()
        {
            undoBTN.gameObject.SetActive(false);
            undoBTN.onClick.AddListener(() => paintMGR.StatesController.Undo());
        }
        private IEnumerator Start()
        {
            yield return null;

            // PaintManager 초기화 대기
            paintMGR.StatesController.OnUndoStatusChanged += paintMGR_OnUndoStatusChanged;

            paintMGR.PaintObject.OnDrawPoint += paintObject_OnDrawPoint;
            paintMGR.PaintObject.OnPointerUp += paintObject_OnPointerUp;
        }
        private void OnDestroy()
        {
            if (layerPickups != null)
                layerPickups.ForEach(p => p.OnPointerDown -= layerPickup_OnPointerDown);

            paintMGR.StatesController.OnUndoStatusChanged -= paintMGR_OnUndoStatusChanged;

            paintMGR.PaintObject.OnDrawPoint -= paintObject_OnDrawPoint;
            paintMGR.PaintObject.OnPointerUp -= paintObject_OnPointerUp;
        }



        // Unity Coroutines
        IEnumerator coMergeLayer()
        {
            using (LOG.Coroutine($"coMergeLayer()", this))
            {
                var cg = GetComponentInParent<CanvasGroup>();
                cg.blocksRaycasts = false;
                InputController.Instance.EnableInteraction(false);
                layerPickups.ForEach(l => l.EnableInteraction(false));
                yield return null;

                paintMGR.LayersController.MergeLayers();
                paintMGR.StatesController.DisableGrouping(true);
                yield return null;

                cg.blocksRaycasts = true;

                maskLayerUsed = false;
                layerPickups.ForEach(l => l.EnableInteraction(true));
                InputController.Instance.EnableInteraction(true);
                yield return null;
            }
        }
    }
}