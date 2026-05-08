using beyondi.Util;
using DoDoEng.Activity.C2_A03;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C2_A02
{
    [RequireComponent(typeof(CanvasGroup))]
    public class S2TakePhoto : MonoBehaviour
    {
        // Properties
        public Sprite Captured => capturedSprite;

        // Methods
        public void Setup(int[] characterIds)
        {
            LOG.Info($"ReadyToTakePhoto()", this);

            boat.Setup(characterIds);
            anis.ForEach(a => a.Resume());
        }
        public Coroutine StartWaitTakePhoto()
        {
            LOG.Info($"StartWaitTakePhoto()", this);

            boat.PlayCheeseSound();
            crStartWaitTakePhoto = StartCoroutine(coStartWaitTakePhoto());
            return crStartWaitTakePhoto;
        }
        public void FinishWaitTakePhoto()
        {
            LOG.Info($"FinishWaitTakePhoto()", this);

            cg.blocksRaycasts = false;

            boat.StopCheeseSound();
            this.StopCoroutineSafe(ref crStartWaitTakePhoto);
            this.StopCoroutineSafe(ref crAutoTakePhotoTimer);
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= GetComponent<CanvasGroup>();
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private Canvas canvas_ = null;
        private Canvas canvas => canvas_ ??= GetComponentInParent<Canvas>();
        private CharacterAni[] anis_ = null;
        private CharacterAni[] anis => anis_ ??= GetComponentsInChildren<CharacterAni>(true);

        // Fields
        private bool isTaken = false;
        private bool isAutoTimerOut = false;
        private Sprite capturedSprite = null;
        private Coroutine crStartWaitTakePhoto = null;
        private Coroutine crAutoTakePhotoTimer = null;

        // Functions
        private void takePhoto()
        {
            
            anis.ForEach(a => a.Pause());
            StartCoroutine(coTakePhoto());
        }

        // Event Handlers
        private void btn_onClick()
        {
            LOG.Info($"btn_onClick()", this);

            takePhoto();
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Button btn = null;
        [SerializeField] private RectTransform photoPositionRT = null;
        [SerializeField] private S2Boat boat = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip takePhotoCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float takePhotoDelay = 0.5f;
        [SerializeField] private float autoTakePhotoDelay = 5f;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;
            btn.onClick.AddListener(btn_onClick);
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coStartWaitTakePhoto()
        {
            isTaken = false;
            cg.blocksRaycasts = true;
            yield return null;

            this.StopCoroutineSafe(ref crAutoTakePhotoTimer);
            crAutoTakePhotoTimer = StartCoroutine(coAutoTakePhotoTimer());
            yield return new WaitUntil(() => isTaken || isAutoTimerOut);

            cg.blocksRaycasts = false;
            yield return null;
        }
        IEnumerator coTakePhoto()
        {
            using (LOG.Coroutine($"coTakePhoto()", this))
            {
                yield return new WaitForEndOfFrame();

                var texture2D = ScreenCapture.CaptureScreenshotAsTexture();
                var width = photoPositionRT.rect.width;
                var height = photoPositionRT.rect.height;
                var posX = photoPositionRT.position.x - width / 2;
                var posY = photoPositionRT.position.y - height / 2;
                var leftBottom = UtilTransform.LocalToScreen(new Vector2(posX, posY), rt, canvas);
                var rightTop = UtilTransform.LocalToScreen(new Vector2(posX + width, posY + height), rt, canvas);

                var rect = new Rect((int)leftBottom.x, (int)leftBottom.y, (int)(rightTop.x - leftBottom.x), (int)(rightTop.y - leftBottom.y));
                capturedSprite = Sprite.Create(texture2D, rect, Vector2.one / 2, 100);

                isTaken = true;
                yield return new WaitForSeconds(takePhotoDelay);

                AudioMGR.One.PlayEffect(takePhotoCLIP);
                yield return null;
            }
        }
        IEnumerator coAutoTakePhotoTimer()
        {
            using (LOG.Coroutine($"coAutoTakePHotoTimer()", this))
            {
                isAutoTimerOut = false;
                yield return new WaitForSeconds(autoTakePhotoDelay);

                takePhoto();
                yield return null;
            }

        }
    }
}