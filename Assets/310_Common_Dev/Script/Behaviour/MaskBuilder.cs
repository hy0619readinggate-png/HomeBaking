using beyondi.Util;
using DG.Tweening;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DoDoEng.Common
{
    [RequireComponent(typeof(RawImage))]
    public class MaskBuilder : MonoBehaviour
    {
        // Definitions
        public enum Mode { Erase, Drawing }

        // Properties
        public float Progress { get; private set; }

        // Methods
        public void Setup(Mode mode, Texture2D brushTexture, float brushScale = 1)
        {
            LOG.Info($"Setup() | {mode}", this);

            this.mode = mode;
            this.brushTexture = brushTexture;
            this.brushScale = brushScale;
        }
        public void Draw(PointerEventData eventData)
        {
            LOG.Info($"Draw() | {eventData}", this);

            var ptLocal = UtilTransform.ScreenToLocal(eventData.position, rt, canvas);
            draw(ptLocal);
            updateProgress();
        }
        public void Draw(Vector2 position)
        {
            LOG.Info($"Draw() | {position}", this);

            var ptLocal = UtilTransform.ScreenToLocal(position, rt, canvas);
            draw(ptLocal);
            updateProgress();
        }
        public void DrawAll()
        {
            LOG.Info($"DrawAll()", this);

            drawAll();
        }

        // Fields : caching
        private RawImage rawImage_ = null;
        private RawImage rawImage => rawImage_ ??= GetComponent<RawImage>();
        private Canvas canvas_ = null;
        private Canvas canvas => canvas_ ??= GetComponentInParent<Canvas>();
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();

        // Fields
        private RenderTexture renderTexture = null;
        [SerializeField] private Material brushMaterial = null;
        private float totalPixelCount = 0;
        private int initialAlphaPixelCount = 0;
        private bool isCooldown = false;
        private bool needUpdateProgress = false;

        // Functions
        private void draw(Vector2 ptLocal)
        {
            LOG.Info($"draw() | {ptLocal}", this);

            var ptTexture = ptLocal;
            ptTexture += new Vector2(
                    rt.rect.width * rt.pivot.x,
                    rt.rect.height * rt.pivot.y);
            ptTexture.y = rt.rect.height - ptTexture.y;

            RenderTexture.active = renderTexture;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, renderTexture.width, renderTexture.height, 0);

            Graphics.DrawTexture(
            new Rect(
                ptTexture.x - brushTexture.width * 0.5f * brushScale,
                ptTexture.y - brushTexture.height * 0.5f * brushScale,
                brushTexture.width * brushScale,
                brushTexture.height * brushScale),
            brushTexture, brushMaterial);

            GL.PopMatrix();
            RenderTexture.active = null;
        }
        private void drawAll()
        {
            LOG.Info($"drawAll()", this);

            RenderTexture.active = renderTexture;
            GL.PushMatrix();
            GL.LoadPixelMatrix(0, renderTexture.width, renderTexture.height, 0);

            Graphics.DrawTexture(
            new Rect(
                0, 0, renderTexture.width, renderTexture.height),
            new Texture2D(1, 1), brushMaterial);

            GL.PopMatrix();
            RenderTexture.active = null;
        }

        // Function
        private void updateProgress()
        {
            if (!isCooldown)
            {
                isCooldown = true;
                needUpdateProgress = false;
                DOVirtual.DelayedCall(cooldownDuration, () =>
                {
                    isCooldown = false;
                    if (needUpdateProgress)
                        updateProgress();
                });

                var alphaCount = getAlphaPixelCount();
                Progress = mode == Mode.Drawing
                    ? (initialAlphaPixelCount - alphaCount) / (float)initialAlphaPixelCount
                    : (alphaCount - initialAlphaPixelCount) / (float)(totalPixelCount - initialAlphaPixelCount);

                LOG.Info($"Progress - {Progress}", this);
            }
            else needUpdateProgress = true;
        }
        private int getAlphaPixelCount()
        {
            return renderTexture.ToTexture2D().GetPixels().Count(p => p.a != 1);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Texture2D brushTexture = null;
        [SerializeField] private float brushScale = 1;
        [Header("★ Configs")]
        [SerializeField] private Mode mode = Mode.Drawing;
        [SerializeField] private float cooldownDuration = 1f;

        // Unity Messages
        private void Awake()
        {
            rawImage.raycastTarget = false;
        }
        private void Start()
        {
            // Create Texture
            renderTexture = new RenderTexture((int)rt.rect.width, (int)rt.rect.height, 32);

            // Ready for Draw
            Graphics.Blit(rawImage.texture, renderTexture);
            rawImage.texture = renderTexture;

            totalPixelCount = rt.rect.width * rt.rect.height;
            initialAlphaPixelCount = getAlphaPixelCount();

            if (mode == Mode.Erase)
            {
                var shader = Shader.Find("DoDoEng/EraseShader");
                brushMaterial = new Material(shader);

                //var shader = Shader.Find("Transparent/Cutout/Diffuse");
                //var shader = Shader.Find("Standard");
                //brushMaterial = new Material(shader); 

                // Standard 쉐이더를 이용해 투명 머티리얼 만들기
                // https://docs.unity3d.com/Manual/StandardShaderMaterialParameterRenderingMode.html
                //var shader = Shader.Find("Standard");
                //brushMaterial = new Material(shader);
                //brushMaterial.SetOverrideTag("RenderType", "Transparent");
                //brushMaterial.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                //brushMaterial.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                //brushMaterial.SetFloat("_ZWrite", 0.0f);
                //brushMaterial.DisableKeyword("_ALPHATEST_ON");
                //brushMaterial.DisableKeyword("_ALPHABLEND_ON");
                //brushMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            }
        }
    }
}