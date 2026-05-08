using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using Cysharp.Threading.Tasks;
using SRDebugger;
using DG.Tweening;
using System.Threading;
using DoDoEng.TodaysStudy.Framework;
using DoDoEng.Launcher;
using DoDoEng.TodaysStudy.UI;
using System;
using UnityEngine.SceneManagement;
using DoDoEng.TodaysStudy.Controls;
using DoDoEng.Launcher.UI;
using System.Collections.Generic;

namespace DoDoEng.TodaysStudy
{
    [RequireComponent(typeof(TodaysStudyRunner))]
    [RequireComponent(typeof(TodaysStudyTester))]
    public class TodaysStudy : MonoBehaviour
    {
        // Definitions
        // Properties

        // Methods
        public async UniTask Prepare()
        {
            LOG.Info($"Prepare()", this);

            await onPrepare();
        }
        public void StartTodaysStudy()
        {
            LOG.Function(this);

#if !DISABLE_SRDEBUGGER
            srOptionContainer = new DynamicOptionContainer();
            onBuildOption(srOptionContainer);
            SRDebug.Instance.AddOptionContainer(srOptionContainer);
#endif

            DOVirtual.DelayedCall(startDelay, () => onStartTodaysStudy());
        }
        public void FinishTodaysStudy()
        {
            LOG.Function(this);

#if !DISABLE_SRDEBUGGER
            if (SRDebug.Instance != null && srOptionContainer != null)
            {
                SRDebug.Instance.RemoveOptionContainer(srOptionContainer);
                srOptionContainer = null;
            }
#endif

            onFinishTodaysStudy();
        }



        // Virtual
        protected virtual void onInitTodaysStudy()
        {
            UserData.One.LoadLanguage();

            UITodaysStudy.One.VisibleBackButton = true;
            UITodaysStudy.One.VisibleCoinInfo = true;
            UITodaysStudy.One.BackButton.onClick.AddListener(() => back_Click());

            todaysReward.gameObject.SetActive(false);
        }
        protected virtual async UniTask onPrepare()
        {
            todaysStudy.Interactable = false;

            // 코스, 스테이지 정보 로드
            courseTableResult = await CourseTableLoader.One.LoadTable();
            if (courseTableResult == null)
                LOG.Warning("There is no Course Table.", this);
            await UITodaysStudy.One.CoursePU.Init(CoursePopupType.Edit, courseTableResult);

            // 리워드 정보 로드
            var rewardTableResult = await RewardTableLoader.One.LoadTable();
            if (rewardTableResult == null)
                LOG.Warning("There is no Reward Table.", this);
            UITodaysStudy.One.ContinuousAttendPU.Init(rewardTableResult);

            // 액티비티, 게임 정보 로드
            await LibraryTableLoader.One.LoadActivityTable();
            await LibraryTableLoader.One.LoadGameTable();

            todaysStudy.MovieTableResult = await LibraryTableLoader.One.LoadMovieTable();
            todaysStudy.EBookTableResult = await LibraryTableLoader.One.LoadEBookTable();

            var stageData = courseTableResult.StageList.SingleOrDefault(data => data.Stage == RunnerParam.TodaysStage);
            await todaysStudy.Show(stageData.Course, RunnerParam.TodaysStage, RunnerParam.TodaysDay, RunnerParam.TodaysOrder);
            if (!RunnerParam.TodaysCompleted)
            {
                todaysStudy.CompleteANIReady();
            }
        }
        protected virtual void onStartTodaysStudy()
        {
            startTodaysStudy().Forget();
        }
        protected virtual void onFinishTodaysStudy() { }
#if !DISABLE_SRDEBUGGER
        protected virtual void onBuildOption(DynamicOptionContainer srOptionContainer)
        {
            var sort = 400;

            srOptionContainer.AddOption(OptionDefinition.Create("Learning Time", () => debugLearningTime, (value) => debugLearningTime = value, "Todays Study", ++sort));
            srOptionContainer.AddOption(OptionDefinition.Create("Incorrect Answer Rate", () => debugIncorrectAnswerRate, (value) => debugIncorrectAnswerRate = value, "Todays Study", ++sort));
            srOptionContainer.AddOption(
                OptionDefinition.FromMethod("Complete Current Todays Study", () =>
                {
                    completeCurrentTodaysStudy().Forget();
                }, "Todays Study", ++sort)
            );

            srOptionContainer.AddOption(OptionDefinition.Create("Test Reward Coin", () => debugRewardCoin, (value) => debugRewardCoin = value, "Todays Study", ++sort));
            srOptionContainer.AddOption(
                OptionDefinition.FromMethod("Test Day Reward", () =>
                {
                    todaysReward.PlayDayReward(debugRewardCoin).Forget();
                }, "Todays Study", ++sort)
            );
            srOptionContainer.AddOption(
                OptionDefinition.FromMethod("Test Stage Reward", () =>
                {
                    todaysReward.PlayStageReward(debugRewardCoin).Forget();
                }, "Todays Study", ++sort)
            );
            srOptionContainer.AddOption(
                OptionDefinition.FromMethod("Test Course Reward", () =>
                {
                    todaysReward.PlayCourseReward(debugRewardCoin).Forget();
                }, "Todays Study", ++sort)
            );
            srOptionContainer.AddOption(
                OptionDefinition.FromMethod("Test Stage Emblem", () =>
                {
                    todaysReward.PlayStageEmblem(1).Forget();
                }, "Todays Study", ++sort)
            );
            srOptionContainer.AddOption(
                OptionDefinition.FromMethod("Test Course Emblem", () =>
                {
                    todaysReward.PlayCourseEmblem(1).Forget();
                }, "Todays Study", ++sort)
            );
            srOptionContainer.AddOption(
                OptionDefinition.FromMethod("Test First Sign In Reward", () =>
                {
                    todaysReward.PlayFirstSignInReward(100).Forget();
                }, "Todays Study", ++sort)
            );
            srOptionContainer.AddOption(
                OptionDefinition.FromMethod("Test Stamp Reward", () =>
                {
                    var rewards = new int[] { 10, 10, 10 };
                    todaysReward.PlayStampReward(rewards).Forget();
                }, "Todays Study", ++sort)
            );
            srOptionContainer.AddOption(
                OptionDefinition.FromMethod("Test Attendance", () =>
                {
                    attendanceProccess(10).Forget();
                }, "Todays Study", ++sort)
            );
            srOptionContainer.AddOption(
                OptionDefinition.FromMethod("Test Complete Contents", () =>
                {
                    todaysStudy.CompleteANIReady();
                    todaysStudy.CompleteANIPlay().Forget();
                }, "Todays Study", ++sort)
            );
        }
#endif



        // Fields
#if !DISABLE_SRDEBUGGER
        private DynamicOptionContainer srOptionContainer;
#endif
        protected CancellationTokenSource cancel = new CancellationTokenSource();
        private CourseTableLoaderResult courseTableResult;
        private int debugLearningTime = 10;
        private int debugIncorrectAnswerRate = 0;
        private int debugRewardCoin = 10;

        // Functions
        private async UniTask startTodaysStudy()
        {
            if (UserData.One.Child.HasSignedIn)
            {
                todaysStudy.Interactable = false;

                var rewards = await LMS.One.DoAttendance();
                int rewardAttendance = 0;
                if (rewards != null)
                {
                    var stampPoints = new List<int>();
                    foreach (var reward in rewards) {
                        var code = reward.Value<string>("rewardPolicyCode");
                        var point = reward.Value<int>("point");
                        if (code == "9001")
                        {
                            await todaysReward.PlayFirstSignInReward(point);
                        }
                        else if (code == "9007")
                        {
                            stampPoints.Add(point);
                        }
                        else
                        {
                            rewardAttendance += point;
                        }
                    }

                    if (stampPoints.Count > 0)
                    {
                        await todaysReward.PlayStampReward(stampPoints.ToArray());
                    }
                }

                if (UserData.One.Child.IsAttendanceFirst)
                {
                    await attendanceProccess(rewardAttendance);
                }

                await UserData.One.CheckEvents();

                if (!RunnerParam.TodaysCompleted)
                {
                    await todaysStudy.CompleteANIPlay();
                }

                bool hasReward = false;
                if (UserData.One.Child.RewardDay > 0)
                {
                    await todaysReward.PlayDayReward(UserData.One.Child.RewardDay);
                    UserData.One.Child.RewardDay = 0;
                    hasReward = true;
                }
                if (UserData.One.Child.RewardStage > 0)
                {
                    await todaysReward.PlayStageReward(UserData.One.Child.RewardStage);
                    UserData.One.Child.RewardStage = 0;
                    hasReward = true;
                }
                if (UserData.One.Child.RewardCourse > 0)
                {
                    await todaysReward.PlayCourseReward(UserData.One.Child.RewardCourse);
                    UserData.One.Child.RewardCourse = 0;
                    hasReward = true;
                }

                if (hasReward)
                {
                    await LMS.One.LoadDayProgress();
                    var course = UserData.One.Child.DayProgress.Value<int>("courseId");
                    var stage = UserData.One.Child.DayProgress.Value<int>("stageId");
                    var day = UserData.One.Child.DayProgress.Value<int>("day");
                    var prevStage = todaysStudy.Stage;
                    var prevDay = todaysStudy.Day;
                    await todaysStudy.Show(course, stage, day);
                    if (UserData.One.Child.Course != course)
                    {
                        UserData.One.Child.Course = course;
                        await todaysReward.PlayCourseEmblem(course);
                    }
                    else if (prevStage != stage)
                    {
                        await todaysReward.PlayStageEmblem(stage);
                    }
                    else if (prevDay == day)
                    {// 오늘의 학습 모두 완료
                        if (await SystemUI.One.ShowPopupDayComplete(stage == 24))
                        {
                            SystemEventManager.SystemButtonClick(SystemButtonType.Back);
                        }
                    }
                }

                todaysStudy.Interactable = true;
            }

        }
        private async UniTask attendanceProccess(int point)
        {
            await SystemUI.One.CalendarPU.ShowPopup(true);
            await UITodaysStudy.One.ContinuousAttendPU.ShowPopup(UserData.One.Child.AttendanceContinuous, point);
        }
        private async UniTask completeCurrentTodaysStudy()
        {
            if (todaysStudy.gameObject.activeSelf && todaysStudy.CurrentTodaysItem)
            {
                RunnerParam.TodaysCompleted = todaysStudy.CurrentTodaysItem.IsComplete;

                var learningIndexId = todaysStudy.CurrentTodaysItem.LearningIndexId;
                var typeId = todaysStudy.CurrentTodaysItem.ContentIndex.Substring(0, 1);
                if (typeId == "1") // Activity
                    await LMS.One.CompleteStudy(true, learningIndexId, debugLearningTime, debugIncorrectAnswerRate);
                else if (typeId == "2" || typeId == "4") // Movie or eBook
                    await LMS.One.CompleteStudyRead(true, learningIndexId, debugLearningTime);
                else if (typeId == "5") // Review Game
                    await LMS.One.CompleteStudy(true, learningIndexId, debugLearningTime);
                await todaysStudy.Refresh();
                if (!RunnerParam.TodaysCompleted)
                {
                    todaysStudy.CompleteANIReady();
                    await todaysStudy.CompleteANIPlay();
                }
            }
        }

        // Event Handlers
        private void back_Click()
        {
            if (todaysStudy.AllCompleted)
            {
                AudioMGR.One.PlayEffectUI(SfxMoment.System_Back);
                SystemEventManager.SystemButtonClick(SystemButtonType.Back);
            }
            else
            {
                UITodaysStudy.One.ExitPU.ShowPopup().Forget();
            }
        }
        private void todaysStudy_OnClickItem(IndexBase index, int learningIndexId)
        {
            LOG.Function(this, $"| index={index} | learningIndexId={learningIndexId}");

            downloadAndStartContents(index, learningIndexId).Forget();
        }
        private async UniTask downloadAndStartContents(IndexBase index, int learningIndexId)
        {
            try
            {
                if (await todaysStudy.DownloadAllStagesAssets())
                {
                    RunnerParam.LaunchedFrom = RunnerParam.LaunchType.Day;
                    RunnerParam.SelectedIDX = index;
                    RunnerParam.LearningIndex = learningIndexId;
                    RunnerParam.MovieMode = RunnerParam.MovieModeType.Watch;
                    await loadScene(index.SceneName);
                }
            }
            catch (Exception ex)
            {
                LOG.Error($"{ex.Message}", this);
                LOG.Error($"Cannot run any more because of an error!!!!", this);

                await SystemUI.One.ErrorPU.ShowPopup(ex.Message);
            }
        }
        private async UniTask loadScene(string sceneName)
        {
            try
            {
                todaysStudy.Interactable = false;

                RunnerParam.ReturnScene = SceneManager.GetActiveScene().name;
                SceneLoader.One.LoadScene(sceneName);
                FinishTodaysStudy();
            }
            catch (Exception ex)
            {
                LOG.Error($"{ex.Message}", this);
                LOG.Error($"Cannot run any more because of an error!!!!", this);

                await SystemUI.One.ErrorPU.ShowPopup(ex.Message);
            }
        }

        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TodaysStudyPopup todaysStudy = null;
        [SerializeField] private TodaysReward todaysReward = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip bgmCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float startDelay = 0.5f;

        // Unity Messages
        protected virtual void Awake()
		{
            onInitTodaysStudy();
        }
        protected virtual void Start()
		{
            AudioMGR.One.PlayBGM(bgmCLIP);
        }
        protected virtual void OnEnable()
        {
            todaysStudy.OnClickItem += todaysStudy_OnClickItem;
        }
        protected virtual void OnDisable()
        {
            todaysStudy.OnClickItem -= todaysStudy_OnClickItem;
        }
        protected virtual void OnDestroy()
        {
            cancel.Cancel();
        }

        // Unity Coroutine
    }
}