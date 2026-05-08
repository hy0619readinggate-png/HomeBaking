using beyondi.Util;
using DoDoEng.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


namespace DoDoEng.Game.C3_G02
{
    public class JellyControl : MonoBehaviour
    {
        // Methods
        public void Open(Vector3 pos)
        {
            JellyObject jelly = null;
            if (hasJelly(pos, out jelly))
            {
                jelly?.Show();
            }
        }
        public bool HasJellyAround(Vector3 pos)
        {
            var jelly = getJellyAround(pos);
            if (jelly != null && jelly.IsShowing)
                return true;
            else return false;
        }
        public Coroutine Get()
        {
            if (hasJellyAround(transform.position))
            {
                return StartCoroutine(coGet());
            }
            else
            {
                return null;
            }
        }
        public void SetJellyControl(int max, float benefit, float penalty, float effect)
        {
            if (foundJellyIdx.Count > 0) foundJellyIdx.Clear();

            for (int i = 0; i < max; i++)
                foundJellyIdx.Add(i);

            benefitVelocity = benefit;
            penaltyVelocity = penalty;
            effectTime = effect;

            originSpeed = player.MoveSpeed;
        }
        public void InitJellyFeedback() { initJellyFeedback(); }
        public Direction GetJellyDirectionAround(Vector3 pos)
        {
            return getJellyDirectionAround(pos);
        }
        // Event
        public event Action OnLevelOver = null;



        // Fields
        private List<int> foundJellyIdx = new();
        private Coroutine moveCoroutine = null;
        private Coroutine coroutineFeedbackTimer = null;

        //private int correctJellyCount = 0;

        private float originSpeed = 0;
        private float benefitVelocity = 0;
        private float penaltyVelocity = 0;
        private float effectTime = 0;

        // Fields : cachings
        private Player player_ = null;
        private Player player => player_ ??= GetComponent<Player>();
        private PlayerInput playerInput_ = null;
        private PlayerInput playerInput => playerInput_ ??= GetComponent<PlayerInput>();

        // Functions
        private Vector3 getDirectionPosition(Vector3 targetPos, Direction direction)
        {
            Vector3 pos = targetPos;

            switch (direction)
            {
                case Direction.Top: pos.y += Definition.OneBlockSize; break;
                case Direction.Bottom: pos.y -= Definition.OneBlockSize; break;
                case Direction.Left: pos.x -= Definition.OneBlockSize; break;
                case Direction.Right: pos.x += Definition.OneBlockSize; break;

                default:
                    break;
            }

            return pos;
        }
        private bool hasJellyAround(Vector3 pos)
        {
            var jelly = getJellyAround(pos);
            return jelly != null;
        }
        private JellyObject getJellyAround(Vector3 pos)
        {
            foreach (Direction dir in Enum.GetValues(typeof(Direction)))
            {
                if (dir == Direction.None)
                    continue;

                var dirPos = getDirectionPosition(pos, dir);
                var col = Physics2D.OverlapPoint(dirPos);
                if (col != null)
                {
                    var jelly = col.GetComponentInParent<JellyObject>();
                    if (jelly != null)
                        return jelly;
                }
            }

            return null;
        }
        private Direction getJellyDirectionAround(Vector3 pos)
        {
            foreach (Direction dir in Enum.GetValues(typeof(Direction)))
            {
                if (dir == Direction.None)
                    continue;

                var dirPos = getDirectionPosition(pos, dir);
                var col = Physics2D.OverlapPoint(dirPos);
                if (col != null)
                {
                    var jelly = col.GetComponentInParent<JellyObject>();
                    if (jelly != null)
                        return dir;
                }
            }

            return Direction.None;
        }
        private bool hasJelly(Vector3 pos, out JellyObject jelly)
        {
            jelly = getJelly(pos);
            return jelly != null;
        }
        private JellyObject getJelly(Vector3 pos)
        {
            var col = Physics2D.OverlapPoint(pos);
            if (col != null)
            {
                var jelly = col.GetComponentInParent<JellyObject>();
                if (jelly != null)
                    return jelly;
            }

            return null;
        }
        private void initJellyFeedback()
        {
            player.InitCondition();
            player.HideAfterImages();

            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
                moveCoroutine = null;
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private PlayableDirector[] _SuccessTL = null;
        [SerializeField] private PlayableDirector[] _FailTL = null;
        [SerializeField] private PlayableDirector[] _Timelines = null;
        [SerializeField] private GameObject[] _TypeObjects = null;
        [Space()]
        [SerializeField] private Timer _Timer = null;
        //[SerializeField] private JellyHint jellyHint = null;

        // Unity Messages
        private void Start()
        {
            foreach (var go in _TypeObjects)
                go.SetActive(false);
        }


        // Unity Coroutine
        IEnumerator coGet()
        {
            // 타이머 정지
            _Timer.Pause(true);

            // 피드백 시간 정지
            if (coroutineFeedbackTimer != null)
            {
                StopCoroutine(coroutineFeedbackTimer);
                coroutineFeedbackTimer = null;
            }
            yield return null;


            Vector3 pos = transform.position;

            var jelly = getJellyAround(pos);
            var jellyDir = getJellyDirectionAround(pos);
            var jellyTypeIndex = (int)jelly.JellyType;

            var isCorrect = false;

            if (jelly.JellyIdx == foundJellyIdx[0]) isCorrect = true;
            foundJellyIdx.Remove(jelly.JellyIdx);

            //if(foundJellyIdx.Count > 0) jellyHint.SetCorrectHintJelly(foundJellyIdx[0]);

            LOG.Important($"Jelly : [{isCorrect}]", this);

            jelly.Hide();
            yield return null;


            // 먹는 젤리 모양
            for (int i = 0; i < _TypeObjects.Length; i++)
                _TypeObjects[i].SetActive(i == jellyTypeIndex);


            //
            _Timelines[(int)jellyDir].Play();
            yield return null;
            yield return new WaitUntil(() => _Timelines[(int)jellyDir].state != PlayState.Playing);

            C3_G02_Main.Instance.CountJelly(isCorrect);
            yield return null;

            //if (isCorrect) correctJellyCount++;
            //yield return null;

            // Feedback
            if (foundJellyIdx.Count > 0)
            {
                if (isCorrect) C3_G02_Main.Instance.SetQuizSound(foundJellyIdx[0]);

                PlayableDirector[] feedbackTL = isCorrect ? _SuccessTL : _FailTL;
                if (feedbackTL != null)
                {
                    player.PlayFeedback(isCorrect);

                    int rndIdx = UtilArray.RandomOne(0, feedbackTL.Length - 1);

                    feedbackTL[rndIdx].Play();
                    yield return null;
                    yield return new WaitUntil(() => feedbackTL[rndIdx].state != PlayState.Playing);
                }



                // init
                float velocity = isCorrect ? benefitVelocity : penaltyVelocity;
                player.MoveSpeed = originSpeed * velocity;
                player.IsAccelerated = isCorrect;

                if (isCorrect == false)
                    player.HideAfterImages();


                // effect
                if (coroutineFeedbackTimer == null)
                    coroutineFeedbackTimer = StartCoroutine(coFeedbackTimer());
            }
            else
            {
                playerInput.DisableDirectionButton();
                OnLevelOver?.Invoke();
            }

            _Timer.Pause(false);
            yield return null;
        }


        // 
        IEnumerator coFeedbackTimer()
        {
            float time = 0f;
            while (time < effectTime)
            {
                time += Time.deltaTime;
                yield return null;
            }

            initJellyFeedback();
            player.HideAfterImages();
            yield return null;

            coroutineFeedbackTimer = null;

        }
    }
}