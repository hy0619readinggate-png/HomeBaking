using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.Game.Framework;
using FlexFramework.Excel;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using GameData = DoDoEng.Common.GameData_C1_G01;

namespace DoDoEng.Game.C1_G01
{
    public class C1_G01_ProblemMGR : GameProblemMGR<GameData, ProblemData>
    {
        // Propertes
        public int[] CustomerCounts => Problems.Select(p => p.CustomerCount).ToArray();
        public int TotalProblemCount => CustomerCounts.Sum();
        public RoundConfig Config => config.RoundConfigs[PNO - 1];
        public int InitialHP => config.InitialHP;



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
                    var iceCreamCount = config.IceCreamCount;
                    LOG.Assert(
                        iceCreamCount >= 3 && iceCreamCount <= 6,
                        "IceCreamCount must >=3 and <=6.", this);

                    if (GameIDX.GameMode == GameMode.Review) // 리뷰 게임에서는 아이스크림을 3개로 고정
                        iceCreamCount = 3;

                    // 문제풀에서 Avoid 제외
                    var extractedAll = UtilArray.Extract(tables, tables.Length);
                    var tableAvoided = new List<GameData>();
                    foreach (var t in extractedAll)
                    {
                        t.AvoidTexts.ForEach(t => LOG.Info($"{t}", this));
                        var avoid = tableAvoided.Any(e => t.AvoidTexts.Contain(e.Text1))
                                    || tableAvoided.Any(e => e.AvoidTexts.Contain(t.Text1));
                        if (!avoid)
                            tableAvoided.Add(t);
                    }

                    LOG.Assert(
                        tableAvoided.Count() >= iceCreamCount,
                        $"tableAvoided count must >= {iceCreamCount}.", this);

                    var tableExtracted = UtilArray.Extract(tableAvoided.ToArray(), iceCreamCount);
                    var iceCreamList = new List<IceCreamData>();
                    foreach (var td in tableExtracted)
                    {
                        var id = new IceCreamData
                        {
                            Alphabet = td.Text1.ToLower(),
                            ColorID = 1,
                            SoundCLIP = await loadSound(td.SoundWord)
                        };
                        iceCreamList.Add(id);
                    }
                    for (var i = 0; i < 6 - iceCreamCount; i++)
                        iceCreamList.Add(null);

                    var iceCreams = iceCreamList.ToArray();
                    setupColor(iceCreams);

                    problemList.Add(new ProblemData
                    {
                        Round = round++,
                        IceCreams = iceCreams,
                        Customers = buildCustomer(iceCreams, config)
                    });
                }

                return problemList.ToArray();
            }
        }



        // Fields
        private ConfigSO config = null;

        // Functions 
        private void setupColor(IceCreamData[] data)
        {
            const int COL = 3;
            for (var i = 0; i < COL; i++)
            {
                var reverse = UtilRandom.RandomSuccess(0.5f);

                data[i].ColorID = i + 1 + (reverse ? COL : 0);
                if (data[i + COL] != null)
                    data[i + COL].ColorID = i + 1 + COL - (reverse ? COL : 0);
            }
        }
        private CustomerData[] buildCustomer(IceCreamData[] iceCreams, RoundConfig config)
        {
            // 일반 손님
            var pool = iceCreams.Where(ic => ic != null).ToArray();
            var list = new List<CustomerData>();
            for (var c = 0; c < config.CustomerCount; c++)
            {
                var orderCount = config.OrderAlphabet.RandomOne();
                var order = UtilArray.Extract(pool, orderCount);
                list.Add(new CustomerData
                {
                    OrderIceCreams = order,
                    Duration = config.CustomerDuration,
                    Interval = config.CustomerInterval
                });
            }

            // 도둑팡을 문제 생성시 생성하지 않음 (#2259)
            //// 도둑 팡 (4~8번째 손님 교체)
            //var thiefOrder = config.ThiefOrder.Randome(config.ThiefCount);
            //foreach (var order in thiefOrder)
            //{
            //    list.Add(list[order - 1]);
            //    list[order - 1] = new CustomerData
            //    {
            //        OrderIceCreams = null,
            //        Duration = config.ThiefDuration,
            //        Interval = config.CustomerInterval
            //    };
            //}

            return list.ToArray();
        }



        // Unity Inspectors
        [Header("★ Configs")]
        [SerializeField] private ConfigSO playGroundConfig = null;
        [SerializeField] private ConfigSO reviewConfig = null;
    }

    public class ProblemData
    {
        public int Round;
        public IceCreamData[] IceCreams;
        public CustomerData[] Customers;

        public int CustomerCount => Customers.Where(c => !c.IsThief).Count();

        public override string ToString()
        {
            var iceCreamStr = string.Join(",", IceCreams.Where(ice => ice != null).Select(ice => ice.Alphabet));
            var customerStr = string.Join(",", Customers.Select(c => c.IsThief ? "X" : "O"));

            return $"<color=red>ProblemData [{Round} | ({iceCreamStr}) ({customerStr})]</color>";
        }
    }
    public class IceCreamData
    {
        public string Alphabet;
        public int ColorID;
        public AudioClip SoundCLIP;

        public override string ToString()
        {
            return $"<color=red>IceCreamData [{Alphabet} | {ColorID}]</color>";
        }
    }
    public class CustomerData
    {
        public IceCreamData[] OrderIceCreams;
        public float Duration;
        public float Interval;

        public bool IsThief => OrderIceCreams == null;

        public override string ToString()
        {
            var alphabets = IsThief
                ? "THIEF"
                : string.Join(",", OrderIceCreams.Select(o => o.Alphabet));
            return $"<color=red>CustomerData [{alphabets} {Duration} {Interval}]</color>";
        }
    }
}