using System.Collections;
using UnityEngine;
using TMPro;
using DoDoEng.Common;

namespace DoDoEng.Game.C4_G02
{
    public class BubbleText : Bubble
    {

        // Properties
        public string Text => text;


        // Methods
        public void SetText(string text)
        {
            this.text = text;

            int index = 0;
            var textLength = text.Length;
            if (textLength <= 6) // S 사이즈 텍스트 6자 이하
            {
                index = 0;
            }
            else if (textLength <= 8) // M 사이즈
            {
                index = 1;
            }
            else // L 사이즈
            {
                index = 2;
            }

            runAnimator = _Animators[index];
            runTMP = _TMPs[index];
            runTMP.text = text;
        }



        // Fields
        private bool moving = false;
        private string text = string.Empty;
        private Animator runAnimator = null;
        private TextMeshProUGUI runTMP = null;



        // Overrides
        protected override void pop()
        {
            if (moving)
            {
                moving = false;
            }
            else
            {
                if (runCoroutine != null)
                {
                    StopCoroutine(runCoroutine);
                    runCoroutine = null;
                }

                StartCoroutine(coForcePop());
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Animator[] _Animators = null;  // S, M, L 순서
        [SerializeField] private TextMeshProUGUI[] _TMPs = null; // S, M, L 순서
        [Space()]
        [SerializeField] private GameObject _ColliderGO = null;
        [Space()]
        [SerializeField] private AudioClip _CreateClip = null;
        [SerializeField] private AudioClip _PopClip = null;

        // Unity Messages
        protected override void Start()
        {
            _ColliderGO.SetActive(false);

            foreach (var tmp in _TMPs)
                tmp.gameObject.SetActive(false);

            base.Start();
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.attachedRigidbody != null)
            {
                var bullet = collision.attachedRigidbody.GetComponent<Bullet>();
                if (bullet != null)
                {
                    BubbleMGR.One.CheckAnswerBubble(this);

                    moving = false;
                }
            }
        }


        // Unity Coroutine
        protected override IEnumerator coRun()
        {
            runAnimator.SetTrigger(hashKey_Show);
            yield return null;
            yield return new WaitForSeconds(0.5f);

            runTMP.gameObject.SetActive(true);

            AudioMGR.One.PlayEffect(_CreateClip);
            yield return new WaitUntil(() => runAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Idle"));


            _ColliderGO.SetActive(true);


            float moveSpeed = 0f;
            moving = true;
            while (moving)
            {
                if (moveSpeed < speed)
                    moveSpeed += Time.deltaTime;

                transform.position += transform.up * moveSpeed * Time.deltaTime;
                yield return null;
            }

            _ColliderGO.SetActive(false);
            runTMP.gameObject.SetActive(false);


            AudioMGR.One.PlayEffect(_PopClip);
            runAnimator.SetTrigger(hashKey_Pop);
            yield return null;
            yield return new WaitUntil(() => runAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f);


            Destroy(this.gameObject);
            yield return null;
        }
        IEnumerator coForcePop()
        {
            AudioMGR.One.PlayEffect(_PopClip);

            runAnimator.SetTrigger(hashKey_ForcePop);
            yield return null;
            yield return new WaitUntil(() => runAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f);


            Destroy(this.gameObject);
            yield return null;
        }
    }
}