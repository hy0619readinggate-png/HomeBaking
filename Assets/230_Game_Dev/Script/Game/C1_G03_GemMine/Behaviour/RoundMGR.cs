using beyondi.Coroutine;
using beyondi.FSM;
using DoDoEng.Common;
using DoDoEng.Game.Framework;
using DoDoEng.Game.UI;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Game.C1_G03
{
    public class RoundMGR : MonoBehaviour, ICompletable
    {
        // Definitions
        private enum State
        {
            Problem,
            Build, Move, Mine,
            Correct, Wrong, Next,
            Complete, TimeOut,
            Fin
        }

        // Properties
        public Vector2 EntrancePosition => map.EntrancePosition;
        public bool IsCleared { get; private set; }

        // Methods
        public async void Setup(RoundData roundData)
        {
            LOG.Function(this, $"{roundData}");

            problems = roundData.Problems;
            pNO = 1;
            pCount = problems.Length;

            jack.Hide();
            await map.Setup(roundData);

            jack.SetInitialPosition(map.EntranceCell);
            jack.MoveSpeed = roundData.JackSpeed;

            UIGameCommon.One.Progress.Setup(roundData.ProblemCount);
        }
        public void StartRound()
        {
            LOG.Function(this);

            IsCleared = false;

            AffordanceMGR.One.StopMonitor(); // 메인 FSM에서 시작되었던 모니터링을 중지
            fsm.StartFSM(State.Problem);
        }
        public void StopRound()
        {
            LOG.Function(this);

            fsm.StopFSM();
            CameraControl.One.EnablePan(false);
        }
        public void Ending()
        {
            LOG.Function(this);

            var lastMoveDir = Cell.DirectionOf(pathResult.LastBeforeCellOfPath, pathResult.LastCellOfPath);
            jack.RideAndHappy(lastMoveDir);
        }



        // Methods
        public string DEV_GetCurrentWord()
        {
            if (!fsm.IsStarted) return null;
            if (pNO < 1) return null;
            if (pNO > problems.Length) return null;

            return pProblem.Word;
        }



        // Fields
        private FSM<State> fsm = null;
        private ProblemData[] problems = null;
        private int pNO = 0;
        private int pCount = 0;
        private PathResult pathResult = null;

        // Functions : problem
        private ProblemData pProblem => problems[pNO - 1];

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Problem,     E_Problem,      X_Problem);
            fsm.AddState(State.Build,       E_Build,        X_Build);
            fsm.AddState(State.Move,        E_Move,         X_Move);
            fsm.AddState(State.Correct,     E_Correct,      X_Correct);
            fsm.AddState(State.Wrong,       E_Wrong,        X_Wrong);
            fsm.AddState(State.Next,        E_Next);
            fsm.AddState(State.Complete,    E_Complete,     X_Complete);
            fsm.AddState(State.TimeOut,     E_TimeOut,      X_TimeOut);
            fsm.AddState(State.Fin,         E_Fin,          X_Fin);
            #pragma warning restore format
        }

        // Event Handlers 
        private void timer_OnTimeOut()
        {
            LOG.Function(this);

            fsm.PerformTransition(State.TimeOut);
        }
        private void toolSet_OnListenClick()
        {
            LOG.Function(this);

            AudioMGR.One.PlayNarration(pProblem.WordCLIP);
        }
        private void toolSet_OnUndoClick()
        {
            LOG.Function(this);

            map.Undo();
        }



        // FSM
        IEnumerator E_Problem()
        {
            UIGameCommon.One.TimerWithAnimator.ResumeTimer();
            toolSet.Show();
            yield return new WaitForSeconds(1f);

            yield return AudioMGR.One.PlayNarrationAndWait(pProblem.WordCLIP);
            yield return null;

            fsm.PerformTransition(State.Build);
        }
        IEnumerator X_Problem()
        {
            AudioMGR.One.StopNarration();
            yield return null;
        }
        IEnumerator E_Build()
        {
            AffordanceMGR.One.StartMonitor(10);
            CameraControl.One.EnablePan(true);
            yield return null;

            yield return new WaitForComplete(this, map);
            toolSet.EnableInteraction(false);
            CameraControl.One.EnablePan(false);
            yield return new WaitForSeconds(1f);

            pathResult = map.FindPathToGem();

            fsm.PerformTransition(State.Move);
        }
        IEnumerator X_Build()
        {
            UIGameCommon.One.TimerWithAnimator.PauseTimer();
            AffordanceMGR.One.StopMonitor();
            CameraControl.One.EnablePan(false);
            toolSet.Hide();
            yield return null;
        }
        IEnumerator E_Move()
        {
            CameraControl.One.FollowPlayer();
            yield return jack.StartMove(pathResult.PathCell);
            yield return new WaitForSeconds(0.1f);

            var correct = pathResult.Gem.Word == pProblem.Word;
            fsm.PerformTransition(correct
                ? State.Correct
                : State.Wrong);
        }
        IEnumerator X_Move()
        {
            CameraControl.One.UnfollowPlayer();
            jack.FinishMove(pathResult.LastCellOfPath);
            yield return null;
        }
        IEnumerator E_Correct()
        {
            var lastMoveDir = Cell.DirectionOf(pathResult.LastBeforeCellOfPath, pathResult.LastCellOfPath);
            jack.GetOffForMine(lastMoveDir);
            yield return new WaitForSeconds(jackGetOffDelay);

            var gem = pathResult.Gem;
            var dir = map.FindMineDirection(pathResult.GemCell);
            yield return gem.StartMine(dir, true);

            UIGameCommon.One.Progress.Increase();
            UIGameCommon.One.StarGauge.Success();
            GameProgress.One.Correct();

            jack.Ride(lastMoveDir);
            yield return new WaitForSeconds(0.5f);

            Map.One.ClearRoads();
            yield return new WaitForSeconds(1f);

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_Correct()
        {
            yield return null;
        }
        IEnumerator E_Wrong()
        {
            var lastMoveDir = Cell.DirectionOf(pathResult.LastBeforeCellOfPath, pathResult.LastCellOfPath);
            jack.GetOffForMine(lastMoveDir);
            yield return new WaitForSeconds(jackGetOffDelay);

            var gem = pathResult.Gem;
            var dir = map.FindMineDirection(pathResult.GemCell);
            yield return gem.StartMine(dir, false);

            jack.Ride(lastMoveDir);
            yield return new WaitForSeconds(0.5f);

            Map.One.ClearRoads();
            yield return null;

            fsm.PerformTransition(State.Problem);
        }
        IEnumerator X_Wrong()
        {
            yield return null;
        }
        IEnumerator E_Next()
        {
            map.ResetGems();
            yield return null;

            if (++pNO <= pCount)
                fsm.PerformTransition(State.Problem);
            else fsm.PerformTransition(State.Complete);
        }
        IEnumerator E_Complete()
        {
            IsCleared = true;
            yield return new WaitForSeconds(completeDelay);

            fsm.PerformTransition(State.Fin);
        }
        IEnumerator X_Complete()
        {
            yield return null;
        }
        IEnumerator E_TimeOut()
        {
            Map.One.ClearRoads();

            IsCleared = false;

            var lastMoveDir = Direction.B;
            if (pathResult != null)
                lastMoveDir = Cell.DirectionOf(pathResult.LastBeforeCellOfPath, pathResult.LastCellOfPath);
            jack.TimeOut(lastMoveDir);
            yield return new WaitForSeconds(timeOutDelay);

            fsm.PerformTransition(State.Fin);
        }
        IEnumerator X_TimeOut()
        {
            yield return null;
        }
        IEnumerator E_Fin()
        {
            yield return null;
        }
        IEnumerator X_Fin()
        {
            yield return null;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Map map = null;
        [SerializeField] private Jack jack = null;
        [SerializeField] private ToolSet toolSet = null;
        [Header("★ Sound")]
        [SerializeField] private float completeDelay = 0.3f;
        [SerializeField] private float timeOutDelay = 1;
        [SerializeField] private float jackGetOffDelay = 0.5f;

        // Unity Messages
        private void Awake()
        {
            initFSM();
        }
        private void Start()
        {

        }
        private void OnEnable()
        {
            UIGameCommon.One.TimerWithAnimator.OnTimeOut += timer_OnTimeOut;
            toolSet.OnListenClick += toolSet_OnListenClick;
            toolSet.OnUndoClick += toolSet_OnUndoClick;
        }
        private void OnDisable()
        {
            if (UIGameCommon.One != null)
                UIGameCommon.One.TimerWithAnimator.OnTimeOut -= timer_OnTimeOut;

            toolSet.OnListenClick -= toolSet_OnListenClick;
            toolSet.OnUndoClick -= toolSet_OnUndoClick;
        }



        // Interface : ICompletable
        bool ICompletable.IsComplete => fsm.IsStarted && fsm.CurrentState == State.Fin;
    }
}