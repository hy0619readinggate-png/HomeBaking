using beyondi.Behaviour;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Launcher;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Common
{
    public class LMS : BYDSingleton<LMS>
    {
        // Definitions
        private const int CANDY_MAX = 3;
        public enum CompleteType
        {
            Read,
            Record,
            Quiz
        }
        public enum StampType
        {
            SUPERB, 
            AWESOME,
            AMAZING,
            COOL, 
            BRAVO
        }
        public enum RecordType
        {
            Default,
            Star,
            Full,
            StarAndFull
        }

        // Static
        public static bool DEV_ForceRewardCoin = false;
        public static int DEV_ForceRewardCoinCount = 5;

        // Properties
        public int Coin
        {
            get
            {
                return coin;
            }
            set
            {
                coin = value;
                OnChangeCoin?.Invoke(value);
            }
        }
        public bool IsMaxPlaygroundPlayCount => candy == CANDY_MAX;
        public int PlaygroundPlayCount => candy;
        public string[] PlaygroundCodes => playgroundCodes;
        public int[] PlaygroundScores => playgroundScores;

        // Methods : for Check Auth Status
        public async UniTask<bool> StartChildCheckAuthStatus()
        {
            LOG.LMS($"StartChildCheckAuthStatus()", this);

            nowCheckAuthStatusChild = true;
            while (nowCheckAuthStatusChild)
            {
                await UniTask.Delay(checkAuthStatusTime);
                if (!nowCheckAuthStatusChild)
                    break;
                API.Result result = await API.One.Call(UserData.One.Child.AccessToken, $"/api/v1/child/check-auth-status");
                if (!nowCheckAuthStatusChild)
                    break;
                if (result.Success)
                {
                }
                else
                {
                    LOG.Warning($"Error Checking Auth Status :  {result.Data}", this);
                    if (result.Data.Value<int>("status") == 412)
                    {
                        await SystemUI.One.ShowPopupDuplicatedSignIn();
                        SignOutChild().Forget();
                        UserData.One.Child.AccessToken = "";
                        UserData.One.Child.AutoSignIn = false;
                        //RunnerParam.ReturnScene = SceneManager.GetActiveScene().name;
                        SceneLoader.One.LoadScene("Launcher");
                        return false;
                    }
                }
            }

            return true;
        }
        public void StopChildCheckAuthStatus()
        {
            LOG.LMS($"StopChildCheckAuthStatus()", this);
            nowCheckAuthStatusChild = false;
        }
        public async UniTask<bool> StartParentCheckAuthStatus()
        {
            LOG.LMS($"StartParentCheckAuthStatus()", this);

            nowCheckAuthStatusParent = true;
            while (nowCheckAuthStatusParent)
            {
                await UniTask.Delay(checkAuthStatusTime);
                if (!nowCheckAuthStatusParent)
                    break;
                API.Result result = await API.One.Call(UserData.One.Parent.AccessToken, $"/api/v1/user/check-auth-status");
                if (!nowCheckAuthStatusParent)
                    break;
                if (result.Success)
                {
                }
                else
                {
                    LOG.Warning($"Error Checking Auth Status :  {result.Data}", this);
                    if (result.Data.Value<int>("status") == 412)
                    {
                        await SystemUI.One.ShowPopupDuplicatedSignIn();
                        SignOutParent().Forget();
                        UserData.One.Parent.AccessToken = "";
                        UserData.One.Child.AccessToken = "";
                        SceneLoader.One.LoadScene("Launcher");
                        return false;
                    }
                }
            }

            return true;
        }
        public void StopParentCheckAuthStatus()
        {
            LOG.LMS($"StopParentCheckAuthStatus()", this);
            nowCheckAuthStatusParent = false;
        }

        // Methods : for Complete Study
        /// <summary>
        /// 무비 완료 시 호출
        /// </summary>
        /// <param name="launchedFrom">RunnerParam으로부터 받은 LaunchedFrom</param>
        /// <param name="learningIndex">RunnerParam으로부터 받은 LearningIndex</param>
        /// <param name="time">학습시간 (초)</param>
        public async UniTask<int> CompleteMovie(RunnerParam.LaunchType launchedFrom, int learningIndex, int time)
        {
            return await CompleteStudyRead(launchedFrom == RunnerParam.LaunchType.Day, learningIndex, time);
        }
        /// <summary>
        /// 나의 무비 완료 시 호출
        /// </summary>
        /// <param name="launchedFrom">RunnerParam으로부터 받은 LaunchedFrom</param>
        /// <param name="learningIndex">RunnerParam으로부터 받은 LearningIndex</param>
        /// <param name="time">학습시간 (초)</param>
        public async UniTask<int> CompleteMovieRecord(RunnerParam.LaunchType launchedFrom, int learningIndex, int time)
        {
            return await CompleteStudyRecord(launchedFrom == RunnerParam.LaunchType.Day, learningIndex, time);
        }
        /// <summary>
        /// 액티비티 완료 시 호출
        /// </summary>
        /// <param name="launchedFrom">RunnerParam으로부터 받은 LaunchedFrom</param>
        /// <param name="learningIndex">RunnerParam으로부터 받은 LearningIndex</param>
        /// <param name="time">학습시간 (초)</param>
        /// <param name="wrongRate">오답률 (0~100%)</param>
        public async UniTask<int> CompleteActivity(RunnerParam.LaunchType launchedFrom, int learningIndex, int time, int wrongRate)
        {
            return await CompleteStudy(launchedFrom == RunnerParam.LaunchType.Day, learningIndex, time, wrongRate);
        }
        /// <summary>
        /// 이북 읽기 완료 시 호출
        /// </summary>
        /// <param name="launchedFrom">RunnerParam으로부터 받은 LaunchedFrom</param>
        /// <param name="learningIndex">RunnerParam으로부터 받은 LearningIndex</param>
        /// <param name="time">학습시간 (초)</param>
        public async UniTask<int> CompleteEBookRead(RunnerParam.LaunchType launchedFrom, int learningIndex, int time)
        {
            return await CompleteStudyRead(launchedFrom == RunnerParam.LaunchType.Day, learningIndex, time);
        }
        /// <summary>
        /// 나의 이북 완료 시 호출
        /// </summary>
        /// <param name="learningIndex">RunnerParam으로부터 받은 learningIndex</param>
        /// <param name="time">학습시간 (초)</param>
        public async UniTask<int> CompleteEBookRecord(RunnerParam.LaunchType launchedFrom, int learningIndex, int time)
        {
            return await CompleteStudyRecord(launchedFrom == RunnerParam.LaunchType.Day, learningIndex, time);
        }
        /// <summary>
        /// 이북 퀴즈 완료 시 호출
        /// </summary>
        /// <param name="learningIndex">RunnerParam으로부터 받은 learningIndex</param>
        /// <param name="time">학습시간 (초)</param>
        public async UniTask<int> CompleteEBookQuiz(RunnerParam.LaunchType launchedFrom, int learningIndex)
        {
            return await CompleteStudyQuiz(launchedFrom == RunnerParam.LaunchType.Day, learningIndex);
        }
        /// <summary>
        /// ReviewGame 완료 시 호출
        /// </summary>
        /// <param name="learningIndex">RunnerParam으로부터 받은 learningIndex</param>
        /// <param name="time">학습시간 (초)</param>
        public async UniTask<int> CompleteReviewGame(int learningIndex, int time)
        {
            return await CompleteStudy(true, learningIndex, time);
        }
        /// <summary>
        /// AI Studio 핛습 완료 시 호출
        /// </summary>
        /// <param name="curriculumId">LMS로부터 받은 curriculumId</param>
        /// <param name="time">학습시간 (초)</param>
        /// <param name="recordType">녹음 종류 (Star, Full)</param>
        public async UniTask<int> CompleteAIStudio(int curriculumId, int time, RecordType recordType)
        {
            return await CompleteStudyRecord(false, curriculumId, time, (int)recordType);
        }
        /// <summary>
        /// PlaygroundGame 완료 시 호출
        /// </summary>
        /// <param name="numSlot">슬롯 번호</param>
        /// <param name="stars">획득한 별 갯수</param>
        public async UniTask<int> CompletePlaygroundGame(int curriculumId, int learningTime, int stars)
        {
            LOG.LMS($"CompletePlaygroundGame() | curriculumId={curriculumId} | learningTime={learningTime} | stars={stars}", this);

            // SROption으로 리워드를 강제 설정 by swon 2024.07.04
            if (DEV_ForceRewardCoin)
                return DEV_ForceRewardCoinCount;

            // 정상 진행
            API.Result result = await API.One.Call(
                    UserData.One.Child.AccessToken,
                    $"/api/v1/child/learning/playground-progress/{curriculumId}",
                    API.Method.Post,
                    JObject.Parse($"{{isComplete: true, learningTime: {learningTime}, stars: {stars}}}"));

            if (result.Success)
            {
                int point = 0;
                result.Data.Value<JArray>("rewardPoints").ForEach(rewardPoint => point += rewardPoint.Value<int>("point"));
                return point;
            }
            else
            {
                LOG.Warning($"API error: {result.Data.Value<string>("code")}", this);
                return 0;
            }
        }

        // Methods : for Reward
        public async UniTask<bool> LoadReward()
        {
            LOG.LMS($"LoadReward()", this);

            API.Result result = await API.One.Call(UserData.One.Child.AccessToken, $"/api/v1/child/learning/reward-point");

            if (result.Success)
            {
                Coin = result.Data.Value<int>("point");
                return true;
            }
            else
                return false;
        }
        public async UniTask<int> SaveReward(int rewardIndex)
        {
            LOG.LMS($"SaveReward() | rewardIndex={rewardIndex}", this);

            API.Result result = await API.One.Call(
            UserData.One.Child.AccessToken,
                $"/api/v1/child/learning/reward-point/{rewardIndex}",
                API.Method.Post);

            if (result.Success)
            {
                var point = result.Data.Value<int>("point");
                //Coin += point;
                return point;
            }
            else
            {
                SystemUI.One.ErrorPU.ShowPopup("포인트 저장에 실패하였습니다.").Forget();
                return 0;
            }
        }
        public async UniTask<JObject> LoadRewardHistory(string searchDate)
        {
            LOG.LMS($"LoadRewardHistory() | searchDate={searchDate}", this);

            API.Result result = await API.One.Call(UserData.One.Child.AccessToken, $"/api/v1/child/learning/reward-point/history?searchDate={searchDate}");

            if (result.Success)
                return result.Data;
            else
                return new JObject();
        }

        // Methods : for Record
        public async UniTask<bool> SaveAudioRecords(int contentIndex, AudioClip[] clips)
        {
            LOG.LMS($"SaveAudioRecords() | contentIndex={contentIndex} | clips={clips}", this);

            bool allComplete = true;
            for (var i = 0; i < clips.Length; i++)
            {
                var file = $"REC_{i + 1:D3}.wav";
                var bytes = WavUtility.FromAudioClip(clips[i]);

                if (!await UploadFile(contentIndex, file, bytes))
                    allComplete = false;
            }

            if (allComplete)
            {
                allComplete = await CreateInvalidation(contentIndex);
            }

            return allComplete;
        }
        public async UniTask<AudioClip[]> LoadAudioRecords(int index, int count)
        {
            LOG.LMS($"LoadAudioRecords() | index={index} | count={count}", this);

            AudioClip[] clips = new AudioClip[count];

            var result = await API.One.Call(
                UserData.One.Child.AccessToken,
                $"/api/v1/child/my-content/list?contentIndex={index}"
                );

            if (result.Success)
            {
                for (var i = 0; i < count; i++)
                {
                    try
                    {
                        var file = $"REC_{i + 1:D3}.wav";
                        var data = result.Datas.First(data => data.Value<string>("targetKey") == file);
                        if (data != null)
                        {
                            var bytes = await API.One.Download(data.Value<string>("playUrl"));
                            clips[i] = WavUtility.ToAudioClip(bytes);
                        }
                    }
                    catch (Exception e)
                    {
                        LOG.Warning(e.Message, this);
                    }
                }
            }

            return clips;
        }
        public async UniTask<bool> UploadFile(int contentIndex, string filePath, RecordType recordType = RecordType.Default)
        {
            var bytes = await File.ReadAllBytesAsync(filePath);
            return await UploadFile(contentIndex, Path.GetFileName(filePath), bytes, recordType);
        }
        public async UniTask<bool> UploadFile(int contentIndex, string fileName, byte[] bytes, RecordType recordType = RecordType.Default)
        {
            LOG.LMS($"UploadFile() | contentIndex={contentIndex} | fileName={fileName} | bytes={bytes} | recordType={recordType}", this);

            var result = await API.One.Call(
                UserData.One.Child.AccessToken,
                $"/api/v1/child/my-content/upload-presigned-url?contentIndex={contentIndex}&targetKey={fileName}");

            if (result.Success)
            {
                int userContentId = result.Data.Value<int>("userContentId");
                var resultUpload = await API.One.Upload(result.Data.Value<string>("presignedUrl"), bytes);
                if (resultUpload)
                {
                    var resultUC = await API.One.Call(
                        UserData.One.Child.AccessToken,
                        $"/api/v1/child/my-content/upload-complete?userContentId={userContentId}&recordType={(int)recordType}",
                        API.Method.Put);

                    return resultUC.Success;
                }
                else
                    return false;
            }
            else
                return false;
        }
        public async UniTask<bool> CreateInvalidation(int contentIndex)
        {
            LOG.LMS($"CreateInvalidation() | contentIndex={contentIndex}", this);

            var result = await API.One.Call(
                        UserData.One.Child.AccessToken,
                        $"/api/v1/child/my-content/create-invalidation?contentIndex={contentIndex}",
                        API.Method.Put);

            return result.Success;
        }
        public async UniTask<byte[][]> DownloadFiles(int contentIndex, string[] fileNames)
        {
            LOG.LMS($"DownloadFiles() | index={contentIndex} | fineNames={fileNames}", this);

            byte[][] binaries = new byte[fileNames.Length][];

            var result = await API.One.Call(
                UserData.One.Child.AccessToken,
                $"/api/v1/child/my-content/list?contentIndex={contentIndex}"
                );

            if (result.Success)
            {
                for (var i = 0; i < fileNames.Length; i++)
                {
                    try
                    {
                        var data = result.Datas.First(data => data.Value<string>("targetKey") == fileNames[i]);
                        if (data != null)
                        {
                            binaries[i] = await API.One.Download(data.Value<string>("playUrl"));
                        }
                    }
                    catch (Exception e)
                    {
                        LOG.Warning(e.Message, this);
                    }
                }
            }

            return binaries;
        }
        public async UniTask<bool> RemoveFiles(int contentIndex, string fileName = "")
        {
            LOG.LMS($"RemoveFiles() | contentIndex={contentIndex} | fileName={fileName}", this);

            var api = $"/api/v1/child/my-content?contentIndex={contentIndex}";
            if (!string.IsNullOrEmpty(fileName))
                api += $"&targetKey={fileName}";
            API.Result result = await API.One.Call(
                UserData.One.Child.AccessToken,
                api,
                API.Method.Delete);

            if (result.Success)
            {
                return true;
            }
            else
            {
                SystemUI.One.ErrorPU.ShowPopup("저장 파일 삭제에 실패하였습니다.").Forget();
                return false;
            }
        }
        public async UniTask<bool> RemoveRecord(int curriculumId)
        {
            LOG.LMS($"RemoveRecord() | curriculumId={curriculumId}", this);

            API.Result result = await API.One.Call(
                UserData.One.Child.AccessToken,
                $"/api/v1/child/learning/library-progress/{curriculumId}/record",
                API.Method.Delete);

            if (result.Success)
            {
                return true;
            }
            else
            {
                LOG.Warning($"RemoveRecord() Fail.", this);
                return false;
            }
        }


        // Methods : for AI Studio Report
        public async UniTask<string> LoadAIStudioReport(int contentIndex, RecordType recordType)
        {
            LOG.LMS($"LoadAIStudioReport() | contentIndex={contentIndex} | recordType={recordType}", this);

            API.Result result = await API.One.Call(UserData.One.Child.AccessToken, $"/api/v1/child/learning/ai-studio-report/{contentIndex}/{(int)recordType}");

            if (result.Success)
                return result.Data.ToString();
            else
                return "{}";
        }
        public async UniTask<bool> SaveAIStudioReport(int contentIndex, RecordType recordType, string reportData)
        {
            LOG.LMS($"SaveAIStudioReport() | contentIndex={contentIndex} | recordType={recordType} | reportData ={reportData}", this);

            reportData = reportData.Replace("\\\\", "/").Replace("\"", "\\\"");

            API.Result result = await API.One.Call(
                UserData.One.Child.AccessToken,
                $"/api/v1/child/learning/ai-studio-report/{contentIndex}/{(int)recordType}",
                API.Method.Post,
                JObject.Parse($"{{reportData: \"{reportData}\"}}"));

            if (result.Success)
            {
                return true;
            }
            else
            {
                SystemUI.One.ErrorPU.ShowPopup("AI Studio Report 저장에 실패하였습니다.").Forget();
                return false;
            }
        }
        public async UniTask<bool> RemoveAIStudioReport(int contentIndex, RecordType recordType)
        {
            LOG.LMS($"RemoveAIStudioReport() | contentIndex={contentIndex} | recordType={recordType}", this);

            API.Result result = await API.One.Call(
                UserData.One.Child.AccessToken,
                $"/api/v1/child/learning/ai-studio-report/{contentIndex}/{(int)recordType}",
                API.Method.Delete);

            if (result.Success)
            {
                return true;
            }
            else
            {
                SystemUI.One.ErrorPU.ShowPopup("AI Studio Report 삭제에 실패하였습니다.").Forget();
                return false;
            }
        }

        // Methods : for Launcher
        public async UniTask<API.Result> SignInChild(string id, string pw, bool autoSignIn = false)
        {
            LOG.LMS($"SignIn() | id={id} | pw=?? | autoSignIn={autoSignIn}", this);

            API.Result result = await API.One.Call(
                null,
                "/api/v1/child/login",
                API.Method.Post,
                JObject.Parse($"{{loginId: '{id}', password: '{pw}'}}"));

            if (result.Success)
            {
                UserData.One.Child.ID = result.Data.Value<string>("loginId");
                UserData.One.Child.Password = pw;
                UserData.One.Child.AutoSignIn = autoSignIn;
                UserData.One.Child.AccessToken = result.Data.Value<string>("accessToken");
                UserData.One.Child.NickName = result.Data.Value<string>("nickName");
                UserData.One.Child.Course = result.Data.Value<int>("learningCourse");
                UserData.One.Child.ParentID = result.Data.Value<string>("parentLoginId");
                UserData.One.Child.ParentSignupType = result.Data.Value<string>("parentSignupType");

                UserData.One.SaveLocalData();

                StartChildCheckAuthStatus().Forget();

                LOG.LMS($"SignIn Success : {UserData.One.Child}", this);
            }
            else
            {
                LOG.Warning($"SignIn error: {result.Data.Value<string>("code")}", this);
            }
            return result;
        }
        public async UniTask<bool> SignOutChild()
        {
            LOG.LMS($"SignOutChild()", this);

            StopChildCheckAuthStatus();

            API.Result result = await API.One.Call(UserData.One.Child.AccessToken, $"/api/v1/child/logout");

            if (result.Success)
                return true;
            else
                return false;
        }
        public async UniTask<bool> LoadDayProgress()
        {
            LOG.LMS($"LoadDayProgress()", this);

            API.Result result = await API.One.Call(UserData.One.Child.AccessToken, $"/api/v1/child/learning/day-progress");

            if (result.Success)
            {
                UserData.One.Child.DayProgress = result.Data;

                //var courseId = result.Data.Value<int>("courseId");
                //var stageId = result.Data.Value<int>("stageId");
                //var weekly = result.Data.Value<int>("weekly");
                //var day = result.Data.Value<int>("day");
                //var dayType = result.Data.Value<int>("dayType");
                //var courseLearningHistory = result.Data.Value<JArray>("courseLearningHistory");

                return true;
            }
            else
            {
                SystemUI.One.ErrorPU.ShowPopup("Failed to load day progress.").Forget();
                return false;
            }
        }
        public async UniTask<JArray> LoadDayCurriculum()
        {
            LOG.LMS($"LoadDayCurriculum()", this);

            API.Result result = await API.One.Call(UserData.One.Child.AccessToken, $"/api/v1/child/learning/day-progress/curriculum/all");

            if (result.Success)
                return result.Datas;
            else
                return null;
        }
        public async UniTask<JArray> LoadDayCurriculum(int stage)
        {
            LOG.LMS($"LoadDayCurriculum() | stage={stage}", this);

            API.Result result = await API.One.Call(UserData.One.Child.AccessToken, $"/api/v1/child/learning/day-progress/curriculum?stageId={stage}");

            if (result.Success)
                return result.Datas;
            else
                return null;
        }
        public async UniTask<bool> SaveLearningCourse(int course)
        {
            LOG.LMS($"SaveLearningCourse() | course={course}", this);

            API.Result result = await API.One.Call(
                UserData.One.Child.AccessToken,
                $"/api/v1/child/learning/course?learningCourse={course}",
                API.Method.Put);

            if (result.Success)
            {
                return true;
            }
            else
            {
                SystemUI.One.ErrorPU.ShowPopup("자녀 정보 저장에 실패하였습니다.").Forget();
                return false;
            }
        }
        public async UniTask<int> CompleteStudy(bool isTodaysStudy, int curriculumId, int learningTime, int incorrectAnswerRate = 0)
        {
            LOG.LMS($"CompleteStudy() | isTodaysStudy={isTodaysStudy} | curriculumId={curriculumId} | learningTime={learningTime} | incorrectAnswerRate={incorrectAnswerRate}", this);

            var data = $"{{learningTime: {learningTime}, incorrectAnswerRate: {incorrectAnswerRate}}}";

            return await CompleteStudy(isTodaysStudy, curriculumId, "", data);
        }
        public async UniTask<int> CompleteStudyRead(bool isTodaysStudy, int curriculumId, int learningTime)
        {
            LOG.LMS($"CompleteStudyRead() | isTodaysStudy={isTodaysStudy} | curriculumId={curriculumId} | learningTime={learningTime}", this);

            var data = $"{{learningTime: {learningTime}}}";

            return await CompleteStudy(isTodaysStudy, curriculumId, "/read", data);
        }
        public async UniTask<int> CompleteStudyRecord(bool isTodaysStudy, int curriculumId, int learningTime, int recordType = 0)
        {
            LOG.LMS($"CompleteStudyRecord() | isTodaysStudy={isTodaysStudy} | curriculumId={curriculumId} | learningTime={learningTime} | recordType={recordType}", this);

            var data = $"{{isRecorded: true, learningTime: {learningTime}, recordType: {recordType}}}";

            return await CompleteStudy(isTodaysStudy, curriculumId, "/record", data);
        }
        public async UniTask<int> CompleteStudyQuiz(bool isTodaysStudy, int curriculumId)
        {
            LOG.LMS($"CompleteStudyQuiz() | isTodaysStudy={isTodaysStudy} | curriculumId={curriculumId}", this);

            return await CompleteStudy(isTodaysStudy, curriculumId, "/quiz");
        }
        public async UniTask<int> CompleteStudy(bool isTodaysStudy, int curriculumId, string completeType = "", string data = "{}")
        {
            LOG.LMS($"CompleteStudy() | isTodaysStudy={isTodaysStudy} | curriculumId={curriculumId} | completeType={completeType} | data={data}", this);

            // SROption으로 리워드를 강제 설정 by swon 2024.07.04
            if (DEV_ForceRewardCoin)
                return DEV_ForceRewardCoinCount;

            // 정상 진행
            var apiType = isTodaysStudy ? "day-progress" : "library-progress";

            API.Result result = await API.One.Call(
                    UserData.One.Child.AccessToken,
                    $"/api/v1/child/learning/{apiType}/{curriculumId}{completeType}",
                    API.Method.Post,
                    JObject.Parse(data));

            if (result.Success)
            {
                int totalPoint = 0;
                result.Data.Value<JArray>("rewardPoints").ForEach(rewardPoint => {
                    var rewardPolicyCode = rewardPoint.Value<string>("rewardPolicyCode");
                    var point = rewardPoint.Value<int>("point");
                    if (rewardPolicyCode == "8001")
                        UserData.One.Child.RewardDay = point;
                    else if (rewardPolicyCode == "8002")
                        UserData.One.Child.RewardStage = point;
                    else if (rewardPolicyCode == "8003")
                        UserData.One.Child.RewardCourse = point;
                    else
                        totalPoint += point;
                    });
                return totalPoint;
            }
            else
            {
                LOG.Warning($"API error: {result.Data.Value<string>("code")}", this);

                return 0;
            }
        }

        // Methods : for Library
        public async UniTask<JArray> LoadLibraryCategory(string learningTypeCode)
        {
            LOG.LMS($"LoadLibraryCategory() | learningTypeCode={learningTypeCode}", this);

            API.Result result = await API.One.Call(UserData.One.Child.AccessToken, $"/api/v1/child/learning/library-progress/catetories/{learningTypeCode}");

            if (result.Success)
                return result.Datas;
            else
                return null;
        } 
        public async UniTask<JArray> LoadLibraryProgress(string learningTypeCode, int mainCatetoryId, int subCatetoryId = 0)
        {
            LOG.LMS($"LoadLibraryProgress() | learningTypeCode={learningTypeCode} | mainCatetoryId={mainCatetoryId} | subCatetoryId={subCatetoryId}", this);

            string subCatetoryIdQuery = "";
            if (subCatetoryId > 0)
                subCatetoryIdQuery = $"&subCatetoryId={subCatetoryId}";
            API.Result result = await API.One.Call(
                UserData.One.Child.AccessToken, 
                $"/api/v1/child/learning/library-progress/{learningTypeCode}?mainCatetoryId={mainCatetoryId}{subCatetoryIdQuery}");

            if (result.Success)
                return result.Datas;
            else
                return new JArray();
        }
        public async UniTask<JArray> LoadLibraryRecorded(string learningTypeCode)
        {
            LOG.LMS($"LoadLibraryRecorded() | learningTypeCode={learningTypeCode}", this);

            API.Result result = await API.One.Call(
                UserData.One.Child.AccessToken, 
                $"/api/v1/child/learning/library-progress/recorded/{learningTypeCode}");

            if (result.Success)
                return result.Datas;
            else
                return null;
        }
        public async UniTask<JArray> LoadLibraryFavorite(string learningTypeCode)
        {
            LOG.LMS($"LoadLibraryFavorite() | learningTypeCode={learningTypeCode}", this);

            API.Result result = await API.One.Call(
                UserData.One.Child.AccessToken,
                $"/api/v1/child/learning/library-favorites/{learningTypeCode}");

            if (result.Success)
                return result.Datas;
            else
                return new JArray();
        }
        public async UniTask<int> RemoveLibraryRecorded(int curriculumId)
        {
            LOG.LMS($"RemoveLibraryRecorded() | curriculumId={curriculumId}", this);

            var data = $"{{isRecorded: false, learningTime: 0, recordType: null}}";

            return await CompleteStudy(false, curriculumId, "/record", data);
        }
        public async UniTask<bool> RemoveAIStudioRecorded(int curriculumId, RecordType recordType)
        {
            LOG.LMS($"RemoveAIStudioRecorded() | curriculumId={curriculumId} | recordType={recordType}", this);

            API.Result result = await API.One.Call(
                UserData.One.Child.AccessToken,
                $"/api/v1/child/learning/library-progress/ai-studio/{curriculumId}/{(int)recordType}",
                API.Method.Delete);

            if (result.Success)
            {
                return true;
            }
            else
            {
                SystemUI.One.ErrorPU.ShowPopup("My Movie 삭제에 실패하였습니다.").Forget();
                return false;
            }
        }
        public async UniTask<bool> AddLibraryFavorite(string learningTypeCode, int contentIndex)
        {
            LOG.LMS($"AddLibraryFavorite() | contentIndex={contentIndex}", this);

            API.Result result = await API.One.Call(
                UserData.One.Child.AccessToken,
                $"/api/v1/child/learning/library-favorites/{learningTypeCode}",
                API.Method.Post,
                JObject.Parse($"{{contentIndex: {contentIndex}}}"));

            if (result.Success)
            {
                return true;
            }
            else
            {
                SystemUI.One.ErrorPU.ShowPopup("즐겨찾기 추가에 실패하였습니다.").Forget();
                return false;
            }
        }
        public async UniTask<bool> RemoveLibraryFavorite(string learningTypeCode, int contentIndex)
        {
            LOG.LMS($"RemoveLibraryFavorite() | contentIndex={contentIndex}", this);

            API.Result result = await API.One.Call(
                UserData.One.Child.AccessToken,
                $"/api/v1/child/learning/library-favorites/{learningTypeCode}?contentIndex={contentIndex}",
                API.Method.Delete);

            if (result.Success)
            {
                return true;
            }
            else
            {
                SystemUI.One.ErrorPU.ShowPopup("즐겨찾기 삭제에 실패하였습니다.").Forget();
                return false;
            }
        }
        public bool IsCompleteActivityLibrary(string index)
        {
            return activityLibraryCompletes.IndexOf(index) >= 0;
        }

        // Methods : for Playground
        public async UniTask<JArray> LoadPlaygroundData()
        {
            LOG.LMS($"LoadPlaygroundData()", this);

            API.Result result = await API.One.Call(UserData.One.Child.AccessToken, $"/api/v1/child/learning/playground-progress");

            if (result.Success)
                return result.Datas;
            else
                return new JArray();
        }

        // Methods : for Attendance
        public async UniTask<JArray> DoAttendance()
        {
            LOG.LMS($"DoAttendance()", this);

            API.Result result = await API.One.Call(
            UserData.One.Child.AccessToken,
                $"/api/v1/child/attendance",
                API.Method.Put);

            if (result.Success)
            {
                UserData.One.Child.IsAttendanceFirst = result.Data.Value<bool>("isFirstToday");
                UserData.One.Child.IsGuideFirst = result.Data.Value<bool>("showHelp");
                UserData.One.Child.AttendanceContinuous = result.Data.Value<int>("continuousCount");
                return result.Data.Value<JArray>("rewardPoints");
            }
            else
            {
                SystemUI.One.ErrorPU.ShowPopup("출석 처리가 실패하였습니다.").Forget();
                return null;
            }
        }
        public async UniTask<JObject> LoadAttendanceCalendar(string currentYearMonth)
        {
            LOG.LMS($"LoadAttendanceCalendar() | searchDate={currentYearMonth}", this);

            API.Result result = await API.One.Call(UserData.One.Child.AccessToken, $"/api/v1/child/my-info/attendance-calendar?currentYearMonth={currentYearMonth}");

            if (result.Success)
                return result.Data;
            else
                return null;
        }

        // Methods : for Event
        public async UniTask<JArray> NotificationEvent()
        {
            LOG.LMS($"NotificationEvent()", this);

            string eventDeviceType = "WINDOW";
#if UNITY_ANDROID
            eventDeviceType = "ANDROID";
#elif UNITY_IOS
            eventDeviceType = "IOS";
#endif

            API.Result result = await API.One.Call(
                null,
                $"/api/v1/common/notification-event/{eventDeviceType}");

            if (result.Success)
            {
                return result.Datas;
            }
            else
            {
                SystemUI.One.ErrorPU.ShowPopup("Fail to load events").Forget();
                return null;
            }
        }

        // Methods : for Stamp
        public async UniTask<bool> SetStamp(string childLoginId, string date, StampType stampType)
        {
            LOG.LMS($"SetStamp()", this);

            API.Result result = await API.One.Call(
            UserData.One.Parent.AccessToken,
                $"/api/v1/user/child-learning/{childLoginId}/praise-stamp?date={date}&stampType={stampType}",
                API.Method.Post);

            if (result.Success)
            {
                return true;
            }
            else
            {
                SystemUI.One.ErrorPU.ShowPopup("칭찬 도장 처리가 실패하였습니다.").Forget();
                return false;
            }
        }

        // Methods : for Push Notification
        public async UniTask<bool> SavePushTokenChild(string pushToken, string platform, string deviceId, string device)
        {
            LOG.LMS($"SavePushTokenChild() | pushToken={pushToken} | platform={platform} | deviceId={deviceId} | device={device}", this);

            API.Result result = await API.One.Call(
                UserData.One.Child.AccessToken,
                $"/api/v1/child/push-token",
                API.Method.Post,
                JObject.Parse($"{{pushToken: \"{pushToken}\", platform: \"{platform}\", deviceId: \"{deviceId}\", device: \"{device}\"}}"));

            if (result.Success)
            {
                return true;
            }
            else
            {
                //SystemUI.One.ErrorPU.ShowPopup("푸시 토큰 저장에 실패하였습니다.").Forget();
                LOG.Warning($"SavePushTokenChild() Failed.", this);
                return false;
            }
        }
        public async UniTask<bool> SavePushTokenParent(string pushToken, string platform, string deviceId, string device)
        {
            LOG.LMS($"SavePushTokenParent() | pushToken={pushToken} | platform={platform} | deviceId={deviceId} | device={device}", this);

            API.Result result = await API.One.Call(
                UserData.One.Parent.AccessToken,
                $"/api/v1/user/push-token",
                API.Method.Post,
                JObject.Parse($"{{pushToken: \"{pushToken}\", platform: \"{platform}\", deviceId: \"{deviceId}\", device: \"{device}\"}}"));

            if (result.Success)
            {
                return true;
            }
            else
            {
                //SystemUI.One.ErrorPU.ShowPopup("푸시 토큰 저장에 실패하였습니다.").Forget();
                LOG.Warning($"SavePushTokenParent() Failed.", this);
                return false;
            }
        }

        // Methods : for Setting
        public async UniTask<bool> LoadSettingsChild()
        {
            LOG.LMS($"LoadSettingsChild()", this);

            API.Result result = await API.One.Call(UserData.One.Child.AccessToken, $"/api/v1/child/setting");

            if (result.Success)
            {
                var language = result.Data.Value<string>("language");
                if (!string.IsNullOrEmpty(language))
                    UserData.One.SaveLanguage(Enum.Parse<LocalizationLocale>(language));
                UserData.One.Child.IsAppNotificationMarketing = result.Data["isAppNotificationMarketing"] != null ? result.Data.Value<bool>("isAppNotificationMarketing") : true;
                UserData.One.Child.IsAppNotificationLearning = result.Data["isAppNotificationLearning"] != null ? result.Data.Value<bool>("isAppNotificationLearning") : true;
                UserData.One.Child.IsAppSoundEffect = result.Data["isAppSoundEffect"] != null ? result.Data.Value<bool>("isAppSoundEffect") : true;
                UserData.One.Child.IsAppSoundBackground = result.Data["isAppSoundBackground"] != null ? result.Data.Value<bool>("isAppSoundBackground") : true;
                UserData.One.Child.DayLimit = result.Data["isDayLimit"] != null && result.Data["isDayLimit"] != null ? result.Data.Value<bool>("isDayLimit") : true;
                return true;
            }
            else
                return false;
        }
        public async UniTask<bool> SaveSettingsChild()
        {
            LOG.LMS($"SaveSettingsChild()", this);

            string data = "{" +
                $"language: \"{LocalizationMGR.One.Locale}\"," +
                $"isAppNotificationMarketing: {UserData.One.Child.IsAppNotificationMarketing.ToString().ToLower()}," +
                $"isAppNotificationLearning: {UserData.One.Child.IsAppNotificationLearning.ToString().ToLower()}," +
                $"isAppSoundEffect: {UserData.One.Child.IsAppSoundEffect.ToString().ToLower()}," +
                $"isAppSoundBackground: {UserData.One.Child.IsAppSoundBackground.ToString().ToLower()}" +
                "}";

            API.Result result = await API.One.Call(
                UserData.One.Child.AccessToken,
                $"/api/v1/child/setting",
                API.Method.Put,
                JObject.Parse(data));

            if (result.Success)
            {
                return true;
            }
            else
            {
                SystemUI.One.ErrorPU.ShowPopup("설정 저장에 실패하였습니다.").Forget();
                return false;
            }
        }
        public async UniTask<bool> LoadSettingsParent()
        {
            LOG.LMS($"LoadSettingsParent()", this);

            API.Result result = await API.One.Call(UserData.One.Parent.AccessToken, $"/api/v1/user/setting");

            if (result.Success)
            {
                var language = result.Data.Value<string>("language");
                if (!string.IsNullOrEmpty(language))
                    UserData.One.SaveLanguage(Enum.Parse<LocalizationLocale>(language));
                UserData.One.Parent.IsAppNotificationMarketing = result.Data["isAppNotificationMarketing"] != null ? result.Data.Value<bool>("isAppNotificationMarketing") : true;
                UserData.One.Parent.IsAppNotificationLearning = result.Data["isAppNotificationLearning"] != null ? result.Data.Value<bool>("isAppNotificationLearning") : true;
                UserData.One.Parent.IsSmsReceive = result.Data["isSmsReceive"] != null ? result.Data.Value<bool>("isSmsReceive") : true;
                UserData.One.Parent.IsAppSoundEffect = result.Data["isAppSoundEffect"] != null ? result.Data.Value<bool>("isAppSoundEffect") : true;
                UserData.One.Parent.IsAppSoundBackground = result.Data["isAppSoundBackground"] != null ? result.Data.Value<bool>("isAppSoundBackground"): true;
                return true;
            }
            else
                return false;
        }
        public async UniTask<bool> SaveSettingsParent()
        {
            LOG.LMS($"SaveSettingsParent()", this);

            string data = "{" +
                $"language: \"{LocalizationMGR.One.Locale}\"," +
                $"isAppNotificationMarketing: {UserData.One.Parent.IsAppNotificationMarketing.ToString().ToLower()}," +
                $"isAppNotificationLearning: {UserData.One.Parent.IsAppNotificationLearning.ToString().ToLower()}," +
                $"isSmsReceive: {UserData.One.Parent.IsSmsReceive.ToString().ToLower()}," +
                $"isAppSoundEffect: {UserData.One.Parent.IsAppSoundEffect.ToString().ToLower()}," +
                $"isAppSoundBackground: {UserData.One.Parent.IsAppSoundBackground.ToString().ToLower()}" +
                "}";

            API.Result result = await API.One.Call(
                UserData.One.Parent.AccessToken,
                $"/api/v1/user/setting",
                API.Method.Put,
                JObject.Parse(data));

            if (result.Success)
            {
                return true;
            }
            else
            {
                SystemUI.One.ErrorPU.ShowPopup("설정 저장에 실패하였습니다.").Forget();
                return false;
            }
        }

        // Methods : for My Pet
        public async UniTask<bool> LoadMyPet()
        {
            LOG.LMS($"LoadMyPet()", this);

            API.Result result = await API.One.Call(UserData.One.Child.AccessToken, $"/api/v1/child/my-pet");

            UserData.One.ClearPets();
            if (result.Success)
            {
                for (int i = 0; i < result.Datas.Count; i++)
                {
                    var data = result.Datas[i];
                    var userPetId = data.Value<int>("userPetId");
                    var petCodeStr = data.Value<string>("petCode");
                    var petCode = Enum.Parse<PetCode>(petCodeStr);
                    var petName = data.Value<string>("petName");
                    var affection = data.Value<float>("affection");
                    var pet = new UserDataPet(userPetId, (int)petCode, affection);
                    pet.Name = petName;
                    UserData.One.Pets.Add(pet);
                }

                return true;
            }
            else
            {
                SystemUI.One.ErrorPU.ShowPopup("펫 정보를 불러오는데 실패하였습니다.").Forget();
                return false;
            }
        }
        public async UniTask<int> AddMyPet(PetCode petCode, string petName)
        {
            LOG.LMS($"AddMyPet()", this);

            API.Result result = await API.One.Call(
                UserData.One.Child.AccessToken, 
                $"/api/v1/child/my-pet", 
                API.Method.Post, 
                JObject.Parse($"{{petCode: \"{petCode}\", petName: \"{petName}\", affection: 0}}"));

            if (result.Success)
            {
                return result.Data.Value<int>("userPetId");
            }
            else
            {
                LOG.Warning($"AddMyPet() Fail.", this);
                return 0;
            }
        }
        public async UniTask<bool> ChangeMyPet(int userPetId, string petName, float affection)
        {
            LOG.LMS($"ChangeMyPet()", this);

            API.Result result = await API.One.Call(
                UserData.One.Child.AccessToken,
                $"/api/v1/child/my-pet/{userPetId}",
                API.Method.Put,
                JObject.Parse($"{{petName: \"{petName}\", affection: {affection}}}"));

            if (result.Success)
            {
                return true;
            }
            else
            {
                LOG.Warning($"ChangeMyPet() Fail.", this);
                return false;
            }
        }
        public async UniTask<bool> RemoveMyPet(int userPetId)
        {
            LOG.LMS($"RemoveMyPet()", this);

            API.Result result = await API.One.Call(
                UserData.One.Child.AccessToken,
                $"/api/v1/child/my-pet/{userPetId}",
                API.Method.Delete);

            if (result.Success)
            {
                return true;
            }
            else
            {
                LOG.Warning($"RemoveMyPet() Fail.", this);
                return false;
            }
        }
        public async UniTask<bool> LoadPetBook()
        {
            LOG.LMS($"LoadPetBook()", this);

            API.Result result = await API.One.Call(UserData.One.Child.AccessToken, $"/api/v1/child/my-pet/book-list");

            for (int i=0; i< UserData.One.PetBooks.Length; i++)
            {
                for (int j=0; j < UserData.One.PetBooks[i].Length; j++)
                {
                    UserData.One.PetBooks[i][j] = false;
                }
            }
            if (result.Success)
            {
                for (int i = 0; i < result.Datas.Count; i++)
                {
                    var data = result.Datas[i];
                    var petCodeStr = data.Value<string>("petCode");
                    var petCode = Enum.Parse<PetCode>(petCodeStr);
                    var affection = data.Value<float>("affection");
                    int idxLevel = Mathf.Min((int)Mathf.Floor(affection), 2);
                    for (int j = 0; j < idxLevel; j++)
                    {
                        UserData.One.PetBooks[(int)petCode][j] = true;
                    }
                }

                return true;
            }
            else
            {
                SystemUI.One.ErrorPU.ShowPopup("펫 북을 불러오는데 실패하였습니다.").Forget();
                return false;
            }
        }
        public async UniTask<bool> ValidatePetName(string name)
        {
            LOG.LMS($"ValidatePetName() | name={name}", this);

            API.Result result = await API.One.Call(
                UserData.One.Child.AccessToken,
                $"/api/v1/child/my-pet/validate-pet-name?petName={name}",
                API.Method.Post);

            if (result.Success)
            {
                return true;
            }
            else
            {
                LOG.Warning($"ValidateChildID failed: {result.Data.Value<string>("message")}", this);
                return false;
            }
        }

        // Methods : for Candy
        public async UniTask<int> LoadCandy()
        {
            LOG.LMS($"LoadCandy()", this);

            API.Result result = await API.One.Call(UserData.One.Child.AccessToken, $"/api/v1/child/candy");

            if (result.Success)
            {
                candy = result.Data.Value<int>("candy");
            }
            else
            {
                SystemUI.One.ErrorPU.ShowPopup("캔디 정보를 불러오는데 실패하였습니다.").Forget();
                LOG.Warning($"LoadCandy failed: {result.Data.Value<string>("message")}", this);
                candy = 0;
            }
            OnChangePlaygroundPlayCount?.Invoke(candy);
            return candy;
        }
        public async UniTask<bool> GetCandy()
        {
            LOG.LMS($"GetCandy()", this);

            int amount = 1;
            API.Result result = await API.One.Call(
                UserData.One.Child.AccessToken,
                $"/api/v1/child/candy",
                API.Method.Post,
                JObject.Parse($"{{candy: \"{amount}\"}}"));

            if (result.Success)
            {
                await LoadCandy();
                return true;
            }
            else
            {
                LOG.Warning($"GetCandy failed: {result.Data.Value<string>("message")}", this);
                return false;
            }
        }
        public async UniTask<bool> UseCandy()
        {
            LOG.LMS($"GetCandy()", this);

            int amount = -1;
            API.Result result = await API.One.Call(
                UserData.One.Child.AccessToken,
                $"/api/v1/child/candy",
                API.Method.Post,
                JObject.Parse($"{{candy: \"{amount}\"}}"));

            if (result.Success)
            {
                await LoadCandy();
                return true;
            }
            else
            {
                LOG.Warning($"UseCandy failed: {result.Data.Value<string>("message")}", this);
                return false;
            }
        }

        // Methods : for Child Ticket
        public async UniTask<JObject> LoadTicketInfo()
        {
            LOG.LMS($"LoadTicketInfo()", this);

            API.Result result = await API.One.Call(UserData.One.Child.AccessToken, $"/api/v1/child/ticket");

            if (result.Success)
            {
                return result.Data;
            }
            return new JObject();
        }
        public async UniTask<bool> CheckAvaliable()
        {
            LOG.LMS($"CheckAvaliable()", this);

            var data = await LoadTicketInfo();

            return data.Value<bool>("available");
        }

        // Methods : for Child to Parent
        public async UniTask<JToken> SignInParentFromChild()
        {
            LOG.LMS($"SignInParentFromChild()", this);

            API.Result result = await API.One.Call(UserData.One.Child.AccessToken, $"/api/v1/child/mypage/parent-login");

            if (result.Success)
                return result.Data;
            else
                return null;
        }

        // Methods : for Parent
        public async UniTask<bool> SignOutParent()
        {
            LOG.LMS($"SignOutParent()", this);

            StopParentCheckAuthStatus();

            API.Result result = await API.One.Call(UserData.One.Parent.AccessToken, $"/api/v1/user/logout");

            if (result.Success)
                return true;
            else
                return false;
        }
        public async UniTask<bool> LoadParentProfile()
        {
            LOG.LMS($"LoadParentProfile()", this);

            API.Result result = await API.One.Call(UserData.One.Parent.AccessToken, $"/api/v1/user/mypage/profile");

            if (result.Success)
            {
                UserData.One.Parent.LoginID = result.Data.Value<string>("loginId");
                UserData.One.Parent.CreatedDateTime = result.Data.Value<DateTime>("createdDatetime");
                return true;
            }
            return false;
        }
        public async UniTask<bool> LoadUserTicket()
        {
            LOG.LMS($"LoadUserTicket()", this);

            API.Result result = await API.One.Call(UserData.One.Parent.AccessToken, $"/api/v1/user/ticket");

            if (result.Success)
            {
                UserData.One.Parent.VoucherStartDate = result.Data.Value<DateTime>("startDate");
                UserData.One.Parent.VoucherEndDate = result.Data.Value<DateTime>("endDate");
                UserData.One.Parent.VoucherDays = result.Data.Value<int>("remainingDays");
                UserData.One.Parent.VoucherAvailable = result.Data.Value<bool>("available");
                return true;
            }
            return false;
        }
        public async UniTask<bool> SaveUserTicket(int productId, string transactionId, int quantity = 1, string orderId = "")
        {
            LOG.LMS($"SaveUserTicket() | productId={productId} | transactionId={transactionId}", this);

            API.Result result = await API.One.Call(
                UserData.One.Parent.AccessToken,
                $"/api/v1/user/ticket",
                API.Method.Post,
                JObject.Parse($"{{productId: \"{productId}\", transactionId: \"{transactionId}\", quantity: {quantity}, orderId: \"{orderId}\"}}"));

            if (result.Success)
            {
                await LoadUserTicket();
                return true;
            }
            else
            {
                LOG.Warning($"SaveUserTicket failed: {result.Data.Value<string>("message")}", this);
                return false;
            }
        }
        public async UniTask<bool> CheckNew(string childLoginId, int logSn)
        {
            LOG.LMS($"CheckNew() | childLoginId={childLoginId} | logSn ={logSn}", this);

            API.Result result = await API.One.Call(
                UserData.One.Parent.AccessToken,
                $"/api/v1/user/child-learning/day/{childLoginId}/check-new/{logSn}",
                API.Method.Put);

            if (result.Success)
            {
                return true;
            }
            else
            {
                var status = result.Data.Value<int>("status");
                if (status == 404)
                    LOG.Warning($"CheckNew failed: {result.Data.Value<string>("message")}", this);
                else
                    SystemUI.One.ErrorPU.ShowPopup("Failed to check new.").Forget();
                return false;
            }
        }
        public async UniTask<bool> CheckNew(int logSn)
        {
            LOG.LMS($"CheckNew() | logSn={logSn}", this);

            if (logSn == 0)
                return false;

            API.Result result = await API.One.Call(
                UserData.One.Child.AccessToken,
                $"/api/v1/child/my-info/today/check-new/{logSn}",
                API.Method.Put);

            if (result.Success)
            {
                return true;
            }
            else
            {
                SystemUI.One.ErrorPU.ShowPopup("Failed to check new.").Forget();
                return false;
            }
        }
        public async UniTask<bool> ValidateChildID(string id)
        {
            LOG.LMS($"ValidateChildID() | id={id}", this);

            API.Result result = await API.One.Call(
                UserData.One.Parent.AccessToken,
                $"/api/v1/user/children/validate-loginid?loginId={id}",
                API.Method.Post);

            if (result.Success)
            {
                return true;
            }
            else
            {
                LOG.Warning($"ValidateChildID failed: {result.Data.Value<string>("message")}", this);
                return false;
            }
        }
        public async UniTask<API.Result> SignInChildFromParent(string childLoginId)
        {
            LOG.LMS($"SignInChildFromParent() | childLoginId={childLoginId}", this);

            API.Result result = await API.One.Call(
                UserData.One.Parent.AccessToken, 
                $"/api/v1/user/children/{childLoginId}/login");

            if (result.Success)
            {
                UserData.One.Child.ID = result.Data.Value<string>("loginId");
                UserData.One.Child.Password = "";
                UserData.One.Child.AutoSignIn = false;
                UserData.One.Child.AccessToken = result.Data.Value<string>("accessToken");
                UserData.One.Child.NickName = result.Data.Value<string>("nickName");
                UserData.One.Child.Course = result.Data.Value<int>("learningCourse");
                UserData.One.Child.ParentID = result.Data.Value<string>("parentLoginId");
                UserData.One.Child.ParentSignupType = result.Data.Value<string>("parentSignupType");

                UserData.One.SaveLocalData();

                StartChildCheckAuthStatus().Forget();

                LOG.LMS($"SignIn Success : {UserData.One.Child}", this);
            }
            else
            {
                LOG.Warning($"SignIn error: {result.Data.Value<string>("code")}", this);
            }

            return result;
        }
        public async UniTask<JArray> LoadFamily()
        {
            LOG.LMS($"LoadFamily()", this);

            API.Result result = await API.One.Call(UserData.One.Parent.AccessToken, $"/api/v1/user/family");

            if (result.Success)
            {
                if (result.Datas.Count > 0)
                {
                    UserData.One.Parent.FamilyID = result.Datas[0].Value<string>("familyLoginId");
                    UserData.One.Parent.FamilyNickName = result.Datas[0].Value<string>("familyNickName");
                    UserData.One.Parent.FamilyStatus = result.Datas[0].Value<string>("familyStatus");
                    UserData.One.Parent.FamilyManager = result.Datas[0].Value<bool>("isManager");
                }
                else
                {
                    UserData.One.Parent.FamilyID = "";
                    UserData.One.Parent.FamilyNickName = "";
                    UserData.One.Parent.FamilyStatus = "";
                    UserData.One.Parent.FamilyManager = false;
                }
                return result.Datas;
            }
            else
            {
                SystemUI.One.ErrorPU.ShowPopup("Fail to load family.").Forget();
                return null;
            }
        }
        public async UniTask<bool> SaveFamily(string nickName)
        {
            LOG.LMS($"SaveFamily() | nickName={nickName}", this);

            API.Result result = await API.One.Call(
                UserData.One.Parent.AccessToken,
                $"/api/v1/user/family?nickName={nickName}",
                API.Method.Post);

            if (result.Success)
            {
                UserData.One.Parent.FamilyID = result.Data.Value<string>("familyLoginId");
                UserData.One.Parent.FamilyNickName = result.Data.Value<string>("familyNickName");
                UserData.One.Parent.FamilyStatus = "WAITING";
                UserData.One.Parent.FamilyManager = false;
                return true;
            }
            else
            {
                if (result.Data.Value<int>("status") == 409)
                    SystemUI.One.ShowPopupCannotAddFamily();
                else
                    SystemUI.One.ShowErrorAddFamily();
                return false;
            }
        }
        public async UniTask<bool> RemoveFamily(string nickName)
        {
            LOG.LMS($"RemoveFamily() | nickName={nickName}", this);

            API.Result result = await API.One.Call(
                UserData.One.Parent.AccessToken,
                $"/api/v1/user/family?nickName={nickName}",
                API.Method.Delete);

            if (result.Success)
            {
                UserData.One.Parent.FamilyID = "";
                UserData.One.Parent.FamilyNickName = "";
                UserData.One.Parent.FamilyStatus = "";
                UserData.One.Parent.FamilyManager = false;
                return true;
            }
            else
            {
                SystemUI.One.ShowErrorRemoveFamily();
                return false;
            }
        }
        public async UniTask<JToken> ValidateFamilyNickName(string nickName)
        {
            LOG.LMS($"ValidateFamilyNickName() | nickName={nickName}", this);

            API.Result result = await API.One.Call(
                UserData.One.Parent.AccessToken,
                $"/api/v1/user/family/validate-nickname?nickName={nickName}",
                API.Method.Post);

            return result.Data;
        }

        // Methods : for Photo
        public async UniTask<bool> SavePhotoParent(UserDataChild childData, Texture2D texture)
        {
            LOG.LMS($"SavePhotoParent() | childLoginId={childData.ID} | texture={texture}", this);

            childData.Photo = texture;

            var result = await API.One.Upload(
                    UserData.One.Parent.AccessToken,
                    $"/api/v1/user/children/{childData.ID}/photo",
                    API.Method.Post, texture.EncodeToPNG());

            if (!result)
                await SystemUI.One.ErrorPU.ShowPopup("Failed to save photo.");

            return result;
        }
        public async UniTask<Texture2D> LoadPhotoParent(UserDataChild childData)
        {
            LOG.LMS($"LoadPhotoParent() | childLoginId={childData.ID}", this);

            childData.Photo = await API.One.DownloadImage(UserData.One.Parent.AccessToken, $"/api/v1/user/children/{childData.ID}/photo");
            return childData.Photo;
        }
        public async UniTask<bool> SavePhotoChild(Texture2D texture)
        {
            LOG.LMS($"SavePhotoChild() | texture={texture}", this);

            UserData.One.Child.Photo = texture;

            var result = await API.One.Upload(
                    UserData.One.Child.AccessToken,
                    $"/api/v1/child/mypage/photo",
                    API.Method.Post, texture.EncodeToPNG());

            if (!result)
                await SystemUI.One.ErrorPU.ShowPopup("Failed to save photo.");

            return result;
        }
        public async UniTask<Texture2D> LoadPhotoChild()
        {
            LOG.LMS($"LoadPhotoChild()", this);

            UserData.One.Child.Photo = await API.One.DownloadImage(UserData.One.Child.AccessToken, $"/api/v1/child/mypage/photo");
            return UserData.One.Child.Photo;
        }

        // Methods : for Mail
        public async UniTask<JObject> LoadMails(string token)
        {
            LOG.LMS($"LoadMails()", this);

            API.Result result = await API.One.Call(token, $"/api/v1/common/mailbox");

            if (result.Success)
            {
                return result.Data;
            }
            else
            {
                SystemUI.One.ErrorPU.ShowPopup("Fail to load mails").Forget();
                return null;
            }
        }
        public async UniTask<JObject> LoadMail(string token, int sn)
        {
            LOG.LMS($"LoadMail() | sn={sn}", this);

            API.Result result = await API.One.Call(token, $"/api/v1/common/mailbox/{sn}");

            if (result.Success)
            {
                return result.Data;
            }
            else
            {
                SystemUI.One.ErrorPU.ShowPopup("Fail to load mails").Forget();
                return null;
            }
        }
        public async UniTask<bool> RemoveMail(string token, int sn)
        {
            LOG.LMS($"RemoveMail() | sn={sn}", this);

            API.Result result = await API.One.Call(token, $"/api/v1/common/mailbox/{sn}", API.Method.Delete);

            if (result.Success)
            {
                return true;
            }
            else
            {
                SystemUI.One.ErrorPU.ShowPopup("Faild to delete a mail.").Forget();
                return false;
            }
        }
        public async UniTask<bool> RemoveAllMails(string token)
        {
            LOG.LMS($"RemoveMailsAll()", this);

            API.Result result = await API.One.Call(token, $"/api/v1/common/mailbox/all", API.Method.Delete);

            if (result.Success)
            {
                return true;
            }
            else
            {
                SystemUI.One.ErrorPU.ShowPopup("Faild to delete all mails.").Forget();
                return false;
            }
        }

        // Events
        public event Action<int> OnChangeCoin;
        public event Action<int> OnChangePlaygroundPlayCount;



        // Fields
        private int coin = 0;
        private int candy = 0;
        private string[] playgroundCodes = {};
        private int[] playgroundScores;
        private List<string> activityLibraryCompletes = new();
        private bool nowCheckAuthStatusChild = false;
        private bool nowCheckAuthStatusParent = false;



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private int checkAuthStatusTime = 30000;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            Coin = 0;
        }
    }

    public static class LearningTypeCode
    {
        public static string Movie =    "010";
        public static string eBook =    "020";
        public static string AIStudio = "030";
        public static string Activity = "040";
    }
}