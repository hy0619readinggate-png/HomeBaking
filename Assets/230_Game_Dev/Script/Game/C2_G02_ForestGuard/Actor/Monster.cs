using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Game.C2_G02
{
    public class Monster : MonoBehaviour, IPointerDownHandler
    {
        // Properties
        public string SoundPhonics => monsterData.SoundPhonetic;
        public int Lane => lane;
        public bool IsAlive { get; private set; }
        public float MovedDistance => startPosition.x - UtilTransform.LocalToScreen(transform.position, rt, canvas).x;

        // Methods
        public void Setup(MonsterData monsterData, int lane)
        {
            LOG.Info($"Setup() | {monsterData}", this);

            this.lane = lane;
            this.monsterData = monsterData;
            this.speed = monsterData.MonsterSpeed;
            this.IsAlive = true;
            startPosition = UtilTransform.LocalToScreen(transform.position, rt, canvas);

            updateMonsterType(monsterData);
        }
        public void Attack(PeachBullet bullet)
        {
            LOG.Info($"Attack()", this);

            if (bullet.SoundPhonics != SoundPhonics)
            {
                bullet.BounceOff();
                StartCoroutine(coDefend());
            }
            else
            {
                this.IsAlive = false;
                bullet.Burst();
                StartCoroutine(coDie());
            }
        }
        public void BlowAway()
        {
            LOG.Info($"BlowAway()", this);

            StopAllCoroutines();
            StartCoroutine(coBlowAway());
        }
        public void Halt()
        {
            LOG.Info($"Halt()", this);

            StopAllCoroutines();
            currentMonster.PlayAnimationLoopT2(MonsterAnimation.Idle);
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private Canvas canvas_ = null;
        private Canvas canvas => canvas_ ??= GetComponentInParent<Canvas>();

        // Fields
        private MonsterData monsterData = null;
        private MonsterAni currentMonster = null;
        private Collider2D currentCollider = null;
        private CanvasGroup currentTextCG = null;
        private float speed = 0;
        private int lane = 0;
        private Coroutine crRush = null;
        private int monsterType = 0;
        private Vector3 startPosition;

        // Functions
        private void updateMonsterType(MonsterData monsterData)
        {
            monsterType = (int)monsterData.MonsterType - 1;

            foreach (var (m, i) in monsterANI.Select((i, j) => (i, j)))
                m.gameObject.SetActive(i == monsterType);

            currentMonster = monsterANI[monsterType];

            currentCollider = currentMonster.GetComponent<Collider2D>();
            currentCollider.enabled = false;

            phoneticTXT[monsterType].text = monsterData.Phonetic;
            currentTextCG = phoneticTXT[monsterType].AddComponent<CanvasGroup>();
            currentTextCG.alpha = 0;
        }

        // Functions
        private void startRush()
        {
            currentCollider.enabled = true;
            crRush = StartCoroutine(coRush());
        }
        private void stopRush()
        {
            currentCollider.enabled = false;
            this.StopCoroutineSafe(ref crRush);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private MonsterAni[] monsterANI = null;
        [SerializeField] private TextMeshProUGUI[] phoneticTXT = null;
        [SerializeField] private Animator hitANIM = null;
        [SerializeField] private GameObject spawnFxGO = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip attackCLIP = null;
        [SerializeField] private AudioClip spawnCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float cannonAttackImpact = 0.4f;
        [SerializeField] private float cannonAttackDuration = 1.2f;
        [SerializeField] private float plantAttackImpact = 0.6f;
        [SerializeField] private float plantAttackDuration = 1.3f;
        [SerializeField] private float plantAttackDamage = 10f;
        [SerializeField] private float textShowDelay = 0.5f;
        [SerializeField] private float textShowDuration = 0.3f;
        [SerializeField] private float[] textHideDelay = null;
        [SerializeField] private float textHideDuration = 0.3f;
        [SerializeField] private float knockBackDistance = 1f;
        [SerializeField] private float knockBackDuration = 0.2f;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;
            spawnFxGO.SetActive(false);
        }
        private void Start()
        {
            // 미리 spawn할 준비를 함
            monsterANI.ForEach(ani => ani.SetAnimation(MonsterAnimation.Spawn));

            StartCoroutine(coSpawn());
        }
        private void OnCollisionEnter2D(Collision2D collision)
        {
            LOG.Info($"OnCollisionEnter2D() | {collision.gameObject.name}", this);

            var cannon = collision.gameObject.GetComponent<Cannon>();
            if (cannon != null)
                StartCoroutine(coAttackCannon(cannon));

            var plant = collision.gameObject.GetComponent<Plant>();
            if (plant != null)
                StartCoroutine(coAttackPlant(plant));
        }

        // Unity Coroutine
        IEnumerator coSpawn()
        {
            using (LOG.Coroutine($"coSpawn()", this))
            {
                AudioMGR.One.PlayEffect(spawnCLIP);
                yield return null;

                currentTextCG.DOFade(1, textShowDuration).SetDelay(textShowDelay);
                yield return currentMonster.PlayAnimationAndWait(MonsterAnimation.Spawn);

                AudioMGR.One.PlayNarration(monsterData.PhoneticCLIP);
                yield return null;

                cg.blocksRaycasts = true;
                yield return null;

                startRush();
            }
        }
        IEnumerator coRush()
        {
            using (LOG.Coroutine($"coRush()", this))
            {
                currentMonster.PlayAnimationLoop(MonsterAnimation.Walk);
                while (true)
                {
                    this.transform.Translate(new Vector3(-speed, 0, 0) * Time.deltaTime);
                    yield return null;
                }
            }
        }
        IEnumerator coDefend()
        {
            using (LOG.Coroutine($"coDefend()", this))
            {
                stopRush();
                yield return null;

                EventBus.Raise<EventBus.MonsterDefendEvent>(this);
                yield return null;

                transform.DOMoveX(knockBackDistance, knockBackDuration).SetRelative();
                yield return currentMonster.PlayAnimationAndWait(MonsterAnimation.Block);
                yield return null;

                startRush();
            }
        }
        IEnumerator coDie()
        {
            using (LOG.Coroutine($"coDie()", this))
            {
                cg.blocksRaycasts = false;
                stopRush();
                yield return null;

                var trigger = monsterData.MonsterType == MonsterType.Crab ? "HitBig" : "HitSmall";
                hitANIM.SetTrigger(trigger);
                yield return null;

                EventBus.Raise<EventBus.MonsterDiedEvent>(this);
                yield return null;

                currentTextCG.DOFade(0, textHideDuration).SetDelay(textHideDelay[monsterType]);
                yield return currentMonster.PlayAnimationAndWait(MonsterAnimation.Death, false);
                yield return null;

                Destroy(gameObject);
            }
        }
        IEnumerator coAttackCannon(Cannon cannon)
        {
            using (LOG.Coroutine($"coAttackCannon()", this))
            {
                stopRush();

                AudioMGR.One.PlayEffect(attackCLIP);
                currentMonster.PlayAnimation(MonsterAnimation.Attack1);
                yield return new WaitForSeconds(cannonAttackImpact * 0.5f);

                EventBus.Raise<EventBus.CannonBeAttackedEvent>();
                yield return new WaitForSeconds(cannonAttackImpact * 0.5f);

                cannon.Kill();
                yield return new WaitForSeconds(cannonAttackDuration - cannonAttackImpact);

                startRush();
            }
        }
        IEnumerator coAttackPlant(Plant plant)
        {
            using (LOG.Coroutine($"coAttackPlant()", this))
            {
                stopRush();
                yield return null;

                EventBus.Raise<EventBus.PlantBeAttackedEvent>(lane);

                while (true)
                {
                    AudioMGR.One.PlayEffect(attackCLIP);

                    currentMonster.PlayAnimation(MonsterAnimation.Attack2);
                    yield return new WaitForSeconds(plantAttackImpact);

                    plant.Damage(plantAttackDamage);
                    yield return new WaitForSeconds(plantAttackDuration - plantAttackImpact);
                }
            }
        }
        IEnumerator coBlowAway()
        {
            using (LOG.Coroutine($"coBlowAway()", this))
            {
                cg.blocksRaycasts = false;
                stopRush();
                yield return null;

                yield return currentMonster.PlayAnimationAndWait(MonsterAnimation.Out, false);
                yield return null;

                Destroy(gameObject);
            }
        }



        // Interface : IPointerDownHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            AudioMGR.One.PlayNarration(monsterData.PhoneticCLIP);
        }


    }
}