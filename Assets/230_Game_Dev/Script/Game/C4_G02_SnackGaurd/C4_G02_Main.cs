using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DoDoEng.Common;
using DoDoEng.Game.UI;
using DoDoEng.Game.Framework;
using beyondi.FSM;
using UnityEngine.UI;

using GameData = DoDoEng.Common.GameData_C4_G02;
using ProblemMGR = DoDoEng.Game.C4_G02.C4_G02_ProblemMGR;

namespace DoDoEng.Game.C4_G02
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C4_G02_Main : GameMain<C4_G02_Main, GameData>
    {
        // Definitions
        private enum State { Intro, Round, Game, Next, Outro, Reward }



        // Methods
        public void Success()
        {
            success = true;

            UIGameCommon.One.StarGauge.Success();
            GameProgress.One.Correct();
            Player.One.Success();
        }
        public void Fail()
        {
            Player.One.Freeze();
        }
        public void GameOver()
        {
            if (fsm.CurrentState == State.Game)
            {
                gameOver = true;
                fsm.PerformTransition(State.Outro);
            }
        }



        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();
        private Player player_ = null;
        private Player player => player_ ??= FindObjectOfType<Player>();

        // Fields
        private FSM<State> fsm = null;
        private bool success = false;
        private bool gameOver = false;
        private AudioClip questionClip = null;


        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Intro,       E_Intro,        X_Intro);
            fsm.AddState(State.Round,       E_Round,        X_Round);
            fsm.AddState(State.Game,        E_Game,        X_Game);
            fsm.AddState(State.Next,        E_Next);
            fsm.AddState(State.Outro,       E_Outro,        X_Outro);
            fsm.AddState(State.Reward,      E_Reward);
            #pragma warning restore format
        }
        private void setupProblem(ProblemData pData)
        {
            UIGameCommon.One.Progress.Setup(pMGR.CustomerCounts[pMGR.PNO - 1]);

            BubbleMGR.One.SetConfig(pData.BubbleGeneratorConfig);
            BubbleMGR.One.SetRule(pData.BubbleGeneratorRule);

            _Acorn.Setup();
        }



        // Overrides
        protected override GameID onGameID() => GameID.C4_G02_ProtectAcron;
        protected override System.Type onStateType() => typeof(State);
        protected override async UniTask onPrepare(GameIndex gameIDX)
        {
            await base.onPrepare(gameIDX);

            // Load Unit Data
            await pMGR.Build(gameIDX, CURRICULUM, TABLES);

            // UI
            UIGameCommon.One.StarGauge.Setup(GameReward.StarRatio, pMGR.TotalProblemCount);

            GameProgress.One.Setup(pMGR.TotalProblemCount);

            // setup
            setupProblem(pMGR.Current);
            gameOver = false;
        }
        protected override void onStartGame()
        {
            base.onStartGame();

            playBGM();
            fsm.StartFSM(State.Intro, RunnerParam.SkipStateTo);
        }
        protected override void onFinishGame()
        {
            base.onFinishGame();

            if (fsm.CurrentState == State.Game)
                CP(CheckPoint.UserFinish);

            stopBGM();
            fsm?.StopFSM();
        }
        protected override void onDebugNextStep()
        {
            switch (fsm.CurrentState)
            {
                case State.Intro: fsm.PerformTransition(State.Round); break;
                case State.Round: fsm.PerformTransition(State.Game); break;
                case State.Game: fsm.PerformTransition(State.Next); break;
                case State.Outro: fsm.PerformTransition(State.Reward); break;
            }
        }
        protected override void onDebugForceFinish()
        {
            base.onDebugForceFinish();

            fsm.PerformTransition(State.Reward);
        }



        // Event Handlers
        private void onSpeackerBTNOnClick()
        {
            AudioMGR.One.PlayNarration(questionClip);
        }



        // FSM
        IEnumerator E_Intro()
        {
            CP(CheckPoint.Start);
            yield return new WaitForSeconds(1);
            yield return null;

            AudioMGR.One.PlayAmbient(_AmbClip);
            fsm.PerformTransition(State.Round);
        }
        IEnumerator X_Intro()
        {
            yield return null;
        }
        IEnumerator E_Round()
        {
            if (pMGR.PNO > 1)
                setupProblem(pMGR.Current);

            var tl = pMGR.PNO == 1
                ? UIGameCommon.One.ReadyGoTL
                : UIGameCommon.One.LevelUpTL;
            yield return playTimeline(tl);
            yield return null;

            //if (pMGR.PNO == 1)
            _Acorn.Show();

            fsm.PerformTransition(State.Game);
        }
        IEnumerator X_Round()
        {
            var tl = pMGR.PNO == 1
                ? UIGameCommon.One.ReadyGoTL
                : UIGameCommon.One.LevelUpTL;
            yield return stopTimeline(tl);
        }
        IEnumerator E_Game()
        {
            UIGameCommon.One.VisiblePauseButton = true;

            CP(CheckPoint.UserStart);
            yield return null;

            player.UseInput = true;
            Player.One.StartPlay();
            BubbleMGR.One.HaltGenerator(false);
            player.StartGesture();

            // Run
            foreach (var data in pMGR.Current.QuestionDatas)
            {
                // Ready
                questionClip = data.SoundClip;
                success = false;

                BubbleMGR.One.SetTexts(data.AnswerText, pMGR.Current.ExampleTexts);
                Question.One.Setup(data.AnswerTextIndex, data.QuestionTexts);
                yield return new WaitForSeconds(1f);

                Question.One.Show();
                AudioMGR.One.PlayNarration(data.SoundClip, 0.5f);
                yield return null;


                // Run
                UIGameCommon.One.Progress.Increase();
                BubbleMGR.One.Generate(true);
                yield return new WaitForSeconds(data.SoundClip.length);


                _SpeakerBTN.interactable = true;
                yield return new WaitUntil(() => success);

                questionClip = null;
                _SpeakerBTN.interactable = false;


                // Feedback
                Question.One.Success();
                BubbleMGR.One.HaltGenerator(true);
                AudioMGR.One.PlayNarration(data.SoundClip, 0.5f);
                yield return new WaitForSeconds(data.SoundClip.length + 0.5f);

                BubbleMGR.One.HaltGenerator(false);
                Question.One.Hide();

                yield return null;
            }

            player.StopGesture();
            Player.One.StopPlay();
            BubbleMGR.One.HaltGenerator(true);
            BubbleMGR.One.PopAllBubble(true);

            MonsterMGR.One.AllMoveFast();
            yield return new WaitForSeconds(1f);


            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_Game()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            _SpeakerBTN.interactable = false;
            questionClip = null;
            player.StopGesture();
            Player.One.StopPlay();

            BubbleMGR.One.Generate(false);
            BubbleMGR.One.PopAllBubble(false);


            MonsterMGR.One.AllMoveFast();

            Question.One.Hide();
            yield return null;
        }
        IEnumerator E_Next()
        {
            yield return new WaitForSeconds(nextDelay);

            _Acorn.Hide();
            yield return new WaitForSeconds(0.5f);

            if (pMGR.Next())
            {
                fsm.PerformTransition(State.Round);
            }
            else
            {
                player.UseInput = false;
                fsm.PerformTransition(State.Outro);
            }
            yield return null;

        }
        IEnumerator E_Outro()
        {
            CP(CheckPoint.Outro);
            yield return null;

            player.StopGesture();
            Player.One.StopPlay();

            if (gameOver)
            {
                AudioMGR.One.PlayEffect(_GameOverClip);
                yield return Player.One.GameOver();
            }
            else
            {
                yield return Player.One.Complete();
            }

            fsm.PerformTransition(State.Reward);
        }
        IEnumerator X_Outro()
        {
            player.StopGesture();
            yield return null;
        }
        IEnumerator E_Reward()
        {
            player.UseInput = false;

            CP(CheckPoint.Complete);
            yield return null;

            AudioMGR.One.StopAll();
            player.StopGesture();

            gameComplete();
            yield return null;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Acorn _Acorn = null;
        [SerializeField] private Button _SpeakerBTN = null;
        [Space()]
        [SerializeField] private AudioClip _GameOverClip = null;
        [SerializeField] private AudioClip _AmbClip = null;
        [Header("★ Timing")]
        [SerializeField] private float nextDelay = 1f;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            initFSM();
            _SpeakerBTN.interactable = false;

        }
        protected override void Start()
        {
            _SpeakerBTN.onClick.AddListener(onSpeackerBTNOnClick);
        }
        protected override void OnEnable()
        {
            base.OnEnable();
        }
        protected override void OnDisable()
        {
            base.OnDisable();
        }
    }
}