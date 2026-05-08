using beyondi.FSM;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.Game.Framework;
using DoDoEng.Game.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GameData = DoDoEng.Common.GameData_C2_G03;
using ProblemMGR = DoDoEng.Game.C2_G03.C2_G03_ProblemMGR;

namespace DoDoEng.Game.C2_G03
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C2_G03_Main : GameMain<C2_G03_Main, GameData>
    {
        // Definitions
        enum State
        {
            Intro,
            Start, Ready,
            Game,
            Outro, Reward,
            GameOver
        }

        // Fields : caching
        ProblemMGR pMGR => GetComponent<ProblemMGR>();
        [HideInInspector] public int currentRound => pMGR.PNO;

        // Fields
        FSM<State> fsm = null;

        // Functions
        void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Intro,       E_Intro,        X_Intro);
            fsm.AddState(State.Start,       E_Start,        X_Start);
            fsm.AddState(State.Ready,       E_Ready,        X_Ready);
            fsm.AddState(State.Game,        E_Game,         X_Game);
            fsm.AddState(State.Outro,       E_Outro,        X_Outro);
            fsm.AddState(State.Reward,      E_Reward);
            fsm.AddState(State.GameOver,    E_GameOver);
            #pragma warning restore format
        }

        bool bSetupQuiz = false;
        public bool IsSetupQuiz => bSetupQuiz;
        void setupProblem(ProblemData pData)
        {
            //Debug.Log(pData);
            //foreach (var b in pData.Quizs) Debug.Log(b);
            //StartCoroutine(IEPlayNarrations(pData));
            bSetupQuiz = true;
        }

        object lockObject = new object();
        Quiz[] QuizBuff;
        public Quiz GetQuiz()
        {
            lock (lockObject)
            {
                if (!bSetupQuiz) return null;
                if (QuizBuff == null || QuizBuff.Length == 0) QuizBuff = (Quiz[])pMGR.Current.Quizs.Clone();
                Quiz quiz = QuizBuff[QuizBuff.Length - 1];
                QuizBuff = QuizBuff.SkipLast(1).ToArray();
                if (QuizBuff.Length == 0) pMGR.Next();
                ShowQuizIndigator();
                return quiz;
            }
        }

        public void UpdateHP()
        {
            if (currentLife < Life) UIGameCommon.One.HealthBar.UpdateHP(++currentLife);
        }

        public void Correct()
        {
            UIGameCommon.One.StarGauge.Success();
            GameProgress.One.Correct();
        }
        public void Wrong()
        {
            UIGameCommon.One.HealthBar.UpdateHP(--currentLife);
            if (currentLife > 0) return;

            fsm.PerformTransition(State.GameOver);
            Character.Instance.GameJustStop();
        }
        public void Fall() => Wrong();
        public void Crash() => Wrong();

        int countQuiz = 0;
        public void CheckQuizCount()
        {
            if (++countQuiz >= pMGR.TotalProblemCount)
            {
                fsm.PerformTransition(State.GameOver);
                Character.Instance.StartEndMotion();
            }
        }

        public bool IsPlay => fsm.CurrentState == State.Game;

        // Overrides
        protected override GameID onGameID() => GameID.C2_G03_WaterSlide;
        protected override System.Type onStateType() => typeof(State);
        protected override async UniTask onPrepare(GameIndex gameIDX)
        {
            await base.onPrepare(gameIDX);
            await pMGR.Build(gameIDX, CURRICULUM, TABLES);
            currentLife = Life;
            UIGameCommon.One.HealthBar.Setup(Life);
            UIGameCommon.One.StarGauge.Setup(GameReward.StarRatio, pMGR.TotalProblemCount);
            GameProgress.One.Setup(pMGR.TotalProblemCount);
            setupProblem(pMGR.Current);
        }

        protected override void onStartGame()
        {
            Debug.Log("★★ onStartGame");
            base.onStartGame();

            fsm.StartFSM(State.Intro, RunnerParam.SkipStateTo);
        }
        protected override void onFinishGame()
        {
            Debug.Log("★★ onFinishGame");
            base.onFinishGame();

            if (fsm.CurrentState == State.Game)
                CP(CheckPoint.UserFinish);

            fsm?.StopFSM();
        }

        protected override void onDebugNext()
        {
            switch (fsm.CurrentState)
            {
                case State.Intro: fsm.PerformTransition(State.Start); break;
                case State.Start: fsm.PerformTransition(State.Game); break;
                case State.Game: fsm.PerformTransition(State.Outro); break;
                case State.Outro: fsm.PerformTransition(State.Reward); break;
            }
        }


        // FSM
        IEnumerator E_Intro()
        {
            Debug.Log("★★ E_Intro");
            CP(CheckPoint.Start);
            yield return null;
            fsm.PerformTransition(State.Start);
        }
        IEnumerator X_Intro()
        {
            Debug.Log("★★ X_Intro");
            yield return null;
        }

        UnityEngine.Playables.PlayableDirector PD = null;
        IEnumerator E_Start()
        {
            Debug.Log("★★ E_Start");
            PD = UIGameCommon.One.ReadyGoTL;
            yield return playTimeline(PD);
            fsm.PerformTransition(State.Ready);
        }
        IEnumerator X_Start()
        {
            Debug.Log("★★ X_Start");
            yield return stopTimeline(PD);
        }

        IEnumerator E_Ready()
        {
            Debug.Log("★★ E_Ready");
            UIGameCommon.One.VisiblePauseButton = true;
            yield return new WaitForSeconds(1);
            fsm.PerformTransition(State.Game);
        }
        IEnumerator X_Ready()
        {
            Debug.Log("★★ X_Ready");
            yield return null;
        }

        int roundQuizCount = 0;
        [HideInInspector]
        public bool IsRoundComplete(int up = 0)
        {
            return (countQuiz + up) >= roundQuizCount;
        }
        const int LastRound = 3;
        IEnumerator E_Game()
        {
            Debug.Log("★★ E_Game");
            yield return null;
            CP(CheckPoint.UserStart);
            playBGM();
            PlayAmbient(Character.Instance.GetAudioClip("amb"));
            Character.Instance.PlayAudioAmbientMove();
            AudioMGR.One.PlayEffect(Character.Instance.GetAudioClip("dodo_intro"));

            StartCoroutine(IEResetQuizCounter(0));
            while (currentRound <= LastRound)
            {
                roundQuizCount = 0;
                for (int n = 0; n < currentRound; ++n) roundQuizCount += pMGR.QuizCounts[n];
                yield return new WaitUntil(() => IsRoundComplete());

                StartCoroutine(IEResetQuizCounter(1.5f));

                if (currentRound <= LastRound)
                {
                    PD = UIGameCommon.One.LevelUpTL;
                    yield return playTimeline(PD);
                    yield return stopTimeline(PD);
                    UIGameCommon.One.VisiblePauseButton = true;
                }
            }
        }
        IEnumerator IEResetQuizCounter(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            xCount = 0;
            ShowQuizIndigator();
        }
        IEnumerator X_Game()
        {
            Debug.Log("★★ X_Game");
            yield return new WaitUntil(() => Character.Instance.IsEndMotionComplete);

            AudioMGR.One.StopAll();
            StopAmbient(null);
            //stopBGM();
            Character.Instance.EndGame();
            yield return null;
            CP(CheckPoint.UserFinish);
            yield return null;
            CP(CheckPoint.Complete);
        }

        IEnumerator E_Outro()
        {
            Debug.Log("★★ E_Outro");
            yield return null;
            CP(CheckPoint.Outro);

            yield return null;
            fsm.PerformTransition(State.Reward);
        }
        IEnumerator X_Outro()
        {
            Debug.Log("★★ X_Outro");
            yield return null;
        }

        IEnumerator E_Reward()
        {
            Debug.Log("★★ E_Reward");
            yield return null;
            //CP(CheckPoint.Complete);

            yield return null;
            AudioMGR.One.StopAll();

            yield return null;
            AudioMGR.One.PlayEffect(SfxMoment.Activity_Complete);

            yield return null;
            gameComplete();
        }

        IEnumerator E_GameOver()
        {
            Debug.Log("★★ E_GameOver");
            yield return null;
            gameComplete();
        }


        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField][Min(1)] int Life = 3;
        int currentLife;
        [SerializeField] TextMeshProUGUI QuizCountText;
        int xCount = 0;
        void ShowQuizIndigator()
        {
            int M = pMGR.QuizCounts[currentRound - 1];
            QuizCountText.text = $"{xCount}/{M}";
        }
        public void ShowQuizIndigatorUP()
        {
            xCount++;
            ShowQuizIndigator();
        }

        protected override void Awake()
        {
            base.Awake();
            initFSM();
        }

        ///////////////////////////////////////////////////////////////////////
        List<AudioSource> audioSourceAmbs = new();
        public void PlayAmbient(AudioClip clip)
        {
            for (int i = 0; i < audioSourceAmbs.Count; ++i)
            {
                if (audioSourceAmbs[i].clip == clip)
                {
                    Debug.Log($"★ PlayAmbient(OLD): {clip.name}");
                    audioSourceAmbs[i].Play();
                    return;
                }
            }
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.Play();
            audioSourceAmbs.Add(source);
            Debug.Log($"★ PlayAmbient(NEW): {clip.name}");
        }
        public void StopAmbient(AudioClip clip)
        {
            for (int i = 0; i < audioSourceAmbs.Count; ++i)
            {
                if (!clip)
                {
                    Debug.Log($"★ StopAmbient(A): {audioSourceAmbs[i].clip.name}");
                    audioSourceAmbs[i].Stop();
                    continue;
                }
                if (audioSourceAmbs[i].clip == clip)
                {
                    Debug.Log($"★ StopAmbient(B): {audioSourceAmbs[i].clip.name}");
                    audioSourceAmbs[i].Stop();
                    return;
                }
            }
        }
    }
}