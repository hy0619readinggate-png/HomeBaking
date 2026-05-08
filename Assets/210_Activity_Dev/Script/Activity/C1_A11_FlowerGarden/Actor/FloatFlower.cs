using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Activity.C1_A11
{
    public class FloatFlower : MonoBehaviour
    {
        // Methods
        public void Fly(Flower originFlower, FlowerFloatParam param, AudioClip phoneticCLIP)
        {
            LOG.Info($"Fly() ", this);

            flower.SetupFrom(originFlower);
            StartCoroutine(coFly(param, phoneticCLIP));
        }



        // Fields : caching
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= GetComponent<CanvasGroup>();
        private Flower flower_ = null;
        private Flower flower => flower_ ??= GetComponent<Flower>();



        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coFly(FlowerFloatParam param, AudioClip phoneticCLIP)
        {
            using (LOG.Coroutine($"coFly()", this))
            {
                AudioMGR.One.PlayEffectLL(param.zoomCLIP);
                yield return null;

                yield return transform
                    .DOScale(
                        param.zoomScale,
                        param.zoomDuration)
                    .SetEase(Ease.OutExpo)
                    .WaitForCompletion();

                transform
                    .DOShakeScale(
                        param.shakeDuration,
                        param.shakeStrength,
                        param.shakeVibrato,
                        randomness: 0,
                        randomnessMode: ShakeRandomnessMode.Harmonic)
                    .SetDelay(param.shakeDelay);
                yield return AudioMGR.One.PlayNarrationAndWait(phoneticCLIP);

                AudioMGR.One.PlayEffectLL(param.jumpCLIP);
                yield return null;

                transform.DOScale(
                    param.jumpScale,
                    param.jumpDuration * 0.5f);
                cg.DOFade(
                    param.jumpAlpha,
                    param.jumpDuration);
                yield return rt
                    .DOJump(
                        Gino.One.ReserveFlowerPosition(),
                        param.jumpPower,
                        1,
                        param.jumpDuration)
                    .WaitForCompletion();

                Gino.One.AddFlower(flower.TypeIndex, flower.ColorIndex, flower.Alphabet);
                yield return null;

                Destroy(gameObject);
            }
        }
    }
}