using beyondi.Util;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C1_A11
{
    [RequireComponent(typeof(AffBase))]
    public class FlowerMGR : MonoBehaviour
    {
        // Methods
        public void Setup(int pNO, ExampleData[] exams, AudioClip clip)
        {
            LOG.Info($"Setup()", this);

            phoneticCLIP = clip;

            flowers.ForEach((i, f) => f.Setup(exams[i], pNO - 1, flowerNormalParam));
        }
        public Coroutine StartWaitCollectAll()
        {
            LOG.Info($"StartWaitCollectAll()", this);

            aff.Enabler = () => !isMonsterAppear;
            flowers.ForEach(f => f.EnableInteraction(true));
            crWaitCollectAll = StartCoroutine(coStartWaitCollectAll());
            return crWaitCollectAll;
        }
        public void StopWaitCollectAll()
        {
            LOG.Info($"StopWaitCollectAll()", this);

            aff.Enabler = () => false;
            flowers.ForEach(f => f.EnableInteraction(false));
            monsterFlowers.ForEach(f => f.FinishAppearAniWait());
            this.StopCoroutineSafe(ref crWaitCollectAll);
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }

        // Events
        public event Action OnMonsterFinish;



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Flower[] flowers_ = null;
        private Flower[] flowers => flowers_ ??= GetComponentsInChildren<Flower>(true);
        private AffBase aff_ = null;
        private AffBase aff => aff_ ??= GetComponent<AffBase>();

        // Fields
        private Coroutine crWaitCollectAll = null;
        private AudioClip phoneticCLIP = null;
        private bool collectedAll = false;
        private bool isMonsterAppear = false;
        private Monster[] monsterFlowers = null;

        // Event Handlers
        private void flowerNormal_OnClick(Flower flower)
        {
            LOG.Info($"flowerNormal_OnClick() | {flower}", this);

            if (flower.IsAnswer)
            {
                var flowerD = Instantiate(flower, floatAreaTR);
                var flowerF = flowerD.gameObject.AddComponent<FloatFlower>();
                flowerF.Fly(flower, flowerfloatParam, phoneticCLIP);
                flower.Collect();

                collectedAll = flowers.Where(f => f.IsAnswer).All(f => f.IsCollected);
                if (collectedAll)
                    flowers.ForEach(f => f.EnableInteraction(false));
            }
            else
            {
                ActivityProgress.One.Wrong();

                var monster = monsterFlowers.First(f => f.ID == flower.ID);
                StartCoroutine(coWaitMonsterDie(monster));

            }
        }
        private void flowerMonster_OnHit(Monster monster)
        {
            LOG.Info($"flowerMonster_OnHit() | {monster}", this);

            hammerGO.transform.position = monster.transform.position;
            hammerGO.SetActive(false);
            hammerGO.SetActive(true);
        }
        private void aff_OnAffStart(GameObject go)
        {
            LOG.Info($"aff_OnAffStart() | {go.name}", this);

            var pool = flowers
                .Where(f => f.IsCollectable && f.IsAnswer)
                .ToArray();

            var flower = UtilArray.ExtractOne(pool);
            go.transform.position = flower.transform.position;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Transform floatAreaTR = null;
        [SerializeField] private GameObject hammerGO = null;
        [SerializeField] private GameObject monsterPB = null;
        [Header("★ Config")]
        [SerializeField] private FlowerNormalParam flowerNormalParam = null;
        [SerializeField] private FlowerMonsterParam flowerMonsterParam = null;
        [SerializeField] private FlowerFloatParam flowerfloatParam = null;
        [SerializeField] private float collectedAllDelay = 3f;

        // Unity Messages
        private void Awake()
        {
            aff.Enabler = () => false;
            hammerGO.SetActive(false);

            flowers.AutoFillID();

            var monsters = new List<Monster>();
            foreach (var f in flowers)
            {
                var monsterGO = Instantiate(monsterPB, transform);
                var monster = monsterGO.GetComponent<Monster>();
                monster.Setup(f);
                monster.gameObject.SetActive(false);
                monsters.Add(monster);
            }
            monsterFlowers = monsters.ToArray();

            Gino.One.Init(flowerNormalParam);
        }
        private void Start()
        {
        }
        private void OnEnable()
        {
            aff.OnAffStart += aff_OnAffStart;
            flowers.ForEach(f => f.OnClick += flowerNormal_OnClick);
            monsterFlowers.ForEach(f => f.OnHit += flowerMonster_OnHit);
        }
        private void OnDisable()
        {
            aff.OnAffStart -= aff_OnAffStart;
            flowers.ForEach(f => f.OnClick -= flowerNormal_OnClick);
            monsterFlowers.ForEach(f => f.OnHit -= flowerMonster_OnHit);
        }
        private void Update()
        {
            // For Develop Test
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                var flower = flowers.FirstOrDefault(f => f.IsAnswer && f.IsCollectable);
                if (flower != null)
                    flowerNormal_OnClick(flower);
            }
        }

        // Unity Coroutine
        IEnumerator coStartWaitCollectAll()
        {
            using (LOG.Coroutine($"coStartWaitCollectAll()", this))
            {
                collectedAll = false;

                yield return new WaitUntil(() => collectedAll == true);
                yield return null;

                yield return new WaitForSeconds(collectedAllDelay);
            }
        }
        IEnumerator coWaitMonsterDie(Monster monster)
        {
            using (LOG.Coroutine($"coWaitMonsterDie()", this))
            {
                isMonsterAppear = true;
                flowers.ForEach(f => f.EnableInteraction(false));

                Gino.One.LocateBehindWagon();
                Gino.One.SurpriseAndRun();
                yield return null;

                monster.gameObject.SetActive(true);
                yield return monster.AppearAndWait(flowerMonsterParam);
                yield return new WaitForSeconds(0.5f);

                monster.gameObject.SetActive(false);
                yield return null;

                yield return Gino.One.RelaxAndReturn();
                yield return null;

                flowers.ForEach(f => f.EnableInteraction(f.IsCollectable));
                Gino.One.LocateFrontOfWagon();
                isMonsterAppear = false;
                yield return null;

                OnMonsterFinish?.Invoke();
            }
        }
    }

    [System.Serializable]
    public class FlowerNormalParam
    {
        public int typeCount = 2;
        public int colorCount = 5;
        public int idleMotionCount = 3;

        public Color petalTextColor = new Color(255, 123, 20);
        public Color blueTulipTextColor = new Color(48, 122, 240);
        public Color pinkTulipTextColor = new Color(252, 76, 138);
        public Color purpleTulipTextColor = new Color(122, 26, 217);
        public Color redTulipTextColor = new Color(252, 65, 43);
        public Color yellowTulipTextColor = new Color(255, 125, 33);
    }

    [System.Serializable]
    public class FlowerFloatParam
    {
        public float zoomScale = 2f;
        public float zoomDuration = 0.25f;

        public float shakeDuration = 0.3f;
        public float shakeStrength = 0.6f;
        public int shakeVibrato = 10;
        public float shakeDelay = 0.3f;

        public float jumpPower = 2f;
        public float jumpDuration = 0.7f;
        public float jumpScale = 0.5f;
        public float jumpAlpha = 1f;

        public AudioClip zoomCLIP = null;
        public AudioClip jumpCLIP = null;
    }

    [System.Serializable]
    public class FlowerMonsterParam
    {
        public float appearDelay = 0.3f;
        public float dieDelay = 0.5f;

        public int colorCount = 3;
        public float goalCount = 10;

        public AudioClip hitCLIP = null;
    }
}