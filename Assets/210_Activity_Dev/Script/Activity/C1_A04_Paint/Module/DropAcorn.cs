using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DoDoEng.Activity.C1_A04
{
    public class DropAcorn : MonoBehaviour
    {
        // Methods
        public void DoDrop(int count)
        {
            LOG.Info($"DoDrop() | {count}", this);

            StartCoroutine(coDrop(count));
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject acornTemplateGO = null;
        [SerializeField] private RectTransform[] respawnPositionTR = null;
        [SerializeField] private Transform respawnAreaTR = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip cornDropCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float respawnRatio = 5; // 초당 생성 개수
        [SerializeField] private float scaleMin = 0.7f;
        [SerializeField] private float scaleMax = 1.2f;
        [SerializeField] private float angleMin = -45f;
        [SerializeField] private float angleMax = +45f;
        [SerializeField] private bool randomScale = true;
        [SerializeField] private bool randomRotation = true;

        // Unity Messages
        private void Awake()
        {
            acornTemplateGO.SetActive(false);
        }
        private void Start()
        {

        }

        // Unity Coroutine
        IEnumerator coDrop(int count)
        {
            using (LOG.Coroutine($"coDrop() | {count}", this))
            {
                AudioMGR.One.PlayEffectLL(cornDropCLIP, true);
                yield return null;

                var respawnPositionQ = new Queue<RectTransform>(respawnPositionTR);
                
                var intervalBase = 1 / respawnRatio;
                var acorns = count;
                while (acorns-- > 0)
                {
                    var positionTR = respawnPositionQ.Peek();
                    var position = UtilRandom.RandomPositionIn(positionTR);
                    var rotation = UtilRandom.RandomRotation(randomRotation, angleMin, angleMax);
                    var scale = UtilRandom.RandomScale(randomScale, scaleMin, scaleMax);

                    var go = Instantiate(acornTemplateGO, position, rotation, respawnAreaTR);
                    go.transform.localScale = scale;
                    go.SetActive(true);

                    respawnPositionQ.Enqueue(respawnPositionQ.Dequeue());

                    var interval = intervalBase * (1 + Random.Range(-0.2f, +0.2f));
                    yield return new WaitForSeconds(interval);
                }

                AudioMGR.One.StopEffectLL(true, 0.5f);
                yield return null;
            }
        }
    }
}