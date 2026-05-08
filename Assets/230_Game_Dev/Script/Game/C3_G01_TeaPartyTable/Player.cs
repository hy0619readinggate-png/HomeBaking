using UnityEngine;
using UnityEngine.Playables;
using System.Collections;
using System;
using DoDoEng.Game.UI;
using DoDoEng.Common;
using DG.Tweening;

namespace DoDoEng.Game.C3_G01
{
    [RequireComponent(typeof(Character))]
    public class Player : MonoBehaviour
    {
        // Properties
        public bool UseInput
        {
            get { return useInput; }
            set { useInput = value; }
        }
        public GameObject PlayerGO => this.gameObject;


        // Methods
        public void StartPlay()
        {
            gameOver = false;
            useInput = true;
        }
        public void StopPlay()
        {
            gameOver = true;
            useInput = false;
            _GestureGuide.SetActive(false);
        }
        public void StartGesture()
        {
            StartCoroutine(coGesture());
        }
        public void StopGesture()
        {
            StopCoroutine(coGesture());
        }
        public void MoveOriginPosition()
        {
            _GestureGuide.SetActive(false);

            gameOver = true;
            collided = true;
            buttonDownPressed = false;
            destPoint = new Vector3(0f, transform.position.y, transform.position.z);
        }
        public void PlayCorrectClip()
        {
            var rndIdx = UnityEngine.Random.Range(0, _CorrectClip.Length);

            AudioMGR.One.PlayEffect(_CorrectClip[rndIdx]);
        }


        // Event
        public event Action OnCrash = null;



        // Fields : caching
        private TableMGR tableMGR_ = null;
        private TableMGR tableMGR => tableMGR_ ??= FindObjectOfType<TableMGR>();
        private GamePausePopup popup_ = null;
        private GamePausePopup popup => popup_ ??= FindObjectOfType<GamePausePopup>(true);

        // Fields
        private bool buttonDownPressed = false;
        private bool gameOver = false;
        private bool isPlayEffect = false;

        private bool useInput = false;
        private Vector3 destPoint = Vector3.zero;
        private float curPosX = 0f;
        private float prevPosX = 0f;
        private bool useMoveingTimeline = true;
        private bool collided = false;

        private Coroutine collideCR = null;
        private PlayableDirector runningTimeline = null;
        private CharacterAnimation currentAnimation = CharacterAnimation.Idle;



        // Functions
        private void setupTimeline(CharacterAnimation value)
        {
            if (currentAnimation == value)
                return;


            if (runningTimeline != null && runningTimeline.state == PlayState.Playing)
            {
                runningTimeline.Stop();
                runningTimeline = null;
            }


            PlayableDirector timeline = null;
            switch (value)
            {
                case CharacterAnimation.Idle: timeline = _IdleTL; break;
                case CharacterAnimation.Left: timeline = _LeftTL; break;
                case CharacterAnimation.Right: timeline = _RightTL; break;

                case CharacterAnimation.Correct: timeline = _CorrectTL; break;
                case CharacterAnimation.Crash: timeline = _CrashTL; break;
                case CharacterAnimation.AlphabetWrong: timeline = _WrongTL; break;
                case CharacterAnimation.Broom: timeline = _BroomTL; break;
                default:
                    break;
            }

            runningTimeline = timeline;
            currentAnimation = value;

            runningTimeline?.Play();
        }
        private void releaseInput()
        {
            buttonDownPressed = false;
            rayCollider.SetActive(false);

            destPoint = new Vector3(_PointChecker.XPos, transform.position.y, transform.position.z);
        }
        private void onPlayerPoint_MoveTo(Vector3 targetPoint)
        {
            destPoint = targetPoint;
        }


        // Event Handlers
        private void tableObjectChecker_Obstacle()
        {
            useInput = false;

            releaseInput();

            AudioMGR.One.PlayEffect(_IncorrectClip[1]);

            if (collideCR == null)
                collideCR = StartCoroutine(coColliderWithObstacle());
        }
        private void tableObjectChecker_Text(char? value)
        {
            bool answer = tableMGR.CheckAnswer(value);

            if (answer)
            {
                StartCoroutine(coPlayTimeline(CharacterAnimation.Correct));

                var rndIdx = UnityEngine.Random.Range(0, _CorrectClip.Length);
                //AudioMGR.One.PlayEffect(_CorrectClip[rndIdx]);
                DOVirtual.DelayedCall(0.6f,()=> AudioMGR.One.PlayEffect(_CorrectClip[rndIdx]));
                tableMGR.Correct();
            }
            else
            {
                releaseInput();

                var rndIdx = UnityEngine.Random.Range(0, _IncorrectClip.Length);
                AudioMGR.One.PlayEffect(_IncorrectClip[0]);

                StartCoroutine(coColliderWithObstacle());

                //StartCoroutine(coPlayTimeline(CharacterAnimation.AlphabetWrong));

                //tableMGR.Wrong();
            }

        }
        private void tableObjectChecker_OnTriggerBroom()
        {
            var rndIdx = UnityEngine.Random.Range(0, _CorrectClip.Length);
            AudioMGR.One.PlayEffect(_CorrectClip[rndIdx]);

            StartCoroutine(coColliderWithBroom());

            tableMGR.Broom();
        }
        private void tableObjectChecker_OnTriggerBonus()
        {
            var rndIdx = UnityEngine.Random.Range(0, _CorrectClip.Length);
            AudioMGR.One.PlayEffect(_CorrectClip[rndIdx]);

            StartCoroutine(coPlayTimeline(CharacterAnimation.Correct));

            tableMGR.Bonus();
        }


        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private PlayerPointChecker _PointChecker = null;
        [SerializeField] private PlayerTableObjectChecker _TableObjectChecker = null;
        [SerializeField] private GameObject playerCollider = null;
        [SerializeField] private GameObject rayCollider = null;
        [SerializeField] private GameObject _GestureGuide = null;
        [SerializeField] private PlayerPoint[] points = null;
        [Header("★ Timeline")]
        [SerializeField] private PlayableDirector _LeftTL = null;
        [SerializeField] private PlayableDirector _RightTL = null;
        [SerializeField] private PlayableDirector _CorrectTL = null;
        [SerializeField] private PlayableDirector _WrongTL = null;
        [SerializeField] private PlayableDirector _CrashTL = null;
        [SerializeField] private PlayableDirector _IdleTL = null;
        [SerializeField] private PlayableDirector _BroomTL = null;
        [Header("★ AudioClip")]
        [SerializeField] private AudioClip _MoveClip = null;
        [SerializeField] private AudioClip[] _IncorrectClip = null;
        [SerializeField] private AudioClip[] _CorrectClip = null;
        [Header("★ Config")]
        [SerializeField] private float _LerpSpeed = 5f;


        // Unity Messages
        private void Start()
        {
            gameOver = false;
            _GestureGuide.SetActive(false);
            rayCollider.SetActive(false);

            _TableObjectChecker.OnTriggerObstacle += tableObjectChecker_Obstacle;
            _TableObjectChecker.OnTriggerText += tableObjectChecker_Text;
            _TableObjectChecker.OnTriggerBroom += tableObjectChecker_OnTriggerBroom;
            _TableObjectChecker.OnTriggerBonus += tableObjectChecker_OnTriggerBonus;

            for (int i = 0; i < points.Length; i++)
                points[i].OnPlayerMove += onPlayerPoint_MoveTo;
        }
        private void Update()
        {
            if (popup.gameObject.activeSelf) useInput = false;
            else if (popup.gameObject.activeSelf == false && !collided && buttonDownPressed) useInput = true;

            if (useInput)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    buttonDownPressed = true;

                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out hit))
                    {
                        Debug.Log(hit.collider.gameObject.name);
                        if (hit.collider.gameObject == playerCollider)
                        {
                            rayCollider.SetActive(true);
                        }
                    }

                    if (Physics.Raycast(ray, out hit))
                    {
                        for (int i = 0; i < points.Length; i++)
                        {
                            if (hit.collider.gameObject == points[i].gameObject)
                            {
                                AudioMGR.One.PlayEffect(_MoveClip);
                            }
                        }

                    }
                }


                if (Input.GetMouseButtonUp(0))
                {
                    buttonDownPressed = false;

                    releaseInput();

                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out hit))
                    {
                        for (int i = 0; i < points.Length; i++)
                        {
                            if (hit.collider.gameObject == points[i].gameObject)
                            {
                                rayCollider.SetActive(false);

                                var newpos = points[i].PointPosition;
                                //AudioMGR.One.PlayEffect(_MoveClip);

                                // X축만 반영
                                destPoint = new Vector3(newpos.x, transform.position.y, transform.position.z);
                            }
                        }

                    }
                }
            }



            curPosX = 0f;


            if (buttonDownPressed && rayCollider.activeSelf)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.gameObject == rayCollider || hit.collider.gameObject == playerCollider)
                    {
                        curPosX = Input.mousePosition.x;

                        transform.position = new Vector3(hit.point.x, transform.position.y, transform.position.z);


                    }
                }
                else
                {
                    releaseInput();
                }
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, destPoint, Time.deltaTime * _LerpSpeed);
            }
        }
        private void LateUpdate()
        {
            var delta = curPosX - prevPosX;
            prevPosX = curPosX;

            if (useMoveingTimeline == false)
                return;


            if (Mathf.Approximately(delta, 0))
            {
                setupTimeline(CharacterAnimation.Idle);
            }
            else
            {
                if (!isPlayEffect)
                {
                    AudioMGR.One.PlayEffect(_MoveClip);
                    isPlayEffect = true;

                    DOVirtual.DelayedCall(0.6f, () => isPlayEffect = false);
                }
                if (delta < -3.5f)
                {
                    buttonDownPressed = true;
                    setupTimeline(CharacterAnimation.Right);

                }
                else if (delta > 3.5f)
                {
                    buttonDownPressed = true;
                    setupTimeline(CharacterAnimation.Left);

                }
            }

            var gPosX = _GestureGuide.transform.position;
            gPosX.x = curPosX;
        }




        // Coroutines
        IEnumerator coGesture()
        {
            var time = 0f;
            _GestureGuide.transform.position = transform.position;
            _GestureGuide.SetActive(true);
            yield return new WaitForSeconds(4f);


            while (true)
            {
                if (!gameOver)
                {
                    if (buttonDownPressed)
                    {
                        _GestureGuide.SetActive(false);

                        time = 0f;
                    }
                    else
                    {
                        time += Time.deltaTime;

                        if (time >= 10)
                        {
                            _GestureGuide.transform.position = transform.position;
                            _GestureGuide.SetActive(true);
                            yield return new WaitForSeconds(4f);

                            time = 0;
                        }
                        else
                        {
                            _GestureGuide.SetActive(false);
                        }
                    }
                    yield return null;
                }
                else break;
            }
        }
        IEnumerator coColliderWithObstacle()
        {
            collided = true;

            useMoveingTimeline = false;

            useInput = false;
            tableMGR.Halt(true);
            yield return null;

            setupTimeline(CharacterAnimation.Crash);
            yield return null;

            //if (currentAnimation == CharacterAnimation.Crash && runningTimeline != null)
            //    yield return new WaitUntil(() => runningTimeline.state != PlayState.Playing);


            yield return new WaitForSeconds(2f);
            useMoveingTimeline = true;

            collided = false;
            useInput = true;
            tableMGR.Halt(false);


            collideCR = null;
            OnCrash?.Invoke();
            yield return null;


        }
        IEnumerator coColliderWithBroom()
        {
            collided = true;

            tableMGR.Halt(true);
            useInput = false;
            yield return null;

            setupTimeline(CharacterAnimation.Broom);
            yield return new WaitForSeconds(3f);
            //if (currentAnimation == CharacterAnimation.Broom && runningTimeline != null)
            //    yield return new WaitUntil(() => runningTimeline.state != PlayState.Playing);

            tableMGR.Halt(false);
            useInput = true;
            collided = false;
            yield return null;
        }
        IEnumerator coPlayTimeline(CharacterAnimation value)
        {
            useMoveingTimeline = false;
            yield return null;

            setupTimeline(value);
            yield return null;

            if (currentAnimation == value && runningTimeline != null)
                yield return new WaitUntil(() => runningTimeline.state != PlayState.Playing);

            useMoveingTimeline = true;
            yield return null;
        }

    }
}