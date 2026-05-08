using beyondi.Behaviour;
using DG.Tweening;
using DoDoEng.Common;
using DoDoEng.Game.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

namespace DoDoEng.Game.C4_G02
{
    public class Player : BYDSingleton<Player>
    {
        // Properties
        public bool UseInput
        {
            get { return useInput; }
            set { useInput = value; }
        }

        // Methods
        public void StartPlay()
        {
            gameOver = false;
            useInput = true;
        }
        public void StopPlay()
        {
            useInput = false;
            gameOver = true;
        }
        public void StopGesture()
        {
            StopCoroutine(coGesture());
        }
        public void StartGesture()
        {
            StartCoroutine(coGesture());
        }
        public void Freeze()
        {
            buttonDownPressed = false;
            StartCoroutine(coFreeze());
        }
        public void Success()
        {
            _Guideline.SetActive(false);

            var rnd = Random.Range(0, 3);
            switch (rnd)
            {
                case 0: setupTimeline(CharacterAnimation.Correct);
                    AudioMGR.One.PlayEffect(_CorrectClip);
                    break;

                case 1: setupTimeline(CharacterAnimation.Correct2);
                    AudioMGR.One.PlayEffect(_Correct2Clip);
                    break;

                case 2: setupTimeline(CharacterAnimation.Correct3);
                    AudioMGR.One.PlayEffect(_Correct3Clip);
                    break;
            }

            Invoke("characterIdle", 1f);
        }
        public Coroutine GameOver()
        {
            return StartCoroutine(coGameOver());
        }
        public Coroutine Complete()
        {
            return StartCoroutine(coComplete());
        }


        // Fiedls : caching
        private PlayerPositionLimit limit_ = null;
        private PlayerPositionLimit limit => limit_ ??= FindObjectOfType<PlayerPositionLimit>();
        private GamePausePopup popup_ = null;
        private GamePausePopup popup => popup_ ??= FindObjectOfType<GamePausePopup>(true);

        // Fields
        private bool isFreeze = false;
        private bool useInput = false;
        private bool buttonDownPressed = false;
        private bool gameOver = false;
        private float offsetY = 0f;
        private GameObject hitObject = null;
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
                case CharacterAnimation.Load: timeline = _LoadTL; break;
                case CharacterAnimation.LoadIdle: timeline = _LoadIdleTL; break;
                case CharacterAnimation.Fire: timeline = _FireTL; break;
                case CharacterAnimation.Wrong: timeline = _WrongTL; break;
                case CharacterAnimation.WrongOutro: timeline = _WrongOutroTL; break;
                case CharacterAnimation.Correct: timeline = _CorrectTL; break;
                case CharacterAnimation.Correct2: timeline = _CorrectTL2; break;
                case CharacterAnimation.Correct3: timeline = _CorrectTL3; break;
                case CharacterAnimation.CorrectOutro: timeline = _CorrectOutroTL; break;
                case CharacterAnimation.Freeze: timeline = _FreezeTL; break;

                default:
                    break;
            }

            runningTimeline = timeline;
            currentAnimation = value;

            runningTimeline?.Play();
        }
        private void characterIdle()
        {
            if (!isFreeze)
            {
                setupTimeline(CharacterAnimation.Idle);
                _LoadFX.SetActive(false);
            }
        }
        private void createBullet()
        {
            var bulletGO = Instantiate(_BulletPF, _BulletParent);
            bulletGO.transform.localScale = Vector3.one;
            bulletGO.transform.position = new Vector3(_Guideline.transform.position.x, _Guideline.transform.position.y, 0);
        }
        private void dragPlayer()
        {
            var worldpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var playerY = worldpos.y;

            if (worldpos.y <= limit.YMIN.position.y)
                playerY = limit.YMIN.position.y + offsetY;
            else if (worldpos.y >= limit.MAX.position.y)
                playerY = limit.MAX.position.y + offsetY;

            if (worldpos.x < limit.XMIN.position.x || worldpos.x > limit.XMAX.position.x)
                buttonDownPressed = false;

            transform.position = new Vector3(transform.position.x, playerY - offsetY, 0);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject _Guideline = null;
        [SerializeField] private GameObject _BulletPF = null;
        [SerializeField] private Transform _BulletParent = null;
        [SerializeField] private GameObject _GestureGuide = null;
        [Space()]
        [SerializeField] private GameObject _LoadFX = null;
        [SerializeField] private GameObject _Freeze = null;
        [Space()]
        [Header("★ AudioClip")]
        [SerializeField] private AudioClip _FreezeClip = null;
        [SerializeField] private AudioClip _CorrectClip = null;
        [SerializeField] private AudioClip _Correct2Clip = null;
        [SerializeField] private AudioClip _Correct3Clip = null;
        [SerializeField] private AudioClip[] _WrongClip = null;
        [SerializeField] private AudioClip _ShootClip = null;
        [Header("★ Timeline")]
        [SerializeField] private PlayableDirector _FreezeTL = null;
        [SerializeField] private PlayableDirector _IdleTL = null;
        [SerializeField] private PlayableDirector _FireTL = null;
        [SerializeField] private PlayableDirector _LoadTL = null;
        [SerializeField] private PlayableDirector _LoadIdleTL = null;
        [SerializeField] private PlayableDirector _CorrectTL = null;
        [SerializeField] private PlayableDirector _CorrectTL2 = null;
        [SerializeField] private PlayableDirector _CorrectTL3 = null;
        [SerializeField] private PlayableDirector _WrongTL = null;
        [SerializeField] private PlayableDirector _CorrectOutroTL = null;
        [SerializeField] private PlayableDirector _WrongOutroTL = null;

        // Unity Messages
        private void Start()
        {
            _Guideline.SetActive(false);
            _GestureGuide.SetActive(false);

            if (_GestureGuide != null) _GestureGuide.SetActive(false);
            if (_Freeze != null) _Freeze.SetActive(false);

            StopPlay();

            gameOver = false;

        }
        private void Update()
        {
            if (popup.gameObject.activeSelf) useInput = false;
            else if (popup.gameObject.activeSelf == false && !isFreeze) useInput = true;

            if (useInput)
            {
                if (Input.GetMouseButtonDown(0) && buttonDownPressed == false)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

                    if (hit.collider != null && hit.collider.gameObject == gameObject)
                    {
                        buttonDownPressed = true;

                        setupTimeline(CharacterAnimation.LoadIdle);
                        _LoadFX.SetActive(false);
                        hitObject = hit.collider.gameObject;
                        offsetY = hit.point.y - transform.position.y;
                    }
                }

                if (Input.GetMouseButton(0))
                {
                    if (buttonDownPressed && hitObject != null)
                    {
                        setupTimeline(CharacterAnimation.Load);
                        dragPlayer();
                        _Guideline.SetActive(true);
                    }
                }

                if (Input.GetMouseButtonUp(0))
                {

                    if (!isFreeze) setupTimeline(CharacterAnimation.Idle);
                    else setupTimeline(CharacterAnimation.Freeze);

                    if (hitObject != null)
                    {
                        buttonDownPressed = false;
                        setupTimeline(CharacterAnimation.Fire);
                        createBullet();
                        AudioMGR.One.PlayEffect(_ShootClip);

                        if (runningTimeline.state != PlayState.Playing && runningTimeline != _FreezeTL)
                            DOVirtual.DelayedCall(0.5f, () => setupTimeline(CharacterAnimation.Idle));


                        _LoadFX.SetActive(false);
                        _Guideline.SetActive(false);
                        hitObject = null;

                    }

                }
            }
        }

        // Unity Coroutines
        IEnumerator coGesture()
        {
            var time = 0f;

            while (true)
            {
                if (!gameOver)
                {
                    if (buttonDownPressed)
                    {
                        time = 0f;
                        _GestureGuide.SetActive(false);
                    }
                    else
                    {
                        time += Time.deltaTime;

                        if (time >= 10)
                        {
                            _GestureGuide.SetActive(true);
                            yield return new WaitForSeconds(2f);

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
        IEnumerator coFreeze()
        {
            isFreeze = true;
            useInput = false;
            _Guideline.SetActive(false);

            setupTimeline(CharacterAnimation.Freeze);
            AudioMGR.One.PlayEffect(_FreezeClip);

            var rnd = Random.Range(0, _WrongClip.Length);
            AudioMGR.One.PlayEffect(_WrongClip[rnd]);

            _Freeze.SetActive(true);
            _Guideline.SetActive(false);
            yield return new WaitForSeconds(3f);

            isFreeze = false;
            setupTimeline(CharacterAnimation.Idle);
            useInput = true;
            _Freeze.SetActive(false);
        }
        IEnumerator coComplete()
        {
            isFreeze = true;
            useInput = false;
            gameOver = true;

            _Guideline.SetActive(false);
            _GestureGuide.SetActive(false);

            setupTimeline(CharacterAnimation.CorrectOutro);
            yield return new WaitUntil(() => runningTimeline.state != PlayState.Playing);

            yield return new WaitForSeconds(1f);
        }
        IEnumerator coGameOver()
        {
            isFreeze = true;
            useInput = false;
            gameOver = true;

            _Guideline.SetActive(false);
            _GestureGuide.SetActive(false);

            setupTimeline(CharacterAnimation.WrongOutro);
            yield return new WaitUntil(() => runningTimeline.state != PlayState.Playing);

            yield return new WaitForSeconds(1f);
        }
    }
}
