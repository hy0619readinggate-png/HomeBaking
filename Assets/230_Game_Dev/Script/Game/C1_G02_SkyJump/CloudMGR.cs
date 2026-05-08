using beyondi.Util;
using DoDoEng.Activity.C1_A10;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Game.C1_G02
{
    public enum CloudStyle
    {
        Cloud3_Fix,
        Cloud2_Fix,
        Cloud2_Move,
        Cloud1_Fix,
        Cloud1_Move,
    }

    public class CloudMGR : MonoBehaviour
    {
        // Methods
        public void BuildLevel(ProblemData[] problems, RoundConfig[] rounds)
        {
            LOG.Info($"BuildLevel()", this);

            if (debugGenerating)
            {
                buildForDebug();
                return;
            }

            var problemQ = new Queue<ProblemData>(problems);
            var y = basePositionTR.position.y + floorHeight;
            var floor = 0;
            var heights = new List<float> { 0, y };
            var levelUpFloorList = new List<int>();

            foreach (var round in rounds)
            {
                for (var p = 0; p < round.ProblemCount; p++)
                {
                    // 상승 구간
                    var parentGO = new GameObject($"Floor ({floor + 1:D3} ~ {floor + round.RisingFloor:D3})");
                    parentGO.transform.SetParent(parentTR);
                    for (var f = 0; f < round.RisingFloor; f++)
                    {
                        var cloudStyles = round.CloudStyles;
                        if (f == 3) // #452 난이도 조정을 위해, 정답을 맞춘 후, 부스트 효과로 도착하는 곳을 구름 3개로
                            cloudStyles = new CloudStyle[] { CloudStyle.Cloud3_Fix };

                        generateFloor(++floor, ref y, cloudStyles, round.FastMoveProbability, parentGO.transform);
                        heights.Add(y);
                    }

                    // 부스터
                    var cloudInFloors = parentGO.GetComponentsInChildren<Cloud>()
                        .Where(c => c.Floor != floor)
                        .Where(c => !c.IsMoving)
                        .ToArray();
                    var cloudBooster = UtilArray.ExtractOnlyInSourceCount(cloudInFloors, round.BoosterPerRising);
                    foreach (var c in cloudBooster)
                    {
                        var booster = createBooster(c, parentGO.transform);
                        c.Booster = booster;
                    }
                        

                    // 문제 (레인보우)
                    var problem = problemQ.Dequeue();
                    var rainbow = generateProblem(++floor, ref y, problem);
                    heights.Add(y);

                    // 보기
                    generateExample(++floor, ref y, rainbow, problem.Examples);
                    heights.Add(y);

                    if (problemQ.Count == 0) break;
                }

                {
                    // 상승 구간 (레벨 종료)
                    var parentGO = new GameObject($"Floor ({floor + 1:D3} ~ {floor + round.RisingFloor:D3})");
                    parentGO.transform.SetParent(parentTR);
                    for (var f = 0; f < round.RisingFloor; f++)
                    {
                        var cloudStyles = round.CloudStyles;
                        if (f == 3) // #452 난이도 조정을 위해, 정답을 맞춘 후, 부스트 효과로 도착하는 곳을 구름 3개로
                            cloudStyles = new CloudStyle[] { CloudStyle.Cloud3_Fix };

                        generateFloor(++floor, ref y, cloudStyles, round.FastMoveProbability, parentGO.transform);
                        heights.Add(y);
                    }

                    // 레벨업 구간
                    if (round != rounds.Last())
                    {
                        generateLevelup(ref y);
                        levelUpFloorList.Add(floor);
                    }
                }

                if (problemQ.Count == 0) break;
            }

            // 최상층
            locateCastle(y);

            // 정보 저장
            clouds = parentTR.GetComponentsInChildren<Cloud>();
            heightsForFloor = heights.ToArray();
            levelUpFloors = levelUpFloorList.ToArray();
            topFloor = floor;

            LOG.VeryImportant($"Max height : {y}", this);
            LOG.VeryImportant($"Top Floor  : {topFloor}", this);
        }
        public Cloud GetCloudAtHeight(float height)
        {
            var floor = getFloorForHeight(height);
            var cloud = getCloudOnFloor(floor);
            while (cloud == null && floor <= heightsForFloor.Length)
            {
                floor++;
                cloud = getCloudOnFloor(floor);
            }

            return cloud;
        }

        // Methods : for debug
        public Cloud GetCloudForNextLevelUp(float height)
        {
            var floor = getFloorForHeight(height);
            var floorsLevelUp = levelUpFloors.Where(f => f > floor + 2);

            if (floorsLevelUp.Count() > 0)
            {
                var floorLevelUp = floorsLevelUp.Min() - 2;
                return getCloudOnFloor(floorLevelUp);
            }
            else return null;
        }
        public Cloud GetCloudAtTop()
        {
            return getCloudOnFloor(topFloor - 2);
        }



        // Fields
        private Cloud[] clouds = null;
        private float[] heightsForFloor;    // 각 층의 높이
        private int[] levelUpFloors;        // 레벨업 시작하는 층수
        private int topFloor;

        // Functions
        private void generateFloor(int floor, ref float y, CloudStyle[] styles, float fastMoveProbability, Transform parentTR)
        {
            var style = UtilArray.ExtractOne(styles);
            var fast = UtilRandom.RandomSuccess(fastMoveProbability);

            switch (style)
            {
                case CloudStyle.Cloud3_Fix:
                    {
                        for (var col = 1; col <= 3; col++)
                            createCloud(floor, y, col, parentTR);
                    }
                    break;

                case CloudStyle.Cloud2_Fix:
                    {
                        var cols = UtilArray.Random(1, 3, 2);
                        foreach (var col in cols)
                            createCloud(floor, y, col, parentTR);
                    }
                    break;

                case CloudStyle.Cloud2_Move:
                    {
                        var isLeft = UtilRandom.RandomSuccess(0.5f);
                        if (isLeft)
                        {
                            createCloud(floor, y, 1, parentTR);
                            createCloud(floor, y, 2, 3, fast, parentTR);
                        }
                        else
                        {
                            createCloud(floor, y, 1, 2, fast, parentTR);
                            createCloud(floor, y, 3, parentTR);
                        }
                    }
                    break;

                case CloudStyle.Cloud1_Fix:
                    {
                        var col = UtilArray.RandomOne(1, 3);
                        createCloud(floor, y, col, parentTR);
                    }
                    break;

                case CloudStyle.Cloud1_Move:
                    {
                        var isNarrow = UtilRandom.RandomSuccess(0.5f);
                        if (isNarrow)
                        {
                            var isLeft = UtilRandom.RandomSuccess(0.5f);
                            if (isLeft)
                                createCloud(floor, y, 1, 2, fast, parentTR);
                            else createCloud(floor, y, 2, 3, fast, parentTR);
                        }
                        else createCloud(floor, y, 1, 3, fast, parentTR);
                    }
                    break;
            }

            y += floorHeight;
        }
        private Rainbow generateProblem(int floor, ref float y, ProblemData problem)
        {
            var rainbow = createRainbow(floor, y, problem.SoundCLIP);

            y += floorHeight;
            return rainbow;
        }
        private void generateExample(int floor, ref float y, Rainbow rainbow, ExampleData[] examples)
        {
            var clouds = new List<Cloud>();
            foreach (var (ex, i) in examples.Select((ex, i) => (ex, i)))
            {
                var cloud = createExampleCloud(floor, y, i + 1, ex.Text, ex.IsAnswer, rainbow);
                clouds.Add(cloud);
            }

            rainbow.SetExampleClouds(clouds.ToArray());

            y += floorHeight;
        }
        private void generateLevelup(ref float y)
        {
            var x = xPositionTR[1].position.x;
            var height = levelUpHeight;
            var yN = y + height / 2;

            var trigger = Instantiate(levelUpTrigger, new Vector3(x, yN, 0), Quaternion.identity, parentTR);
            trigger.transform.localScale = new Vector3(
                trigger.transform.localScale.x,
                height,
                trigger.transform.localScale.z);

            y += levelUpHeight;
        }
        private void locateCastle(float y)
        {
            castleTR.position =
                new Vector3(
                    castleTR.position.x,
                    y,
                    castleTR.position.z);
        }

        // Functions
        private Cloud createCloud(int floor, float y, int col, Transform parentTR)
        {
            var x = xPositionTR[col - 1].position.x;
            var pb = UtilArray.ExtractOne(cloudPB);
            var cloud = Instantiate(pb, new Vector3(x, y, 0), Quaternion.identity, parentTR);
            cloud.gameObject.name = $"Cloud(F{floor})";
            cloud.Floor = floor;

            return cloud;
        }
        private Cloud createCloud(int floor, float y, int col1, int col2, bool fast, Transform parentTR)
        {
            var x1 = xPositionTR[col1 - 1].position.x;
            var x2 = xPositionTR[col2 - 1].position.x;
            var pb = UtilArray.ExtractOne(cloudPB);
            var cloud = Instantiate(pb, new Vector3(x1, y, 0), Quaternion.identity, parentTR);
            cloud.gameObject.name = $"Cloud(F{floor})";
            cloud.Floor = floor;
            cloud.SetupMoving(x1, x2, y, fast ? moveVelocityFast : moveVelocitySlow);

            return cloud;
        }
        private Cloud createExampleCloud(int floor, float y, int col, string phonetic, bool isAnswer, Rainbow rainbow)
        {
            var x = xPositionTR[col - 1].position.x;
            var pb = UtilArray.ExtractOne(cloudPB);
            var cloud = Instantiate(pb, new Vector3(x, y, 0), Quaternion.identity, parentTR);
            cloud.gameObject.name = $"Cloud(F{floor})";
            cloud.Floor = floor;
            cloud.SetupExample(phonetic, isAnswer, rainbow);

            return cloud;
        }
        private Rainbow createRainbow(int floor, float y, AudioClip clip)
        {
            var x = xPositionTR[1].position.x;
            var rainbow = Instantiate(rainbowPB, new Vector3(x, y, 0), Quaternion.identity, parentTR);
            rainbow.gameObject.name = $"Rainbow(F{floor})";
            rainbow.Setup(clip);

            return rainbow;
        }
        private Booster createBooster(Cloud cloud, Transform parentTR)
        {
            var cloudPos = cloud.transform.position;
            var position = new Vector3(cloudPos.x, cloudPos.y + boosterOffsetY, cloudPos.z - 0.1f);
            var booster = Instantiate(boosterPB, position, Quaternion.identity, parentTR);
            booster.gameObject.name = $"Booster for {cloud.gameObject.name}";

            return booster;
        }

        // Functions
        private Cloud getCloudOnFloor(int floor)
        {
            var cloudsOnFloor = clouds
                .Where(c => c.Floor == floor)
                .Where(c => c.IsActive)
                .ToArray();

            if (cloudsOnFloor.Length > 0)
                return UtilArray.ExtractOne(cloudsOnFloor);
            else return null;
        }
        private int getFloorForHeight(float height)
        {
            for (var f = 1; f < heightsForFloor.Length; f++)
            {
                if (heightsForFloor[f] > height)
                    return f - 1;
            }

            return heightsForFloor.Length;
        }

        // Functions
        private void buildForDebug()
        {
            var heights = new List<float>();
            var y = basePositionTR.position.y;

            for (var floor = 1; floor <= debugFloorCount; floor++)
            {
                heights.Add(y += floorHeight);
                generateFloor(floor, ref y, debugCloudStyles, debugFastProbability, parentTR);
            }

            locateCastle(y);

            clouds = parentTR.GetComponentsInChildren<Cloud>();
            heightsForFloor = heights.ToArray();
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Cloud[] cloudPB = null;
        [SerializeField] private Rainbow rainbowPB = null;
        [SerializeField] private Booster boosterPB = null;
        [SerializeField] private LevelUpTrigger levelUpTrigger = null;
        [SerializeField] private Transform castleTR = null;
        [SerializeField] private Transform parentTR = null;
        [SerializeField] private Transform basePositionTR = null;
        [SerializeField] private Transform[] xPositionTR = null;
        [Header("★ Config")]
        [SerializeField] private float floorHeight = 4.5f;      // 한층의 높이
        [SerializeField] private int levelUpHeight = 45;        // 레벨업 높이
        [SerializeField] private float moveVelocityFast = 2f;   // 빠른 구름의 속도
        [SerializeField] private float moveVelocitySlow = 1f;   // 느린 구름의 속도
        [SerializeField] private float boosterOffsetY = 1.2f;   // 부스터 오프셋
        [Header("★ Dev.")]
        [SerializeField] private bool debugGenerating = false;
        [SerializeField] private CloudStyle[] debugCloudStyles;
        [SerializeField] private float debugFastProbability = 0;
        [SerializeField] private int debugFloorCount = 20;

        // Unity Messages
        private void Awake()
        {
            //build_Test1();
        }
        private void Start()
        {

        }
    }
}