using beyondi.Util;
using DoDoEng.Common;
using System;
using System.Collections;
using UnityEngine;


namespace DoDoEng.Game.C4_G02
{
    public class BubbleMonster : Bubble
    {

        // Properties
        public int MonsterIndex => monsterIndex;



        // Methods
        public void SetMonster()
        {
            monsterIndex = UtilArray.RandomOne(0, 2);
        }
        public void SetHeightLimit(float yPos, Action<BubbleMonster> action)
        {
            heightCheck = true;
            heightLimit = yPos;

            exceedHegihtEvent = action;
        }



        // Fields
        private bool moving = false;
        private int monsterIndex = 0;
        private bool heightCheck = false;
        private float heightLimit = 0f;
        private Action<BubbleMonster> exceedHegihtEvent = null;



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
        [SerializeField] private Animator _Animator = null;
        [SerializeField] private GameObject _ColliderGO = null;
        [Space()]
        [SerializeField] private AudioClip _CreateClip = null;
        [SerializeField] private AudioClip[] _PopClips = null;
        [Space()]
        [SerializeField] private GameObject[] _MonsterModels = null;

        // Unity Messages
        protected override void Start()
        {
            _ColliderGO.SetActive(false);

            foreach (var go in _MonsterModels)
                go.SetActive(false);

            base.Start();
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.attachedRigidbody != null)
            {
                var bullet = collision.attachedRigidbody.GetComponent<Bullet>();
                if (bullet != null)
                {
                    moving = false;
                }
            }
        }


        // Unity Coroutine
        protected override IEnumerator coRun()
        {
            _Animator.SetTrigger(hashKey_Show);
            yield return null;
            yield return new WaitForSeconds(0.5f);


            for (int i = 0; i < _MonsterModels.Length; i++)
                _MonsterModels[i].SetActive(i == monsterIndex);

            AudioMGR.One.PlayEffect(_CreateClip);
            yield return new WaitUntil(() => _Animator.GetCurrentAnimatorStateInfo(0).IsTag("Idle"));


            _ColliderGO.SetActive(true);


            float moveSpeed = 0f;
            bool exceedHeightLimit = false;

            moving = true;
            while (moving)
            {
                if (heightCheck)
                {
                    if (transform.position.y >= heightLimit)
                    {
                        moving = false;

                        exceedHeightLimit = true;
                        exceedHegihtEvent?.Invoke(this);
                    }
                }

                if (moveSpeed < speed)
                    moveSpeed += Time.deltaTime;

                transform.position += transform.up * moveSpeed * Time.deltaTime;
                yield return null;
            }


            _ColliderGO.SetActive(false);

            for (int i = 0; i < _MonsterModels.Length; i++)
                _MonsterModels[i].SetActive(false);



            if (exceedHeightLimit)
                AudioMGR.One.PlayEffect(_PopClips[0]);
            else AudioMGR.One.PlayEffect(_PopClips[1]);

            _Animator.SetTrigger(hashKey_Pop);
            yield return null;
            yield return new WaitUntil(() => _Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f);


            Destroy(this.gameObject);
            yield return null;
        }
        IEnumerator coForcePop()
        {
            AudioMGR.One.PlayEffect(_PopClips[1]);

            _Animator.SetTrigger(hashKey_ForcePop);
            yield return null;
            yield return new WaitUntil(() => _Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f);


            Destroy(this.gameObject);
            yield return null;
        }
    }
}