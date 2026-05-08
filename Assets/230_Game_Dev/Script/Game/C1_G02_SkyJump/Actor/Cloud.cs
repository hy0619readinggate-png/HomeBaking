using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using DoDoEng.Game.Framework;
using System.Collections;
using TMPro;
using UnityEngine;

namespace DoDoEng.Game.C1_G02
{
    [RequireComponent(typeof(Platform))]
    public class Cloud : MonoBehaviour
    {
        // Properties
        public bool IsActive => gameObject.activeSelf;
        public bool IsMoving => moving;
        public int Floor { get; set; }
        public Booster Booster { get; set; } = null;

        // Methods
        public void SetupMoving(float x1, float x2, float y, float velocity)
        {
            var isLeft = UtilRandom.RandomSuccess(0.5f);
            var xInit = isLeft ? x1 : x2;
            var xVelocity = velocity * (isLeft ? +1 : -1);
            transform.position = new Vector3(xInit, y, transform.position.z);

            this.xVelocity = xVelocity;
            this.x1 = x1;
            this.x2 = x2;
            this.moving = true;
        }
        public void SetupExample(string phonetic, bool isAnswer, Rainbow rainbow)
        {
            phoneticText.gameObject.SetActive(true);
            phoneticText.text = phonetic;

            this.isExample = true;
            this.isAnswer = isAnswer;
            this.rainbow = rainbow;
        }
        public void Disappear()
        {
            LOG.Info($"Disappear()", this);

            if (!isAnswer)
                disappear();
        }



        // Fields : caching
        private Platform platform_ = null;
        private Platform platform => platform_ ??= GetComponent<Platform>();

        // Fields
        private bool moving = false;
        private float xVelocity = 0;
        private float x1;
        private float x2;
        private bool isExample = false;
        private bool isAnswer = false;
        private Rainbow rainbow = null;

        // Functions
        private void disappear()
        {
            if (gameObject.activeInHierarchy)
            {
                var cols = GetComponentsInChildren<Collider2D>(true);
                cols.ForEach(col => col.enabled = false);

                transform
                    .DOScale(0.01f, 0.5f)
                    .OnComplete(() => gameObject.SetActive(false));
            }
        }



        // Event Handlers
        private void platform_OnPlayerEnter(Player player)
        {
            LOG.Info($"platform_OnPlayerEnter()", this);

            if (isExample)
            {
                if (isAnswer)
                    StartCoroutine(coCorrect(player));
                else StartCoroutine(coWrong(player));
            }
            if (Booster != null)
            {
                Booster.Consume();
                player.BoostByItem();
            }
        }
        private void platform_OnPlayerExit(Player player)
        {
            LOG.Info($"platform_OnPlayerExit()", this);

            disappear();
        }



        // Unity Inspectors
        [Header("¡Ú Bindings")]
        [SerializeField] private TextMeshPro phoneticText = null;
        [Header("¡Ú Timing")]
        [SerializeField] private float wrongDelay = 0.2f;

        // Unity Messages
        private void Awake()
        {
            phoneticText.gameObject.SetActive(false);
        }
        private void Start()
        {
        }
        private void Update()
        {
            if (moving)
            {
                if (xVelocity < 0 && transform.position.x < x1 ||
                    xVelocity > 0 && transform.position.x > x2)
                    xVelocity *= -1;

                transform.position =
                    new Vector3(
                        transform.position.x + xVelocity * Time.deltaTime,
                        transform.position.y,
                        transform.position.z);
            }
        }
        private void OnEnable()
        {
            platform.OnPlayerEnter += platform_OnPlayerEnter;
            platform.OnPlayerExit += platform_OnPlayerExit;
        }
        private void OnDisable()
        {
            platform.OnPlayerEnter -= platform_OnPlayerEnter;
            platform.OnPlayerExit -= platform_OnPlayerExit;
        }

        // Unity Coroutine
        IEnumerator coCorrect(Player player)
        {
            using (LOG.Coroutine($"coCorrect()", this))
            {
                rainbow.Complete();
                player.Correct();
                yield return null;
            }
        }
        IEnumerator coWrong(Player player)
        {
            using (LOG.Coroutine($"coWrong()", this))
            {
                yield return new WaitForSeconds(wrongDelay);

                platform.DetachPlayer();
                player.Wrong();
                disappear();
                yield return null;
            }
        }
    }
}