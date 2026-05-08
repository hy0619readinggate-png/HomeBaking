using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DoDoEng.Common
{
    public class LocalizationWindow : EditorWindow
    {
        //[MenuItem("DoDoEng/Localization")]
        //public static void Open()
        //{
        //    var window = GetWindow<LocalizationWindow>();
        //    window.titleContent = new GUIContent("Localization");
        //}

        //private void OnGUI()
        //{
        //    if (GUILayout.Button("Refresh"))
        //    {
        //        LOG.Info($"Refresh()", this);
        //    }
        //}

        [MenuItem("DoDoEng/Localization/Refresh", false, 1)]
        private static void Refresh()
        {
            LOG.Info<LocalizationWindow>($"Refresh()");
            AssetDatabase.Refresh();
            Startup.InitializeOnLoadMethod();
        }
        [MenuItem("DoDoEng/Localization/Korean", false, 12)]
        private static void Korean()
        {
            LOG.Info<LocalizationWindow>($"Korean()");

            Menu.SetChecked("DoDoEng/Localization/Korean", true);
            Menu.SetChecked("DoDoEng/Localization/English", false);
            Menu.SetChecked("DoDoEng/Localization/TiengViet", false);

            LocalizationMGR.One.Locale = LocalizationLocale.ko;

            EditorUtility.SetDirty(GameObject.FindObjectOfType<Transform>());

        }
        [MenuItem("DoDoEng/Localization/English", false, 13)]
        private static void English()
        {
            LOG.Info<LocalizationWindow>($"English()");

            Menu.SetChecked("DoDoEng/Localization/Korean", false);
            Menu.SetChecked("DoDoEng/Localization/English", true);
            Menu.SetChecked("DoDoEng/Localization/TiengViet", false);

            LocalizationMGR.One.Locale = LocalizationLocale.en;

            EditorUtility.SetDirty(GameObject.FindObjectOfType<Transform>());

        }
        [MenuItem("DoDoEng/Localization/TiengViet", false, 14)]
        private static void TiengViet()
        {
            LOG.Info<LocalizationWindow>($"English()");

            Menu.SetChecked("DoDoEng/Localization/Korean", false);
            Menu.SetChecked("DoDoEng/Localization/English", false);
            Menu.SetChecked("DoDoEng/Localization/TiengViet", true);

            LocalizationMGR.One.Locale = LocalizationLocale.vi;
        }
    }
}