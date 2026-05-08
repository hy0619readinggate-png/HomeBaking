using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DoDoEng.Common;
using DoDoEng.EBook.Framework;
using SRDebugger;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.EBook.Quiz
{
    public class EBookQuiz : EBookQuizBase
    {
        // Definitions
        private enum State
        {
            Intro,
            Problem, User, Feedback, Review,
            Next, Outro, Reward
        }



        // Fields
        private FSM<State> fsm = null;
        private int pNO = 1;
        private int pCount = 0;

        // Fields
        private Dictionary<int, EBookQuizPanel> panels = new Dictionary<int, EBookQuizPanel>();
        private EBookQuizPanel pPanel = null;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Intro,      E_Intro,    X_Intro);
            fsm.AddState(State.Problem,    E_Problem,  X_Problem);
            fsm.AddState(State.User,       E_User,     X_User);
            fsm.AddState(State.Feedback,   E_Feedback, X_Feedback);
            fsm.AddState(State.Review,     E_Review,   X_Review);
            fsm.AddState(State.Next,       E_Next);
            fsm.AddState(State.Outro,      E_Outro,    X_Outro);
            fsm.AddState(State.Reward,     E_Reward);
            #pragma warning restore format
        }
        private void setupProblem(EBookQuizData quizData)
        {
            if (pPanel != null)
                pPanel.gameObject.SetActive(false);

            var type = quizData.Type;
            pPanel = panels[type];
            pPanel.gameObject.SetActive(true);
            pPanel.Setup(quizData);

            lifeGauge.Setup(pPanel.Life);
        }
        private Sprite takeScreenShot()
        {
            var canvas = pPanel.GetComponentInParent<Canvas>();
            return Util.CaptureCanvas(canvas);
        }

        // Event Handlers
        private void timer_OnTimeOver()
        {
            LOG.Info($"timer_OnTimeOver()", this);

            fsm.PerformTransition(State.Outro);
        }

        // Overrides
        protected override async UniTask onPrepare(EBookQuizIndex ebIDX)
        {
            await base.onPrepare(ebIDX);

            pNO = 1;
            pCount = QUIZ_DATA.Length;

            EBookQuizProgress.One.Setup(QUIZ_DATA.Length);

            progressSLD.value = 0;

            // Take empty screen shot
            for (var i = pCount; i > 0; i--)
            {
                setupProblem(QUIZ_DATA[i - 1]);

                await UniTask.Yield();
                await UniTask.WaitForEndOfFrame(this);
                EBookQuizProgress.One.ScreenShot(i, takeScreenShot());
            }
        }
        protected override void onStartEBookQuiz()
        {
            base.onStartEBookQuiz();

            fsm.StartFSM(State.Intro, RunnerParam.SkipStateTo);
        }
        protected override void onFinishEBookQuiz()
        {
            base.onFinishEBookQuiz();

            fsm?.StopFSM();
            AudioMGR.One.StopAll();
            AffordanceMGR.One.Clear();
        }
        protected override void onDebugNext()
        {
            switch (fsm.CurrentState)
            {
                case State.Intro: fsm.PerformTransition(State.Problem); break;
                case State.Problem: fsm.PerformTransition(State.User); break;
                case State.User: fsm.PerformTransition(State.Feedback); break;
                case State.Feedback: fsm.PerformTransition(State.Review); break;
                case State.Review: fsm.PerformTransition(State.Next); break;
            }
        }
        protected override void onDebugNextProblem()
        {
            switch (fsm.CurrentState)
            {
                case State.Intro:
                case State.Problem:
                case State.User:
                case State.Feedback:
                case State.Review:
                    fsm.PerformTransition(State.Next);
                    break;
            }
        }
        protected override void onDebugForceFinish()
        {
            base.onDebugForceFinish();

            fsm.PerformTransition(State.Outro);
        }
#if !DISABLE_SRDEBUGGER
        protected override void onBuildOption(DynamicOptionContainer srOptionContainer)
        {
            base.onBuildOption(srOptionContainer);

            var sort = 400;

            srOptionContainer.AddOption(
                OptionDefinition.FromMethod("Skip Timer", () =>
                {
                    timer.DEV_SkipToEnd();
                }, "EBookQuiz", ++sort));
        }
#endif



        // FSM
        IEnumerator E_Intro()
        {
            yield return null;

            timer.StartTimer(timerDuration);
            fsm.PerformTransition(State.Problem);
        }
        IEnumerator X_Intro()
        {
            yield return null;
        }
        IEnumerator E_Problem()
        {
            if (pNO > 1)
                setupProblem(QUIZ_DATA[pNO - 1]);
            yield return new WaitForSeconds(0.5f);
            yield return null;

            yield return pPanel.ShowProblem();
            yield return null;

            fsm.PerformTransition(State.User);
        }
        IEnumerator X_Problem()
        {
            yield return null;
        }
        IEnumerator E_User()
        {
            LOG.Important($"{QUIZ_DATA[pNO - 1]}", this);

            AffordanceMGR.One.StartMonitor(affTimeout);

            pPanel.EnableUserInteraction(true);
            yield return pPanel.SolveProblem();
            yield return null;

            fsm.PerformTransition(State.Feedback);
        }
        IEnumerator X_User()
        {
            AffordanceMGR.One.StopMonitor();
            AudioMGR.One.StopNarration();
            pPanel.EnableUserInteraction(false);
            yield return null;
        }
        IEnumerator E_Feedback()
        {
            yield return new WaitForSeconds(0.2f);
            yield return pPanel.CheckAnswer();

            EBookQuizProgress.One.Correct(pNO, pPanel.IsCorrect);

            if (!pPanel.IsCorrect)
            {
                lifeGauge.Decrease();
                yield return new WaitForSeconds(heartDecDelay);

                if (lifeGauge.Life == 0)
                {
                    yield return pPanel.ShowAnswer();
                    yield return new WaitForSeconds(showAnswerDelay);

                    fsm.PerformTransition(State.Review);
                }
                else
                {
                    yield return new WaitForSeconds(0.5f); // 오답 상태를 보여주는 대기
                    fsm.PerformTransition(State.User);
                }
            }
            else fsm.PerformTransition(State.Review);
        }
        IEnumerator X_Feedback()
        {
            yield return null;
        }
        IEnumerator E_Review()
        {
            progressSLD.DOValue(pNO / (float)pCount, 0.5f);
            yield return new WaitForSeconds(0.5f);

            yield return pPanel.Review();
            yield return new WaitForSeconds(reviewDelay);

            yield return new WaitForEndOfFrame();

            var sprite = takeScreenShot();
            EBookQuizProgress.One.ScreenShot(pNO, sprite);
            yield return null;

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_Review()
        {
            yield return null;
        }
        IEnumerator E_Next()
        {
            yield return null;

            pNO++;
            if (pNO <= pCount)
                fsm.PerformTransition(State.Problem);
            else fsm.PerformTransition(State.Outro);
        }
        IEnumerator E_Outro()
        {
            timer.StopTimer();
            yield return null;

            fsm.PerformTransition(State.Reward);
        }
        IEnumerator X_Outro()
        {
            yield return null;
        }
        IEnumerator E_Reward()
        {
            backButtonGO.SetActive(false);
            yield return null;

            complete();
            yield return null;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private LifeGauge lifeGauge = null;
        [SerializeField] private Timer timer = null;
        [SerializeField] private Slider progressSLD = null;
        [SerializeField] private EBookQuizPanel[] quizPanels = null;
        [SerializeField] private GameObject backButtonGO = null;
        [Header("★ Config")]
        [SerializeField] private float heartDecDelay = 0.5f;
        [SerializeField] private float showAnswerDelay = 0.5f;
        [SerializeField] private float reviewDelay = 1;
        [SerializeField] private float timerDuration = 1200;
        [SerializeField] private float affTimeout = 10f;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            foreach (var p in quizPanels)
            {
                p.gameObject.SetActive(false);
                panels[p.QuizType] = p;
            }

            //prevBTN.onClick.AddListener(prevBTN_OnClick);
            //nextBTN.onClick.AddListener(nextBTN_OnClick);

            initFSM();
        }
        protected override void Start()
        {
            base.Start();
        }
        protected override void OnEnable()
        {
            base.OnEnable();

            timer.OnTimeOver += timer_OnTimeOver;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            timer.OnTimeOver -= timer_OnTimeOver;
        }
    }
}