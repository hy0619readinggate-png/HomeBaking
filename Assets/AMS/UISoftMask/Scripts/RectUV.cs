using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

namespace AMS.UI.SoftMask
{
#if UNITY_EDITOR
    using UnityEditor;
    using static UISoftMaskUtils;
#endif
    [ExecuteAlways, RequireComponent(typeof(RectTransform))]
    public partial class RectUV : MonoBehaviour
    {
        private Canvas m_Canvas = null;
        public Canvas canvas => m_Canvas ?? (m_Canvas = rectTransform.GetComponentsInParent<Canvas>() is Canvas[] canvasArray && canvasArray.Length > 0 ? canvasArray.First() : null);

        private RectTransform m_CanvasTransform = null;
        public RectTransform canvasTransform => m_CanvasTransform ?? (m_CanvasTransform = !canvas ? null : (RectTransform)canvas.transform);

        private RectTransform m_RectTransform = null;
        public RectTransform rectTransform => m_RectTransform ?? (m_RectTransform = GetComponent<RectTransform>());

        private Vector4 m_RectParams = default; //xy: size | zw:canvasSize

        private Matrix4x4 m_WorldCanvasMatrix = Matrix4x4.identity;
        private Matrix4x4 m_OverlayCanvasMatrix = Matrix4x4.identity;

        private readonly int m_RectParamsID = Shader.PropertyToID("_RectParams");
        private readonly int m_WorldCanvasMatrixID = Shader.PropertyToID("_WorldCanvasMatrix");
        private readonly int m_OverlayCanvasMatrixID = Shader.PropertyToID("_OverlayCanvasMatrix");
        internal const string k_WORLDCANVAS = "_WORLDCANVAS";

        private RectProperties m_RectProperties = new RectProperties();

        [Serializable]
        private class RectProperties
        {
            public Vector3 pos = Vector3.zero;
            public float angle = default;
            public Vector2 size = Vector2.zero;
            public Transform parent = null;
            public int childsCount = 0;
        }

        internal delegate void OnBeginContextRendering(List<Camera> cameras);
        internal OnBeginContextRendering m_OnBeginContextRendering = null;
        internal bool m_UsingBuiltInRP = false;

        internal void OnEnable()
        {
            CheckCurrentRenderPipeline(GraphicsSettings.defaultRenderPipeline);
            RenderPipelineManager.activeRenderPipelineAssetChanged += RenderPipelineAssetChanged;
        }

        private void RenderPipelineAssetChanged(RenderPipelineAsset from, RenderPipelineAsset to)
        {
            CheckCurrentRenderPipeline(to);
        }

        private void CheckCurrentRenderPipeline(bool hasRenderPipeline)
        {
            if (hasRenderPipeline)
            {
#if UNITY_EDITOR
                m_SceneViewCameraHandle = null;
#endif
                m_UsingBuiltInRP = false;
                RenderPipelineManager.beginContextRendering += BeginContextRendering;
            }
            else
            {
#if UNITY_EDITOR
                if (!m_UsingBuiltInRP)
                    SceneView.duringSceneGui += BeforeSceneGUI;
#endif
                m_UsingBuiltInRP = true;
            }
        }

        internal void OnDisable()
        {
            RenderPipelineManager.activeRenderPipelineAssetChanged -= RenderPipelineAssetChanged;
            m_OnBeginContextRendering = null;
            RenderPipelineManager.beginContextRendering -= BeginContextRendering;
            m_UsingBuiltInRP = false;
#if UNITY_EDITOR
            SceneView.duringSceneGui -= BeforeSceneGUI;
            if (m_SceneViewCameraHandle)
                m_SceneViewCameraHandle.softMaskRectList.Remove(this);
#endif
        }

        private void BeginContextRendering(ScriptableRenderContext context, List<Camera> cameras)
        {
            m_OnBeginContextRendering?.Invoke(cameras);
        }

#if UNITY_EDITOR
        private SceneViewHandle m_SceneViewCameraHandle = null;

        private void BeforeSceneGUI(SceneView sceneView)
        {
            //if (m_UsingBuiltInRP)
            //    sceneView.SetSceneViewShaderReplace(UISoftMaskUtils.s_SceneViewSoftMaskShader, "UISoftMask");
            if (m_UsingBuiltInRP)
            {
                var camObj = sceneView.camera.gameObject;
                if (camObj.GetComponent<SceneViewHandle>() is SceneViewHandle softMaskViewHandle)
                    m_SceneViewCameraHandle = softMaskViewHandle;
                else
                    m_SceneViewCameraHandle = camObj.AddComponent<SceneViewHandle>();

                if (m_SceneViewCameraHandle && !m_SceneViewCameraHandle.softMaskRectList.Contains(this))
                    m_SceneViewCameraHandle.softMaskRectList.Add(this);
            }
            else
                SceneView.beforeSceneGui -= BeforeSceneGUI;
        }
#endif

        private void UpdateWorldRectParams()
        {
            var canvasTransform = this.canvasTransform;

            if (!canvasTransform)
                return;

            var transform = rectTransform;
            Rect rect = transform.rect;

            var canvasSize = canvasTransform.rect.size;
            var rectSize = rect.size;

            m_RectParams.x = rectSize.x;
            m_RectParams.y = rectSize.y;
            m_RectParams.z = canvasSize.x;
            m_RectParams.w = canvasSize.y;

            var bottomLeftCornerPos = transform.TransformPoint(new Vector3(rect.x, rect.y, 0f));
            m_WorldCanvasMatrix = Matrix4x4.TRS(bottomLeftCornerPos, rectTransform.rotation, Vector3.Scale(transform.localScale, canvasTransform.localScale));
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                bottomLeftCornerPos = canvasTransform.InverseTransformPoint(bottomLeftCornerPos); //Convert point to canvasMatrix
                m_OverlayCanvasMatrix = Matrix4x4.TRS(bottomLeftCornerPos, rectTransform.rotation, transform.localScale);
            }
        }

        /// <summary>
        /// Return true if rectUV has changed.
        /// </summary>
        /// <returns>bool</returns>
        public bool HasChangedRectUV()
        {
            var rect = rectTransform;
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

            bool changed =
                Vector3.Distance(transform.position, m_RectProperties.pos) > 0 ||
                rectTransform.eulerAngles.z != m_RectProperties.angle ||
                (transform.parent != m_RectProperties.parent) ||
                m_RectProperties.childsCount != transform.childCount ||
                m_RectProperties.size != rect.sizeDelta;

            if (changed)
            {
                UpdateWorldRectParams();

                m_RectProperties.parent = transform.parent;
                m_RectProperties.angle = transform.eulerAngles.z;
                m_RectProperties.pos = transform.position;
                m_RectProperties.size = rect.sizeDelta;
                m_RectProperties.childsCount = transform.childCount;
            }

            return changed;
        }

        /// <summary>
        /// Set material mask params;
        /// </summary>
        /// <param name="material"></param>
        public void SetMaterialRectParams(Material material)
        {
            if (!material)
                return;

            material.SetVector(m_RectParamsID, m_RectParams);
            material.SetMatrix(m_WorldCanvasMatrixID, m_WorldCanvasMatrix);
            material.SetMatrix(m_OverlayCanvasMatrixID, m_OverlayCanvasMatrix);
        }
    }
}