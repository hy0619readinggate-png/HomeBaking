using DG.Tweening;
using System;
using UnityEngine;

namespace DoDoEng.Common
{
    public class GetCoinEffect : MonoBehaviour
    {
        // Methods
        public void SpawnAndFly(Vector2 from, Vector2 to,
                                float spawnDuration = 0.25f, float spawnRange = 1.2f, float waitDuration = 0.2f, float flyDuration = 0.6f,
                                AudioClip spawnClip = null, AudioClip flyClip = null,
                                Ease spawnEasing = Ease.OutCirc,
                                Ease flyEasing = Ease.InQuad,
                                Action onCompleteCB = null)
        {
            var tSpawn = transform
                .DOMove(from + UnityEngine.Random.insideUnitCircle * spawnRange, spawnDuration)
                .From(from)
                .SetEase(spawnEasing)
                .OnStart(() => AudioMGR.One.PlayEffect(spawnClip));

            var tFly = transform
                .DOMove(to, flyDuration)
                .SetEase(flyEasing)
                .OnStart(() => AudioMGR.One.PlayEffect(flyClip));

            sequence = DOTween.Sequence();
            sequence.Append(tSpawn);
            sequence.AppendInterval(waitDuration);
            sequence.Append(tFly);
            sequence.AppendCallback(() => { onCompleteCB?.Invoke(); Destroy(gameObject); });
        }



        // Fields
        private Sequence sequence;


        // Unity Messages
        private void Awake()
        {
        }
        private void OnDestroy()
        {
            sequence?.Kill();
        }
    }
}