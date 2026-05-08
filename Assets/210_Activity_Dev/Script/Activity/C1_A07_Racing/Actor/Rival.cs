using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Activity.C1_A07
{
    public enum RivalState
    {
        None,
        Idle, Move, OverTaken, Finish, Hidden
    }

    public class Rival : MonoBehaviour, IID
    {
        // Properties
        public int CharacterID => characterID;

        // Methods
        public void Init(int characterID, int laneNo)
        {
            LOG.Info($"Init()", this);

            this.characterID = characterID;

            character = characters[characterID - 1];
            characters.ForEach(c => { c.gameObject.SetActive(c == character); });

            changeLane(laneNo);

            updateState(RivalState.Idle);
        }
        public void Setup(int characterID, int obstacleCount, int laneNo)
        {
            LOG.Info($"Setup() | {obstacleCount}", this);

            this.obstacleCount = obstacleCount;
            this.characterID = characterID;

            character = characters[characterID - 1];
            characters.ForEach(c => { c.gameObject.SetActive(c == character); });

            changeLane(laneNo);

            updateState(RivalState.Idle);

        }
        public void StartGoAhead()
        {
            LOG.Info($"StartGoAhead()", this);

            updateState(RivalState.Move);

            var destX = Track.One.OutSideRightPositionX;
            var distance = Mathf.Abs(destX - transform.position.x);
            var duration = distance / goAheadSpeed;

            transform.DOMoveX(destX, duration).SetEase(Ease.Linear).OnComplete(() => updateState(RivalState.Hidden));
        }
        public void FinishGoAhead()
        {
            LOG.Info($"FinishGoAhead()", this);

            DOTween.Complete(transform);
        }
        public void StartObstruct()
        {
            LOG.Info($"StartObstruct()", this);

            updateState(RivalState.Move);

            crStartObstruct = StartCoroutine(coStartObstruct());
        }
        public void FinishObstruct()
        {
            LOG.Info($"FinishObstruct()", this);

            this.StopCoroutineSafe(ref crStartObstruct);
            this.StopCoroutineSafe(ref crObstacle);
            this.StopCoroutineSafe(ref crStopObstruct);
            DOTween.Kill(transform);

            transform.position = originPosition;
        }
        public void StopObstruct()
        {
            LOG.Info($"StopObstruct()", this);

            updateState(RivalState.Move);

            this.StopCoroutineSafe(ref crStartObstruct);
            this.StopCoroutineSafe(ref crObstacle);
            this.StopCoroutineSafe(ref crStopObstruct);
            DOTween.Kill(transform);

            crStopObstruct = StartCoroutine(coStopObstruct());
        }
        public Coroutine StartOverTaken()
        {
            LOG.Info($"StartOverTaken()", this);

            updateState(RivalState.OverTaken);

            crStartOverTaken = StartCoroutine(coStartOverTaken());
            return crStartOverTaken;
        }
        public void FinishOverTaken()
        {
            LOG.Info($"FinishOverTaken()", this);

            this.StopCoroutineSafe(ref crStartOverTaken);
            DOTween.Kill(transform, true);
        }
        public void StartGoalIn(int laneNo)
        {
            LOG.Info($"StartGoalIn()", this);

            updateState(RivalState.Move);

            transform.position = new Vector2(Track.One.OutSideLeftPositionX, Track.One.GetLanePosition(laneNo).y);
            changeLane(laneNo);

            var destX = Track.One.GoalInPositionX + goalInBrakingDistance;
            var distance = Mathf.Abs(destX - transform.position.x);
            var duration = distance / goalInSpeed;

            transform.DOMoveX(destX, duration).SetEase(Ease.Linear).OnComplete(() => updateState(RivalState.Finish));
        }
        public void FinishGoalIn()
        {
            LOG.Info($"FinishGoalIn()", this);

            DOTween.Complete(transform);
        }



        // Fields : caching
        private RivalAni[] characters_ = null;
        private RivalAni[] characters => characters_ ??= GetComponentsInChildren<RivalAni>(true);

        // Fields
        private Vector3 originPosition;
        private RivalAni character = null;
        private int characterID = 0;
        private int laneNo = 0;
        private int obstacleCount = 0;
        private Coroutine crStartObstruct = null;
        private Coroutine crObstacle = null;
        private Coroutine crStopObstruct = null;
        private Coroutine crStartOverTaken = null;

        // Functions
        private void updateState(RivalState state)
        {
            transform.gameObject.SetActive(state != RivalState.Hidden);
            vfxMoveGO.SetActive(state != RivalState.Idle
                            && state != RivalState.Finish);

            character.PlayAnimationLoop(state switch
            {
                RivalState.Idle => RivalAnimation.Idle,
                RivalState.Move => RivalAnimation.Idle,
                RivalState.OverTaken => RivalAnimation.Correct,
                RivalState.Finish => RivalAnimation.Finish,
                _ => RivalAnimation.Idle
            });
        }
        private void changeLane(int laneNo)
        {
            this.laneNo = laneNo;

            var positionY = Track.One.GetLanePosition(laneNo).y;
            transform.position = new Vector2(transform.position.x, positionY);

            Track.One.ChangeRivalLane(transform, laneNo);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject vfxMoveGO = null;
        [SerializeField] private GameObject boosterGO = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip obstructLaughCLIP = null;
        [Header("★ Config - GoAhead")]
        [SerializeField] private float goAheadSpeed = 16f;
        [Header("★ Config - Obstacle")]
        [SerializeField] private float appearSpeed = 5f;
        [SerializeField] private float appearDuration = 1.5f;
        [SerializeField] private float disappearSpeed = 5f;
        [SerializeField] private float disappearDuration = 1.5f;
        [SerializeField] private float laneChangeSpeed = 5f;
        [SerializeField] private float obstacleInterval = 2f;
        [SerializeField] private float overTakenSpeed = 8f;
        [Header("★ Config - GoalIn")]
        [SerializeField] private float goalInSpeed = 16;
        [SerializeField] private float goalInBrakingDistance = 1f;

        // Unity Messages
        private void Awake()
        {
            vfxMoveGO.SetActive(false);
            boosterGO.SetActive(false);
        }
        private void Start()
        {
            originPosition = transform.position;
        }

        // Unity Coroutine
        IEnumerator coStartObstruct()
        {
            using (LOG.Coroutine($"coStartObstruct()", this))
            {
                if (obstacleCount > 0)
                {
                    transform.position = originPosition;

                    var destX = Track.One.ObstaclePositionX;
                    var distanceX = Mathf.Abs(destX - transform.position.x);
                    var durationX = distanceX / appearSpeed;

                    AudioMGR.One.PlayEffect(obstructLaughCLIP);
                    yield return null;

                    transform.DOMoveX(destX, durationX).SetEase(Ease.Linear);
                    yield return new WaitForSeconds(appearDuration);

                    crObstacle = StartCoroutine(coObstacle());
                    yield return crObstacle;
                    crStopObstruct = StartCoroutine(coStopObstruct());
                    yield return crStopObstruct;
                }
            }
        }
        IEnumerator coObstacle()
        {
            using (LOG.Coroutine($"coObstacle()", this))
            {
                while (obstacleCount > 0)
                {
                    // 레인 이동
                    var nextLane = Track.One.EdmondLane;

                    var destY = Track.One.GetLanePosition(nextLane).y;
                    var distance = Mathf.Abs(destY - transform.position.y);
                    var duration = distance / laneChangeSpeed;

                    yield return transform
                        .DOMoveY(destY, duration)
                        .SetEase(Ease.Linear)
                        .OnComplete(() => changeLane(nextLane))
                        .WaitForCompletion();

                    // 바나나 떨어뜨리기
                    obstacleCount--;
                    Track.One.InstallObstacle(laneNo, obstacleCount == 0);
                    var waitDuration = Mathf.Max(obstacleInterval - duration, 0);
                    yield return new WaitForSeconds(waitDuration);
                }
            }
        }
        IEnumerator coStopObstruct()
        {
            using (LOG.Coroutine($"coStopObstruct()", this))
            {
                var destX = Track.One.OutSideRightPositionX;
                var distance = Mathf.Abs(destX - transform.position.x);
                var duration = distance / disappearSpeed;

                transform.DOMoveX(destX, duration).SetEase(Ease.Linear);
                yield return new WaitForSeconds(disappearDuration);
            }
        }
        IEnumerator coStartOverTaken()
        {
            using (LOG.Coroutine($"coStartOverTaken()", this))
            {
                var laneNos = Track.One.GetEmptyLanesNo();
                laneNo = laneNos[0];
                changeLane(laneNo);

                var destX = Track.One.OutSideLeftPositionX;
                var distanceX = Mathf.Abs(destX - transform.position.x);
                var durationX = distanceX / overTakenSpeed;

                yield return transform.DOMoveX(destX, durationX).SetEase(Ease.Linear).WaitForCompletion();
            }
        }



        // Interface : IID
        public int ID { get; set; }
    }
}