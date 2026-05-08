using beyondi.Coroutine;
using beyondi.Util;
using DoDoEng.Common;
// using RenderHeads.Media.AVProVideo;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C4_A04
{
    public class SpaceshipStudent : MonoBehaviour
    {
        // Properties
        public ISubmitable[] CheckButtons => new ISubmitable[] { undoBTN, nextBTN };
        public SpaceshipBTN UndoBTN => undoBTN;
        public SpaceshipBTN NextBTN => nextBTN;

        // Methods
        public void Init()
        {
            LOG.Info($"Init()", this);

            baseTXT.text = "";
            baseTXT.gameObject.SetActive(false);
            skinsTXT.ForEach(s => s.text = "");

            activeAllButtons(false);
            capturedImage.gameObject.SetActive(false);
            // videoDisplayUGUI.gameObject.SetActive(false);
        }
        public void Setup(ProblemData pData,  WebCamScreen webCamScreen) //MediaPlayer mediaPlayer - 두번째 매개변수 
        {
            LOG.Info($"Setup()", this);

            wordClip = pData.WordCLIP;
            //videoDisplayUGUI.Player = mediaPlayer;

            baseTXT.text = pData.Word;
            skinsTXT.ForEach(t => t.text = pData.Word);

            this.webCamScreen = webCamScreen;

            webCamScreen.transform.SetParent(webCamTR, false);

            if (!webCamScreen.IsReady)
                noCameraGO.SetActive(true);

            activeAllButtons(true);
        }
        public void EnableInteractionAtCheck(bool enable)
        {
            LOG.Info($"EnableInteractionAtCheck() | {enable}", this);

            undoBTN.EnableInteraction(enable);
            nextBTN.EnableInteraction(enable);
        }
        public void ShowWebcam()
        {
            LOG.Function(this);

            // videoDisplayUGUI.gameObject.SetActive(false);

            activeWebCamToggle(webCamScreen.IsReady);
        }
        public Coroutine StartWaitTakePhoto()
        {
            LOG.Info($"StartWaitTakePhoto()", this);

            captureBTN.EnableInteraction(true);

            crWaitCapture = StartCoroutine(coWaitCapture());
            return crWaitCapture;
        }
        public void StopWaitTakePhoto()
        {
            LOG.Info($"StopWaitTakePhoto()", this);

            this.StopCoroutineSafe(ref crWaitCapture);
            captureBTN.EnableInteraction(false);
        }
        public Coroutine StartWaitCountdown()
        {
            LOG.Info($"StartWaitCountdown()", this);

            capturedImage.gameObject.SetActive(false);
            // videoDisplayUGUI.gameObject.SetActive(false);

            captureBTN.EnableInteraction(true);
            activeWebCamToggle(true);

            crWaitCountdown = StartCoroutine(coWaitCountdown());
            return crWaitCountdown;
        }
        public void StopWaitCountdown()
        {
            LOG.Info($"StopWaitCountdown()", this);

            this.StopCoroutineSafe(ref crWaitCountdown);

            counter.FinishCountDown();

            captureBTN.EnableInteraction(false);
            activeWebCamToggle(false);
        }
        public Coroutine StartCapture()
        {
            LOG.Info($"StartCapture()", this);

            crCapture = StartCoroutine(coCapture());
            return crCapture;
        }
        public void FinishCapture()
        {
            LOG.Info($"FinishCapture()", this);

            this.StopCoroutineSafe(ref crCapture);

            captureAnim.SetTrigger("Normal");
        }
        public void ReadyToDecorate()
        {
            LOG.Info($"ReadyToDecorate()", this);

            baseTXT.gameObject.SetActive(true);
            activeAllButtons(false);
        }
        public void SelectSkin(string color)
        {
            LOG.Info($"SelectSkin() | {color}", this);

            decorateAnim.SetTrigger(color);
            this.color = color;
        }
        public void StartDecorate()
        {
            LOG.Function(this);

            textsBTN.ForEach(b => b.interactable = true);
        }
        public void StopDecorate()
        {
            LOG.Function(this);

            AudioMGR.One.StopNarration();
            this.StopCoroutineSafe(ref crPlaySound);
            textsBTN.ForEach(b => b.interactable = false);
        }

        // Event Handlers
        private void OnTextClicked()
        {
            LOG.Function(this);

            crPlaySound = StartCoroutine(coPlaySound());
        }



        // Fields
        private Coroutine crWaitCapture = null;
        private Coroutine crWaitCountdown = null;
        private Coroutine crCapture = null;
        private Coroutine crPlaySound = null;
        private string color = null;
        private AudioClip wordClip = null;
        private WebCamScreen webCamScreen = null;

        // Fuctions
        //private void resizeDisplay()
        //{
        //    var ratio = webCamMGR.WebCamRatio;
        //    var height = display.rectTransform.sizeDelta.y;
        //    var width = height * ratio;
        //    display.rectTransform.sizeDelta = new Vector2(width, height);
        //}
        private void activeWebCamToggle(bool enable)
        {
            if (webCamScreen.IsReady)
                webCamToggle.interactable = enable;
        }
        private void activeAllButtons(bool active)
        {
            webCamToggle.gameObject.SetActive(active);
            captureBTN.gameObject.SetActive(active);
            undoBTN.gameObject.SetActive(active);
            nextBTN.gameObject.SetActive(active);

            webCamToggle.isOn = webCamScreen && webCamScreen.IsReady;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Transform webCamTR = null;
        [SerializeField] private RawImage capturedImage = null;
        [SerializeField] private GameObject noCameraGO = null;
        // [SerializeField] private DisplayUGUI videoDisplayUGUI = null;
        [SerializeField] private TextMeshProUGUI baseTXT = null;
        [SerializeField] private TextMeshProUGUI[] skinsTXT = null;
        [SerializeField] private Animator captureAnim = null;
        [SerializeField] private Animator decorateAnim = null;
        [SerializeField] private Button[] textsBTN = null;
        [SerializeField] private Counter counter = null;
        [Header("★ Bindings - ButtonGroup")]
        [SerializeField] private Toggle webCamToggle = null;
        [SerializeField] private SpaceshipBTN captureBTN = null;
        [SerializeField] private SpaceshipBTN undoBTN = null;
        [SerializeField] private SpaceshipBTN nextBTN = null;

        // Unity Messages
        private void Awake()
        {
            webCamToggle.interactable = false;
            webCamToggle.onValueChanged.AddListener((bool isOn) => noCameraGO.SetActive(!isOn));

            textsBTN.ForEach(b => b.interactable = false);
            textsBTN.ForEach(b => b.onClick.AddListener(OnTextClicked));
        }
        private void Start()
        {
        }
        private void OnEnable()
        {
            if (color != null)
                decorateAnim.SetTrigger(color);
        }

        // Unity Coroutine
        IEnumerator coWaitCapture()
        {
            using (LOG.Coroutine($"coWaitCapture()", this))
            {
                yield return new WaitUntil(() => captureBTN.IsSubmit);
            }
        }
        IEnumerator coWaitCountdown()
        {
            using (LOG.Coroutine($"coWaitCountdown()", this))
            {
                counter.StartCountDown();

                yield return new WaitUntil(() => counter.IsComplete || captureBTN.IsSubmit);
            }
        }
        IEnumerator coCapture()
        {
            using (LOG.Coroutine($"coCapture()", this))
            {
                captureAnim.SetTrigger("Shoot");
                yield return null;

                var texture = webCamScreen.CapturedTexture();
                if (texture != null)
                {
                    capturedImage.GetComponent<AspectRatioFitter>().aspectRatio = webCamScreen.AspectRatio;
                    capturedImage.texture = texture;
                    capturedImage.gameObject.SetActive(true);
                }
                // videoDisplayUGUI.gameObject.SetActive(noCameraGO.activeSelf);

                yield return new WaitForSeconds(1f);
            }
        }
        IEnumerator coPlaySound()
        {
            using (LOG.Coroutine($"coPlaySound()", this))
            {
                textsBTN.ForEach(b => b.interactable = false);
                yield return null;

                yield return AudioMGR.One.PlayNarrationAndWait(wordClip);

                textsBTN.ForEach(b => b.interactable = true);
                yield return null;
            }
        }
    }
}