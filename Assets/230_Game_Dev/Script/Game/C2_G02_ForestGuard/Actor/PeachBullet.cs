using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Game.C2_G02
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class PeachBullet : MonoBehaviour
    {
        // Properties
        public string SoundPhonics { get; private set; }
        public AudioClip WordCLIP { get; private set; }

        // Methods
        public void Setup(string soundPhonics, AudioClip wordCLIP, Vector2 velocity, float limitX)
        {
            LOG.Info($"Setup() | {soundPhonics} {wordCLIP.name}", this);

            this.limitX = limitX;

            SoundPhonics = soundPhonics;
            WordCLIP = wordCLIP;

            rb.velocity = velocity;
            rb.angularVelocity = angularVelocity.RandomValue();
        }
        public void BounceOff()
        {
            LOG.Info($"BounceOff()", this);

            AudioMGR.One.PlayEffect(bounceCLIP);
            var position = new Vector3(limitX + 10, transform.position.y, transform.position.z);
            transform.DOJump(position, bouncePower, 1, bounceDuration);
        }
        public void Burst()
        {
            LOG.Info($"Burst()", this);

            rb.velocity = Vector3.zero;

            // #475 정답 과일이 몬스터 맞췄을 때, 단어 음원 삭제
            //AudioMGR.One.PlayNarration(WordCLIP);
            anim.SetTrigger("Crush");
        }



        // Fields : caching
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();
        private Rigidbody2D rb_ = null;
        private Rigidbody2D rb => rb_ ??= GetComponent<Rigidbody2D>();
        private Collider2D col_ = null;
        private Collider2D col => col_ ??= GetComponent<Collider2D>();

        // Fields
        private float limitX;



        // Unity Inspectors
        [Header("★ Sound")]
        [SerializeField] private AudioClip bounceCLIP;
        [Header("★ Config")]
        [SerializeField] private float bouncePower = 2f;
        [SerializeField] private float bounceDuration = 2f;
        [SerializeField] private Range angularVelocity = new Range(360, 360 * 3);

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
        private void FixedUpdate()
        {
            if (rb.position.x > limitX)
                Destroy(gameObject);
        }
        private void OnCollisionEnter2D(Collision2D collision)
        {
            LOG.Info($"OnCollisionEnter2D() | {collision.gameObject.name}", this);

            var monster = collision.gameObject.GetComponent<Monster>();
            if (monster != null)
            {
                col.enabled = false;

                monster.Attack(this);
            }
        }

    }
}