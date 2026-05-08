using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.Game.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using GameData = DoDoEng.Common.GameData_C2_G02;

namespace DoDoEng.Game.C2_G02
{
    public class C2_G02_ProblemMGR : GameProblemMGR<GameData, ProblemData>
    {
        // Propertes
        public int[] MonsterCounts => Problems.Select(p => p.MonsterCount).ToArray();
        public int TotalProblemCount => MonsterCounts.Sum();

        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(GameList curriculum, GameData[] tables)
        {
            using (LOG.Coroutine($"Build() | {GameIDX}", this))
            {
                config = GameIDX.GameMode == GameMode.Playground ? playGroundConfig : reviewConfig;

                // 문제 데이터 생성
                var round = 1;
                var problemList = new List<ProblemData>();
                foreach (var config in config.RoundConfigs)
                {
                    var phoneticsPool = tables.Select(t => t.Text).Distinct().ToArray();
                    var phonetics =
                        config.PhoneticValueCount == -1
                        ? phoneticsPool
                        : UtilArray.Extract(phoneticsPool, config.PhoneticValueCount);

                    var tablesPhonetics = tables.Where(t => phonetics.Contain(t.Text)).ToArray();

                    // 몬스터 데이터 생성
                    var tablesMonster = UtilArray.Extract(tablesPhonetics, config.MonsterCount);
                    var monsters = new List<MonsterData>();
                    foreach (var t in tablesMonster)
                    {
                        monsters.Add(new MonsterData
                        {
                            Phonetic = t.Text,
                            SoundPhonetic = t.SoundPhonetic,
                            PhoneticCLIP = await loadSound(t.SoundPhonetic),

                            MonsterInterval = config.MonsterInterval.RandomValue(),
                            MonsterSpeed = config.MonsterSpeed,
                            MonsterType = config.MonsterType
                        });
                    }

                    // 탄환 데이터 생성
                    var bullets = new List<BulletData>();
                    foreach (var t in tablesPhonetics)
                    {
                        bullets.Add(new BulletData
                        {
                            Phonetic = t.Text,
                            SoundPhonetic = t.SoundPhonetic,
                            Word = t.Word,
                            WordSPR = await loadSprite(t.Image),
                            PhoneticCLIP = await loadSound(t.SoundPhonetic),
                            WordCLIP = await loadSound(t.SoundWord)
                        });
                    }

                    problemList.Add(new ProblemData
                    {
                        Round = round++,
                        ConveyorSpeed = config.ConveyorSpeed,
                        Monsters = monsters.ToArray(),
                        Bullets = bullets.ToArray()
                    });
                }

                return problemList.ToArray();
            }
        }



        // Fields
        private ConfigSO config = null;



        // Unity Inspectors
        [Header("★ Configs")]
        [SerializeField] private ConfigSO playGroundConfig = null;
        [SerializeField] private ConfigSO reviewConfig = null;
    }

    public class ProblemData
    {
        public int Round;
        public float ConveyorSpeed;
        public MonsterData[] Monsters;
        public BulletData[] Bullets;

        public int MonsterCount => Monsters.Count();
        public BulletData DEV_GetBullet(string soundPhonics)
        {
            return Bullets.Where(b => b.SoundPhonetic == soundPhonics).FirstOrDefault();
        }

        public override string ToString()
        {
            var monsterStr = string.Join(",", Monsters.Select(m => m.Phonetic));
            var bulletStr = string.Join(",", Bullets.Select(b => b.Word));

            return $"<color=red>ProblemData [{Round} | ({monsterStr}) ({bulletStr})]</color>";
        }
    }

    public class MonsterData
    {
        public string Phonetic;
        public string SoundPhonetic;
        public AudioClip PhoneticCLIP;

        public float MonsterInterval = 5;
        public MonsterType MonsterType = MonsterType.Turtle;
        public float MonsterSpeed = 2;

        public override string ToString()
        {
            return $"<color=red>MonsterData [{Phonetic}]</color>";
        }
    }

    public class BulletData
    {
        public string Phonetic;
        public string SoundPhonetic;
        public string Word;
        public Sprite WordSPR;
        public AudioClip PhoneticCLIP;
        public AudioClip WordCLIP;

        public override string ToString()
        {
            return $"<color=red>BulletData [{Phonetic} | {Word}]</color>";
        }
    }
}