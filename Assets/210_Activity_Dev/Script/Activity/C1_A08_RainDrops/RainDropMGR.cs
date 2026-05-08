using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C1_A08
{
    public class RainDropMGR : MonoBehaviour
    {
        // Methods
        public void StartDrop(Sprite answerSprite, Sprite[] wrongSprites)
        {
            LOG.Info($"StartDrop()", this);

            answerData = new ExampleData(answerSprite, true);
            wrongDatas = wrongSprites.Select(s => new ExampleData(s, false)).ToArray();
            examQueue.Clear();
            crDropRaindrops = StartCoroutine(coDropRaindrops());
        }
        public void StopDrop()
        {
            LOG.Info($"StopDrop()", this);

            StopCoroutine(crDropRaindrops);
            raindrops.ForEach(rd => rd.StopDrop());
        }
        public Coroutine ClearDrop()
        {
            LOG.Info($"ClearDrop()", this);

            return StartCoroutine(coClearDrops());
        }



        // Fields
        private Coroutine crDropRaindrops = null;
        private ExampleData answerData = null;
        private ExampleData[] wrongDatas = null;
        private Queue<ExampleData> examQueue = new Queue<ExampleData>();

        // Functions
        private RainDrop getAvailRaindrop()
        {
            var avails = raindrops.Where(rd => !rd.gameObject.activeSelf).ToArray();
            if (avails.Length > 0)
                return UtilArray.ExtractOne(avails);
            else return null;
        }
        private ExampleData getExamData()
        {
            if (examQueue.Count() == 0)
            {
                var pool = new List<ExampleData>();
                var answerCount = Mathf.RoundToInt(wrongDatas.Length / countPerAnswer);

                for(var i = 0; i < answerCount; i++)
                {
                    var arr = wrongDatas
                                .Skip(i * answerCount)
                                .Take(answerCount)
                                .Append(answerData)
                                .ToArray();
                    UtilArray.Shuffle(arr);
                    pool.AddRange(arr);
                }
                pool.ForEach(ex => examQueue.Enqueue(ex));
            }

            return examQueue.Dequeue();
        }


        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private RainDrop[] raindrops = null;
        [SerializeField] private RectTransform respawnPositionRT = null;
        [Header("★ Config")]
        [SerializeField] private float dropInterval = 1.5f;
        [SerializeField] private Range dropSpeed = new Range(120, 200);
        [SerializeField] private Range clearInterval = new Range(0.1f, 0.5f);
        [SerializeField] private float countPerAnswer = 3; // 오답 3개당 1개의 정답

        // Unity Messages
        private void Awake()
        {
            raindrops.SetActiveAll(false);
        }
        private void Start()
        {

        }

        // Unity Coroutine
        IEnumerator coDropRaindrops()
        {
            using (LOG.Coroutine($"coDropRaindrops()", this))
            {
                while (true)
                {
                    var raindrop = getAvailRaindrop();
                    if (raindrop != null)
                    {
                        var examData = getExamData();
                        var y = respawnPositionRT.position.y;
                        var speed = dropSpeed.RandomValue();
                        raindrop.Drop(y, speed, examData);
                    }

                    var interval = dropInterval * (1 + Random.Range(-0.2f, +0.2f));
                    yield return new WaitForSeconds(interval);
                }
            }
        }
        IEnumerator coClearDrops()
        {
            using (LOG.Coroutine($"coClearDrops()", this))
            {
                foreach (var drop in raindrops)
                {
                    if (drop.gameObject.activeSelf)
                    {
                        drop.Explode();
                        yield return new WaitForSeconds(clearInterval.RandomValue());
                    }
                }
            }
        }
    }

    public class ExampleData
    {
        public Sprite Sprite;
        public bool IsAnswer;

        public ExampleData(Sprite sprite, bool isAnswer)
        {
            Sprite = sprite;
            IsAnswer = isAnswer;
        }
    }
}