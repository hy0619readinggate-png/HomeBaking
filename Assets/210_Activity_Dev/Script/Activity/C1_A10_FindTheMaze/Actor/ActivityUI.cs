using beyondi.Behaviour;
using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Activity.C1_A10
{
    public class ActivityUI : BYDSingleton<ActivityUI>
    {
        // Methods
        public void Setup(int stateNo)
        {
            LOG.Info($"Setup() | {stateNo}", this);

            this.stageNo = stateNo;

            currentIcon.KeyOff();
            vfxIndicatorGO.SetActive(false);
        }
        public void ShowIndicator()
        {
            LOG.Function(this);

            indicatorGO.SetActive(true);
        }
        public void HideIndicator()
        {
            LOG.Function(this);

            indicatorGO.SetActive(false);
        }
        public Coroutine GetKey()
        {
            LOG.Info($"GetKey()", this);

            crGetKey = StartCoroutine(coGetKey());
            return crGetKey;
        }
        public void GetCharacter(int characterID)
        {
            LOG.Info($"GetCharacter()", this);

            switch (characterID)
            {
                case 1: currentIcon.GetBlanc(); break;
                case 2: currentIcon.GetJack(); break;
                case 3: currentIcon.GetSheila(); break;
            }
        }



        // Fields : caching
        private IndicatorIcon[] icons_ = null;
        private IndicatorIcon[] icons => icons_ ??= GetComponentsInChildren<IndicatorIcon>(true);
        private Canvas canvas_ = null;
        private Canvas canvas => canvas_ ??= GetComponentInParent<Canvas>();
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();

        // Fields
        private int stageNo;
        private Coroutine crGetKey = null;
        // Functions
        public IndicatorIcon currentIcon => icons[stageNo - 1];
        public Vector2 iconPosition => Camera.main.WorldToScreenPoint(currentIcon.transform.position);



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject indicatorGO = null;
        [SerializeField] private RectTransform keyRT = null;
        [SerializeField] private GameObject vfxIndicatorGO = null;
        [Header("★ Config")]
        [SerializeField] private float getKeyEffDuration = 1.7f;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            keyRT.gameObject.SetActive(false);
            vfxIndicatorGO.SetActive(false);
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coGetKey()
        {
            using (LOG.Coroutine($"coGetKey()", this))
            {
                // 표시
                keyRT.gameObject.SetActive(true);
                yield return new WaitForSeconds(getKeyEffDuration);

                currentIcon.KeyOn();
                keyRT.gameObject.SetActive(false);
                yield return new WaitForSeconds(0.5f);
            }

        }
    }
}