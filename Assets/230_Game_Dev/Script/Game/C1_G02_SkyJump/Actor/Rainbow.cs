using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using DoDoEng.Game.UI;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Game.C1_G02
{
    public class Rainbow : MonoBehaviour
    {
        // Methods
        public void Setup(AudioClip clip)
        {
            LOG.Info($"Setup()", this);

            problemCLIP = clip;
        }
        public void SetExampleClouds(Cloud[] clouds)
        {
            LOG.Info($"SetExampleClouds()", this);

            this.clouds = clouds;
        }
        public void Complete()
        {
            LOG.Info($"Complete()", this);

            anim.SetTrigger("Hide");
            clouds.ForEach(c => c.Disappear());
        }



        // Fields
        private AudioClip problemCLIP = null;
        private Cloud[] clouds = null;

        // Event Handlers
        private void platform_OnPlayerEnter(Player player)
        {
            LOG.Info($"platform_OnPlayerEnter()", this);

            UIGameCommon.One.Progress.Increase();
            StartCoroutine(coPlayProblemSound(player));
        }
        private void platform_OnPlayerExit(Player player)
        {
            LOG.Info($"platform_OnPlayerExit()", this);

            // 레인보우를 출발해서, 오답 보기를 선택하거나, 떨어지면, 보기 층 스킵을 위해서
            player.IncreaseMaxHeight(2);

            anim.SetTrigger("Hide");
            DOVirtual.DelayedCall(0.5f, () => gameObject.SetActive(false));
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Platform platform = null;
        [SerializeField] private Animator anim = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
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
        IEnumerator coPlayProblemSound(Player player)
        {
            using (LOG.Coroutine($"coPlayProblemSound()", this))
            {
                player.AutoJump = false;
                yield return new WaitForSeconds(0.5f);

                if (problemCLIP != null)
                {
                    yield return AudioMGR.One.PlayNarrationAndWait(problemCLIP);
                    yield return AudioMGR.One.PlayNarrationAndWait(problemCLIP);
                    yield return AudioMGR.One.PlayNarrationAndWait(problemCLIP);
                }
                yield return new WaitForSeconds(0.1f);
                yield return null;

                player.AutoJump = true;
                yield return null;
            }
        }
    }
}