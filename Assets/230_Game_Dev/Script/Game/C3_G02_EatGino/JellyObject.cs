using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Game.C3_G02
{
    public enum JellyType { Normal = 0, Rare, Epic }

    public class JellyObject : MonoBehaviour
    {

        // Properties
        public bool IsShowing => isShowing;
        public JellyType JellyType => jellyType;
        public int JellyIdx = 0;
        public bool IsCorrectHintSet = false;



        // Methods
        public void Init(wordData data, int level, int jellyNum)
        {
            //if (jellyNum == 0) IsCorrectHintSet = true;

            for (int i = 0; i < _TypeObjects.Length; i++)
                _TypeObjects[i].SetActive(i == level);

            jellyType = (JellyType)System.Enum.ToObject(typeof(JellyType), level);
            foreach (var img in _JellyImage) img.sprite = data.WordIMG;
            jellySound = data.SoundClip;
            JellyIdx = jellyNum;
        }
        public void BlinkHint(float blinkTime)
        {
            StartCoroutine(blinkHint(blinkTime));
        }
        public void Show()
        {
            if (isShowing)
                return;

            isShowing = true;

            _Animator.SetTrigger(hashKey_Show);
            PlaySound();
        }
        public void Hide()
        {
            Destroy(this.gameObject);
        }
        public void PlaySound()
        {
            if (isShowing) AudioMGR.One.PlayNarration(jellySound);
        }



        // Fields : caching
        // Fields
        private bool isShowing = false;
        private AudioClip jellySound = null;
        private JellyType jellyType = JellyType.Normal;


        // Fields : ani
        private readonly int hashKey_Show = Animator.StringToHash("Show");

        // Functions
        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Animator _Animator = null;
        [SerializeField] private Animator _HintAnim = null;
        [SerializeField] private SpriteRenderer[] _JellyImage = null;
        //[Space()]
        //[SerializeField] private GameObject _CorrectHintFX = null;
        //[SerializeField] private GameObject _NormalHintFX = null;
        [Space()]
        [SerializeField] private GameObject[] _TypeObjects = null;
        [Header("★ Configs")]
        [SerializeField] private float hintDelayTime = 5.0f;

        // Unity Messages
        private void Awake()
        {

        }
        private void Start()
        {

        }

        // Unity Coroutine
        IEnumerator blinkHint(float blinkTime)
        {
            yield return new WaitForSeconds(blinkTime);

            while (!isShowing)
            {
                //if (IsCorrectHintSet)
                //{
                //    _CorrectHintFX.SetActive(true);
                //    _NormalHintFX.SetActive(false);
                //    IsCorrectHintSet = false;
                //}

                _HintAnim.SetTrigger(hashKey_Show);
                yield return new WaitForSeconds(hintDelayTime + 1f);
            }
        }
    }
}
