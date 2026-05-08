using Newtonsoft.Json.Linq;
using UnityEngine;

namespace DoDoEng.Common
{
    public class UserDataChild
    {
        // Properties
        public bool HasSignedIn
        {
            get { return AccessToken != ""; }
        }
        public string ID = "";
        public string Password = "";
        public bool AutoSignIn = false;
        public string AccessToken = "";
        public string NickName = "";
        public string ParentID = "";
        public string ParentSignupType = "";
        public int YearOfBirth = 2023;
        public int Course = 1;
        public JObject DayProgress;
        public bool IsAttendanceFirst = false;
        public int AttendanceContinuous = 1;
        public int RewardDay = 0;
        public int RewardStage = 0;
        public int RewardCourse = 0;
        public bool Available = false;
        public bool DayLimit = true;
        public bool IsGuideFirst = false;
        public Texture2D Photo = null;

        public bool IsAppNotificationMarketing { get; set; } = true;
        public bool IsAppNotificationLearning { get; set; } = true;
        public bool IsAppSoundEffect
        {
            get => AudioMGR.One.SfxVolume == 1;
            set
            {
                AudioMGR.One.SfxVolume = value ? 1 : 0;
                AudioMGR.One.AMBVolume = value ? 1 : 0;
            }
        }
        public bool IsAppSoundBackground
        {
            get => AudioMGR.One.BgmVolume == 1;
            set
            {
                AudioMGR.One.BgmVolume = value ? 1 : 0;
            }
        }

        public UserDataChild() { }
        public UserDataChild(string id, string nickName, int yearOfBirth, int course)
        {
            ID = id;
            NickName = nickName;
            YearOfBirth = yearOfBirth;
            Course = course;
        }

        public override string ToString()
        {
            return $"{ID} | {AutoSignIn} | {AccessToken} | {NickName} | {YearOfBirth} | {Course}";
        }
    }
}