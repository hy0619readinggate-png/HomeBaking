using DoDoEng.Activity.C4_A04;
using DoDoEng.Common;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Tester
{
    public class TestOrientation : MonoBehaviour
    {
        // Fields
        private WebCamMGR webCamMGR = null;
        private bool isApplyRotation = false;
        private bool isApplyMirror = false;
        private bool isApplyFront = false;
        private bool isApplyCaptureTransform = false;

        // Functions
        private void updateCameraRotation(RawImage rawImage, WebCamTexture webCamTexture)
        {
            var angle = webCamTexture.videoRotationAngle;
            var isMirror = webCamTexture.videoVerticallyMirrored;
            var isFront = webCamMGR.IsFrontFacing;

            var rotationZ = isApplyRotation ? -angle : 0;
            rawImage.transform.localEulerAngles = new Vector3(0, 0, rotationZ);

            var scaleX = isApplyFront && isFront ? -1 : +1;
            var scaleY = isApplyMirror && isMirror ? -1 : +1;
            rawImage.transform.localScale = new Vector3(scaleX, scaleY, 1);
        }

        // Event Handlers
        private void captureBTN_OnClick()
        {
            captureRI.texture = webCamMGR.CapturedTexture(isApplyCaptureTransform);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Text orientationTXT = null;
        [SerializeField] private Button captureBTN = null;
        [SerializeField] private Toggle applyRotationTGL = null;
        [SerializeField] private Toggle applyMirrorTGL = null;
        [SerializeField] private Toggle applyFrontTGL = null;
        [SerializeField] private Toggle applyCaptureTransformTGL = null;
        [SerializeField] private RawImage webCamRI = null;
        [SerializeField] private RawImage captureRI = null;

        // Unity Messages
        private void Awake()
        {
            captureBTN.onClick.AddListener(captureBTN_OnClick);
            applyRotationTGL.onValueChanged.AddListener(isOn => isApplyRotation = isOn);
            applyMirrorTGL.onValueChanged.AddListener(isOn => isApplyMirror = isOn);
            applyFrontTGL.onValueChanged.AddListener(isOn => isApplyFront = isOn);
            applyCaptureTransformTGL.onValueChanged.AddListener(isOn => isApplyCaptureTransform = isOn);

            webCamMGR = new WebCamMGR();
            webCamMGR.InitWebCam(720, 720, true);

            webCamRI.texture = webCamMGR.WebCamTexture;
        }
        private void Start()
        {
            webCamMGR.PlayWebCam();
        }
        private void Update()
        {
            var webCamTexture = webCamMGR.WebCamTexture;
            orientationTXT.text = $"{Input.deviceOrientation} " +
                $"Rotation({webCamTexture.videoRotationAngle}) " +
                $"Mirror({webCamTexture.videoVerticallyMirrored}) " +
                $"Front({webCamMGR.IsFrontFacing})";

            updateCameraRotation(webCamRI, webCamMGR.WebCamTexture);
        }
        private void OnDestroy()
        {
            webCamMGR.StopWebCam();
        }
    }



    public class WebCamMGR
    {
        // Properties
        public WebCamTexture WebCamTexture => webCamTexture;
        public WebCamDevice WebCamDevice => webCamDevice;
        public float WebCamRatio => webCamRatio;
        public bool IsFrontFacing => webCamDevice.isFrontFacing;

        // Methods
        public void InitWebCam(int requestWidth, int requestHeight, bool isFrontFacing = true)
        {
            LOG.Info($"InitWebCam()", this);

            var devices = WebCamTexture.devices;
            if (devices.Length > 0)
                webCamTexture = getWebCamTexture(requestWidth, requestHeight, isFrontFacing);
            else webCamTexture = null;
        }
        public void PlayWebCam()
        {
            LOG.Info($"PlayWebCam()", this);

            if (webCamTexture != null)
            {
                webCamTexture.Play();

                var width = (float)webCamTexture.width;
                var height = (float)webCamTexture.height;
                webCamRatio = width > height ? (width / height) : (height / width);
            }
        }
        public void StopWebCam()
        {
            LOG.Info($"StopWebCam()", this);

            if (webCamTexture != null)
            {
                webCamTexture.Stop();
                webCamTexture = null;
            }
        }
        public Texture2D CapturedTexture(bool applyTransform)
        {
            LOG.Info($"CapturedTexture()", this);

            if (webCamTexture != null && webCamTexture.isPlaying)
            {
                if (!applyTransform)
                {
                    var texture = new Texture2D(webCamTexture.width, webCamTexture.height);
                    texture.SetPixels(webCamTexture.GetPixels());
                    texture.Apply();

                    return texture;
                }
                else
                {
                    var angle = webCamTexture.videoRotationAngle;
                    var isMirror = webCamTexture.videoVerticallyMirrored;
                    var isFront = IsFrontFacing;

                    var texture = new Texture2D(webCamTexture.width, webCamTexture.height);
                    texture.SetPixels(webCamTexture.GetPixels());

                    // ------------------------------
                    // 너무 텍스쳐 할당이 많은데 TT
                    // ------------------------------
                    if (angle == 180) texture = TextureUtil.Rotate180(texture);
                    // Mirror는 고려하지 않음
                    //if (isMirror) texture = TextureUtil.FlipVertical(texture);
                    if (isFront) texture = TextureUtil.FlipHorizontal(texture);

                    return texture;
                }
            }
            else return null;
        }



        // Fields
        private WebCamTexture webCamTexture = null;
        private WebCamDevice webCamDevice;
        private float webCamRatio = 1;

        // Functions
        private WebCamTexture getWebCamTexture(int requestWidth, int requestHeight, bool isFrontFacing)
        {
            webCamDevice =
                WebCamTexture.devices.Any(d => d.isFrontFacing == isFrontFacing)
                ? WebCamTexture.devices.First(d => d.isFrontFacing == isFrontFacing)
                : WebCamTexture.devices.First();

            return new WebCamTexture(webCamDevice.name, requestWidth, requestHeight);

            //if (webCamDevice.availableResolutions != null)
            //{
            //    var res = webCamDevice.availableResolutions[0];
            //    return new WebCamTexture(webCamDevice.name, res.width, res.height);
            //}
            //else return new WebCamTexture(webCamDevice.name);
        }
    }


    public static class TextureUtil
    {
        public static Texture2D Rotate90(Texture2D orig)
        {
            Color32[] origpix = orig.GetPixels32(0);
            Color32[] newpix = new Color32[orig.width * orig.height];
            for (int c = 0; c < orig.height; c++)
            {
                for (int r = 0; r < orig.width; r++)
                {
                    newpix[orig.width * orig.height - (orig.height * r + orig.height) + c] =
                      origpix[orig.width * orig.height - (orig.width * c + orig.width) + r];
                }
            }
            Texture2D newtex = new Texture2D(orig.height, orig.width, orig.format, false);
            newtex.SetPixels32(newpix, 0);
            newtex.Apply();
            return newtex;
        }
        public static Texture2D Rotate180(Texture2D orig)
        {
            Color32[] origpix = orig.GetPixels32(0);
            Color32[] newpix = new Color32[orig.width * orig.height];
            for (int i = 0; i < origpix.Length; i++)
            {
                newpix[origpix.Length - i - 1] = origpix[i];
            }
            Texture2D newtex = new Texture2D(orig.width, orig.height, orig.format, false);
            newtex.SetPixels32(newpix, 0);
            newtex.Apply();
            return newtex;
        }
        public static Texture2D Rotate270(Texture2D orig)
        {
            Color32[] origpix = orig.GetPixels32(0);
            Color32[] newpix = new Color32[orig.width * orig.height];
            int i = 0;
            for (int c = 0; c < orig.height; c++)
            {
                for (int r = 0; r < orig.width; r++)
                {
                    newpix[orig.width * orig.height - (orig.height * r + orig.height) + c] = origpix[i];
                    i++;
                }
            }
            Texture2D newtex = new Texture2D(orig.height, orig.width, orig.format, false);
            newtex.SetPixels32(newpix, 0);
            newtex.Apply();
            return newtex;
        }
        public static Texture2D FlipVertical(Texture2D orig)
        {
            Color32[] origpix = orig.GetPixels32(0);
            Color32[] newpix = new Color32[orig.width * orig.height];

            for (int c = 0; c < orig.height; c++)
            {
                for (int i = 0; i < orig.width; i++)
                {
                    newpix[(orig.width * (orig.height - 1 - c)) + i] = origpix[(c * orig.width) + i];
                }
            }

            Texture2D newtex = new Texture2D(orig.width, orig.height, orig.format, false);
            newtex.SetPixels32(newpix, 0);
            newtex.Apply();
            return newtex;
        }
        public static Texture2D FlipHorizontal(Texture2D orig)
        {
            Color32[] origpix = orig.GetPixels32(0);
            Color32[] newpix = new Color32[orig.width * orig.height];

            for (int c = 0; c < orig.height; c++)
            {
                for (int i = 0; i < orig.width; i++)
                {
                    newpix[(c * orig.width) + (orig.width - 1 - i)] = origpix[(c * orig.width) + i];
                }
            }

            Texture2D newtex = new Texture2D(orig.width, orig.height, orig.format, false);
            newtex.SetPixels32(newpix, 0);
            newtex.Apply();
            return newtex;
        }
    }

}