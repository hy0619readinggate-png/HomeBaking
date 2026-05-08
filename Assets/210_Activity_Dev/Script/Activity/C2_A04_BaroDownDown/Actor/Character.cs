using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C2_A04
{
    public class Character : MonoBehaviour
    {
        // Properties
        public RectTransform RT => rt;

        // Methods
        public void StartDiving()
        {
            LOG.Info($"StartDiving()", this);

            // baroAni는 타임라인으로 제어됨
            targetPosition = originalPosition;

            vfxGlitterGO.SetActive(true);
            isDiving = true;
        }
        public void FinishDiving()
        {
            LOG.Info($"FinishDiving()", this);

            vfxGlitterGO.SetActive(false);
            isDiving = false;
        }
        public void DivingToIdlePose()
        {
            LOG.Info($"DivingToIdlePose()", this);

            baroAni.PlayAnimation(CharacterAnimation.Correct2);
            stingrayAni.PlayAnimation(StingrayAnimation.Wrong, false);
            AudioMGR.One.PlayEffect(wrongAppearCLIP);
        }
        public void MoveTo(Vector2 ptScreen)
        {
            //LOG.Info($"MoveTo() | {ptScreen}", this);

            targetPosition = UtilTransform.ScreenToLocal(ptScreen, rt.parent as RectTransform, canvas);
        }



        // Fields : caching
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private Canvas canvas_ = null;
        private Canvas canvas => canvas_ ??= GetComponentInParent<Canvas>();

        // Fields
        private bool isDiving = false;
        private Vector2 targetPosition;
        private Vector2 originalPosition;



        // Unity Inspectors
        [Header("★ Bindings - Activity")]
        [SerializeField] private CharacterAni baroAni = null;
        [SerializeField] private StingrayAni stingrayAni = null;
        [SerializeField] private AudioClip wrongAppearCLIP = null;
        [SerializeField] private GameObject vfxGlitterGO = null;
        [Header("★ Config")]
        [SerializeField] private float smoothTime = 0.3f;
        [SerializeField] private float moveThreshold = 3;
        [SerializeField] private Vector2 velocity = Vector2.zero;

        // Unity Messages
        private void Awake()
        {
            targetPosition = originalPosition = rt.anchoredPosition;
            vfxGlitterGO.SetActive(false);
        }
        private void Start()
        {
        }
        private void Update()
        {
            if (!isDiving) return;

            if (Mathf.Abs(targetPosition.x - rt.anchoredPosition.x) >= moveThreshold)
            {
                var s = rt.anchoredPosition;
                var f = new Vector2(targetPosition.x, s.y);
                var position = Vector2.SmoothDamp(s, f, ref velocity, smoothTime);
                rt.anchoredPosition = position;
            }
        }
    }
}