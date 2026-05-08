using beyondi.Coroutine;
using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Game.C2_G02
{
    public class MonsterMGR : MonoBehaviour, ICompletable
    {
        // Properties
        public bool IsCaution
        {
            get
            {
                var monsters = GetComponentsInChildren<Monster>()
                                    .Where(m => m.IsAlive && m.MovedDistance + m.Lane * 30 > cautionMoveX );
                return monsters.Count() > 0;
            }

        }

        // Methods
        public void StartSpawn(MonsterData[] monsterData, UIProgress progress)
        {
            LOG.Info($"StartSpawn() | {string.Join(",", monsterData.Select(m => m.Phonetic))}", this);

            IsComplete = false;
            crSpawnMonster = StartCoroutine(coSpawnMonster(monsterData, progress));
        }
        public void StopSpawn()
        {
            LOG.Info($"StopSpawn()", this);

            this.StopCoroutineSafe(ref crSpawnMonster);
        }
        public void ClearAllSurvivors()
        {
            LOG.Info($"ClearAllSurvivors()", this);

            var monsters = GetComponentsInChildren<Monster>(true);
            monsters.ForEach(m => Destroy(m.gameObject));
        }

        // Methods
        public Monster DEV_GetFrontMonster()
        {
            var monsters = GetComponentsInChildren<Monster>();
            if (monsters.Length > 0)
            {
                return monsters
                        .OrderBy(m => m.transform.position.x)
                        .FirstOrDefault();
            }
            else return null;
        }



        // Fields
        private Coroutine crSpawnMonster = null;

        // Functions
        private int getAvailSpawnLane()
        {
            // 각 레인별 몬스터 수
            var counts = new int[laneTR.Length];
            for (var l = 0; l < laneTR.Length; l++)
            {
                counts[l] = laneTR[l].GetComponentsInChildren<Monster>().Length;

                if (!CannonMGR.One.IsCannonAlive(l + 1)) // 대포가 죽었으면, 가중치를 많이 낮춤
                    counts[l] += 100;
            }

            // 몬스터가 제일 적은 레인중 하나 추출
            var minIndics = counts
                .Select((c, i) => new { Count = c, Index = i })
                .GroupBy(x => x.Count)
                .OrderBy(g => g.Key)
                .First()
                .Select(x => x.Index)
                .ToArray();

            return UtilArray.ExtractOne(minIndics) + 1;
        }

        // Event Handlers
        private void plant_OnDied()
        {
            LOG.Info($"plant_OnDied()", this);

            this.StopCoroutineSafe(ref crSpawnMonster);

            var monsters = GetComponentsInChildren<Monster>(true);
            monsters.ForEach(m => m.Halt());
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Monster monsterPB = null;
        [SerializeField] private Transform[] laneTR = null;
        [SerializeField] private float cautionMoveX = 600;

        // Unity Messages
        private void Awake()
        {
            laneTR.ForEach(tr => Util.RemoveAllChildren(tr));
        }
        private void Start()
        {

        }
        private void OnEnable()
        {
            EventBus.Subscribe<EventBus.PlantDiedEvent>(plant_OnDied);
        }
        private void OnDisable()
        {
            EventBus.Unsubscribe<EventBus.PlantDiedEvent>(plant_OnDied);
        }

        // Unity Coroutine
        IEnumerator coSpawnMonster(MonsterData[] monsterData, UIProgress progress)
        {
            using (LOG.Coroutine($"coSpawnMonster()", this))
            {
                var id = 0;
                foreach (var md in monsterData)
                {
                    var lane = getAvailSpawnLane();
                    var parentTR = laneTR[lane - 1];
                    var monster = Instantiate(monsterPB, parentTR);
                    monster.gameObject.name = $"Monster #{++id} L{lane}";
                    monster.transform.localPosition = Vector3.zero;
                    monster.transform.localRotation = Quaternion.identity;
                    monster.Setup(md, lane);

                    progress.Increase();
                    yield return new WaitForSeconds(md.MonsterInterval);
                }

                while (!IsComplete)
                {
                    var monsters = GetComponentsInChildren<Monster>();
                    if (monsters.Length == 0)
                        IsComplete = true;

                    yield return new WaitForSeconds(1f);
                }

                yield return null;
            }
        }



        // Interface : ICompletable
        public bool IsComplete { get; private set; }
    }
}
