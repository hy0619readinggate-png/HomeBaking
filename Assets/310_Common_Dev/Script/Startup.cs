//#define TEST_MODE

using UnityEngine;
using DoDoEng.Launcher;
using Cysharp.Threading.Tasks;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DoDoEng.Common
{
    public class Startup : MonoBehaviour
    {
        // Definition
        private const string PrefabPath_AddressableMGR = "Singleton/AddressableMGR";
        private const string PrefabPath_AffordanceMGR = "Singleton/AffordanceMGR";
        private const string PrefabPath_AudioMGR = "Singleton/AudioMGR";
        private const string PrefabPath_LMS = "Singleton/LMS";
        private const string PrefabPath_SceneLoader = "Singleton/SceneLoader";
        private const string PrefabPath_SystemUI = "Singleton/SystemUI";
        private const string PrefabPath_UserData = "Singleton/UserData";
        private const string PrefabPath_SettingData = "Singleton/SettingData";
        private const string PrefabPath_FirebaseMGR = "Singleton/FirebaseMGR";
        private const string PrefabPath_AutoQuitMGR = "Singleton/AutoQuitMGR";
        private const string PrefabPath_InAppPurchaseMGR = "Singleton/InAppPurchaseMGR";
        private const string PrefabPath_DevicePermissionMGR = "Singleton/DevicePermissionMGR";

        // Methods
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#endif
        public static void InitializeOnLoadMethod()
        {
            LOG.Info<Startup>($"InitializeOnLoadMethod()");

            LocalizationMGR.One.LoadLanguageTable();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            LOG.Info<Startup>($"Initialize()");

            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            createSingleton(PrefabPath_AddressableMGR);
            createSingleton(PrefabPath_AffordanceMGR);
            createSingleton(PrefabPath_AudioMGR);
            createSingleton(PrefabPath_LMS);
            createSingleton(PrefabPath_SceneLoader);
            createSingleton(PrefabPath_SystemUI);
            createSingleton(PrefabPath_UserData);
            createSingleton(PrefabPath_SettingData);
            createSingleton(PrefabPath_AutoQuitMGR);
            createSingleton(PrefabPath_InAppPurchaseMGR);
            createSingleton(PrefabPath_DevicePermissionMGR);

#if !UNITY_EDITOR
            InitializeOnLoadMethod();
#endif
#if TEST_MODE
            SystemUI.One.TestServerModeMarker = true;
            API.One.SwitchTo(APIServer.Test);
            AddressableMGR.One.SwitchTo(HostServerType.Test, false);
            AddressableMGR.One.UpdateCatalog().Forget();
#endif
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeAfterSceneLoad()
        {
            LOG.Info<Startup>($"InitializeAfterSceneLoad()");


#if !PLATFORM_STANDALONE_WIN
            createSingleton(PrefabPath_FirebaseMGR);
#endif

        }



        // Functions
        private static void createSingleton(string path)
        {
            LOG.Info<Startup>($"createSingleton() | {path}");

            var pb = Resources.Load<GameObject>(path);
            var go = Instantiate(pb);
            DontDestroyOnLoad(go);
        }
    }
}