using beyondi.Coroutine;
using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

using ActivityData = DoDoEng.Common.ActivityData_C4_A02;
using ProblemMGR = DoDoEng.Activity.C3_A06.C4_A02_ProblemMGR;

// Variation : C3_A06_Puzzle, C4_A02_NeonArcade
namespace DoDoEng.Activity.C3_A06
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C4_A02_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C4_A02_NeonArcade;
        private enum State
        {
            Permission,
            Intro,
            ShowPuzzle, Puzzle, ToRecord, Read,
            Wait, Record, Feedback, MyVoice, Next, ToPuzzle,
            Outro, Reward
        }

        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();

        // Fields
        private FSM<State> fsm = null;
        private Coroutine crPlaySound = null;
        private MyButton submitNextBTN = null;
        private int recordTryCount = 0;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Intro, E_Intro);
            fsm.AddState(State.Permission,  E_Permission);
            fsm.AddState(State.ShowPuzzle,  E_ShowPuzzle,   X_ShowPuzzle);
            fsm.AddState(State.Puzzle,      E_Puzzle,       X_Puzzle);
            fsm.AddState(State.ToRecord,    E_ToRecord,     X_ToRecord);
            fsm.AddState(State.Read,        E_Read,         X_Read);
            fsm.AddState(State.Wait,        E_Wait,         X_Wait);
            fsm.AddState(State.Record,      E_Record,       X_Record);
            fsm.AddState(State.MyVoice,     E_MyVoice,      X_MyVoice);
            fsm.AddState(State.Feedback,    E_Feedback,     X_Feedback);
            fsm.AddState(State.Next,        E_Next);
            fsm.AddState(State.ToPuzzle,    E_ToPuzzle,     X_ToPuzzle);
            fsm.AddState(State.Outro,       E_Outro,        X_Outro);
            fsm.AddState(State.Reward,      E_Reward);
            #pragma warning restore format  
        }
        private void setupProblem(ProblemData pData)
        {
            puzzleMGR.Setup(pData);
            recordMGR.Setup(pData);
            recordTryCount = 0;
            nextBTN.Activate(false);
            enableButtons(false);
        }
        private void setupSentence(ProblemData pData)
        {
            sentenceTXT.text = pData.Sentence;
        }

        // Functions
        private void enableButtons(bool enable)
        {
            nextBTN.EnableInteraction(enable);
            recordMGR.EnableInteraction(enable);
            speaker.EnableInteraction(enable);
        }

        // Event Handlers
        private void speaker_OnClick()
        {
            LOG.Info($"speaker_OnClick()", this);

            crPlaySound = StartCoroutine(coPlaySound());
        }



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            recordMGR.gameObject.SetActive(false);
        }
        protected override async UniTask onPrepare(ActivityIndex actIDX)
        {
            await base.onPrepare(actIDX);

            // Load Unit Data
            await pMGR.Build(actIDX, CURRICULUM, TABLES);
            ActivityProgress.One.Setup(pMGR.BlanksCount);

            // setup
            setupProblem(pMGR.Current);
        }
        protected override void onStartActivity()
        {
            base.onStartActivity();

            playBGM();
            AudioMGR.One.PlayAmbient(ambientCLIP, ambientVolume);

            fsm.StartFSM(State.Permission, RunnerParam.SkipStateTo);
        }
        protected override void onFinishActivity()
        {
            base.onFinishActivity();

            if (fsm.CurrentState == State.Puzzle)
                CP(CheckPoint.UserFinish);

            stopBGM();
            AudioMGR.One.StopAmbient(true);

            fsm?.StopFSM();
        }
        protected override void onDebugNext()
        {
            switch (fsm.CurrentState)
            {
                case State.ShowPuzzle: fsm.PerformTransition(State.Puzzle); break;
                case State.Puzzle: fsm.PerformTransition(State.ToRecord); break;
                case State.ToRecord: fsm.PerformTransition(State.Read); break;
                case State.Read: fsm.PerformTransition(State.Wait); break;
                case State.Wait: fsm.PerformTransition(State.Next); break;
                case State.Record: fsm.PerformTransition(State.Feedback); break;
                case State.MyVoice:
                    if (submitNextBTN == recordMGR.RecordBTN)
                        fsm.PerformTransition(State.Feedback);
                    else fsm.PerformTransition(State.Wait);
                    break;
                case State.Feedback:
                    if (recordMGR.IsRecorded)
                        fsm.PerformTransition(State.Wait);
                    else fsm.PerformTransition(State.Read);
                    break;
                case State.ToPuzzle: fsm.PerformTransition(State.Puzzle); break;
                case State.Outro: fsm.PerformTransition(State.Reward); break;
            }
        }
        protected override void onDebugForceFinish()
        {
            base.onDebugForceFinish();

            fsm.PerformTransition(State.Reward);
        }



        // FSM
        IEnumerator E_Permission()
        {
            var micPermitted = false;
            yield return permissionChecker.CheckMicrophonePermissionForced().ToCoroutine(ok => micPermitted = ok);

            if (!micPermitted)
                error();
            else fsm.PerformTransition(State.Intro);
        }
        IEnumerator E_Intro()
        {
            CP(CheckPoint.Start);
            yield return null;

            fsm.PerformTransition(State.ShowPuzzle);
        }
        IEnumerator E_ShowPuzzle()
        {
            yield return puzzleMGR.StartShow();
            yield return null;

            fsm.PerformTransition(State.Puzzle);
        }
        IEnumerator X_ShowPuzzle()
        {
            puzzleMGR.FinishShow();
            yield return null;
        }
        IEnumerator E_Puzzle()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            yield return puzzleMGR.StartPuzzle();
            yield return null;

            fsm.PerformTransition(State.ToRecord);
        }
        IEnumerator X_Puzzle()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            puzzleMGR.FinishPuzzle();
            yield return null;
        }
        IEnumerator E_ToRecord()
        {
            setupSentence(pMGR.Current);
            yield return null;

            yield return playTimeline(puzzleCompleteTL);
            yield return null;

            fsm.PerformTransition(State.Read);
        }
        IEnumerator X_ToRecord()
        {
            yield return stopTimeline(puzzleCompleteTL);
            yield return null;
        }
        IEnumerator E_Read()
        {
            crPlaySound = StartCoroutine(coPlaySound());
            yield return crPlaySound;

            fsm.PerformTransition(State.Wait);
        }
        IEnumerator X_Read()
        {
            this.StopCoroutineSafe(ref crPlaySound);
            AudioMGR.One.StopNarration();
            speaker.SetPlay(false);
            yield return null;
        }
        IEnumerator E_Wait()
        {
            enableButtons(true);
            yield return null;

            if (!nextBTN.IsActive && (recordMGR.IsRecorded || recordTryCount >= tryCountForForceNext))
            {
                nextBTN.Activate(true);
                yield return playTimeline(showNextButtonTL);
            }


            var waits = new ISubmitable[] { recordMGR.RecordBTN, recordMGR.MyVoiceBTN, nextBTN };
            var wait = new WaitForSubmit(this, waits);
            yield return wait;

            submitNextBTN = wait.Submited as MyButton;
            if (submitNextBTN == recordMGR.RecordBTN)
                fsm.PerformTransition(State.Record);
            else if (submitNextBTN == recordMGR.MyVoiceBTN)
                fsm.PerformTransition(State.MyVoice);
            else if (submitNextBTN == nextBTN)
                fsm.PerformTransition(State.Next);
        }
        IEnumerator X_Wait()
        {
            if (nextBTN.IsActive && recordMGR.IsRecorded)
                yield return stopTimeline(showNextButtonTL);

            enableButtons(false);
            yield return null;

            this.StopCoroutineSafe(ref crPlaySound);
            AudioMGR.One.StopNarration();
            speaker.SetPlay(false);
            yield return null;
        }
        IEnumerator E_Record()
        {
            yield return recordMGR.StartRecord();
            yield return null;

            if (recordMGR.IsRecorded)
                fsm.PerformTransition(State.MyVoice);
            else fsm.PerformTransition(State.Feedback);
        }
        IEnumerator X_Record()
        {
            recordMGR.FinishRecord();
            yield return null;
        }
        IEnumerator E_MyVoice()
        {
            yield return recordMGR.StartPlayMyVoice();
            yield return null;

            if (submitNextBTN == recordMGR.RecordBTN)
                fsm.PerformTransition(State.Feedback);
            else fsm.PerformTransition(State.Wait);
        }
        IEnumerator X_MyVoice()
        {
            recordMGR.FinishPlayMyVoice();
            yield return null;
        }
        IEnumerator E_Feedback()
        {
            yield return feedback.StartFeedback(recordMGR.Score);

            if (recordMGR.IsRecorded)
            {
                if (pMGR.PNO < pMGR.Count)
                    fsm.PerformTransition(State.Wait);
                else
                {
                    yield return new WaitForSeconds(0.5f);

                    fsm.PerformTransition(State.Outro);
                }
            }
            else
            {
                if (++recordTryCount >= tryCountForForceNext)
                {
                    if (pMGR.PNO < pMGR.Count)
                        fsm.PerformTransition(State.Wait);
                    else
                    {
                        yield return new WaitForSeconds(0.5f);

                        fsm.PerformTransition(State.Outro);
                    }
                }
                else fsm.PerformTransition(State.Read);
            }
        }
        IEnumerator X_Feedback()
        {
            feedback.StopFeedback();
            yield return null;
        }
        IEnumerator E_Next()
        {
            if (pMGR.Next())
                fsm.PerformTransition(State.ToPuzzle);
            else fsm.PerformTransition(State.Outro);
            yield return null;
        }
        IEnumerator E_ToPuzzle()
        {
            setupProblem(pMGR.Current);
            yield return null;

            yield return playTimeline(recordEndTL);
            yield return null;

            fsm.PerformTransition(State.ShowPuzzle);
        }
        IEnumerator X_ToPuzzle()
        {
            yield return stopTimeline(recordEndTL);
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

            AudioMGR.One.PlayEffect(SfxMoment.Activity_Complete);
            yield return new WaitForSeconds(rewardDelay);

            complete();
            yield return null;
        }



        // Unity Inspectors
        [Header("★ Bindings - Activity")]
        [SerializeField] private PuzzleMGR puzzleMGR = null;
        [SerializeField] private RecordMGR recordMGR = null;
        [SerializeField] private Speaker speaker = null;
        [SerializeField] private MyButton nextBTN = null;
        [SerializeField] private Feedback feedback = null;
        [SerializeField] private TextMeshProUGUI sentenceTXT = null;
        [SerializeField] private PermissionChecker permissionChecker = null;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector puzzleCompleteTL = null;
        [SerializeField] private PlayableDirector showNextButtonTL = null;
        [SerializeField] private PlayableDirector recordEndTL = null;
        [Header("★ Timing")]
        [SerializeField] private float rewardDelay = 1f;
        [Header("★ Sound - Ambient")]
        [SerializeField] private AudioClip ambientCLIP = null;
        [Range(0, 100)]
        [SerializeField] int ambientVolume = 100;
        [Header("★ Configs")]
        [SerializeField] private int tryCountForForceNext = 3;



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

            speaker.OnClick += speaker_OnClick;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            speaker.OnClick -= speaker_OnClick;
        }

        // Unity Coroutine
        IEnumerator coPlaySound()
        {
            using (LOG.Coroutine($"coPlaySound()", this))
            {
                enableButtons(false);
                speaker.SetPlay(true);
                yield return null;

                yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.SentenceCLIP);

                speaker.SetPlay(false);
                enableButtons(true);
                yield return null;
            }
        }
    }
}