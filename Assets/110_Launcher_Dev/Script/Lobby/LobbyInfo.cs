using DoDoEng.Common;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Launcher
{
    [RequireComponent(typeof(Button))]
    public class LobbyInfo : MonoBehaviour
    {
        // Methods
        public void Refresh()
        {
            if (UserData.One.Child.HasSignedIn)
            {
                nickNameTMP.text = UserData.One.Child.NickName;
                profilePhoto.SetPhoto(UserData.One.Child.Photo);
            }
            else
            {
                nickNameTMP.text = LocalizationMGR.One.GetText("WORD_7");
                profilePhoto.Clear();
            }
        }

        // Events
        public event Action OnClick;



        // Fields : caching
        private Button button_;
        private Button button => button_ ??= GetComponent<Button>();



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TMP_Text nickNameTMP = null;
        [SerializeField] private ProfilePhoto profilePhoto = null;

        // Unity Messages
        private void Awake()
        {
            button.onClick.AddListener(() => OnClick?.Invoke());
        }
        private void Start()
        {
            Refresh();
        }
    }
}
