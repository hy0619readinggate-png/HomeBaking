using DoDoEng.Game.Framework;
using beyondi.FSM;
using Cysharp.Threading.Tasks;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.Playables;
using DoDoEng.Game.UI;

using GameData = DoDoEng.Common.GameData_C3_G01;
using ProblemMGR = DoDoEng.Game.C3_G01.C3_G01_ProblemMGR;

namespace DoDoEng.Game.C3_G01
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C3_G01_Main : GameMain<C3_G01_Main, GameData>
    {
        // Definitions
        private enum State { Intro, Round, Game, Next, Outro, Reward }

        // Properties
        private float hpInitial = 100;
        private float hp = 0;



        // Methods
        public int[] GetLetterOrderOfWord(int wNO)
        {
            return pMGR.GetLetterOrderOfWord(wNO);
        }
        public char[] GetLetters(int wNO)
        {
            return pMGR.GetLetters(wNO);
        }
        public int GetBroomAvailableCount(int wNO)
        {
            return pMGR.GetBroomAvailableCount(wNO);
        }
        public void UpdateHP()
        {
            hp -= 20;
            if (hp < 0)
                hp = 0;

            UIGameCommon.One.HealthBar.UpdateHP(hp);

            if (hp == 0)
                fsm.PerformTransition(State.Outro);
        }
        public void UpdateBonusHP()
        {
            hp += 10;

            if (hp > hpInitial)
                hp = hpInitial;
            UIGameCommon.One.HealthBar.UpdateHP(hp);
        }


        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();

        // Fields
        private FSM<State> fsm = null;
        private readonly int hashkey_Next = Animator.StringToHash("Next");
        private readonly int hashkey_Reset = Animator.StringToHash("Reset");
        private readonly int hashkey_Open = Animator.StringToHash("Open");
        private readonly int hashkey_Close = Animator.StringToHash("Close");


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
            _TableMGR.Setup(pData);

            UIGameCommon.One.Progress.Setup(pMGR.ProblemCounts[pMGR.PNO - 1]);
        }



        // Event Handlers
        private void player_OnCrash()
        {
            hp -= 10;
            if (hp <= 0)
                hp = 0;

            UIGameCommon.One.HealthBar.UpdateHP(hp);

            if (hp == 0)
                fsm.PerformTransition(State.Reward);
        }


        // Overrides
        protected override GameID onGameID() => GameID.C3_G01_TeaPartyTable;
        protected override System.Type onStateType() => typeof(State);
        protected override async UniTask onPrepare(GameIndex gameIDX)
        {
            await base.onPrepare(gameIDX);

            // Load Unit Data
            await pMGR.Build(gameIDX, CURRICULUM, TABLES);


            // Init
            hp = hpInitial;

            UIGameCommon.One.HealthBar.Setup(hp);
            UIGameCommon.One.StarGauge.Setup(GameReward.StarRatio, pMGR.TotalProblemCount);
            GameProgress.One.Setup(pMGR.TotalProblemCount);

            // setup
            setupProblem(pMGR.Current);
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

            fsm?.StopFSM();

            stopBGM();
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



        // FSM
        IEnumerator E_Intro()
        {
            CP(CheckPoint.Start);
            yield return new WaitForSeconds(1);

            fsm.PerformTransition(State.Round);
        }
        IEnumerator X_Intro()
        {
            yield return null;
        }
        IEnumerator E_Round()
        {
            var tl = pMGR.PNO == 1
               ? UIGameCommon.One.ReadyGoTL
               : UIGameCommon.One.LevelUpTL;
            yield return playTimeline(tl);

            if (pMGR.PNO == 1)
                UIGameCommon.One.VisiblePauseButton = true;
            else if (pMGR.PNO > 1)
                setupProblem(pMGR.Current);
            yield return null;

            fsm.PerformTransition(State.Game);
        }
        IEnumerator X_Round()
        {
            yield return null;
        }
        IEnumerator E_Game()
        {
            CP(CheckPoint.UserStart);

            _Player.UseInput = true;
            _Player.StartPlay();
            yield return null;


            // 단어 반복..
            int wordNO = 1;
            while (wordNO <= pMGR.Current.WordDatas.Length)
            {
                // Ready
                entranceANI.SetTrigger(hashkey_Open);

                _TableMGR.Ready(wordNO);
                yield return new WaitForSeconds(0.5f);


                // Run
                UIGameCommon.One.Progress.Increase();
                _Player.StartGesture();
                _TableMGR.Run();
                yield return new WaitUntil(() => _TableMGR.Completed);

                _Player.PlayCorrectClip();
                _TableMGR.Stop();
                yield return _TableMGR.Clear();

                entranceANI.SetTrigger(hashkey_Close);
                yield return new WaitForSeconds(1f);

                UIGameCommon.One.StarGauge.Success();
                GameProgress.One.Correct();

                wordNO++;
                yield return null;
            }

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_Game()
        {
            CP(CheckPoint.UserFinish);

            _Player.StopPlay();
            _Player.StopGesture();
            _TableMGR.Stop();
            yield return null;
        }
        IEnumerator E_Next()
        {
            yield return new WaitForSeconds(nextDelay);

            if (pMGR.Next())
            {
                UIGameCommon.One.Progress.Setup(pMGR.ProblemCounts[pMGR.PNO - 1]);

                tableClothANI.SetTrigger(hashkey_Next);

                fsm.PerformTransition(State.Round);
            }
            else fsm.PerformTransition(State.Outro);
            yield return null;
        }
        IEnumerator E_Outro()
        {
            CP(CheckPoint.Outro);
            yield return null;

            _Player.UseInput = false;
            _Player.StopGesture();
            _Player.MoveOriginPosition();
            yield return new WaitForSeconds(2f);

            yield return playTimeline(endingTL);

            fsm.PerformTransition(State.Reward);
        }
        IEnumerator X_Outro()
        {
            yield return null;
        }
        IEnumerator E_Reward()
        {
            Debug.Log("Reward");

            _Player.UseInput = false;

            CP(CheckPoint.Complete);
            yield return null;

            //AudioMGR.One.StopAll();
            AudioMGR.One.StopBGM(true, 4f);
            yield return new WaitForSeconds(3f);

            gameComplete();
            yield return null;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Player _Player = null;
        [SerializeField] private TableMGR _TableMGR = null;
        [Space()]
        [SerializeField] private PlayableDirector endingTL = null;
        [SerializeField] private Animator tableClothANI = null;
        [SerializeField] private Animator entranceANI = null;

        [Header("★ Timing")]
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

            _Player.OnCrash += player_OnCrash;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            _Player.OnCrash -= player_OnCrash;

        }

    }
}