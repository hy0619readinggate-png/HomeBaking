using System;
using UnityEditor;
using UnityEngine;


#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using UnityEditor.Callbacks;



namespace DoDoEng
{
    public static class iOS_XcodeProject
    {
#if UNITY_IOS
        // iOS 아카이브 프레임워크 관련 에러를 없애기 위한 방법
        // UnityFramework.framework contains disallowed file 'Frameworks'. Getting this error while getting Unity IOS build
        // https://stackoverflow.com/a/76065241
        [PostProcessBuild(Int32.MaxValue)]
        public static void OnPostProcessBuild1(BuildTarget buildTarget, string buildPath)
        {
            if (buildTarget != BuildTarget.iOS) return;

            var projectPath = buildPath + "/Unity-iPhone.xcodeproj/project.pbxproj";
            var pbxProject = new PBXProject();
            pbxProject.ReadFromFile(projectPath);

            var target = pbxProject.GetUnityFrameworkTargetGuid();
            pbxProject.SetBuildProperty(target, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
            pbxProject.WriteToFile(projectPath);
        }

        // 수출 규정 문서 누락 경고 없애기 위해
        // https://booiljung.github.io/technical_articles/unity3d/unity3d_ios_info_plist_script.html
        [PostProcessBuild(Int32.MaxValue - 1)]
        public static void OnPostProcessBuild2(BuildTarget buildTarget, string buildPath)
        {
            var infoPlistPath = buildPath + "/Info.plist";

            var plistDoc = new PlistDocument();
            plistDoc.ReadFromFile(infoPlistPath);
            if (plistDoc.root != null)
            {
                plistDoc.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);
                plistDoc.WriteToFile(infoPlistPath);
            }
        }

        // Push(FCM) 관련 설정
        // https://firebase.google.com/docs/cloud-messaging/unity/client?hl=ko#enable_push_notifications_on_apple_platforms
        // https://discussions.unity.com/t/how-do-i-enable-remote-push-notification-capability-using-unity-cloud-build/656304/12
        [PostProcessBuild(Int32.MaxValue - 2)]
        public static void OnPostProcessBuild3(BuildTarget buildTarget, string buildPath)
        {
            if (buildTarget != BuildTarget.iOS) return;

            var pbxPath = PBXProject.GetPBXProjectPath(buildPath);
            var pbxProj = new PBXProject();
            pbxProj.ReadFromFile(pbxPath);
            
            var guid = pbxProj.GetUnityMainTargetGuid();

            var idArray = Application.identifier.Split('.');
            var entitlementsPath = $"Unity-iPhone/{idArray[idArray.Length - 1]}.entitlements";

            var capManager = new ProjectCapabilityManager(pbxPath, entitlementsPath, null, guid);
            capManager.AddPushNotifications(true);
            capManager.AddBackgroundModes(BackgroundModesOptions.RemoteNotifications);
            capManager.AddBackgroundModes(BackgroundModesOptions.BackgroundFetch);
            capManager.WriteToFile();
        }
#endif
    }
}
