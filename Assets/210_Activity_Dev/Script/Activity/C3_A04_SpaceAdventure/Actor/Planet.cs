using beyondi.Coroutine;
using DG.Tweening;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using TMPro;
using UnityEngine;

namespace DoDoEng.Activity.C3_A04
{
    [RequireComponent(typeof(Follower))]
    public class Planet : MonoBehaviour
    {
        // Properties
        public float OverlabRadius { get; set; } = 1f;
        public string Value { get; private set; }
        public bool IsCollected { get; private set; } = false;

        // Methods
        public void Init(string text, AudioClip clip)
        {
            Value = text;
            IsCollected = false;
            alphabetClip = clip;

            phonicsTXT.text = text;

            // idle 애니메이션을 다르게 적용하기 위해
            DOVirtual.DelayedCall(Random.Range(0, 2f), () => anim.SetTrigger("appear"));
        }
        public void Correct()
        {
            LOG.Function(this);

            AudioMGR.One.PlayEffect(correctCLIP);
            AudioMGR.One.PlayNarration(alphabetClip);

            anim.SetTrigger("correct");
            col.enabled = false;
            IsCollected = true;
        }
        public void Wrong()
        {
            LOG.Function(this);

            ActivityProgress.One.Wrong();

            AudioMGR.One.PlayEffect(wrongCLIP);
            anim.SetTrigger("wrong");
        }
        public void SetAsAnswer(bool isAnswer)
        {
            LOG.Function(this, $"{isAnswer}");

            col.isTrigger = isAnswer;
        }
        public void Deactive()
        {
            LOG.Function(this);

            col.enabled = false;
            if (!IsCollected)
                anim.SetTrigger("disappear");
        }
        public void Clear()
        {
            LOG.Function(this);

            follower.Unfollow();
            Destroy(this.gameObject);
        }



        // Fields : caching
        private Collider2D col_ = null;
        private Collider2D col => col_ ??= GetComponent<Collider2D>();
        private Follower follower_ = null;
        private Follower follower => follower_ ??= GetComponent<Follower>();

        // Fields
        private AudioClip alphabetClip = null;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI phonicsTXT = null;
        [SerializeField] private Animator anim = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip correctCLIP = null;
        [SerializeField] private AudioClip wrongCLIP = null;

        // Unity Messages
        private void Awake()
        {

        }
        private void Start()
        {

        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, OverlabRadius);
        }
    }
}