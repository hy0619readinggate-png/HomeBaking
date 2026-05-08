using beyondi.Behaviour;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Common
{
    public sealed class UserData : BYDSingleton<UserData>
    {
        // Definitions
        public enum LauncherPage
        {
            Lobby,
            TodayStudy
        }

        private const string PP_LANGUAGE = "LANGUAGE";
        private const string PP_EVENT_NO_AGAIN = "PP_EVENT_NO_AGAIN";

        // Properties
        public UserDataParent Parent => parent;
        public UserDataChild Child => child;
        public List<UserDataPet> Pets => pets;
        public bool[][] PetBooks => petBooks;
        public bool Launched { get; set; }
        public int LastTodayStudyStage { get; set; }
        public int LastTodayStudyDay { get; set; }
        public int LastTodayStudyOrder { get; set; }
        public string LastActivityLibraryThumbnailPath { get; set; } = "";
        public int LastActivityLibraryMainCategoryId { get; set; } = 0;
        public int LastActivityLibrarySubCategoryId { get; set; } = 0;
        public Library.Framework.LibraryBase.PageType LastLibraryPageType { get; set; } = Library.Framework.LibraryBase.PageType.Category;
        public int LastLibraryMainCategory { get; set; } = 0;
        public int LastLibrarySubCategory { get; set; } = 0;
        public float LastLibraryMainScroll { get; set; } = 1;
        public float LastLibrarySubScroll { get; set; } = 0;
        public float LastLibraryPageScroll { get; set; } = 1;
        public bool IsReportContents { get; set; } = false;
        public int LastReportChild { get; set; } = 0;
        public int LastReportPage { get; set; } = 0;
        public DateTime LastReportDate { get; set; } = DateTime.Today;
        public bool SetAppInitialization { get; set; } = false;

        // Methods
        public void SaveLocalData()
        {
            LOG.Function(this);

            string path = Application.persistentDataPath + "/userdata.json";
            string jsonStr = JsonUtility.ToJson(Child);
            File.WriteAllText(path, jsonStr);
            LOG.Info($"SaveLocalData() complete : {path}", this);
        }
        public bool LoadLocalData()
        {
            LOG.Function(this);

            string path = Application.persistentDataPath + "/userdata.json";

            if (File.Exists(path) == false)
            {
                LOG.Info("there is no saved file.", this);
                return false;
            }

            string fileStr = File.ReadAllText(path);
            UserDataChild data = JsonUtility.FromJson<UserDataChild>(fileStr);
            if (data != null)
            {
                data.AccessToken = "";
                child = data;
            }

            LOG.Info($"LoadLocalData() complete : {path}", this);

            return true;
        }
        public bool HasLanguage()
        {
            var language = PlayerPrefs.GetString(PP_LANGUAGE, "");
            return !string.IsNullOrEmpty(language);
        }
        public void LoadLanguage()
        {
            LOG.Function(this);

            var language = PlayerPrefs.GetString(PP_LANGUAGE, "ko");
            LocalizationMGR.One.Locale = Enum.Parse<LocalizationLocale>(language);
        }
        public void SaveLanguage(LocalizationLocale locale)
        {
            LOG.Function(this, $"| locale={locale}");

            LocalizationMGR.One.Locale = locale;
            PlayerPrefs.SetString(PP_LANGUAGE, locale.ToString());
        }
        public void ClearPets()
        {
            pets = new List<UserDataPet>();
        }
        public async UniTask CheckEvents()
        {
            if (hasCheckedEvents)
                return;

            var events = await LMS.One.NotificationEvent();
            loadWatchedEventToday();
            foreach (var data in events)
            {
                var sn = data.Value<int>("sn");
                if (todaysWatchedEvents.IndexOf(sn.ToString()) != -1)
                {
                    LOG.Function(this, $"watched event. sn={sn}");
                }
                else
                {
                    var title = data.Value<string>("title");
                    var imageUrl = data.Value<string>("imageUrl");
                    var moreUrl = data.Value<string>("moreUrl");
                    await SystemUI.One.EventPU.ShowPopup(title, imageUrl, moreUrl);
                    if (SystemUI.One.EventPU.IsNoWatchAgain)
                        todaysWatchedEvents.Add(sn.ToString());
                }
            }
            saveWatchedEventToday();

            hasCheckedEvents = true;
        }



        // Funcitons
        private void loadWatchedEventToday()
        {
            var today = DateTime.Today.ToString("yyMMdd");
            var loadedString = PlayerPrefs.GetString(PP_EVENT_NO_AGAIN, $"{today};");
            LOG.Function(this, $"| loadedString={loadedString}");
            var datas = loadedString.Split(";");
            if (datas[0] == today)
                todaysWatchedEvents = datas[1].Split(",").ToList();
            else
                todaysWatchedEvents = new List<string>();
        }
        private void saveWatchedEventToday()
        {
            var today = DateTime.Today.ToString("yyMMdd");
            var saveString = $"{today};{string.Join(",", todaysWatchedEvents)}";
            LOG.Function(this, $"| saveString={saveString}");
            PlayerPrefs.SetString(PP_EVENT_NO_AGAIN, saveString);
        }

        // Fields
        private UserDataParent parent;
        private UserDataChild child;
        private List<UserDataPet> pets;
        private bool[][] petBooks;
        private List<string> todaysWatchedEvents;
        private bool hasCheckedEvents = false;



        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            Launched = false;
            parent = new UserDataParent();
            child = new UserDataChild();
            pets = new List<UserDataPet>();
            petBooks = new bool[12][];
            for (int i = 0; i < petBooks.Length; i++)
                petBooks[i] = new bool[3];
            todaysWatchedEvents = new List<string>();

            //// 임의의 펫 생성
            //for (int i = 0; i < 10; i++)
            //{
            //    for (int j = 0; j < 3; j++)
            //    {
            //        float affection = j + Random.Range(0.0f, 0.9f);
            //        pets.Add(new UserDataPet(i, i + 1, affection));
            //        //petBooks[i + 1][j] = true;
            //    }
            //}
        }
    }
}