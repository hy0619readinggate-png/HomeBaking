using beyondi.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace DoDoEng.Common
{
    public class UserDataParent
    {
        // Properties
        public string Name = "";
        public string NickName = "";
        public string LoginID = "";
        public DateTime CreatedDateTime;
        public bool HasSignedIn
        {
            get { return AccessToken != ""; }
        }
        public string AccessToken = "";
        public int ChildrenSlotLimitCount = 0;
        public List<UserDataChild> ChildDatas = new();
        public DateTime VoucherStartDate { get; set; } = DateTime.Now;
        public int VoucherDays { get; set; } = 30;
        public DateTime VoucherEndDate { get; set; } = DateTime.Now.AddDays(30);
        public bool VoucherAvailable { get; set; } = false;
        public bool AutoSignIn { get; set; } = false;

        public bool IsAppNotificationMarketing { get; set; } = true;
        public bool IsAppNotificationLearning { get; set; } = true;
        public bool IsSmsReceive { get; set; } = true;
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
        public string FamilyID { get; set; } = "";
        public string FamilyNickName { get; set; } = "";
        public string FamilyStatus{ get; set; } = "";
        public bool FamilyManager{ get; set; } = false;

        // Methods
        public void InitChildren(JArray data)
        {
            ChildDatas.Clear();
            data.ForEach(d =>
            {
                var child = new UserDataChild(d.Value<string>("loginId"), d.Value<string>("nickName"), d.Value<int>("yearOfBirth"), d.Value<int>("courseId"));
                child.ParentID = d.Value<string>("parentLoginId");
                ChildDatas.Add(child);
            });
        }
    }
}