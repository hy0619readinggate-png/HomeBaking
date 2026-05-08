using beyondi.FSM;
using beyondi.Util;
using DoDoEng.Activity.Framework;
using DoDoEng.Activity.UI;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C1_A10
{
    public class Problem : AffBase
    {
        // Definitions
        private enum State { In, Problem, Solve, Correct, Wrong, Fin }

        // Methods
        public void Init(CameraMGR cameraMGR)
        {
            LOG.Info($"Init()", this);

            this.cameraMGR = cameraMGR;
        }
        public void Setup(ProblemData pData, Friend friend, Friend[] friends)
        {
            LOG.Info($"Setup() | {pData}", this);

            this.ProblemCLIP = pData.ProblemCLIP;
            this.textCLIP = pData.TextCLIP;

            this.friend = friend;
            this.friends = friends;

            Dodo.One.TeleportTo(waitTR.position);

            cameraMGR.MoveTo(cameraTR.position);

            examples.ForEach((i, e) => e.Setup(pData.Examples[i]));
        }
        public Coroutine StartProblem()
        {
            LOG.Info($"StartProblem()", this);

            isComplete = false;
            fsm.StartFSM(State.In);

            crWaitComplete = StartCoroutine(coWaitComplete());
            return crWaitComplete;
        }
        public void FinishProblem()
        {
            LOG.Info($"FinishProblem()", this);

            this.StopCoroutineSafe(ref crWaitComplete);

            UIActivityCommon.One.EnableSpeakerButton = false;

            Dodo.One.ChangeSpeed(false);
            friends.ForEach(f => f.ChangeSpeed(false));

            activateAff = false;
            affTargetGO.SetActive(false);

            fsm.StopFSM();

            this.StopCoroutineSafe(ref crMoveAndCorrect);
        }



        // Fields : caching
        private Example[] examples_ = null;
        private Example[] examples => examples_ ??= GetComponentsInChildren<Example>(true);

        // Fields
        private FSM<State> fsm = null;
        private bool isComplete = false;
        private Friend friend = null;
        private Friend[] friends = null;
        private AudioClip ProblemCLIP = null;
        private AudioClip textCLIP = null;
        private Example submitExample = null;
        private Coroutine crWaitComplete = null;
        private CameraMGR cameraMGR = null;
        private bool activateAff = false;
        private Coroutine crMoveAndCorrect = null;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.In,          E_In,           X_In);
            fsm.AddState(State.Problem,     E_Problem,      X_Problem);
            fsm.AddState(State.Solve,       E_Solve,        X_Solve);
            fsm.AddState(State.Correct,     E_Correct,      X_Correct);
            fsm.AddState(State.Wrong,       E_Wrong,        X_Wrong);
            fsm.AddState(State.Fin,         E_Fin);
            #pragma warning restore format
        }

        // Overrides
        protected override IEnumerator onStartAff()
        {
            if (activateAff)
            {
                var answerExample = examples.First(e => e.IsAnswer);

                var targetPosition = answerExample.transform.position + affDistance;
                affTargetGO.transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
                affTargetGO.SetActive(true);
            }
            yield return null;
        }
        protected override IEnumerator onFinishAff()
        {
            affTargetGO.SetActive(false);
            yield return null;
        }



        // FSM
        protected IEnumerator E_In()
        {
            Dodo.One.ChangeSpeed(true);
            friends.ForEach(f => f.ChangeSpeed(true));
            yield return null;

            var enterPos = enterTR[Dodo.One.FriendCount].position;
            yield return Dodo.One.MoveAndWait(enterPos);
            yield return new WaitForSeconds(0.5f);

            fsm.PerformTransition(State.Problem);
        }
        protected IEnumerator X_In()
        {
            Dodo.One.ChangeSpeed(false);
            friends.ForEach(f => f.ChangeSpeed(false));
            yield return null;
        }
        protected IEnumerator E_Problem()
        {
            yield return AudioMGR.One.PlayNarrationAndWait(ProblemCLIP);
            yield return null;

            fsm.PerformTransition(State.Solve);
        }
        protected IEnumerator X_Problem()
        {
            AudioMGR.One.StopNarration();
            yield return null;
        }
        protected IEnumerator E_Solve()
        {
            UIActivityCommon.One.EnableSpeakerButton = true;
            activateAff = true;
            yield return null;

            submitExample = null;
            yield return new WaitUntil(() => submitExample != null);
            yield return null;

            if (submitExample.IsAnswer)
                fsm.PerformTransition(State.Correct);
            else fsm.PerformTransition(State.Wrong);
        }
        protected IEnumerator X_Solve()
        {
            UIActivityCommon.One.EnableSpeakerButton = false;
            activateAff = false;
            affTargetGO.SetActive(false);
            yield return null;
        }
        protected IEnumerator E_Correct()
        {
            // 도도 이동 기록 비활성화
            Dodo.One.EnableRecordPath(false);

            // 도도 문 옆으로 이동
            var doorPos = doorTR[submitExample.ID - 1].position;
            yield return Dodo.One.MoveAndWait(doorPos);

            // 도도 움직임 기능 정지
            Dodo.One.EnableMotion(false);
            //Dodo.One.StopMoving();

            // 도도 문 바라보기
            Dodo.One.Idle();
            Dodo.One.Direction(false);

            // 친구들 선택한 문 바라보기
            var position = submitExample.transform.position;
            friends
                .Where(f => f != friend)
                .ForEach(f =>
                {
                    f.JustLook();
                    f.Direction(submitExample.ID > 1);
                });
            yield return null;

            AudioMGR.One.PlayEffect(exampleCLIP);
            yield return new WaitForSeconds(0.7f);

            // 문 정답 처리
            AudioMGR.One.PlayEffect(correctCLIP);
            var friendPos = friendsTR[submitExample.ID - 1].position;
            friend.TeleportTo(friendPos);
            submitExample.Correct();
            yield return new WaitForSeconds(0.2f);

            examples.Where(e => !e.IsAnswer).ForEach(e => e.HideText());
            yield return null;

            // 친구 등장
            ActivityUI.One.GetCharacter(friend.CharacterID);
            AudioMGR.One.PlayEffect(SfxMoment.Activity_Correct);
            friend.Appear();
            yield return new WaitForSeconds(appearDuration);

            // 도도와 친구들 정답 모션
            Dodo.One.Correct();
            friends.Where(f => f.IsFollowing).ForEach(f => f.Correct());
            friend.Correct();

            // 단어 읽어주기
            yield return AudioMGR.One.PlayNarrationAndWait(ProblemCLIP);
            if (textCLIP != null)
            {
                yield return AudioMGR.One.PlayNarrationAndWait(ProblemCLIP);
                yield return new WaitForSeconds(0.5f);
            }
            else yield return new WaitForSeconds(2.5f);
            yield return new WaitForSeconds(0.5f);

            // 도도는 원위치로, 친구는 도도뒤에 줄서기 위해  이동, 기존 친구 아이들
            friends
                .Where(f => f != friend)
                .ForEach(f =>
                {
                    f.Idle();
                    f.Direction(true);
                });

            Dodo.One.EnableMotion(true);
            var enterPos = enterTR[Dodo.One.FriendCount].position;
            //var newfriendPos = enterPos - new Vector3(Dodo.One.Distance, 0, 0);
            var pos = Dodo.One.GetNextFriendPosition();
            friend.MoveAndWait(pos, true);
            yield return Dodo.One.MoveAndWait(enterPos, true);
            yield return new WaitUntil(() => !friend.IsMoving);
            yield return null;

            // 합류
            Dodo.One.AddFriend(friend);
            Dodo.One.Join();

            // 출발 모션
            //Dodo.One.Gogo();
            //friends.ForEach(f => f.Gogo());
            //yield return new WaitForSeconds(1f);

            // 도도 이동 기록 활성화
            Dodo.One.EnableRecordPath(true);
            Dodo.One.ChangeSpeed(true);
            friends.ForEach(f => f.ChangeSpeed(true));
            yield return Dodo.One.MoveAndWait(exitTR.position);

            fsm.PerformTransition(State.Fin);
        }
        protected IEnumerator X_Correct()
        {
            this.StopCoroutineSafe(ref crMoveAndCorrect);
            Dodo.One.ChangeSpeed(false);
            //friends.ForEach(f => f.SetEnableFollowing(true));
            friends.ForEach(f => f.ChangeSpeed(false));
            yield return null;
        }
        protected IEnumerator E_Wrong()
        {
            // 오답 기록
            ActivityProgress.One.Wrong();

            // 도도 이동 기록 비활성화
            Dodo.One.EnableRecordPath(false);

            // 도도 문 옆으로 이동
            var doorPos = doorTR[submitExample.ID - 1].position;
            yield return Dodo.One.MoveAndWait(doorPos);

            // 도도 움직임 기능 정지
            Dodo.One.EnableMotion(false);

            // 도도 문 바라보기
            Dodo.One.Idle();
            Dodo.One.Direction(false);

            // 친구들 선택한 문 바라보기
            var position = submitExample.transform.position;
            friends
                .Where(f => f != friend)
                .ForEach(f =>
                {
                    f.JustLook();
                    f.Direction(submitExample.ID > 1);
                });
            yield return null;

            AudioMGR.One.PlayEffect(exampleCLIP);
            yield return new WaitForSeconds(0.7f);

            // 문 오답 처리
            AudioMGR.One.PlayEffect(wrongCLIP);
            submitExample.Wrong();
            yield return new WaitForSeconds(0.7f);

            // 도도와 친구들 오답연출
            friends.ForEach(f => f.Wrong());
            Dodo.One.Wrong();
            yield return new WaitForSeconds(wrongDuration);

            // 문닫히며, 도도 원위치로 이동
            AudioMGR.One.PlayEffect(closeCLIP);
            submitExample.Close();
            friends
                .ForEach(f =>
                {
                    f.Idle();
                    f.Direction(true);
                });

            Dodo.One.EnableMotion(true);
            var enterPos = enterTR[Dodo.One.FriendCount].position;
            yield return Dodo.One.MoveAndWait(enterPos, true);
            yield return new WaitForSeconds(1f);

            // 도도 이동 기록 활성화
            Dodo.One.EnableRecordPath(true);

            fsm.PerformTransition(State.Problem);
        }
        protected IEnumerator X_Wrong()
        {
            yield return null;
        }
        protected IEnumerator E_Fin()
        {
            isComplete = true;
            yield return null;
        }


        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Transform[] enterTR = null;
        [SerializeField] private Transform[] doorTR = null;
        [SerializeField] private Transform waitTR = null;
        [SerializeField] private Transform exitTR = null;
        [SerializeField] private Transform cameraTR = null;
        [SerializeField] private Transform[] friendsTR = null;
        [Header("★ Config")]
        [SerializeField] private float appearDuration = 1f;
        [SerializeField] private float wrongDuration = 4f;
        [SerializeField] private Vector3 affDistance;
        [Header("★ Sound")]
        [SerializeField] private AudioClip exampleCLIP = null;
        [SerializeField] private AudioClip closeCLIP = null;
        [SerializeField] private AudioClip correctCLIP = null;
        [SerializeField] private AudioClip wrongCLIP = null;



        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            activateAff = false;
            affTargetGO.SetActive(false);

            initFSM();
            examples.AutoFillID();
        }
        private void Start()
        {
        }
        private void Update()
        {
            if (fsm.CurrentState == State.Solve && submitExample == null && Input.GetMouseButton(0))
            {
                var moustPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var ray = Camera.main.ScreenPointToRay(moustPosition);

                var raycastHit2D = Physics2D.Raycast(moustPosition, Vector3.forward);
                var example = raycastHit2D.transform?.GetComponent<Example>();
                if (example != null)
                    submitExample = example;
            }
        }

        // Unity Coroutine
        IEnumerator coWaitComplete()
        {
            using (LOG.Coroutine($"coWaitComplete()", this))
            {
                yield return new WaitUntil(() => isComplete);
            }
        }
    }
}