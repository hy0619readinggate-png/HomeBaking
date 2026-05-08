using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Game.Common
{
    [RequireComponent(typeof(Animator))]
    public class Round : MonoBehaviour
    {
        // Methods
        public Coroutine PlayRoundAndWait(int round)
        {
            LOG.Info($"PlayRoundAndWait() | {round}", this);

            gameObject.SetActive(true);
            return StartCoroutine(coPlayRound(round));
        }
        public void Hide()
        {
            LOG.Info($"Hide()", this);
            gameObject.SetActive(false);
        }



        // Fields : caching
        private Animator anim => GetComponent<Animator>();

        // Functions
        private void showRoundImages(int round)
        {
            for (var i = 0; i < roundImages.Length; i++)
                roundImages[i].SetActive(i + 1 == round);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject[] roundImages = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip[] sounds = null;
        [Header("★ Config")]
        [SerializeField] private float duration = 2.0f;
        [SerializeField] private float postDelay = 0.5f;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coPlayRound(int round)
        {
            using (LOG.Coroutine($"coPlayRound() | {round}", this))
            {
                showRoundImages(round); 

                AudioMGR.One.PlayEffect(sounds[round - 1]);
                anim.SetBool("show", true);
                yield return new WaitForSeconds(duration);

                anim.SetBool("show", false);
                yield return new WaitForSeconds(postDelay);
            }
        }
    }
}