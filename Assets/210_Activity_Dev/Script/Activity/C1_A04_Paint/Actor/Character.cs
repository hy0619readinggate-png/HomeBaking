using DoDoEng.Common;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 캐릭터 인터렉션을 위한 클래스 
// 현재 사용하지 않음 at 2023.08.08
namespace DoDoEng.Activity.C1_A04
{
    [RequireComponent(typeof(CharacterAni))]
    public class Character : MonoBehaviour
    {
        // Methods
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            btn.interactable = enable;
        }
        public void PlayHappy()
        {
            LOG.Info($"PlayHappy()", this);

            crPlayHappy = StartCoroutine(coPlayHappy());
        }
        public void StopHappy()
        {
            if (crPlayHappy != null)
                StopCoroutine(crPlayHappy);

            charAni.AbortAnimation();
        }

        // Events
        public event Action<Character> OnClick;



        // Fields : caching
        private CharacterAni charAni_ = null;
        private CharacterAni charAni => charAni_ ??= GetComponent<CharacterAni>();

        // Fields
        private Coroutine crPlayHappy = null;



        // Unity Inspector
        [SerializeField] private Button btn = null;

        // Unity Messages
        private void Awake()
        {
            btn.onClick.AddListener(() => OnClick?.Invoke(this));
        }
        private void Start()
        {

        }

        // Unity Coroutine
        IEnumerator coPlayHappy()
        {
            btn.interactable = false;
            yield return null;

            yield return charAni.PlayAnimationAndWait(CharacterAnimation.Happy);
            yield return null;

            btn.interactable = true;
            yield return null;
        }
    }
}