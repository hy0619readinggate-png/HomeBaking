using beyondi.Util;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C1_A12
{
    public class SheepMGR : MonoBehaviour
    {
        // Methods
        public void InitSheeps(int[] types)
        {
            LOG.Info($"InitSheeps() | {string.Join(",", types)}", this);

            for (var i = 0; i < types.Length; i++)
                spawnSheep(types[i], i + 1);
        }
        public Vector3 GetRespawnPosition(out Vector3 startPos)
        {
            var pos = getRespawnPosition();
            startPos = new Vector3(pos.x, respawnTR.position.y);

            return pos;
        }
        public Vector3? GetMovingPosition(Vector3 position, float maxDistance)
        {
            return getMovingPosition(position, maxDistance);
        }
        public void ClearSheeps()
        {
            var sheeps = parentTR.GetComponentsInChildren<Sheep>();
            sheeps.ForEach(s => Destroy(s.gameObject));
        }

        // Methods
        public bool IsRemainFreeSheep()
        {
            var sheeps = parentTR.GetComponentsInChildren<Sheep>();
            return sheeps.Any(s => s.IsFree);
        }



        // Fields
        private Queue<BoxCollider2D> spawnQ = new Queue<BoxCollider2D>();

        // Functions
        private void spawnSheep(int type, int id)
        {
            var position = getRespawnPosition();
            var sheep = Instantiate(sheepPB[type - 1], position, Quaternion.identity, parentTR);
            sheep.gameObject.name = $"Sheep_{id}";
            sheep.gameObject.SetActive(true);
        }
        private Vector3 getRespawnPosition()
        {
            // 큐가 비어있으면 랜덤하게 다시 채우기
            if (spawnQ.Count < 1)
            {
                var count = spawnCOL.Length;
                var L = spawnCOL.Take(count / 2).ToArray();
                var R = spawnCOL.Skip(count / 2).ToArray();

                var L1 = UtilArray.ExtractWithRemain(L, L.Length / 2, out var L2);
                var R1 = UtilArray.ExtractWithRemain(R, R.Length / 2, out var R2);

                var first = UtilArray.Shuffled(Enumerable.Concat(L1, R1).ToArray());
                var second = UtilArray.Shuffled(Enumerable.Concat(L2, R2).ToArray());

                first.ForEach(c => spawnQ.Enqueue(c));
                second.ForEach(c => spawnQ.Enqueue(c));
            }

            // 컬라이더내에 랜덤 위치 반환
            var col = spawnQ.Dequeue();
            return getRandomPositionInCollider(col);
        }
        private Vector3? getMovingPosition(Vector3 origin, float maxDistance)
        {
            origin = new Vector3(origin.x, origin.y, 0);

            const int maxAttempt = 40;
            for (var i = 0; i < maxAttempt; i++)
            {
                var position = getRandomPositionInCollider(boundCOL);
                var dist = Vector3.Distance(origin, position);
                if (dist > maxDistance) continue;

                var collider = Physics2D.OverlapCircle(position, overlapRadius, layerMask);
                if (collider == null)
                    return position;
            }

            LOG.Warning($"No avail position for sheep", this);
            return null;
        }

        // Functions
        private Vector3 getRandomPositionInCollider(BoxCollider2D areaCOL)
        {
            var x = Random.Range(0f, 1f);
            var y = Random.Range(0f, 1f);
            var pos = new Vector3(
                Mathf.Lerp(areaCOL.bounds.min.x, areaCOL.bounds.max.x, x),
                Mathf.Lerp(areaCOL.bounds.min.y, areaCOL.bounds.max.y, y));

            return pos;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Transform parentTR = null;
        [SerializeField] private Sheep[] sheepPB = null;
        [SerializeField] private BoxCollider2D boundCOL = null;
        [SerializeField] private BoxCollider2D[] spawnCOL = null;
        [SerializeField] private Transform respawnTR = null;
        [Header("★ Config")]
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private float overlapRadius = 1;

        // Unity Messages
        private void Awake()
        {
            sheepPB.ForEach(s => s.gameObject.SetActive(false));
        }
        private void Start()
        {

        }
    }
}