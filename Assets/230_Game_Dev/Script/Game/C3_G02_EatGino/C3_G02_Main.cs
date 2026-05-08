using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.Game.Framework;
using DoDoEng.Game.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using GameData = DoDoEng.Common.GameData_C3_G02;
using ProblemMGR = DoDoEng.Game.C3_G02.C3_G02_ProblemMGR;

namespace DoDoEng.Game.C3_G02
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C3_G02_Main : GameMain<C3_G02_Main, GameData>
    {
        // Definitions
        private enum State { Intro, Ready, Game, Next, Outro, Reward }
        // Properties
        public float ClockTime => clockTime;
        public float ScanTime => scanTime;
        public int JellyCount => jellyCount;

        // Methods
        public void Clock(float clockTime)
        {
            _Timer.Clock(clockTime);
        }
        public void CountJelly(bool isCorrect)
        {
            if (isCorrect)
            {
                UIGameCommon.One.StarGauge.Success();
                GameProgress.One.Correct();

                correctJellyNum++;
                levelUI.SetJellyCount(correctJellyNum);
            }
        }
        public void SetQuizSound(int idx)
        {
            var quizSound = wordList[idx].SoundClip;
            levelUI.PlaySpeaker(quizSound);
        }



        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();
        private JellyControl jellyControl_ = null;
        private JellyControl jellyControl => jellyControl_ ??= _Player.GetComponent<JellyControl>();
        private ItemControl itemControl_ = null;
        private ItemControl itemControl => itemControl_ ??= _Player.GetComponent<ItemControl>();

        // Fields
        private FSM<State> fsm = null;

        private float timer = 0;
        private float effectTime = 0;

        private float clockTime = 0;
        private float scanTime = 0;

        private int itemCount = 0;
        private int jellyCount = 0;
        private int totalJellyCount = 0;
        private int correctJellyNum = 0;

        private float benefitVelocity;
        private float penaltyVelocity;

        private List<wordData> wordList;
        private List<GameObject> jellyList;
        private List<GameObject> itemList;
        private Vector3 playerInitialPos = new();

        private bool isTimeOver = false;



        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Intro,       E_Intro,        X_Intro);
            fsm.AddState(State.Ready,       E_Ready,        X_Ready);
            fsm.AddState(State.Game,        E_Game,        X_Game);
            fsm.AddState(State.Next,        E_Next);
            fsm.AddState(State.Outro,       E_Outro,        X_Outro);
            fsm.AddState(State.Reward,      E_Reward);
            #pragma warning restore format
        }
        private void setupProblem(LevelData pData)
        {
            var level = pMGR.PNO - 1;
            //jellyHint.ResetJellyHint();

            if (pMGR.PNO == 1)
            {
                totalJellyCount = pData.TotalJellyCount;
                UIGameCommon.One.StarGauge.Setup(GameReward.StarRatio, totalJellyCount);
                GameProgress.One.Setup(totalJellyCount);
            }
            else wordList.Clear();

            wordList = pData.WordList;

            timer = pData.Timer;

            jellyCount = pData.JellyBlockCount;
            itemCount = pData.ItemCount;

            clockTime = pData.ClockTime;
            scanTime = pData.ScanTime;

            benefitVelocity = pData.BenefitVelocity;
            penaltyVelocity = pData.PenaltyVelocity;

            effectTime = pData.EffectTime;

            _Timer.Setup(timer);

            slotMGR.SetSlot(level, jellyCount, itemCount);
            levelUI.SetJellyCount(correctJellyNum);

            jellyList = slotMGR.getAllJelly();
            //jellyHint.JellyList = jellyList;

            for (int i = 0; i < jellyList.Count; i++)
                jellyList[i].GetComponent<JellyObject>().Init(wordList[i], level, i);
        }



        // Event Handlers
        private void timer_onTimeOver()
        {
            // 젤리 수가 0이면 게임 종료, 아니면 다음 단계로
            if (correctJellyNum < jellyCount)
            {
                isTimeOver = true;
                fsm.PerformTransition(State.Reward);
            }

            else fsm.PerformTransition(State.Next);
        }
        private void jelly_onLevelOver()
        {
            if (correctJellyNum > 0) fsm.PerformTransition(State.Next);
            else fsm.PerformTransition(State.Reward);
        }
        // Overrides
        protected override GameID onGameID() => GameID.C3_G02_EatGino;
        protected override System.Type onStateType() => typeof(State);
        protected override async UniTask onPrepare(GameIndex gameIDX)
        {
            await base.onPrepare(gameIDX);

            // Start Screen
            _FadeOutTL.Play();

            // Load Unit Data
            await pMGR.Build(gameIDX, CURRICULUM, TABLES);

            // Player
            _Player.GetComponent<PlayerInput>().DisableDirectionButton();

            // UI
            //UIGameCommon.One.StarGauge.Setup(GameReward.StarRatio, totalJellyCount);
            //GameProgress.One.Setup(totalJellyCount);
        }
        protected override void onStartGame()
        {
            base.onStartGame();

            // -----------------------------------------------------------
            // 게임 전체 공용으로 BGM 재생과 볼륨조절 기능을 넣기 위해 (#880)
            // 해당 코드를 GameBase로 이동하였습니다. playBGM() 함수를 호출하면 됩니다.
            // 확인 부탁드립니다. by 문영삼
            // -----------------------------------------------------------
            // 추가로, AudioMGR.One.BgmVolume은 사용자의 환경설정 창에서 바꾸기 위한 용도로
            // AudioMixer의 채널 볼륨 자체를 줄여버립니다.
            // 그와 별개로 AudioMGR.One.PlayBGM() 함수에 볼륨을 추가하였습니다.
            // -----------------------------------------------------------
            //AudioMGR.One.PlayBGM(bgmCLIP);
            playBGM();

            fsm.StartFSM(State.Intro, RunnerParam.SkipStateTo);
        }
        protected override void onFinishGame()
        {
            base.onFinishGame();

            if (fsm.CurrentState == State.Game)
                CP(CheckPoint.UserFinish);

            fsm?.StopFSM();

            // -----------------------------------------------------------
            // 게임 전체 공용으로 BGM 재생과 볼륨조절 기능을 넣기 위해 (#880)
            // 해당 코드를 GameBase로 이동하였습니다. playBGM() 함수를 호출하면 됩니다.
            // 확인 부탁드립니다. by 문영삼
            // -----------------------------------------------------------
            // 추가로, AudioMGR.One.BgmVolume은 사용자의 환경설정 창에서 바꾸기 위한 용도로
            // AudioMixer의 채널 볼륨 자체를 줄여버립니다.
            // 그와 별개로 AudioMGR.One.PlayBGM() 함수에 볼륨을 추가하였습니다.
            // -----------------------------------------------------------
            //AudioMGR.One.StopAll();
            stopBGM();
        }
        protected override void onDebugNextStep()
        {
            switch (fsm.CurrentState)
            {
                case State.Intro: fsm.PerformTransition(State.Ready); break;
                case State.Ready: fsm.PerformTransition(State.Game); break;
                case State.Game: fsm.PerformTransition(State.Next); break;
                case State.Outro: fsm.PerformTransition(State.Reward); break;
            }
        }
        protected override void onDebugForceFinish()
        {
            base.onDebugForceFinish();
            fsm.PerformTransition(State.Reward);
        }



        // FSM
        IEnumerator E_Intro()
        {
            CP(CheckPoint.Start);

            _Timer.Reset();

            setupProblem(pMGR.Current);
            yield return null;

            _FadeInTL.Play();
            yield return playTimeline(_IntroTL);
            fsm.PerformTransition(State.Ready);
        }
        IEnumerator X_Intro()
        {
            yield return null;
        }
        IEnumerator E_Ready()
        {
            correctJellyNum = 0;
            _Player.GetComponent<PlayerInput>().EndLevel(false);
            yield return null;

            levelUI.ResetJellyCount();
            yield return null;

            UIGameCommon.One.VisiblePauseButton = true;
            yield return null;


            if (pMGR.PNO > 1)
            {
                setupProblem(pMGR.Current);
                yield return null;

                levelUI.SetJellyIllust(pMGR.PNO - 1);
                yield return new WaitForSeconds(1.0f);

                _FadeInTL.Play();
                yield return playTimeline(_LevelStartTL);
            }

            itemList = slotMGR.getAllItem();
            for (int i = 0; i < itemList.Count; i++)
                itemList[i].GetComponent<Item>().Appear();
            yield return null;

            jellyControl.SetJellyControl(jellyCount, benefitVelocity, penaltyVelocity, effectTime);
            yield return null;

            yield return playTimeline(UIGameCommon.One.ReadyGoTL);
            fsm.PerformTransition(State.Game);
        }
        IEnumerator X_Ready()
        {
            yield return null;
        }
        IEnumerator E_Game()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            //
            _Player.Halt(false);


            float blinkTime = 0f;

            // 0. 젤리 힌트
            for (int i = 0; i < jellyList.Count; i++)
            {
                jellyList[i].GetComponent<JellyObject>().BlinkHint(blinkTime);
                blinkTime += 1.0f;
            }

            for (int j = 0; j < itemList.Count; j++)
                itemList[j].GetComponent<Item>().Appear();

            // 1. 타이머 세팅
            _Timer.Ready();
            yield return null;

            // 2. 타이머 실행
            _Timer.Run();
            yield return null;

            // 3. Player 이동 가능
            _Player.GetComponent<PlayerInput>().EnableDirectionButton();
            yield return null;

            // 4. Affordance 시작
            _Player.GetComponent<PlayerInput>().StartAffordance();

            // 5. 문제 음원 출력 (최초)
            levelUI.SetSpeaker(true);
            levelUI.PlaySpeaker(wordList[0].SoundClip);
        }
        IEnumerator X_Game()
        {
            CP(CheckPoint.UserFinish);
            _Player.GetComponent<PlayerInput>().EndLevel(true);
            _Player.GetComponent<PlayerInput>().DisableDirectionButton();

            // Affordance 종료
            _Player.GetComponent<PlayerInput>().StopAffordance();


            //
            _Player.Halt(true);

            _Timer.Stop();
            yield return null;
        }
        IEnumerator E_Next()
        {
            yield return new WaitForSeconds(nextDelay);

            _Timer.Stop();

            levelUI.SetSpeaker(false);
            itemControl.HideScan();

            // Clear! - Level Up
            jellyControl.InitJellyFeedback();
            yield return null;

            if (correctJellyNum == jellyCount)
                levelUI.PlayCompleteFX();

            if (pMGR.Next())
            {
                _LevelUpTL.Play();
                yield return null;

                yield return new WaitForSeconds((float)_LevelUpTL.duration + 1.0f);

                yield return playTimeline(_FadeOutTL);

                _Player.transform.position = playerInitialPos;
                _Player.SetPosition(playerInitialPos);
                yield return null;

                slotMGR.ClearSlots();
                tileMGR.InitTile(_Player.transform.position);
                yield return null;

                fsm.PerformTransition(State.Ready);
            }
            else
            {
                yield return playTimeline(_CompleteTL);

                fsm.PerformTransition(State.Outro);
            }
            yield return null;
        }
        IEnumerator E_Outro()
        {
            CP(CheckPoint.Outro);
            yield return null;

            fsm.PerformTransition(State.Reward);
        }
        IEnumerator X_Outro()
        {
            yield return null;
        }
        IEnumerator E_Reward()
        {
            if (isTimeOver)
            {
                int rndIdx = UtilArray.RandomOne(0, _TimeOutTL.Length - 1);
                yield return playTimeline(_TimeOutTL[rndIdx]);
            }

            CP(CheckPoint.Complete);
            yield return null;

            AudioMGR.One.StopAll();
            yield return null;

            gameComplete();
            yield return null;
        }



        // Unity Inspectors
        [Header("★ TimeLines")]
        [SerializeField] private PlayableDirector _IntroTL = null;
        [SerializeField] private PlayableDirector _LevelUpTL = null;
        [SerializeField] private PlayableDirector _LevelStartTL = null;
        [SerializeField] private PlayableDirector[] _TimeOutTL = null;
        [Space()]
        [SerializeField] private PlayableDirector _CompleteTL = null;
        [SerializeField] private PlayableDirector _FadeInTL = null;
        [SerializeField] private PlayableDirector _FadeOutTL = null;
        [Header("★ Bindings")]
        [SerializeField] private SlotManager slotMGR = null;
        //[SerializeField] private JellyHint jellyHint = null;
        [SerializeField] private TileMapMGR tileMGR = null;
        [Space()]
        [SerializeField] private Timer _Timer = null;
        [SerializeField] private Player _Player = null;
        [SerializeField] private LevelUI levelUI = null;
        // -----------------------------------------------------------
        // 게임 전체 공용으로 BGM 재생과 볼륨조절 기능을 넣기 위해 (#880)
        // 해당 코드를 GameBase로 이동하였습니다. playBGM() 함수를 호출하면 됩니다.
        // 확인 부탁드립니다. by 문영삼
        // -----------------------------------------------------------
        //[Header("★ Sound")]
        //[SerializeField] private AudioClip bgmCLIP = null;
        [Header("★ Timing")]
        [SerializeField] private float nextDelay = 1f;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            initFSM();
        }
        protected override void Update()
        {
            // COIN
            // 테스트를 위한 임시코드
            if (Input.GetKeyDown(KeyCode.A))
            {
                fsm.PerformTransition(State.Next);
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                _Player.MoveSpeed = 10;
            }
        }
        protected override void Start()
        {
            playerInitialPos = _Player.transform.position;
        }
        protected override void OnEnable()
        {
            base.OnEnable();

            _Timer.OnTimeOver += timer_onTimeOver;
            jellyControl.OnLevelOver += jelly_onLevelOver;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            _Timer.OnTimeOver -= timer_onTimeOver;
            jellyControl.OnLevelOver -= jelly_onLevelOver;
        }
    }
}