using beyondi.Util;
using DoDoEng.Common;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C1_A11
{
    public class Monster : MonoBehaviour, IID
    {
        // Properties
        public int ID { get; set; }

        // Methods
        public void Setup(Flower flower)
        {
            LOG.Info($"Setup()", this);

            this.flower = flower;
            ID = flower.ID;

            transform.position = flower.transform.position;
            transform.SetSiblingIndex(flower.transform.GetSiblingIndex() + 1);
        }
        public Coroutine AppearAndWait(FlowerMonsterParam param)
        {
            LOG.Info($"AppearAndWait()", this);

            if (this.param == null)
                monsterAni.SetColor(UnityEngine.Random.Range(1, param.colorCount));

            this.param = param;

            crAction = StartCoroutine(coAction(param));
            return crAction;
        }
        public void FinishAppearAniWait()
        {
            LOG.Info($"FinishAppearAniWait()", this);

            this.StopCoroutineSafe(ref crAff);
        }

        // Events
        public event Action<Monster> OnHit;



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= GetComponent<CanvasGroup>();
        private Button hitAreaBTN_ = null;
        private Button hitAreaBTN => hitAreaBTN_ ??= GetComponent<Button>();

        // Fields
        private Flower flower = null;
        private FlowerMonsterParam param = null;
        private int hitCount = 0;
        private Coroutine crAction = null;
        private Coroutine crAff = null;

        // Event Handlers
        private void onHit()
        {
            LOG.Info($"onHit()", this);

            hitCount++;

            AudioMGR.One.PlayEffect(param.hitCLIP);

            monsterAni.PlayAnimation(MonsterAnimation.Touch);
            this.StopCoroutineSafe(ref crAff);

            OnHit?.Invoke(this);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private MonsterAni monsterAni = null;
        [SerializeField] private GameObject flowerAndSoilGO = null;
        [SerializeField] private GameObject vfxMonsterAppearGO = null;
        [SerializeField] private GameObject vfxMonsterDieGO = null;
        [SerializeField] private GameObject affGO = null;
        [Header("★ Configs")]
        [SerializeField] private float affPreDelay = 1;



        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;

            flowerAndSoilGO.SetActive(false);
            affGO.SetActive(false);

            hitAreaBTN.onClick.AddListener(onHit);
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coAction(FlowerMonsterParam param)
        {
            using (LOG.Coroutine($"coAction()", this))
            {
                vfxMonsterDieGO.SetActive(false);
                vfxMonsterAppearGO.SetActive(true);
                yield return new WaitForSeconds(param.appearDelay);

                flower.Hide();
                flowerAndSoilGO.SetActive(true);
                monsterAni.PlayAnimationLoop(MonsterAnimation.Idle);
                yield return null;

                hitCount = 0;
                crAff = StartCoroutine(coAff());
                yield return null;

                cg.blocksRaycasts = true;
                yield return new WaitUntil(() => hitCount == param.goalCount);

                cg.blocksRaycasts = false;
                yield return null;

                vfxMonsterDieGO.SetActive(true);
                yield return new WaitForSeconds(param.dieDelay);

                flower.Show();
                flowerAndSoilGO.SetActive(false);
                monsterAni.AbortAnimation();
                yield return null;
            }
        }
        IEnumerator coAff()
        {
            yield return new WaitForSeconds(affPreDelay);

            if (hitCount == 0)
            {
                AffordanceMGR.One.StartAffNow();
                yield return null;
            }
        }
    }
}