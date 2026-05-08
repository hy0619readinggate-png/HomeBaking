using beyondi.Behaviour;
using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C1_A07
{
    public class Track : BYDSingleton<Track>
    {
        // Properties
        public int LaneCount => lanes.Length;
        public float ObstacleSpeed => normalSpeed * speedRatio * obstacleSpeedRatio;
        public float OutSideRightPositionX => outsideRightPositionTR.position.x;
        public float OutSideLeftPositionX => outsideLeftPositionTR.position.x;
        public float ObstaclePositionX => obstaclePositionTR.position.x;
        public float CorrectPositionX => correctPositionTR.position.x;
        public float GoalInPositionX => goalInPositionTR.position.x;
        public float ExampleSpeed => normalSpeed * speedRatio * exampleSpeedRatio;
        public int EdmondLane => edmondLaneNo;
        public float BoostSpeedRatio => boostSpeedRatio;

        // Methods
        public void Init(Edmond edmond, Example[] examples)
        {
            LOG.Info($"Init()", this);

            this.edmond = edmond;

            examples.ForEach((i, e) => e.transform.SetParent(lanes[i]));
        }
        public void ChangeEdmondLane(int laneNo)
        {
            LOG.Info($"ChangeEdmondLane() | {laneNo}", this);

            edmondLaneNo = laneNo;
            edmond.transform.SetParent(lanes[laneNo - 1]);
        }
        public void ChangeRivalLane(Transform tr, int laneNo)
        {
            LOG.Info($"ChangeRivalLane() | {laneNo}", this);

            tr.transform.SetParent(lanes[laneNo - 1]);
        }
        public void StartRivalGoAhead()
        {
            LOG.Info($"StartRivalGoAhead()", this);

            racingBGAnim.SetTrigger("Start");
        }

        // Methods
        public Transform GetLaneTR(int laneNo)
        {
            return lanes[laneNo - 1];
        }
        public Vector2 GetLanePosition(int laneNo)
        {
            return lanes[laneNo - 1].position;
        }
        public int GetLaneNo(Vector2 ptScreen)
        {
            foreach (var (l, i) in lanes.Select((l, i) => (l, i)))
            {
                var pt = UtilTransform.ScreenToLocal(ptScreen, l as RectTransform, canvas);
                if (l.rect.Contains(pt))
                    return i + 1;
            }
            return -1;
        }
        public int GetLaneNo(Vector3 ptWorld)
        {
            foreach (var (l, i) in lanes.Select((l, i) => (l, i)))
            {
                var pt = UtilTransform.WorldToLocal(ptWorld, l as RectTransform, canvas);
                if (l.rect.Contains(pt))
                    return i + 1;
            }
            return -1;
        }
        public int[] GetEmptyLanesNo()
        {
            return UtilArray.Random(1, LaneCount).Where(x => x != edmondLaneNo).ToArray();
        }

        // Methods
        public Coroutine StartObstruct(int obstacleCount)
        {
            LOG.Info($"StartObstruct()", this);

            this.obstacleCount = obstacleCount;
            obstacles.Clear();

            crObstacle = StartCoroutine(coObstacle());
            return crObstacle;
        }
        public void FinishObstruct()
        {
            LOG.Info($"FinishObstruct()", this);

            this.StopCoroutineSafe(ref crObstacle);
            this.StopCoroutineSafe(ref crCollision);

            foreach (var o in obstacles)
            {
                if (!o.IsDestroyed())
                    Destroy(o.gameObject);
            }
        }
        public void InstallObstacle(int laneNo, bool isLast)
        {
            LOG.Info($"InstallObstacle() | {laneNo} {isLast}", this);

            AudioMGR.One.PlayEffect(installObstactCLIP);

            var laneTR = Track.One.GetLaneTR(laneNo);
            var go = Instantiate(obstaclePB, laneTR);
            go.transform.SetAsFirstSibling();
            go.transform.position = new Vector2(obstaclePositionTR.position.x, laneTR.position.y);
            var obstacle = go.GetComponent<Obstacle>();
            obstacles.Add(obstacle);
        }

        // Methods
        public Coroutine SpeedUpByObstacle()
        {
            LOG.Function(this);

            this.StopCoroutineSafe(ref crSpeedChange);

            crSpeedChange = StartCoroutine(coSpeedChange(normalSpeedRatio, obstacleAccel));
            return crSpeedChange;
        }
        public Coroutine SpeedDnByObstacle()
        {
            LOG.Function(this);

            this.StopCoroutineSafe(ref crSpeedChange);

            crSpeedChange = StartCoroutine(coSpeedChange(obstacleSpeedRatio, obstacleDecel));
            return crSpeedChange;
        }
        public Coroutine SpeedUpByWrongExample()
        {
            LOG.Function(this);

            this.StopCoroutineSafe(ref crSpeedChange);

            crSpeedChange = StartCoroutine(coSpeedChange(normalSpeedRatio, wrongAccel));
            return crSpeedChange;
        }
        public Coroutine SpeedDnByWrongExample()
        {
            LOG.Function(this);

            this.StopCoroutineSafe(ref crSpeedChange);

            crSpeedChange = StartCoroutine(coSpeedChange(wrongSpeedRatio, wrongDecel));
            return crSpeedChange;
        }
        public Coroutine SpeedUpByBoost()
        {
            LOG.Function(this);

            this.StopCoroutineSafe(ref crSpeedChange);

            crSpeedChange = StartCoroutine(coSpeedChange(boostSpeedRatio, boostAccel));
            return crSpeedChange;
        }
        public Coroutine SpeedDnByBoost()
        {
            LOG.Function(this);

            this.StopCoroutineSafe(ref crSpeedChange);

            crSpeedChange = StartCoroutine(coSpeedChange(normalSpeedRatio, boostDecel));
            return crSpeedChange;
        }
        public void SpeedNormalNow()
        {
            LOG.Function(this);

            this.StopCoroutineSafe(ref crSpeedChange);

            speedRatio = 1.0f;
        }
        public void SpeedStopNow()
        {
            LOG.Function(this);

            this.StopCoroutineSafe(ref crSpeedChange);

            speedRatio = 0f;
        }
        



        // Fields : caching
        private Canvas canvas_ = null;
        private Canvas canvas => canvas_ ??= GetComponentInParent<Canvas>();

        // Fields
        private Coroutine crObstacle = null;
        private Coroutine crCollision = null;
        private Coroutine crSpeedChange = null;
        private List<Obstacle> obstacles = new List<Obstacle>();
        private int obstacleCount = 0;
        private int edmondLaneNo = 0;
        private Edmond edmond = null;

        // Functions
        private float speedRatio
        {
            get
            {
                return racingBGAnim.speed;
            }
            set
            {
                racingBGAnim.speed = Mathf.Max(value, 0);
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Animator racingBGAnim = null;
        [SerializeField] private GameObject obstaclePB = null;
        [Header("★ Bindings - Position")]
        [SerializeField] private Transform outsideRightPositionTR = null;
        [SerializeField] private Transform outsideLeftPositionTR = null;
        [SerializeField] private Transform obstaclePositionTR = null;
        [SerializeField] private Transform correctPositionTR = null;
        [SerializeField] private Transform goalInPositionTR = null;
        [SerializeField] private RectTransform[] lanes = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip installObstactCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float obstacleSpeedRatio = 0.5f;
        [SerializeField] private float exampleSpeedRatio = 0.2f;
        [SerializeField] private float normalSpeed = 16.88098f;
        [Header("★ Config - SpeedRatio")]
        [SerializeField] private float boostAccel = 1f;
        [SerializeField] private float boostDecel = 1f;
        [SerializeField] private float obstacleAccel = 0.5f;
        [SerializeField] private float obstacleDecel = 1f;
        [SerializeField] private float wrongAccel = 1f;
        [SerializeField] private float wrongDecel = 0.3f;
        [SerializeField] private float normalSpeedRatio = 1f;
        [SerializeField] private float boostSpeedRatio = 1.5f;
        [SerializeField] private float wrongSpeedRatio = 0.4f;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coObstacle()
        {
            using (LOG.Coroutine($"coObstacle()", this))
            {
                yield return new WaitUntil(()
                => obstacles.Where(o => o.IsDestroyed()).Count() == obstacleCount);

                yield return new WaitForSeconds(2f);
            }
        }
        IEnumerator coSpeedChange(float targetSpeedRatio, float accel)
        {
            using (LOG.Coroutine($"coSpeedChange()", this))
            {
                var duration = Mathf.Abs(targetSpeedRatio - speedRatio) / accel;
                yield return DOTween
                    .To(() => speedRatio,
                        x => speedRatio = x,
                        targetSpeedRatio,
                        duration)
                    .WaitForCompletion();
            }
        }
    }
}