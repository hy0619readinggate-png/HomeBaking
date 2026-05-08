using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C1_A09
{
    public class PopcornSpawner : MonoBehaviour
    {
        // Methods
        public void Spawn(Popcorn popcorn)
        {
            LOG.Info($"Spawn()", this);

            var position = UtilRandom.RandomPositionIn(rt);
            var speed = Random.Range(speedMin, speedMax);
            var angle = Random.Range(angleMin, angleMax);

            popcorn.transform.position = position;
            popcorn.Fire(speed, angle, gravityScale);
        }



        // Fields : caching
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private float speedMin = 10f;
        [SerializeField] private float speedMax = 12f;
        [SerializeField] private float angleMin = -15f;
        [SerializeField] private float angleMax = +15f;
        [SerializeField] private float gravityScale = 0.7f;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}