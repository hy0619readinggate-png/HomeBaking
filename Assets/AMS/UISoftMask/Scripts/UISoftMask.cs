using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AMS.UI.SoftMask
{
    using static UISoftMaskUtils;

    [ExecuteAlways]
    public class UISoftMask : RectUV
    {
        [SerializeField]
        private Sprite m_Mask = null;
        public Sprite mask { get { return m_Mask; } set { m_Mask = value; m_UpdateMaskSetup = true; } }

        [SerializeField]
        private MaskSize m_MaskSize = MaskSize._128;
        public MaskSize maskSize { get { return m_MaskSize; } set { m_MaskSize = value; m_UpdateMaskSetup = true; } }

        [SerializeField, Range(0, 1)]
        private float m_FallOff = 1;
        public float fallOff { get { return m_FallOff; } set { m_FallOff = value; m_UpdateMaskSetup = true; } }
        [SerializeField, Range(0, 1)]
        private float m_Opacity = 1;
        public float opacity { get { return m_Opacity; } set { m_Opacity = value; m_UpdateMaskSetup = true; } }

        [SerializeField, Tooltip("Use this to override the temporary UISoftMask material with a material asset from your project. Note: Its required an unique material per mask and shader must be compatible with UISoftMask.")]
        private Material m_OverrideMaskMaterial = null;
        private bool m_UsingOverrideMaterial = false;
        /// <summary>
        ///  Use this to override the temporary UISoftMask material with a material asset from your project. Note: Its required an unique material per mask and shader must be compatible with UISoftMask.
        /// </summary>
        public Material overrideMaskMaterial
        {
            get
            {
                var usingOverride = m_OverrideMaskMaterial ? true : false;

                if (m_UsingOverrideMaterial != usingOverride)
                {
                    m_UsingOverrideMaterial = usingOverride;
                    m_MaskableObjects.ForEach(g => { if (g.material == m_TargetMaterial) g.material = null; });

                    if (m_TargetMaterial && !usingOverride && !m_TargetMaterial.name.StartsWith(k_SoftMaskMatTag))
                        m_TargetMaterial = null;

                    m_UpdateMaskSetup = true;
                }

                return m_OverrideMaskMaterial;
            }
            set
            {
                m_OverrideMaskMaterial = value;
                m_UpdateMaskSetup = true;
            }
        }

        private RenderTexture m_MaskForRenderingRT = null;

        [Space(5)]

        [SerializeField, HideInInspector]
        private List<MaskableGraphic> m_MaskableObjects = new List<MaskableGraphic>();
        public List<MaskableGraphic> maskableObjects => m_MaskableObjects;

        [SerializeField, HideInInspector]
        private List<Material> m_ExternalMaterials = new List<Material>();
        public List<Material> extraMaterials => m_ExternalMaterials;

        private Material m_SoftMaskBlitMaterial = null;

        private Material m_TargetMaterial = null;
        private Material m_TempMaterial = null;

        private Dictionary<TMP_Asset, Material> m_TMPFontAssets = new Dictionary<TMP_Asset, Material>();

        [SerializeField, HideInInspector]
        private List<Material> m_FontMaterials = new List<Material>();

        public static UISoftMaskUtils utils = null;

        private bool m_UpdateMaskSetup = false;

        public enum MaskSize
        {
            _32 = 32,
            _64 = 64,
            _128 = 128,
            _256 = 256,
            _512 = 512,
            _1024 = 1024,
            _2048 = 2048,
            _4096 = 4096
        }

        private void Awake() => ResetSetup();
        private void Reset() => ResetSetup();

        private new void OnEnable()
        {
            base.OnEnable();
            m_OnBeginContextRendering = OnBeginFrameRendering;
            UpdateMaskSetup();
        }

        private new void OnDisable()
        {
            base.OnDisable();

            if (m_TargetMaterial)
                m_TargetMaterial.DisableKeyword(k_USE_SOFTMASK);

            if (m_ExternalMaterials != null)
                foreach (var externalMaterial in m_ExternalMaterials)
                    externalMaterial.DisableKeyword(k_USE_SOFTMASK);

            if (m_MaskForRenderingRT)
            {
                if (RenderTexture.active == m_MaskForRenderingRT)
                    RenderTexture.active = null;
                DestroyImmediate(m_MaskForRenderingRT);
            }

            ResetSetup();
        }

        private void OnValidate()
        {
            if (enabled)
                UpdateMaskSetup();
        }

        private void Update()
        {
            if (m_UpdateMaskSetup)
            {
                UpdateMaskSetup();
                m_UpdateMaskSetup = false;
            }
        }

        private void LateUpdate()
        {
            if (HasChangedRectUV())
                ComputeFinalMaskForRendering();

            CheckMaskableObjects();
            CheckMaskableObjectsMaterial();
            UpdateMaterials();
        }

        private void UpdateMaterials()
        {
            if (m_TargetMaterial)
                UpdateMaterial(m_TargetMaterial);

            m_FontMaterials.ForEach(fontMaterial => UpdateMaterial(fontMaterial));
            m_ExternalMaterials.ForEach(externalMaterial => UpdateMaterial(externalMaterial));

            if (m_UsingBuiltInRP)
            {
                if (canvas && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    SetOverlayCanvasMaterials();
                else
                    SetWorldCanvasMaterials();
            }
        }

        internal void SetWorldCanvasMaterials()
        {
            if (m_TargetMaterial)
                m_TargetMaterial.EnableKeyword(k_WORLDCANVAS);

            m_FontMaterials.ForEach(fontMaterial => { if (fontMaterial) fontMaterial.EnableKeyword(k_WORLDCANVAS); });
            m_ExternalMaterials.ForEach(externalMaterial => { if (externalMaterial) externalMaterial.EnableKeyword(k_WORLDCANVAS); });
            m_ExternalMaterials.ForEach(externalMaterial => { if (externalMaterial) externalMaterial.EnableKeyword(k_WORLDCANVAS); });
        }

        internal void SetOverlayCanvasMaterials()
        {
            if (m_TargetMaterial)
                m_TargetMaterial.DisableKeyword(k_WORLDCANVAS);

            m_FontMaterials.ForEach(fontMaterial => { if (fontMaterial) fontMaterial.DisableKeyword(k_WORLDCANVAS); });
            m_ExternalMaterials.ForEach(externalMaterial => { if (externalMaterial) externalMaterial.DisableKeyword(k_WORLDCANVAS); });
            m_ExternalMaterials.ForEach(externalMaterial => { if (externalMaterial) externalMaterial.DisableKeyword(k_WORLDCANVAS); });
        }

        private void OnBeginFrameRendering(List<Camera> cameras)
        {
            if (canvas && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                SetOverlayCanvasMaterials();
#if UNITY_EDITOR
            foreach (Camera cam in cameras)
                if (cam.cameraType == CameraType.SceneView)
                    SetWorldCanvasMaterials();
#else
            else
                SetWorldCanvasMaterials();
#endif
        }

        private void UpdateMaterial(Material material)
        {
            if (!material)
                return;

            material.SetTexture(s_SoftMaskID, m_MaskForRenderingRT);
            SetMaterialRectParams(material);
            material.EnableKeyword(k_USE_SOFTMASK);
        }

        /// <summary>
        /// Force update mask setup.
        /// </summary>
        public void UpdateMaskSetup()
        {
            CheckMaskableObjects();
            CheckTargetMaterial();
            CheckMaskableObjectsMaterial();
            ComputeFinalMaskForRendering();
        }

        private void CheckRenderingMaskSetup()
        {
            if (!m_SoftMaskBlitMaterial)
            {
                if (!s_SoftMaskBlitShader)
                    s_SoftMaskBlitShader = Shader.Find(k_SoftMaskBlitShader);

                m_SoftMaskBlitMaterial = new Material(s_SoftMaskBlitShader);
                m_SoftMaskBlitMaterial.name = $"SoftMaskBlit [{m_SoftMaskBlitMaterial.GetInstanceID()}]";
            }

            int selectedSize = (int)m_MaskSize;

            if (!m_MaskForRenderingRT)
            {
                m_MaskForRenderingRT = new RenderTexture(selectedSize, selectedSize, 0);
                m_MaskForRenderingRT.name = $"SoftMask [{m_MaskForRenderingRT.GetInstanceID()}]";
                m_MaskForRenderingRT.format = RenderTextureFormat.R16;
            }
            else if (m_MaskForRenderingRT && m_MaskForRenderingRT.width != selectedSize)
            {
                if (RenderTexture.active == m_MaskForRenderingRT)
                    RenderTexture.active = null;

                m_MaskForRenderingRT.Release();
                m_MaskForRenderingRT.width = selectedSize;
                m_MaskForRenderingRT.height = selectedSize;
            }
        }

        private void CheckMaskableObjects()
        {
            var maskableGraphicChildren = GetComponentsInChildren<MaskableGraphic>(true).ToList();

            m_MaskableObjects.ForEach(m =>
            {
                if (m && !maskableGraphicChildren.Contains(m) && m.material == m_TargetMaterial)
                    m.material = null;
            });

            m_MaskableObjects = new List<MaskableGraphic>(maskableGraphicChildren);

            var childMasks = GetComponentsInChildren<UISoftMask>().ToList();
            childMasks.Remove(this);

            var maskedObjects = new List<MaskableGraphic>();
            foreach (var mask in childMasks)
                maskedObjects.AddRange(mask.maskableObjects);

            maskableGraphicChildren.ForEach(maskableObj =>
            {
                if (!maskableObj.maskable || maskedObjects.Contains(maskableObj))
                {
                    m_MaskableObjects.Remove(maskableObj);
                    if (maskableObj.material == m_TargetMaterial)
                        maskableObj.material = null;
                }

                if (maskableObj is TMP_Text textMesh && textMesh.font)
                {
                    if (maskableObj.maskable)
                    {
                        if (!m_TMPFontAssets.ContainsKey(textMesh.font) && textMesh.font.material)
                        {
                            var fontMaterialInstance = new Material(textMesh.font.material);
                            fontMaterialInstance.name = $"SoftMaskFontMat [{fontMaterialInstance.GetInstanceID()}]";
                            m_TMPFontAssets.Add(textMesh.font, fontMaterialInstance);
                        }
                    }
                    else
                        textMesh.fontSharedMaterial = textMesh.font.material;
                }
            });

            m_FontMaterials = m_TMPFontAssets.Select(m => m.Value).ToList();
        }

        private void ComputeFinalMaskForRendering()
        {
            if (!m_Mask)
                return;

            var textureMask = m_Mask.texture;
            var textureSize = Vector2.one / new Vector2(textureMask.width, textureMask.height);
            var spriteOffset = new Vector2(m_Mask.textureRect.xMin, m_Mask.textureRect.yMin);

            Vector4 textureMaskOffset = Vector4.one;
            textureMaskOffset.x = m_Mask.textureRect.width * textureSize.x;
            textureMaskOffset.y = m_Mask.textureRect.height * textureSize.y;
            textureMaskOffset.z = spriteOffset.x * textureSize.x;
            textureMaskOffset.w = spriteOffset.y * textureSize.y;

            CheckRenderingMaskSetup();

            var masks = GetComponentsInParent<UISoftMask>().ToList();

            m_SoftMaskBlitMaterial.SetFloat(s_FalloffID, m_FallOff);
            m_SoftMaskBlitMaterial.SetFloat(s_OpacityID, m_Opacity);
            m_SoftMaskBlitMaterial.SetVector(s_SpriteAtlasOffsetID, textureMaskOffset);

            if (masks.Count >= 2)
            {
                masks.Remove(this);
                if (masks.First() is UISoftMask parentMask && parentMask.enabled)
                {
                    var parentRect = parentMask.rectTransform;
                    var maskSize = rectTransform.rect.size * rectTransform.localScale;

                    var parentMatrix = parentRect.worldToLocalMatrix;
                    var maskCenterPos = parentMatrix * rectTransform.TransformPoint(rectTransform.rect.center);
                    var parentMaskCenterPos = parentMatrix * parentRect.TransformPoint(parentRect.rect.center);

                    Vector2 offset = (parentMaskCenterPos - maskCenterPos) / maskSize;
                    Vector2 scale = maskSize / parentRect.rect.size;

                    m_SoftMaskBlitMaterial.SetTexture(s_OverridingMaskID, parentMask.m_MaskForRenderingRT);
                    m_SoftMaskBlitMaterial.SetVector(s_OffsetID, offset);
                    m_SoftMaskBlitMaterial.SetVector(s_ScaleID, scale);
                    var angle = parentRect.eulerAngles.z - rectTransform.eulerAngles.z;
                    m_SoftMaskBlitMaterial.SetFloat(s_AngleID, Mathf.Deg2Rad * angle);
                    Graphics.Blit(textureMask, m_MaskForRenderingRT, m_SoftMaskBlitMaterial);
                    goto Computed;
                }
            }

            m_SoftMaskBlitMaterial.SetTexture(s_OverridingMaskID, Texture2D.whiteTexture);
            m_SoftMaskBlitMaterial.SetVector(s_OffsetID, Vector2.zero);
            m_SoftMaskBlitMaterial.SetVector(s_ScaleID, Vector2.one);
            m_SoftMaskBlitMaterial.SetFloat(s_AngleID, 0);
            Graphics.Blit(textureMask, m_MaskForRenderingRT, m_SoftMaskBlitMaterial);

        Computed:
            var childMasks = GetComponentsInChildren<UISoftMask>().ToList();
            childMasks.Remove(this);
            if (childMasks.Count > 0)
                childMasks.First().ComputeMaskOnChain();
        }

        private void ComputeMaskOnChain()
        {
            CheckMaskableObjects();
            ComputeFinalMaskForRendering();
        }

        private void CheckTargetMaterial()
        {
            if (overrideMaskMaterial is Material overrideMaterial && overrideMaterial && MaterialHasSoftMask(overrideMaterial) && (!m_TargetMaterial || m_TargetMaterial != overrideMaterial))
                m_TargetMaterial = overrideMaterial;
            else
            {
                if (s_SoftMaskShader is Shader defaultShader)
                {
                    if (!m_TargetMaterial)
                    {
                        if (!m_TempMaterial)
                        {
                            m_TempMaterial = new Material(defaultShader);
                            m_TempMaterial.name = $"{k_SoftMaskMatTag}{m_TempMaterial.GetInstanceID()}]";
                            m_TempMaterial.hideFlags = HideFlags.DontSaveInEditor;
                        }

                        m_TargetMaterial = m_TempMaterial;
                    }
                    else if (m_TargetMaterial && m_TargetMaterial.name.Contains(k_SoftMaskMatTag) && m_TargetMaterial.shader != defaultShader)
                        m_TargetMaterial.shader = defaultShader;
                }
            }
        }

        private void CheckMaskableObjectsMaterial()
        {
            m_ExternalMaterials.Clear();
            m_MaskableObjects.ForEach(maskableObj =>
            {
                if (maskableObj is TMP_Text textMesh && textMesh.font && m_TMPFontAssets.TryGetValue(textMesh.font, out Material fontSharedMaterial))
                {
                    if (fontSharedMaterial is Material && MaterialHasSoftMask(fontSharedMaterial))
                        textMesh.fontSharedMaterial = fontSharedMaterial;
                }
                else if (maskableObj.material is Material material)
                {
                    if (material == maskableObj.defaultMaterial) //If Unity UI shader
                        maskableObj.material = m_TargetMaterial;
                    else if (material != m_TargetMaterial && MaterialHasSoftMask(material))
                    {
                        if (m_ExternalMaterials != null && !m_ExternalMaterials.Contains(material))
                            m_ExternalMaterials.Add(material);
                    }
                }
            });
        }

        private bool MaterialHasSoftMask(Material targetMaterial) => targetMaterial && targetMaterial.HasProperty(s_SoftMaskID);

        private void ResetSetup()
        {
            if (m_MaskableObjects != null && m_MaskableObjects.Count > 0)
            {
                foreach (var maskableObj in m_MaskableObjects)
                    if (maskableObj.material && (maskableObj.material == m_TargetMaterial || maskableObj.material.name.StartsWith(k_SoftMaskMatTag)))
                        maskableObj.material = null;

                m_MaskableObjects.Clear();
            }
        }
    }
}