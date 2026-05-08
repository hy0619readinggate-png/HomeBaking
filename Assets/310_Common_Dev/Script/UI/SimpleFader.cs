using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Common
{
    public class SimpleFader : MonoBehaviour
    {
        // Methods
        public Coroutine FadeIn()
        {
            return StartCoroutine(coFadeIn());
        }
        public Coroutine FadeOut()
        {
            return StartCoroutine(coFadeOut());
        }
        public void FadeOutNow()
        {
            image.color = Color.black;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Image image = null;
        [Header("★ Config")]
        [SerializeField] private float duration = 0.5f;

        // Unity Messages
        private void Awake()
        {
            image.color = new Color(0, 0, 0, 0);
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coFadeIn()
        {
            yield return image.DOFade(0, duration).WaitForCompletion();
        }
        IEnumerator coFadeOut()
        {
            yield return image.DOFade(1, duration).WaitForCompletion();
        }
    }
}