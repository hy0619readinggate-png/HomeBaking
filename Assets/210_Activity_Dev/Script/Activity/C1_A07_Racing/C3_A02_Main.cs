using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using FlexFramework.Excel;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

using ActivityData = DoDoEng.Common.ActivityData_C3_A02;
using ProblemMGR = DoDoEng.Activity.C1_A07.C3_A02_ProblemMGR;


// Variation : C1_A07_Racing, C3_A02_CosmosRacing
namespace DoDoEng.Activity.C1_A07
{
    public class C3_A02_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C3_A02_CosmoRacing;
        private enum State
        {
            Intro, Start,
            Obstacle,
            Problem, Solve, Correct, SpeedDown, Wrong, Next,
            GoalIn, Outro, Reward
        }

        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();

        // Fields
        private FSM<State> fsm = null;
        private Coroutine crObstacleCollision = null;
        private Example userExample = null;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Intro,       E_Intro,        X_Intro);
            fsm.AddState(State.Start,       E_Start,        X_Start);
            fsm.AddState(State.Obstacle,    E_Obstacle,     X_Obstacle);
            fsm.AddState(State.Problem,     E_Problem,      X_Problem);
            fsm.AddState(State.Solve,       E_Solve,        X_Solve);
            fsm.AddState(State.Correct,     E_Correct,      X_Correct);
            fsm.AddState(State.SpeedDown,   E_SpeedDown,    X_SpeedDown);
            fsm.AddState(State.Wrong,       E_Wrong,        X_Wrong);
            fsm.AddState(State.Next,        E_Next);
            fsm.AddState(State.GoalIn,      E_GoalIn,       X_GoalIn);
            fsm.AddState(State.Outro,       E_Outro,        X_Outro);
            fsm.AddState(State.Reward,      E_Reward);
            #pragma warning restore format  
        }
        private void setupProblem(ProblemData pData)
        {
            board.Setup(pData);
            var exampleIndices = UtilArray.Shuffled(pData.Examples);
            examples.ForEach((i, exam) => exam.Setup(exampleIndices[i]));
        }

        // Event Handlers
        private void edmond_OnObstacleCollision(Obstacle obstacle)
        {
            LOG.Info($"edmond_OnObstacleCollision() ", this);

            this.StopCoroutineSafe(ref crObstacleCollision);
            crObstacleCollision = StartCoroutine(coObstacleCollision(obstacle));
        }
        private void edmond_OnExampleCollision(Example example)
        {
            LOG.Info($"edmond_OnExampleCollision() ", this);

            if (fsm.CurrentState == State.Solve)
            {
                userExample = example;
                fsm.PerformTransition(userExample.IsAnswer ? State.Correct : State.Wrong);
            }
        }

        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            evaluateTimeline(introTL);

            stepPanelCG.SetActiveOnly(0);

            Track.One.Init(edmond, examples);
            edmond.Init(2);
            var rivalIDs = UtilArray.Random(1, rivals.Length);
            var laneNos = new int[] { 1, 3 };
            rivals.AutoFillID();
            rivals.ForEach((i, r) => r.Init(rivalIDs[i], laneNos[i]));

            foreach (var (r, i) in s2Rivals.Select((r, i) => (r, i)))
            {
                var rivalAnis = r.transform.GetChildren().ToArray();
                rivalAnis.SetActiveOnly(rivalIDs[i] - 1);
            }
            vfxGoalInGO.SetActive(false);
        }
        protected override async UniTask onPrepare(ActivityIndex actIDX)
        {
            await base.onPrepare(actIDX);

            // Load Unit Data
            await pMGR.Build(actIDX, CURRICULUM, TABLES);
            ActivityProgress.One.Setup(pMGR.BlanksCount);
        }
        protected override void onStartActivity()
        {
            base.onStartActivity();

            playBGM();
            AudioMGR.One.PlayAmbient(ambientCLIP, ambientVolume);
            fsm.StartFSM(State.Intro, RunnerParam.SkipStateTo);
        }
        protected override void onFinishActivity()
        {
            base.onFinishActivity();

            if (fsm.CurrentState == State.Obstacle
                || fsm.CurrentState == State.Problem
                || fsm.CurrentState == State.Solve)
                CP(CheckPoint.UserFinish);

            stopBGM();
            AudioMGR.One.StopAmbient(true);
            fsm?.StopFSM();
        }
        protected override void onDebugNext()
        {
            LOG.Info($"onDebugNext() | {fsm.CurrentState}", this);
            switch (fsm.CurrentState)
            {
                case State.Intro: fsm.PerformTransition(State.Start); break;
                case State.Start: fsm.PerformTransition(State.Obstacle); break;
                case State.Obstacle: fsm.PerformTransition(State.Problem); break;
                case State.Problem: fsm.PerformTransition(State.Solve); break;
                case State.Solve:
                    userExample = examples.FirstOrDefault(e => e.IsAnswer);
                    fsm.PerformTransition(State.Correct);
                    break;
                case State.Correct: fsm.PerformTransition(State.Next); break;
                case State.Wrong: fsm.PerformTransition(State.Next); break;
                case State.GoalIn: fsm.PerformTransition(State.Outro); break;
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
            AudioMGR.One.PlayEffect(startCLIP);
            yield return playTimeline(introTL);

            fsm.PerformTransition(State.Start);
        }
        IEnumerator X_Intro()
        {
            yield return stopTimeline(introTL);
            yield return null;
        }
        IEnumerator E_Start()
        {
            Track.One.StartRivalGoAhead();
            edmond.TurnOn();
            rivals.ForEach((i, r) => r.StartGoAhead());
            yield return new WaitForSeconds(0.3f);

            yield return new WaitForSeconds(goAheadDuration);

            fsm.PerformTransition(State.Obstacle);
        }
        IEnumerator X_Start()
        {
            rivals.ForEach(r => r.FinishGoAhead());
            yield return null;
        }
        IEnumerator E_Obstacle()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            if (pMGR.PNO == 1)
                AffordanceMGR.One.StartAffNow();
            affordance.Run();
            playerController.EnableInteraction(true);
            var rival = rivals[rivals.Length - (pMGR.PNO - 1) / 3 - 1];
            obstacleRival.Setup(rival.CharacterID, obstacleCount, 2);
            obstacleRival.StartObstruct();
            yield return Track.One.StartObstruct(obstacleCount);
            yield return null;

            fsm.PerformTransition(State.Problem);
        }
        IEnumerator X_Obstacle()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            playerController.EnableInteraction(false);
            obstacleRival.FinishObstruct();
            Track.One.FinishObstruct();
            this.StopCoroutineSafe(ref crObstacleCollision);
            yield return null;
        }
        IEnumerator E_Problem()
        {
            setupProblem(pMGR.Current);
            playerController.EnableInteraction(true);
            edmond.Move();
            Track.One.SpeedNormalNow();
            yield return null;

            if (!board.IsShow)
            {
                board.Show();
                yield return new WaitForSeconds(boardAppearDuration);
            }

            AudioMGR.One.PlayEffect(exampleAppearCLIP);
            yield return null;

            examples.ForEach(e => e.Move());
            yield return new WaitForSeconds(exampleAppearDuration);

            yield return board.PlayWordSound();
            yield return null;

            fsm.PerformTransition(State.Solve);
        }
        IEnumerator X_Problem()
        {
            playerController.EnableInteraction(false);
            examples.ForEach(e => e.Move());
            AudioMGR.One.StopNarration();
            yield return null;
        }
        IEnumerator E_Solve()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            AffordanceMGR.One.StartAffNow();
            playerController.EnableInteraction(true);
            board.EnableInteraction(true);
            yield return null;

            // Wait for edmond_OnExampleCollision
            examples.ForEach(exam => exam.EnableColliderable(true));
            yield return null;
        }
        IEnumerator X_Solve()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            board.StopWordSound();
            board.EnableInteraction(false);
            yield return null;

            playerController.EnableInteraction(false);
            examples.ForEach(exam => exam.EnableColliderable(false));
            yield return null;
        }
        IEnumerator E_Correct()
        {
            AudioMGR.One.PlayEffectLL(correctCLIP, true);
            yield return null;

            userExample.Correct();

            //edmond.Correct();
            yield return null;

            // 속도 증가
            edmond.MoveToCorrectPosition(edmondCorrectSpeed);
            yield return Track.One.SpeedUpByBoost();

            yield return new WaitForSeconds(3);
            AudioMGR.One.PlayNarration(pMGR.Current.WordClip);

            // 속도 유지
            if (pMGR.PNO % 3 != 0)
                yield return new WaitForSeconds(correctDuration);
            else
            {
                board.Hide();
                yield return obstacleRival.StartOverTaken();
            }

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_Correct()
        {
            AudioMGR.One.StopEffectLL(true);
            yield return null;
        }
        IEnumerator E_Next()
        {
            yield return null;

            if (pMGR.Next())
                fsm.PerformTransition(State.SpeedDown);
            else fsm.PerformTransition(State.GoalIn);
        }
        IEnumerator E_SpeedDown()
        {
            // 속도 감속
            AudioMGR.One.StopEffectLL(true);
            yield return null;

            edmond.Return(edmondCorrectSpeed);
            yield return Track.One.SpeedDnByBoost();

            edmond.Move();
            yield return new WaitForSeconds(1);

            if (pMGR.PNO % 3 == 1)  // 셋트 완료
                fsm.PerformTransition(State.Obstacle);
            else fsm.PerformTransition(State.Problem);
        }
        IEnumerator X_SpeedDown()
        {
            edmond.FinishCorrect();
            Track.One.SpeedNormalNow();
            obstacleRival.FinishOverTaken();
            yield return null;
        }
        IEnumerator E_Wrong()
        {
            ActivityProgress.One.Wrong();

            AudioMGR.One.PlayEffect(wrongCLIP);
            yield return null;

            examples.ForEach(ex => ex.Wrong());
            edmond.Wrong();
            yield return null;

            // 속도 감속
            yield return Track.One.SpeedDnByWrongExample();

            // 속도 유지
            yield return new WaitForSeconds(wrongDuration);

            playerController.EnableInteraction(true);

            // 속도 증가
            AudioMGR.One.PlayEffect(restartClip);
            edmond.Move();
            yield return Track.One.SpeedUpByWrongExample();

            yield return new WaitForSeconds(wrongPostDelay);

            fsm.PerformTransition(State.Problem);
        }
        IEnumerator X_Wrong()
        {
            playerController.EnableInteraction(false);
            edmond.Move();
            Track.One.SpeedNormalNow();
            yield return null;
        }
        IEnumerator E_GoalIn()
        {
            yield return new WaitForSeconds(goalInPreDelay);
            yield return null;

            goalInAnim.speed = Track.One.BoostSpeedRatio;
            goalInAnim.SetTrigger("GoalIn");
            yield return new WaitForSeconds(goalInDuration);

            Track.One.SpeedStopNow();
            goalInAnim.speed = 0f;
            yield return null;

            AudioMGR.One.PlayEffect(edmondGoalInCLIP);

            edmond.StartGoalIn(edmondGoalInSpeed);
            yield return null;

            AudioMGR.One.PlayEffect(goalInCLIP);
            DOVirtual.DelayedCall(0.4f, () => vfxGoalInGO.SetActive(true));
            yield return new WaitForSeconds(0.3f);

            var emptyLanes = Track.One.GetEmptyLanesNo();
            yield return null;

            foreach (var (r, i) in rivals.Select((r, i) => (r, i)))
            {
                r.StartGoalIn(emptyLanes[i]);
                yield return new WaitForSeconds(rivalGoalInInterval);
            }
            yield return new WaitForSeconds(rivalGoalInWaitDuration);

            goalInFadeOutAnim.SetTrigger("Show");
            yield return new WaitForSeconds(1f);

            fsm.PerformTransition(State.Outro);
        }
        IEnumerator X_GoalIn()
        {
            edmond.FinishGoalIn();
            rivals.ForEach(r => r.FinishGoalIn());
            yield return null;

            Track.One.SpeedStopNow();
            goalInAnim.speed = 0f;
            yield return null;
        }
        IEnumerator E_Outro()
        {
            CP(CheckPoint.Outro);
            yield return null;

            yield return playTimeline(outroTL);
            yield return null;

            fsm.PerformTransition(State.Reward);
        }
        IEnumerator X_Outro()
        {
            yield return stopTimeline(outroTL);
            yield return null;
        }
        IEnumerator E_Reward()
        {
            CP(CheckPoint.Complete);
            yield return null;

            AudioMGR.One.PlayEffect(SfxMoment.Activity_Complete);
            yield return new WaitForSeconds(rewardDelay);

            complete();
            yield return null;
        }



        // Unity Inspectors
        [Header("★ Bindings - Panels")]
        [SerializeField] private CanvasGroup[] stepPanelCG = null;
        [Header("★ Bindings - S1.Race")]
        [SerializeField] private Edmond edmond = null;
        [SerializeField] private Rival[] rivals = null;
        [SerializeField] private Rival obstacleRival = null;
        [SerializeField] private PlayerController playerController = null;
        [SerializeField] private Board board = null;
        [SerializeField] private Example[] examples = null;
        [SerializeField] private Affordance affordance = null;
        [Header("★ Bindings - S2.GoalIn")]
        [SerializeField] private GameObject vfxGoalInGO = null;
        [SerializeField] private Animator goalInAnim = null;
        [SerializeField] private Animator goalInFadeOutAnim = null;
        [SerializeField] private GameObject[] s2Rivals = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip startCLIP = null;
        [SerializeField] private AudioClip obstructCollisionCLIP = null;
        [SerializeField] private AudioClip obstructStopCLIP = null;
        [SerializeField] private AudioClip exampleAppearCLIP = null;
        [SerializeField] private AudioClip correctCLIP = null;
        [SerializeField] private AudioClip wrongCLIP = null;
        [SerializeField] private AudioClip restartClip = null;
        [SerializeField] private AudioClip edmondGoalInCLIP = null;
        [SerializeField] private AudioClip goalInCLIP = null;
        [Header("★ Sound - Ambient")]
        [SerializeField] private AudioClip ambientCLIP = null;
        [Range(0, 100)]
        [SerializeField] int ambientVolume = 100;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector introTL = null;
        [SerializeField] private PlayableDirector outroTL = null;
        [Header("★ Config - GoAhead")]
        [SerializeField] private float goAheadDuration = 3f;
        [Header("★ Config - Obstacle")]
        [SerializeField] private int obstacleCount = 5;
        [SerializeField] private float obstacleCollisionDelay = 2f;
        [Header("★ Config - Problem")]
        [SerializeField] private float boardAppearDuration = 1f;
        [SerializeField] private float exampleAppearDuration = 1.5f;
        [SerializeField] private float correctDuration = 1.8f;
        [SerializeField] private float wrongDuration = 1f;
        [SerializeField] private float wrongPostDelay = 1f;
        [Header("★ Config - GoalIn")]
        [SerializeField] private float edmondCorrectSpeed = 20;
        [SerializeField] private float edmondGoalInSpeed = 20;
        [SerializeField] private float rivalGoalInInterval = 0.3f;
        [SerializeField] private float rivalGoalInWaitDuration = 3f;
        [SerializeField] private float goalInPreDelay = 1f;
        [SerializeField] private float goalInDuration = 1.3f;
        [Header("★ Timing")]
        [SerializeField] private float rewardDelay = 1f;

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

            edmond.OnObstacleCollision += edmond_OnObstacleCollision;
            edmond.OnExampleCollision += edmond_OnExampleCollision;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            edmond.OnObstacleCollision -= edmond_OnObstacleCollision;
            edmond.OnExampleCollision -= edmond_OnExampleCollision;
        }

        // Unity Coroutine
        IEnumerator coObstacleCollision(Obstacle obstacle)
        {
            LOG.Coroutine($"coObstacleCollision()", this);
            {
                AudioMGR.One.PlayEffect(obstructCollisionCLIP);
                yield return null;

                playerController.EnableInteraction(false);
                edmond.Obstacle();
                obstacleRival.StopObstruct();
                yield return null;

                // 속도 감속
                AudioMGR.One.PlayEffect(obstructStopCLIP);
                yield return Track.One.SpeedDnByObstacle();

                yield return new WaitForSeconds(obstacleCollisionDelay);

                playerController.EnableInteraction(true);
                edmond.Move();
                yield return null;

                // 속도 증가
                yield return Track.One.SpeedUpByObstacle();

                obstacleRival.StartObstruct();
                yield return null;
            }
        }
    }
}