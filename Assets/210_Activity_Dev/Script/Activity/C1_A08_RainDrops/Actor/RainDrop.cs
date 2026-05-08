using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C1_A08
{
    [RequireComponent(typeof(Collider2D))]
    public class RainDrop : MonoBehaviour
    {
        // Properties
        public bool IsAnswer { get; private set; }

        // Methods
        public void Drop(float y, float speed, ExampleData examData)
        {
            LOG.Info($"Drop() | {y} {speed}", this);

            IsAnswer = examData.IsAnswer;

            rainGO.SetActive(true);
            examIMG.sprite = examData.Sprite;
            gameObject.SetActive(true);

            startDrop(y, speed);
        }
        public void StopDrop()
        {
            LOG.Info($"StopDrop()", this);

            if (gameObject.activeSelf)
                stopDrop();
        }
        public void Explode()
        {
            LOG.Info($"Explode()", this);

            stopDrop();
            StartCoroutine(coExplode(false));
        }


        // Fields : caching
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private Collider2D col_ = null;
        private Collider2D col => col_ ??= GetComponent<Collider2D>();

        // Fields
        private Coroutine crDrop = null;
        private bool isValid = false;


        // Functions
        private void startDrop(float y, float speed)
        {
            isValid = true;
            rt.position = new Vector2(rt.position.x, y);
            col.enabled = true;

            crDrop = StartCoroutine(coDrop(speed));
        }
        private void stopDrop()
        {
            isValid = false;
            col.enabled = false;

            StopCoroutine(crDrop);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Image examIMG = null;
        [SerializeField] private GameObject rainGO = null;
        [SerializeField] private GameObject vfxWaterDropGO = null;
        [SerializeField] private GameObject sampleGO = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip correctCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float explodeDuration = 1f; 

        // Unity Messages
        private void Awake()
        {
            col.enabled = false;

            examIMG.gameObject.SetActive(true);
            sampleGO.SetActive(false);
            vfxWaterDropGO.gameObject.SetActive(false);
        }
        private void Start()
        {
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            LOG.Info($"OnTriggerEnter2D() | {collision.gameObject.name}", this);

            var contact = collision.GetComponent<RainDropContact>();
            if (contact != null)
            {
                LOG.Info($"Contact with <color=red>{contact.ContactType}</color>", this);

                if (contact.ContactType == ContactType.FloorZone)
                {
                    stopDrop();
                    StartCoroutine(coExplode(false));
                }

                if (contact.ContactType == ContactType.NoHitZone)
                    isValid = false;

                if (contact.ContactType == ContactType.Player
                    && isValid
                    && Player.ActivePlayer.IsValid)
                {
                    stopDrop();

                    //if (IsAnswer && !Player.ActivePlayer.IsRemainOne)
                        StartCoroutine(coExplode(IsAnswer));
                    //else gameObject.SetActive(false);

                    Player.ActivePlayer.TakeRaindrop(this);
                }
            }
        }

        // Unity Coroutine
        IEnumerator coDrop(float speed)
        {
            using (LOG.Coroutine($"coDrop() | {speed}", this))
            {
                while (true)
                {
                    rt.anchoredPosition += new Vector2(0, -1) * speed * Time.deltaTime;
                    yield return null;
                }
            }
        }
        IEnumerator coExplode(bool correct)
        {
            using (LOG.Coroutine($"coExplode()", this))
            {
                if (correct)
                    AudioMGR.One.PlayEffect(correctCLIP);

                vfxWaterDropGO.SetActive(true);
                rainGO.SetActive(false);
                yield return new WaitForSeconds(explodeDuration);

                vfxWaterDropGO.SetActive(false);
                gameObject.SetActive(false);
            }
        }
    }
}