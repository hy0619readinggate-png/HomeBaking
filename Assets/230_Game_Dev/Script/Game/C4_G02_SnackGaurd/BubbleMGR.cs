using beyondi.Behaviour;
using beyondi.Util;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace DoDoEng.Game.C4_G02
{

    public class BubbleMGR : BYDSingleton<BubbleMGR>
    {

        // Methods
        public void SetConfig(BubbleGeneratorConfig value)
        {
            // Available Generator
            for (int i = 0; i < _Generators.Length; i++)
            {
                bool closed = i < value.AvailableGeneratorCount ? false : true;

                if (i < value.BubbleSpeeds.Length)
                    _Generators[i].Setup(closed, value.BubbleSpeeds[i]);
                else _Generators[i].Setup(closed, 0f);
            }


            // Spawn time
            List<float> list = new List<float>();
            for (float i = value.SpawnIntervalMIN; i < value.SpawnIntervalMAX; i += _SpawnTimeUnit)
                list.Add(i);

            spawnIntervaleTimes = null;
            spawnIntervaleTimes = list.ToArray();

        }
        public void SetRule(BubbleGeneratorRule value)
        {
            rule = value;
        }
        public void SetTexts(string answerText, string[] exampleTexts)
        {
            this.answerText = answerText;
            this.exampleTexts = exampleTexts.Where((v) => string.IsNullOrEmpty(v) == false).ToArray();
        }
        public void Generate(bool start)
        {
            if (start)
            {
                if (generateCoroutine == null)
                    generateCoroutine = StartCoroutine(coGenerate());
            }
            else
            {
                if (generateCoroutine != null)
                {
                    StopCoroutine(generateCoroutine);
                    generateCoroutine = null;
                }
            }
        }
        public void HaltGenerator(bool halt)
        {
            foreach (var generator in _Generators)
                generator.Halt(halt);
        }
        public void PopAllBubble(bool delay)
        {
            foreach (var generator in _Generators)
                generator.PopAll(delay);
        }
        public void CheckAnswerBubble(BubbleText bubble)
        {
            if (bubble == null)
                return;

            if (string.IsNullOrEmpty(answerText))
                return;


            if (answerText.Equals(bubble.Text))
            {

                Generate(false);
                C4_G02_Main.Instance.Success();

                int useCount = getGeneratorUse();
                if (useCount >= 2)
                {
                    // 물방울 생성기가 2개이상이면, 구멍 막기.
                    foreach (var generator in _Generators)
                        generator.Close(bubble);
                }

            }
            else
            {
                C4_G02_Main.Instance.Fail();
            }
        }





        // Fields
        private BubbleGeneratorRule rule = null;
        private float[] spawnIntervaleTimes = null;
        private string answerText = null;
        private string[] exampleTexts = null;
        private Coroutine generateCoroutine = null;



        // Functions
        private int getGeneratorUse()
        {
            int count = 0;
            foreach (var generator in _Generators)
            {
                if (generator.Closed == false)
                    count++;
            }

            return count;
        }




        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject _MonsterBubblePF = null;
        [SerializeField] private GameObject _TextBubblePF = null;
        [Space()]
        [SerializeField] private BubbleGenerator[] _Generators = null;
        [Header("★ Config")]
        [SerializeField] private float _SpawnTimeUnit = 0.5f;


        // Unity Messages
        private void Start()
        {

        }
        private void Update()
        {

        }

        // Unity Coroutine
        IEnumerator coGenerate()
        {
            float time = 0f;
            float spawnTime = 0f;

            int monsterCount = 0;
            int monsterMaxCount = UtilArray.RandomOne(rule.MonsterBubbleCountMIN_BeforeText, rule.MonsterBubbleCountMAX_BeforeText);

            bool textSpawned = false;
            bool answerSpanwed = false;

            bool loop = true;
            while (loop)
            {
                if (time < spawnTime)
                {
                    time += Time.deltaTime;
                }
                else
                {
                    time = 0f;
                    spawnTime = UtilArray.ExtractOne(spawnIntervaleTimes);


                    // 사용가능한 생성기 찾기..
                    var generatorArr = _Generators.Where((v) => v.Closed == false).ToArray();
                    var generator = UtilArray.ExtractOne(generatorArr);

                    // 연속으로 텍스트 
                    if (textSpawned)
                    {
                        var range = Random.value;
                        var continueTextRatio = (int)rule.ProbabilityText / 100f;

                        if (range > continueTextRatio)
                        {
                            textSpawned = false;

                            monsterCount = 0;
                            monsterMaxCount = UtilArray.RandomOne(rule.MonsterBubbleCountMIN_BeforeText, rule.MonsterBubbleCountMAX_BeforeText);
                        }
                    }



                    // 몬스터 or 텍스트
                    if (monsterCount < monsterMaxCount)
                    {
                        generator.Create(_MonsterBubblePF);
                        monsterCount++;
                    }
                    else
                    {
                        textSpawned = true;

                        var range = Random.value;
                        var answerRatio = (int)rule.ProbabilityAnswer / 100f;

                        if (range <= answerRatio && !answerSpanwed)
                        {
                            generator.SetText(answerText);
                            answerSpanwed = true;
                        }
                        else
                        {
                            var exampleText = UtilArray.ExtractOne(exampleTexts);
                            generator.SetText(exampleText);
                            answerSpanwed = false;
                        }

                        generator.Create(_TextBubblePF);
                    }
                }


                yield return null;
            }


            generateCoroutine = null;
            yield return null;
        }
    }
}
