using Oddworm.Framework;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Assertions;

namespace DoDoEng
{
    public class BeyondiBuilder : IPostprocessBuildWithReport
    {
        // Definitions
        const string APPNAME = "HiDODO";
        const string TARGET_PATH = "./Build";
        const string ADDRESSABLE_PROFILE = "Default";
        const string ADDRESSABLE_BUILD_SCRIPT = "Assets/AddressableAssetsData/DataBuilders/BuildScriptPackedMode.asset";
        const string ADDRESSABLE_ASSET_APP_VERSION = "ABAppVersion";

        // Methods - Build
        [MenuItem("DoDoEng/Build/BuildPlayer - Current Platform (Production)", false, 1)]
        static void BuildPlayerProduction()
        {
            buildPlayer(true);
        }
        [MenuItem("DoDoEng/Build/BuildPlayer - Current Platform", false, 2)]
        static void BuildPlayer()
        {
            buildPlayer(false);
        }
        [MenuItem("DoDoEng/Build/BuildAddressables - Current Platform", false, 3)]
        static void BuildAddressables()
        {
            // 커맨드라인에서 정보 가져오기
            CommandLine.Init(Environment.CommandLine);
            Debug.Log($"CommandLine.text : {CommandLine.text}");

            var buildTarget = CommandLine.GetEnum("-buildTarget", EditorUserBuildSettings.activeBuildTarget);
            var buildNumber = CommandLine.GetString("-buildNumber", "None");
            Debug.Log($"buildTarget : {buildTarget}");
            Debug.Log($"buildNumber : {buildNumber}");

            var targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
            Debug.Log($"targetGroup : {targetGroup}");

            // 빌드 타켓 변경
            EditorUserBuildSettings.SwitchActiveBuildTarget(targetGroup, buildTarget);

            // 어드레서블 빌드
            buildAddressables();
        }



        // Functions - util
        private static string getPlayerBuildPath(BuildTarget buildTarget)
        {
            var folder = $"{TARGET_PATH}/{buildTarget}";

            return buildTarget switch
            {
                BuildTarget.StandaloneWindows64 => $"{folder}/{Application.version}/{APPNAME}.exe",
                BuildTarget.Android => $"{folder}/{APPNAME}-{PlayerSettings.Android.bundleVersionCode}({Application.version}).aab",
                BuildTarget.iOS => $"{folder}/{APPNAME}-{PlayerSettings.iOS.buildNumber}({Application.version})",
                _ => string.Empty
            };
        }
        private static void clearFolder(string path)
        {
            if (Directory.Exists(path))
            {
                var di = new DirectoryInfo(path);
                var files = di.GetFiles("*.*", SearchOption.AllDirectories);
                foreach (var file in files)
                    file.Attributes = FileAttributes.Normal;

                Directory.Delete(path, true);
            }
        }
        private static void injectBuildNumber(string buildNumber)
        {
            var path = "Assets/StreamingAssets/BuildNumber.txt";
            using (var writer = new StreamWriter(path))
                writer.Write(buildNumber);
        }

        // Functions - build
        private static void buildPlayer(bool isProduction)
        {
            // 초기 리프레쉬
            AssetDatabase.Refresh();

            // 커맨드라인에서 정보 가져오기
            CommandLine.Init(Environment.CommandLine);
            Debug.Log($"CommandLine.text : {CommandLine.text}");

            var buildTarget = CommandLine.GetEnum("-buildTarget", EditorUserBuildSettings.activeBuildTarget);
            var buildNumber = CommandLine.GetString("-buildNumber", "Not Specified.");
            Debug.Log($"buildTarget : {buildTarget}");
            Debug.Log($"buildNumber : {buildNumber}");

            // 폴더 설정
            var targetPath = getPlayerBuildPath(buildTarget);
            var targetDir = Path.GetDirectoryName(targetPath);
            targetDir = targetDir.Replace(@"\", "/");
            Debug.Log($"targetPath : {targetPath}");
            Debug.Log($"targetDir : {targetDir}");

            if (buildTarget == BuildTarget.StandaloneWindows64 ||
                buildTarget == BuildTarget.iOS)
                clearFolder(targetDir);

            // 윈도우 빌드인 경우에만 빌드경로 설정 필요
            if (buildTarget == BuildTarget.StandaloneWindows64)
                EditorUserBuildSettings.SetBuildLocation(BuildTarget.StandaloneWindows64, targetDir);

            // 배치모드인 경우, 빌드번호를 프로젝트에 추가
            if (Application.isBatchMode)
                injectBuildNumber(buildNumber);

            // 플레이어 빌드 실행
            buildPlayer(buildTarget, targetPath, isProduction);
        }
        private static void buildPlayer(BuildTarget target, string fileName, bool isProduction)
        {
            // Set addressable profile
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var profileId = settings.profileSettings.GetProfileId(ADDRESSABLE_PROFILE);
            if (string.IsNullOrEmpty(profileId))
            {
                Debug.LogError($"Couldn't find a profile named, {ADDRESSABLE_PROFILE}");
                return;
            }
            settings.profileSettings.SetValue(profileId, ADDRESSABLE_ASSET_APP_VERSION, Application.version);

            // Set player build option
            var activeScenes = EditorBuildSettings.scenes
             .Where(scene => scene.enabled)
             .Select(scene => scene.path)
             .ToArray();

            if (target == BuildTarget.Android)
            {
                EditorUserBuildSettings.buildAppBundle = true;
                EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Public;
            }

            var buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = activeScenes;
            buildPlayerOptions.locationPathName = fileName;
            buildPlayerOptions.target = target;
            if (isProduction)
                buildPlayerOptions.options = BuildOptions.CleanBuildCache;
            else buildPlayerOptions.options = BuildOptions.CleanBuildCache | BuildOptions.Development | BuildOptions.AllowDebugging;

            // Build
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            var summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[Build] Build Success");
                Debug.Log($"Time : {summary.totalTime}  Size : {summary.totalSize}bytes");
            }
            else
            {
                Debug.Log("[Build] Build Failed");
                Debug.Log($"Time : {summary.totalTime} Total Errors : {summary.totalErrors}");

                // https://support.unity.com/hc/en-us/articles/211195263-Why-doesn-t-a-failed-BuildPipeline-BuildPlayer-return-an-error-code-in-the-command-line-
                if (Application.isBatchMode)
                    throw new Exception(report.summary.ToString());
            }


            Debug.Log($"report : {report.summary}");
        }
        private static void buildAddressables()
        {
            // Set profile
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var profileId = settings.profileSettings.GetProfileId(ADDRESSABLE_PROFILE);
            if (string.IsNullOrEmpty(profileId))
            {
                Debug.LogError($"Couldn't find a profile named, {ADDRESSABLE_PROFILE}");
                return;
            }
            settings.profileSettings.SetValue(profileId, ADDRESSABLE_ASSET_APP_VERSION, Application.version);
            settings.activeProfileId = profileId;

            // Get Builder
            var builder = AssetDatabase.LoadAssetAtPath<ScriptableObject>(ADDRESSABLE_BUILD_SCRIPT) as IDataBuilder;
            if (builder == null)
            {
                Debug.LogError($"{ADDRESSABLE_BUILD_SCRIPT} couldn't be found or isn't a build script.");
                return;
            }

            // Set Builder
            var index = settings.DataBuilders.IndexOf((ScriptableObject)builder);
            if (index <= 0)
            {
                Debug.LogError($"{builder} must be added to the " +
                               $"DataBuilders list before it can be made active.");
                return;
            }
            settings.ActivePlayerDataBuilderIndex = index;

            // Build
            AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
            if (!string.IsNullOrEmpty(result.Error))
            {
                Debug.LogError($"Addressables build error encountered: {result.Error}");
                return;
            }
        }



        // Implementation - IPostprocessBuildWithReport
        public int callbackOrder => 0;
        public void OnPostprocessBuild(BuildReport report)
        {
            try
            {
                var appName = Path.GetFileNameWithoutExtension(report.summary.outputPath);
                var appFolder = Path.GetDirectoryName(report.summary.outputPath);
                Assert.IsNotNull(appFolder);

                appFolder = Path.GetFullPath(appFolder);

                //Delete Burst Debug Folder
                var path1 = Path.Combine(appFolder, $"{appName}_BurstDebugInformation_DoNotShip");
                if (Directory.Exists(path1))
                {
                    Debug.Log($" > Deleting : {path1}");
                    Directory.Delete(path1, true);
                }

                //Delete il2cpp Debug Folder
                var path2 = Path.Combine(appFolder, $"{appName}_BackUpThisFolder_ButDontShipItWithYourGame");
                if (Directory.Exists(path2))
                {
                    Debug.Log($" > Deleting : {path2}");
                    Directory.Delete(path2, true);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"ERROR : {e}");
            }
        }
    }
}