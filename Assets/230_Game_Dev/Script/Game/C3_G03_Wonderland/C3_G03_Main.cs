using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.Game.Framework;
using DoDoEng.Game.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

using GameData = DoDoEng.Common.GameData_C3_G03;
using ProblemMGR = DoDoEng.Game.C3_G03.C3_G03_ProblemMGR;


namespace DoDoEng.Game.C3_G03
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C3_G03_Main : GameMain<C3_G03_Main, GameData>
    {
        // Definitions
        private enum State { Intro, Transition, Ready, Game, Next, Outro, Reward }
        // Properties
        public string AnswerSentence => answerSentence;



        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();

        // Fields
        private FSM<State> fsm = null;
        private float hpInitial = 50;
        private float hp = 0;

        private string answerSentence;

        private int chunkNum = 0;
        private int transitionIndex = 0;
        private int currBlockNum = 0;

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
        private void setupProblem(ProblemData pData)
        {
            var currSentence = pData.Sentence;

            int mustCorrectIdxNum = currSentence.CorrectBlockCount;

            List<string> currChunks = new();
            List<string> currSelectedChunks = new();

            foreach (string chunk in currSentence.Chunk)
                if (chunk != "") currChunks.Add(chunk);

            List<string> restChunks = new();

            answerSentence = pData.Sentence.Sentence;
            chunkNum = currSentence.ChunkNum;
            currBlockNum = chunkNum * 2 + currSentence.BlockCount;


            foreach (SentenceData st in pData.RestSentences)
            {
                for (int i = 0; i < st.ChunkNum; i++)
                {
                    if (!currChunks.Contains(st.Chunk[i])) restChunks.Add(st.Chunk[i]);
                }
            }
            List<string> distinctRestChunks = new();

            distinctRestChunks = restChunks.Distinct().ToList();

            puzzleSlots[pMGR.PNO - 1].SetRestChunkRndIdx(chunkNum, distinctRestChunks);

            puzzleSlots[pMGR.PNO - 1].SetPuzzleType(chunkNum, currSentence.BlockCount);
            var rndCorrectIdx = UtilArray.Random(0, chunkNum, mustCorrectIdxNum);

            puzzleSlots[pMGR.PNO - 1].SetAnswer(chunkNum, currChunks);
            puzzleSlots[pMGR.PNO - 1].SetMustCorrectBlock(chunkNum, mustCorrectIdxNum, currChunks, distinctRestChunks, currSentence);

            puzzleSlots[pMGR.PNO - 1].SetBlock(chunkNum, distinctRestChunks, currChunks, currSentence);
            puzzleSlots[pMGR.PNO - 1].Setup(pData.Sentence.sentenceIMG, pData.Sentence.SoundCLIP);

            puzzleSlots[pMGR.PNO - 1].StackType.OnClear += stackType_onClear;
            if (pMGR.PNO - 1 > 0) puzzleSlots[pMGR.PNO - 2].StackType.OnClear -= stackType_onClear;
        }



        // Event Handlers
        private void timer_onTimeOver()
        {
            hp -= 10;
            UIGameCommon.One.HealthBar.UpdateHP(hp);

            if (hp <= 0)
                fsm.PerformTransition(State.Reward);
        }
        private void timer_onTimerRing()
        {
            int rndIdx = UtilArray.RandomOne(0, _TimeOverTL.Length - 1);
            _TimeOverTL[rndIdx].Play();
        }
        private void timeer_pnPlayerTeeter()
        {
            _PlayerIdleTL.Play();
        }
        private void stackType_onClear()
        {
            fsm.PerformTransition(State.Next);
        }



        // Overrides
        protected override GameID onGameID() => GameID.C3_G03_Wonderland;
        protected override System.Type onStateType() => typeof(State);
        protected override async UniTask onPrepare(GameIndex gameIDX)
        {
            await base.onPrepare(gameIDX);

            // Load Unit Data
            await pMGR.Build(gameIDX, CURRICULUM, TABLES);


            // init
            hp = hpInitial;
            transitionIndex = 0;

            _Timer.Setup(maxTime);
            _Timer.SetChance((int)hp / 10);


            // UI
            UIGameCommon.One.HealthBar.Setup(hp);
            UIGameCommon.One.StarGauge.Setup(GameReward.StarRatio, pMGR.Problems.Length);
            GameProgress.One.Setup(pMGR.Problems.Length);
            //GameProgress.One.Setup(pMGR.Problems.Length / 2);

            // setup
            setupProblem(pMGR.Current);
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
            AudioMGR.One.PlayAmbient(abmCLIP, abmVolume / 100f);

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
            AudioMGR.One.StopAmbient();
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
            yield return playTimeline(_IntroTL);

            fsm.PerformTransition(State.Ready);
            yield return null;
        }
        IEnumerator X_Intro()
        {
            yield return null;
        }
        IEnumerator E_Ready()
        {
            UIGameCommon.One.VisiblePauseButton = true;
            yield return null;

            if (pMGR.PNO > 1)
            {
                yield return new WaitForSeconds(1.0f);

                setupProblem(pMGR.Current);
                yield return null;

                int rndIdx = UtilArray.RandomOne(0, _CorrectTLArr.Length - 1);
                yield return playTimeline(_CorrectTLArr[rndIdx]);

                yield return playTimeline(_TransitionTLArr[transitionIndex++]);
            }

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

            // Ready
            // 1.이미지 
            puzzleSlots[pMGR.PNO - 1].ShowProblemBoard();
            yield return new WaitForSeconds(2f);

            // 2.타이머 UI Show!!
            _Timer.Ready();
            puzzleSlots[pMGR.PNO - 1].MoveProblemBoard();
            yield return new WaitForSeconds(1f);

            _Timer.Show();
            yield return null;

            // 3.사각형 떨어짐
            var block = FindObjectsOfType<Block>();
            yield return StartCoroutine(crBlockFall(block));

            // 4.사각형 다떨어지고 시작~!!
            _Timer.Run();
        }
        IEnumerator X_Game()
        {
            CP(CheckPoint.UserFinish);

            if (hp > 0) _Timer.Stop();
            yield return null;
        }
        IEnumerator E_Next()
        {
            yield return new WaitForSeconds(nextDelay);

            //if (pMGR.PNO % 2 == 0)
            //{
            UIGameCommon.One.StarGauge.SuccessForWonderland();
            GameProgress.One.Correct();
            yield return null;
            //}


            if (pMGR.Next())
            {
                fsm.PerformTransition(State.Ready);
            }
            else
            {
                yield return new WaitForSeconds(2.0f);

                int rndIdx = UtilArray.RandomOne(0, _CorrectTLArr.Length - 1);
                yield return playTimeline(_CorrectTLArr[rndIdx]);

                yield return playTimeline(_EndingTL);

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
            CP(CheckPoint.Complete);
            yield return null;

            AudioMGR.One.StopAll();
            yield return null;

            gameComplete();
            yield return null;
        }



        // Unity Inspectors
        [Header("★ Binding")]
        // -----------------------------------------------------------
        // 게임 전체 공용으로 BGM 재생과 볼륨조절 기능을 넣기 위해 (#880)
        // 해당 코드를 GameBase로 이동하였습니다. playBGM() 함수를 호출하면 됩니다.
        // 확인 부탁드립니다. by 문영삼
        //[SerializeField] private AudioClip bgmCLIP = null;
        [SerializeField] private AudioClip abmCLIP = null;
        [Range(0, 100)] [SerializeField] int abmVolume = 100;
        [Space()]
        [SerializeField] private PlayableDirector _IntroTL = null;
        [SerializeField] private PlayableDirector _EndingTL = null;
        [SerializeField] private PlayableDirector _PlayerIdleTL = null;
        [SerializeField] private PlayableDirector[] _TimeOverTL = null;
        [Space()]
        [SerializeField] private PlayableDirector[] _CorrectTLArr = null;
        [SerializeField] private PlayableDirector[] _TransitionTLArr = null;
        [Space()]
        [SerializeField] private Timer _Timer = null;
        [SerializeField] private PuzzleSlot[] puzzleSlots = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip introSFX = null;
        [Header("★ Timing")]
        [SerializeField, Range(0, 100)] private float maxTime = 90f;
        [SerializeField] private float nextDelay = 1f;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            initFSM();
        }
        protected override void Start()
        {

        }
        protected override void OnEnable()
        {
            base.OnEnable();

            _Timer.OnTimeOver += timer_onTimeOver;
            _Timer.OnTimerRing += timer_onTimerRing;
            _Timer.OnPlayerTeeter += timeer_pnPlayerTeeter;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            _Timer.OnTimeOver -= timer_onTimeOver;
            _Timer.OnTimerRing -= timer_onTimerRing;
            _Timer.OnPlayerTeeter -= timeer_pnPlayerTeeter;
        }


        // Coroutines
        IEnumerator crBlockFall(Block[] block)
        {
            AudioMGR.One.PlayEffect(introSFX);
            for (int i = currBlockNum; i > 0; i--)
            {
                float rndDelayTime = Random.Range(0.0f, 0.5f);
                yield return new WaitForSeconds(rndDelayTime);

                block[i - 1].Intro();
                yield return null;

            }
            yield return new WaitForEndOfFrame();

            puzzleSlots[pMGR.PNO - 1].SetGlowBox(chunkNum, 0);
            yield return new WaitForSeconds(1.0f);

            var introGlowBlocks = puzzleSlots[pMGR.PNO - 1].StackType.IntroGlowBlocks;

            if (introGlowBlocks != null)
                foreach (Block b in introGlowBlocks) b.InitialGlow();
            yield return null;

            foreach (var b in block) b.enabled = true;
        }
    }
}