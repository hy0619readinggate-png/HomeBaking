using beyondi.Coroutine;
using beyondi.Util;
using DoDoEng.Common;
using DoDoEng.Game.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Game.C2_G02
{
    [RequireComponent(typeof(PeachItemPool))]
    public class Plant : MonoBehaviour, ICompletable
    {
        // Definitions
        private const float PRODUCE_MOTION_DELAY = 0.2f;

        // Properties
        public Transform[] MovingPeacheTRs
        {
            get
            {
                var peaches = conveyorTR.GetComponentsInChildren<PeachItem>();
                return peaches
                    .Where(p => p.isActiveAndEnabled
                                && p != PeachItem.CurrentDrag)
                    .OrderBy(p => p.transform.position.y)
                    .Select(p => p.transform)
                    .ToArray();
            }
        }

        // Methods
        public void Init()
        {
            LOG.Info($"Init()", this);

            hp = initialHP;
            UIGameCommon.One.HealthBar.Setup(initialHP);
        }
        public void Setup(BulletData[] bulletData, float conveyorSpeed = 2)
        {
            LOG.Info($"Setup() | {string.Join(",", bulletData.Select(b => b.Word))}", this);

            bulletDatas = bulletData;
            this.conveyorSpeed = conveyorSpeed;
        }
        public void StartProduction()
        {
            LOG.Info($"StartProduction()", this);

            IsComplete = false;
            startProduction();
        }
        public void StopProduction()
        {
            LOG.Info($"StopProduction()", this);

            stopProduction();
        }
        public void Damage(float damage)
        {
            LOG.Info($"Damage() | {damage}", this);

            this.StopCoroutineSafe(ref crDamage);

            hp -= damage;
            hp = Mathf.Max(hp, 0);
            UIGameCommon.One.HealthBar.UpdateHP(hp);

            if (hp > 0)
                crDamage = StartCoroutine(coDamage(damage));
            else StartCoroutine(coDie());
        }



        // Fields : caching
        private PeachItemPool pool_ = null;
        private PeachItemPool pool => pool_ ??= GetComponent<PeachItemPool>();

        // Fields
        private Coroutine crProduce = null;
        private BulletData[] bulletDatas = null;
        private Queue<BulletData> randomBulletDatas = new Queue<BulletData>();

        // Fields
        private float conveyorSpeed = 2;
        private bool isDamaging = false;
        private float hp = 0;

        // Fields
        private Coroutine crDamage = null;

        // Functions
        private BulletData getNextBulletData()
        {
            if (randomBulletDatas.Count == 0)
                randomBulletDatas = new Queue<BulletData>(UtilArray.Shuffled(bulletDatas));

            return randomBulletDatas.Dequeue();
        }
        private void startProduction()
        {
            conveyor.TurnOn();
            crProduce = StartCoroutine(coProduce());
        }
        private void stopProduction(bool stopConveyor = true)
        {
            if (stopConveyor)
                conveyor.TurnOff();
            this.StopCoroutineSafe(ref crProduce);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private PlantAni ani = null;
        [SerializeField] private Conveyor conveyor = null;
        [SerializeField] private Transform itemStartTR = null;
        [SerializeField] private Transform itemFinishTR = null;
        [SerializeField] private Transform conveyorTR = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip fruitCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float initialHP = 120f;
        [SerializeField] private float spawnDelay = 3;
        [SerializeField] private float damageDelay = 1;

        // Unity Messages
        private void Awake()
        {

        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coProduce()
        {
            using (LOG.Coroutine($"coProduce()", this))
            {
                while (true)
                {
                    AudioMGR.One.PlayEffect(fruitCLIP);
                    ani.PlayAnimation(PlantAnimation.Produce);
                    yield return new WaitForSeconds(PRODUCE_MOTION_DELAY);

                    var peachItem = pool.Get();
                    peachItem.transform.SetParent(conveyorTR);
                    peachItem.Setup(getNextBulletData());
                    peachItem.StartMove(itemStartTR.position, itemFinishTR.position, conveyorSpeed);
                    yield return new WaitForSeconds((spawnDelay - PeachItem.SPAWN_DURATION) / conveyorSpeed - PRODUCE_MOTION_DELAY);

                    // 데미지를 받고 있으면, 대기
                    yield return new WaitWhile(() => isDamaging);
                }
            }
        }
        IEnumerator coDamage(float damage)
        {
            using (LOG.Coroutine($"coDamage() | {damage}", this))
            {
                isDamaging = true;
                ani.PlayAnimation(PlantAnimation.Damage);
                conveyor.Damage();
                yield return new WaitForSeconds(damageDelay);

                isDamaging = false;
                yield return null;
            }
        }
        IEnumerator coDie()
        {
            using (LOG.Coroutine($"coDie()", this))
            {
                EventBus.Raise<EventBus.PlantDiedEvent>();

                stopProduction(false);
                yield return ani.PlayAnimationAndWait(PlantAnimation.Death, false);
                yield return null;

                IsComplete = true;
            }
        }



        // Interface : ICompletable
        public bool IsComplete { get; private set; }
    }
}