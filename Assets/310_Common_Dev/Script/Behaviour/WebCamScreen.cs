using beyondi.Util;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Common
{
    [RequireComponent(typeof(RawImage))]
    [RequireComponent(typeof(AspectRatioFitter))]
    public class WebCamScreen : MonoBehaviour
    {
        // Properties
        public bool IsReady => webCamTexture != null;
        public bool IsPlaying => webCamTexture != null && webCamTexture.isPlaying;
        public float AspectRatio => aspectRatio;

        // Methods
        public bool Init()
        {
            LOG.Function(this);

            var devices = WebCamTexture.devices;
            if (devices.Length > 0)
            {
                webCamDevice =
                    WebCamTexture.devices.Any(d => d.isFrontFacing == isFrontFacing)
                    ? WebCamTexture.devices.First(d => d.isFrontFacing == isFrontFacing)
                    : WebCamTexture.devices.First();

                webCamTexture = new WebCamTexture(webCamDevice.name, requestWidth, requestHeight);
                image.texture = webCamTexture;

                return true;
            }
            else
            {
                LOG.Warning($"Failed to initialize the webcam", this);
                return false;
            }
        }
        public void PlayWebCam()
        {
            LOG.Function(this);

            if (IsReady)
                webCamTexture.Play();
            else LOG.Warning($"Not ready to play", this);
        }
        public void StopWebCam()
        {
            LOG.Function(this);

            if (IsReady)
                webCamTexture.Stop();
        }
        public Texture2D CapturedTexture()
        {
            LOG.Function(this);

            if (IsPlaying)
            {
                var angle = webCamTexture.videoRotationAngle;
                var isFront = webCamDevice.isFrontFacing;

                var texture = new Texture2D(webCamTexture.width, webCamTexture.height);
                texture.SetPixels(webCamTexture.GetPixels());

                if (angle == 180) texture = texture.Rotate180();
                if (isFront) texture = texture.FlipHorizontal();

                return texture;
            }
            else return null;
        }



        // Fields : caching
        private RawImage image_ = null;
        private RawImage image => image_ ??= GetComponent<RawImage>();
        private AspectRatioFitter fitter_ = null;
        private AspectRatioFitter fitter => fitter_ ??= GetComponent<AspectRatioFitter>();

        // Fields
        private WebCamTexture webCamTexture = null;
        private WebCamDevice webCamDevice;
        private float aspectRatio = 1;


        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private bool isFrontFacing = true;
        [SerializeField] private int requestWidth = 0;
        [SerializeField] private int requestHeight = 0;

        // Unity Messages
        private void Awake()
        {
            fitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
        }
        private void Start()
        {
        }
        private void Update()
        {
            if (IsReady)
            {
                var angle = webCamTexture.videoRotationAngle;
                var isMirror = webCamTexture.videoVerticallyMirrored;
                var isFront = webCamDevice.isFrontFacing;

                image.transform.localEulerAngles = new Vector3(0, 0, -angle);

                var scaleX = isFront ? -1 : +1;
                var scaleY = isMirror ? -1 : +1;
                image.transform.localScale = new Vector3(scaleX, scaleY, 1);

                aspectRatio = (float)webCamTexture.width / webCamTexture.height;
                fitter.aspectRatio = aspectRatio;
            }
        }
    }
}