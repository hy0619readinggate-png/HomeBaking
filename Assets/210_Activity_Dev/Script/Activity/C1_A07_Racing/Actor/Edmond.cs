using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System;
using UnityEngine;

namespace DoDoEng.Activity.C1_A07
{
    public enum EdmondState
    {
        None,
        Idle, Move, Obstacle, Correct, Wrong, GoalIn, Finish
    }

    public class Edmond : MonoBehaviour
    {
        // Methods
        public void Init(int laneNo)
        {
            LOG.Info($"Init()", this);

            changeLaneClipSeq = 0;
            changeLane(laneNo);
        }
        public void MoveToCorrectPosition(float speedX)
        {
            LOG.Info($"MoveToCorrectPosition()", this);

            updateState(EdmondState.Correct, true);

            var destX = Track.One.CorrectPositionX;
            var distance = Vector2.Distance(new Vector2(destX, transform.position.y), transform.position);
            var duration = distance / speedX;

            transform.DOMoveX(destX, duration).SetEase(Ease.Linear);
        }
        public void Return(float speedX)
        {
            LOG.Info($"Return()", this);

            updateState(EdmondState.Correct, false);

            var destX = originPosition.x;
            var distance = Vector2.Distance(new Vector2(destX, transform.position.y), transform.position);
            var duration = distance / speedX;

            DOTween.Kill(transform);
            transform.DOMoveX(destX, duration).SetEase(Ease.Linear).OnComplete(() => updateState(EdmondState.Move));
        }
        public void FinishCorrect()
        {
            LOG.Info($"FinishCorrect()", this);

            updateState(EdmondState.Move);

            DOTween.Kill(transform);
            transform.position = new Vector2(originPosition.x, transform.position.y);
        }
        public void StartGoalIn(float speedX)
        {
            LOG.Info($"StartGoalIn()", this);

            updateState(EdmondState.GoalIn);

            var destX = Track.One.GoalInPositionX;
            var distance = destX - transform.position.x;
            var duration = distance / speedX;

            transform.DOMoveX(destX, duration).SetEase(Ease.Linear).OnComplete(() => updateState(EdmondState.Finish));
        }
        public void FinishGoalIn()
        {
            LOG.Info($"FinishGoalIn()", this);

            DOTween.Complete(transform);
        }
        public void MoveTo(int laneNo)
        {
            var currentLaneNo = Track.One.GetLaneNo(transform.position);
            if (currentLaneNo != laneNo)
            {
                if (laneNo != targetLaneNo)
                {
                    changeLaneClipSeq = ++changeLaneClipSeq % changeLaneCLIP.Length;
                    AudioMGR.One.PlayEffect(changeLaneCLIP[changeLaneClipSeq]);
                    targetLaneNo = laneNo;
                }

                var pt = Track.One.GetLanePosition(laneNo);
                var distance = Mathf.Abs(pt.y - transform.position.y);
                var duration = distance / laneChangeSpeed;

                DOTween.Kill(transform, false);

                transform.DOMoveY(pt.y, duration).SetEase(Ease.Linear).OnComplete(() => changeLane(laneNo));
            }
        }
        public void MoveTo(Example example)
        {
            DOTween.Kill(transform, false);

            var laneNo = Track.One.GetLaneNo(example.transform.position);
            var pt = Track.One.GetLanePosition(laneNo);
            var distance = Mathf.Abs(pt.y - transform.position.y);
            var duration = distance / laneChangeSpeed;

            DOTween.Kill(transform, false);

            transform.DOMoveY(pt.y, duration).SetEase(Ease.Linear).OnComplete(() => changeLane(laneNo));
        }

        // Methods
        public void Idle()
        {
            LOG.Info($"Idle()", this);

            updateState(EdmondState.Idle);
        }
        public void TurnOn()
        {
            LOG.Info($"TurnOn()", this);

            updateState(EdmondState.Move);
        }
        public void Move()
        {
            LOG.Info($"Move()", this);

            updateState(EdmondState.Move);
        }
        public void Obstacle()
        {
            LOG.Info($"Obstacle()", this);

            updateState(EdmondState.Obstacle);
        }
        public void Correct()
        {
            LOG.Info($"Correct()", this);

            updateState(EdmondState.Correct);
        }
        public void Wrong()
        {
            LOG.Info($"Wrong()", this);

            updateState(EdmondState.Wrong, true);
        }

        // Events
        public event Action<Obstacle> OnObstacleCollision;
        public event Action<Example> OnExampleCollision;



        // Fields : caching
        private EdmondAni character_ = null;
        private EdmondAni character => character_ ??= GetComponentInChildren<EdmondAni>(true);

        // Fields
        private int targetLaneNo = -1;
        private Vector3 originPosition;
        private int changeLaneClipSeq = 0;
        private EdmondAnimation[] correctAnis = { EdmondAnimation.Correct1, EdmondAnimation.Correct2, EdmondAnimation.Correct3 };
        private EdmondAnimation[] wrongAnis = { EdmondAnimation.Wrong1, EdmondAnimation.Wrong2 };

        // Functions
        private void updateState(EdmondState state, bool audio = false)
        {
            LOG.Info($"updateState() | {state}", this);

            vfxMoveGO.SetActive(state == EdmondState.Move || state == EdmondState.GoalIn);
            boosterGO.SetActive(state == EdmondState.Correct);

            switch (state)
            {
                case EdmondState.Idle:
                case EdmondState.Move: character.PlayAnimationLoop(EdmondAnimation.Idle); break;
                case EdmondState.Obstacle: character.PlayAnimation(EdmondAnimation.Obstacle); break;
                case EdmondState.Correct:
                    var correctIDX = UtilArray.RandomOne(0, correctAnis.Length - 1);
                    if (audio)
                        AudioMGR.One.PlayEffect(correctCLIP[correctIDX]);
                    character.PlayAnimationLoop(correctAnis[correctIDX]);
                    break;
                case EdmondState.Wrong:
                    var wrongIDX = UtilArray.RandomOne(0, wrongAnis.Length - 1);
                    if (audio)
                        AudioMGR.One.PlayEffect(wrongCLIP[wrongIDX]);
                    character.PlayAnimationLoop(wrongAnis[wrongIDX]);
                    break;
                case EdmondState.GoalIn: character.PlayAnimationLoop(EdmondAnimation.GoalIn); break;
                case EdmondState.Finish: character.PlayAnimationLoop(EdmondAnimation.GoalIn); break;
            }
        }
        private void changeLane(int laneNo)
        {
            targetLaneNo = laneNo;
            Track.One.ChangeEdmondLane(laneNo);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject vfxMoveGO = null;
        [SerializeField] private GameObject boosterGO = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip[] changeLaneCLIP = null;
        [SerializeField] private AudioClip[] correctCLIP = null;
        [SerializeField] private AudioClip[] wrongCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float laneChangeSpeed = 10f;


        // Unity Messages
        private void Awake()
        {
            originPosition = transform.position;

            vfxMoveGO.SetActive(false);
            boosterGO.SetActive(false);
        }
        private void Start()
        {
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            LOG.Info($"OnTriggerEnter2D() | {collision.gameObject.name}", this);

            var obstacle = collision.GetComponent<Obstacle>();
            if (obstacle != null)
                OnObstacleCollision?.Invoke(obstacle);

            var example = collision.GetComponentInParent<Example>();
            if (example != null && example.IsColliderable)
                OnExampleCollision?.Invoke(example);
        }
    }
}