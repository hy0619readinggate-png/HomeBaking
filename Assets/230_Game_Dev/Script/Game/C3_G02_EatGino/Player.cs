using beyondi.Util;
using DoDoEng.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DoDoEng.Game.C3_G02
{
    public class Player : MonoBehaviour
    {

        // Properties
        public float MoveSpeed
        {
            get { return _MoveSpeed; }
            set { _MoveSpeed = value; }
        }
        public bool IsAccelerated
        {
            get { return isAccelerated; }
            set { isAccelerated = value; }
        }



        // Methods
        public void PlayFeedback(bool isCorrect)
        {
            if (isCorrect) _Feedback.SetTrigger(hashKey_Correct);
            else _Feedback.SetTrigger(hashKey_Wrong);
        }
        public void InitCondition()
        {
            _MoveSpeed = initialSpeed;
            _Feedback.SetTrigger(hashKey_Normal);
        }
        public void HideAfterImages()
        {
            isAccelerated = false;
            foreach (var img in _AfterImages) img.SetActive(false);
        }
        public void SetPosition(Vector3 value)
        {
            targetPos = value;
        }
        public void Halt(bool value)
        {
            halt = value;
        }



        // Fields : cachings
        private PlayerAnimation animation_ = null;
        private PlayerAnimation playerAnimation => animation_ ??= GetComponent<PlayerAnimation>();
        private PlayerInput input_ = null;
        private PlayerInput playerInput => input_ ??= GetComponent<PlayerInput>();

        private JellyControl jellyControl_ = null;
        private JellyControl jellyControl => jellyControl_ ??= GetComponent<JellyControl>();
        private ItemControl itemControl_ = null;
        private ItemControl itemControl => itemControl_ ??= GetComponent<ItemControl>();

        // Fields
        private const float reachDistance = 0.2f;
        private Coroutine getJellyCoroutine = null;

        private Vector3 targetPos = Vector3.zero;
        private bool isRemoveingTile = false;

        private float initialSpeed = 0.0f;
        public bool isAccelerated = false;

        private bool halt = true;


        // Fields : ani
        private readonly int hashKey_Wrong = Animator.StringToHash("Wrong");
        private readonly int hashKey_Correct = Animator.StringToHash("Correct");
        private readonly int hashKey_Normal = Animator.StringToHash("Normal");



        // Functions
        private bool canMove(Vector3 value)
        {
            var col = Physics2D.OverlapPoint(value);
            if (col == null)
                return true;

            var obstacle = col.GetComponent<Obstacles>();
            if (obstacle == null)
                return true;

            return false;
        }
        private Vector3 getNextPlayerBlockPosition(Direction direction, Vector3 value)
        {
            Vector3 pos = value;

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
        private Vector3 getCurrentPlayerBlockPosition()
        {
            var clampX = getCurrentPositionValue(transform.position.x);
            var clampY = getCurrentPositionValue(transform.position.y);

            return new Vector3(clampX, clampY, 0);
        }
        private float getCurrentPositionValue(float value)
        {
            bool minus = value < 0;

            var aValue = Mathf.Abs(value);
            var rValue = Mathf.Round(aValue);

            return minus ? -rValue : rValue;
        }
        private bool canRemoveTile(Vector3 value)
        {
            var destPos = getNextPlayerBlockPosition(playerInput.PressedDirection, value);
            if (_TileMapMGR.HasTile(destPos))
            {
                isRemoveingTile = true;

                int rndIdx = UtilArray.RandomOne(0, _EatClips.Length - 1);

                AudioMGR.One.PlayEffect(_EatClips[rndIdx]);
                _TileMapMGR.RemoveTile(destPos);
                return true;
            }

            return false;
        }
        private void updatePlayerAnimator(bool isMove, bool isRemoveTile, Direction direction = Direction.None)
        {
            CharacterState state = CharacterState.Stand;
            if (direction == Direction.Right)
                state = CharacterState.MoveR;
            else if (direction == Direction.Left)
                state = CharacterState.MoveL;


            if (isMove && isRemoveTile)
            {
                if (AudioMGR.One.IsPlayingEffectLL)
                    AudioMGR.One.StopEffectLL(true, 0.1f);

                playerAnimation.WalkEat((int)direction, state);
            }
            else
            {
                if (isMove)
                {
                    if (AudioMGR.One.IsPlayingEffectLL == false)
                        AudioMGR.One.PlayEffectLL(_WalkClip, true);

                    playerAnimation.Walk((int)direction, state);
                }
                else
                {
                    if (isRemoveingTile == false)
                    {
                        if (AudioMGR.One.IsPlayingEffectLL)
                            AudioMGR.One.StopEffectLL(true, 0.1f);

                        if (!playerAnimation.IsPlaying())
                        {
                            playerAnimation.Idle();
                        }
                    }
                }

                if (isRemoveTile)
                {
                    if (playerAnimation.IsSame((int)direction) == false)
                    {
                        if (AudioMGR.One.IsPlayingEffectLL)
                            AudioMGR.One.StopEffectLL(true, 0.1f);

                        playerAnimation.Eat((int)direction, state);
                    }
                }
            }
        }



        // Event Handlers
        private void playerInput_OnDirectionButtonPressed(bool pressed, Direction directionButton)
        {
            // 젤리 먹는중이면 방향키 막기!!
            if (getJellyCoroutine != null)
                return;


            if (pressed)
            {
                if (IsAccelerated)
                {
                    foreach (var img in _AfterImages) img.SetActive(false);
                    _AfterImages[(int)directionButton].SetActive(true);
                }
            }
        }
        private void playerInput_OnOKButton()
        {
            // 움직이는 중이면 확인버튼 막기!!
            if (playerInput.PressedDirection != Direction.None)
                return;

            if (getJellyCoroutine == null)
            {
                playerInput.DisableOKButton();

                getJellyCoroutine = StartCoroutine(coGetJelly());
            }
        }
        private void findMovableDirection()
        {
            List<int> movableDir = new();
            var jellyDir = jellyControl.GetJellyDirectionAround(transform.position);

            Direction[] directions = { Direction.Top, Direction.Bottom, Direction.Left, Direction.Right };
            for (int i = 0; i < directions.Length; i++)
            {
                var curPos = getCurrentPlayerBlockPosition();
                var nextPos = getNextPlayerBlockPosition(directions[i], curPos);
                if (canMove(nextPos))
                    movableDir.Add(i);
            }

            if (jellyDir != Direction.None) movableDir.Remove(Array.IndexOf(directions, jellyDir));

            playerInput.SetMovableDirIdx(movableDir);
        }
        private void _TileMapMGR_OnTileRemoved(Vector3 pos)
        {
            if (isRemoveingTile)
            {
                isRemoveingTile = false;

                jellyControl.Open(pos);
                itemControl.Show(pos);
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TileMapMGR _TileMapMGR = null;
        [SerializeField] private Animator _Feedback = null;
        [SerializeField] private GameObject[] _AfterImages = null;
        [Space()]
        [SerializeField] private AudioClip _WalkClip = null;
        [SerializeField] private AudioClip[] _EatClips = null;
        [Header("★ Config")]
        [SerializeField, Range(1, 10)] private float _MoveSpeed = 3f;



        // Unity Messages
        private void Awake()
        {
            initialSpeed = _MoveSpeed;
            playerInput.OnDirectionButtonPressed += playerInput_OnDirectionButtonPressed;
            playerInput.OnOKButton += playerInput_OnOKButton;

            playerInput.OnAffordanceActive += findMovableDirection;

            _TileMapMGR.OnTileRemoved += _TileMapMGR_OnTileRemoved;
        }
        private void Start()
        {
            targetPos = transform.position;
            isRemoveingTile = false;
        }
        private void Update()
        {
            if (halt)
                return;


            // Block, eat jelly
            if (getJellyCoroutine != null)
                return;



            Direction playerInputDir = playerInput.PressedDirection;
            Vector3 playerPos = transform.position;


            if (playerInputDir == Direction.None)
            {
                // Stop
                if (Vector3.Distance(playerPos, targetPos) == 0)
                {
                    if (jellyControl.HasJellyAround(playerPos))
                        playerInput.EnableOKButton();
                    else playerInput.DisableOKButton();

                    updatePlayerAnimator(false, false);
                }
            }
            else
            {
                playerInput.DisableOKButton();

                if (Vector3.Distance(playerPos, targetPos) <= reachDistance)
                {
                    var curPos = getCurrentPlayerBlockPosition();
                    var nextPos = getNextPlayerBlockPosition(playerInputDir, curPos);

                    bool isMove = canMove(nextPos);
                    bool isRemoveTile = canRemoveTile(curPos);

                    targetPos = isMove ? nextPos : curPos;


                    // Animaton
                    updatePlayerAnimator(isMove, isRemoveTile, playerInputDir);

                    // Item
                    if (itemControl.HasItem(playerPos))
                        itemControl.GetItem(playerPos);
                }



                //if (prevDirection == playerInput.PressedDirection)
                //{
                //    if (Vector3.Distance(transform.position, targetPos) <= _ReachDistance)
                //    {
                //        var curPos = getCurrentPlayerBlockPosition();
                //        var nextPos = getNextPlayerBlockPosition(playerInput.PressedDirection, curPos);

                //        bool isMove = canMove(nextPos);
                //        bool isRemoveTile = canRemoveTile(curPos);

                //        targetPos = isMove ? nextPos : curPos;


                //        // Animaton
                //        updatePlayerAnimator(isMove, isRemoveTile);

                //        // Item
                //        if (itemControl.HasItem(transform.position))
                //            itemControl.GetItem(transform.position);
                //    }
                //}
                //else
                //{
                //    if (Vector3.Distance(transform.position, targetPos) <= _ReachDistance)
                //    {
                //        prevDirection = playerInput.PressedDirection;

                //        var curPos = getCurrentPlayerBlockPosition();
                //        var nextPos = getNextPlayerBlockPosition(playerInput.PressedDirection, curPos);

                //        bool isMove = canMove(nextPos);
                //        bool isRemoveTile = canRemoveTile(curPos);

                //        targetPos = isMove ? nextPos : curPos;


                //        // Animaton
                //        updatePlayerAnimator(isMove, isRemoveTile);

                //        // Item
                //        if (itemControl.HasItem(transform.position))
                //            itemControl.GetItem(transform.position);
                //    }
                //}
            }


            transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * _MoveSpeed);

        }



        // Coroutines
        IEnumerator coGetJelly()
        {
            var direction = jellyControl.GetJellyDirectionAround(transform.position);
            CharacterState state = CharacterState.Stand;
            if (direction == Direction.Right)
                state = CharacterState.MoveR;
            else if (direction == Direction.Left)
                state = CharacterState.MoveL;
            playerAnimation.Flip(state);


            yield return jellyControl.Get();

            getJellyCoroutine = null;
            yield return null;

            playerInput.EndOKButton();
            yield return null;
        }
    }

}