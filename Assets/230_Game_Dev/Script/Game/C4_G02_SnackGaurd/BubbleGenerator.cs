using DoDoEng.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DoDoEng.Game.C4_G02
{
    public class BubbleGenerator : MonoBehaviour
    {

        // Properties
        public bool Closed => closed || closeCoroutine != null;



        // Methods
        public void Setup(bool closed, float speed)
        {
            this.closed = closed;
            this.speed = speed;

            _RockGO.SetActive(Closed);
        }
        public void SetText(string value)
        {
            this.text = value;
        }
        public void Create(GameObject target)
        {
            if (target != null)
            {
                var bubbleGO = Instantiate(target, _SpawnTR);
                bubbleGO.transform.localPosition = Vector3.zero;
                bubbleGO.transform.localRotation = Quaternion.identity;


                var bubble = bubbleGO.GetComponent<Bubble>();
                if (bubble != null)
                {
                    bubble.SetSpeed(speed);
                    AudioMGR.One.PlayEffect(_BubbleInClip);

                    if (bubble.BubbleType == BubbleType.Monster)
                    {
                        var mBubble = (BubbleMonster)bubble;
                        mBubble?.SetMonster();
                    }
                    else if (bubble.BubbleType == BubbleType.Text)
                    {
                        var tBubble = (BubbleText)bubble;
                        tBubble?.SetText(text);
                    }

                    bubbleList.Add(bubble);
                }
            }
        }
        public void Close(Bubble value)
        {
            if (bubbleList.Contains(value))
            {
                if (closeCoroutine == null)
                {
                    float yPos = value.transform.position.y;
                    _RockGO.transform.position = new Vector3(_RockGO.transform.position.x, yPos, _RockGO.transform.position.z);

                    closeCoroutine = StartCoroutine(coClose());
                }
            }
        }
        public void Halt(bool halt)
        {
            // Close 
            haltCloseTimer = halt;
        }
        public void PopAll(bool delay)
        {
            if (popAllCoroutine != null)
                StopCoroutine(popAllCoroutine);

            popAllCoroutine = StartCoroutine(coPopAll(delay));
        }



        // Fields
        private bool closed = false;
        private bool haltCloseTimer = false;
        private float speed = 0;
        private string text = string.Empty;
        private float rockOriginYPos = 0f;

        private List<Bubble> bubbleList = new List<Bubble>();

        private Coroutine closeCoroutine = null;
        private Coroutine popAllCoroutine = null;

        private readonly int hashKey_Landing = Animator.StringToHash("Landing");
        private readonly int hashKey_Hide = Animator.StringToHash("Hide");



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Transform _SpawnTR = null;
        [Space()]
        [SerializeField] private GameObject _RockGO = null;
        [SerializeField] private Animator _RockANI = null;
        [SerializeField] private AudioClip _RockClip = null;
        [SerializeField] private AudioClip _BubbleInClip = null;
        [SerializeField] private AudioClip _BubbleOutClip = null;
        [Header("★ Config")]
        [SerializeField] private float _CloseTime = 10f;
        [SerializeField] private float _RockSpeed = 2f;

        // Unity Messages
        private void Start()
        {
            _RockGO.SetActive(false);
            rockOriginYPos = _RockGO.transform.position.y;
        }

        // Coroutines
        IEnumerator coClose()
        {
            _RockGO.SetActive(true);

            float speed = 0f;
            while (_RockGO.transform.position.y > rockOriginYPos)
            {
                if (speed < _RockSpeed)
                    speed += Time.deltaTime * 2;

                _RockGO.transform.position += Vector3.down * speed * Time.deltaTime;
                yield return null;
            }

            _RockANI.SetTrigger(hashKey_Landing);
            AudioMGR.One.PlayEffect(_RockClip);
            yield return null;
            yield return new WaitForSeconds(0.2f);


            float time = 0f;
            while (time < _CloseTime)
            {
                if (haltCloseTimer == false)
                    time += Time.deltaTime;
                yield return null;
            }


            _RockANI.SetTrigger(hashKey_Hide);
            yield return null;
            yield return new WaitUntil(() => _RockANI.GetCurrentAnimatorStateInfo(0).IsTag("Normal"));

            _RockGO.SetActive(false);
            yield return null;


            closeCoroutine = null;
            yield return null;
        }
        IEnumerator coPopAll(bool delay)
        {
            foreach (var bubble in bubbleList)
            {
                if (bubble != null)
                {
                    bubble.Pop();
                    AudioMGR.One.PlayEffect(_BubbleOutClip);
                }


                if (delay)
                    yield return new WaitForSeconds(0.1f);
            }


            bubbleList.Clear();

            popAllCoroutine = null;
            yield return null;
        }
    }
}

