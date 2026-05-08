using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.Launcher.UI;
using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;

namespace DoDoEng.Launcher
{
    public class CMPopChildInfo : MonoBehaviour
    {
        // Properties
        public string ID => idTMP.text;
        public string Password => passwordTMP.text;
        public string NickName => nickNameTMP.text;
        public int BirthYear => int.Parse(birthYearTMP.options[birthYearTMP.value].text);
        public int Course => course;
        public bool HasChangedPassword => passwordTMP.enabled;
        public bool HasChangedPhoto => hasChangedPhoto;
        public Texture2D Photo => photo;

        // Methods
        public void Activate(bool active)
        {
            LOG.Info($"Activate() | {active}", this);

            gameObject.SetActive(active);
        }
        public void Show(UserDataChild data = null)
        {
            LOG.Info($"Show() | {data}", this);

            gameObject.SetActive(true);

            photo = null;
            hasChangedPhoto = false;
            passwordVisibleTG.isOn = true;
            childData = data;

            if (data == null)
            {
                idValidation = false;
                passwordValidation = false;
                nickNameValidation = false;

                titleAdd.SetActive(true);
                titleEdit.SetActive(false);
                addBT.gameObject.SetActive(true);
                editBT.gameObject.SetActive(false);
                passwordVisibleTG.gameObject.SetActive(true);
                resetPWBT.gameObject.SetActive(false);

                idTMP.text = "";
                idTMP.readOnly = false;
                passwordTMP.text = "";
                passwordTMP.enabled = true;
                nickNameTMP.text = "";
                birthYearTMP.value = 0;

                setCourse(1);

                childSlot.Init(ChildSlot.SlotState.New);
            }
            else
            {
                idValidation = true;
                passwordValidation = true;
                nickNameValidation = true;

                titleAdd.SetActive(false);
                titleEdit.SetActive(true);
                addBT.gameObject.SetActive(false);
                editBT.gameObject.SetActive(true);
                passwordVisibleTG.gameObject.SetActive(false);
                resetPWBT.gameObject.SetActive(true);

                idTMP.text = data.ID;
                idTMP.readOnly = true;
                passwordTMP.text = "******";
                passwordTMP.enabled = false;
                nickNameTMP.text = data.NickName;
                int idxBirthYear = 0;
                for (idxBirthYear = 0; idxBirthYear < birthYearTMP.options.Count; idxBirthYear++)
                {
                    if (int.Parse(birthYearTMP.options[idxBirthYear].text) == data.YearOfBirth)
                        break;
                }
                if (idxBirthYear == birthYearTMP.options.Count)
                {
                    birthYearTMP.options.Add(new TMP_Dropdown.OptionData(data.YearOfBirth.ToString()));
                }
                birthYearTMP.value = idxBirthYear;

                setCourse(data.Course);

                nickNameValidationTMP.text = LocalizationMGR.One.GetText("MESSAGE_13");

                childSlot.Init(ChildSlot.SlotState.Edit, data);
            }
            birthYearTMP_onValueChanged(birthYearTMP.value);

            updateButtonState();
        }

        // Events
        public event Action OnAdd;
        public event Action<UserDataChild> OnEdit;



        // Fields
        private bool idValidation;
        private bool passwordValidation;
        private bool nickNameValidation;
        private int currentYear;
        private int course;
        private Texture2D photo = null;
        private bool hasChangedPhoto = false;
        private UserDataChild childData;

        // Functions
        private void updateButtonState()
        {
            //bool validation = idValidation && passwordValidation && nickNameValidation;
            //addBT.enabled = validation;
            //editBT.enabled = validation;
        }
        private void setCourse(int course)
        {
            LOG.Function(this, $"{course}");
            this.course = course;
            courseNameTMP.text = $"Course{course}";
        }
        private async UniTask showCoursePopup()
        {
            var course = await UILauncher.One.CoursePU.ShowPopup(Course);
            if (course != -1)
                setCourse(course);
        }

        // Event Handlers
        private void idTMP_onValueChanged(string value)
        {
            if (editBT.gameObject.activeSelf)
            {
                idValidationTMP.text = LocalizationMGR.One.GetText("MESSAGE_11");
                return;
            }

            idTMP.text = value.ToLower();
            idValidation = false;

            if (string.IsNullOrEmpty(value))
            {
                idValidationTMP.text = LocalizationMGR.One.GetText("MESSAGE_3");
                //idValidationTMP.color = validationRightColor;
            }
            else if (value.Length < 6)
            {
                idValidationTMP.text = LocalizationMGR.One.GetText("MESSAGE_5");
                //idValidationTMP.color = validationWrongColor;
            }
            else
            {
                //idValidationTMP.text = LocalizationMGR.One.GetText("MESSAGE_10");
                //idValidationTMP.color = validationRightColor;

                idValidation = true;

                validateId(idTMP.text).Forget();
            }

            updateButtonState();
        }
        private void passwordTMP_onValueChanged(string value)
        {
            if (resetPWBT.gameObject.activeSelf)
            {
                passwordValidationTMP.text = LocalizationMGR.One.GetText("MESSAGE_12");
                return;
            }

            passwordTMP.text = value.ToLower();
            passwordValidation = false;

            if (string.IsNullOrEmpty(value))
            {
                passwordValidationTMP.text = LocalizationMGR.One.GetText("MESSAGE_6");
                //passwordValidationTMP.color = validationRightColor;
            }
            else if (value.Length < 4)
            {
                passwordValidationTMP.text = LocalizationMGR.One.GetText("MESSAGE_9");
                //passwordValidationTMP.color = validationWrongColor;
            }
            else
            {
                passwordValidationTMP.text = LocalizationMGR.One.GetText("MESSAGE_10");
                //passwordValidationTMP.color = validationRightColor;

                passwordValidation = true;
            }

            updateButtonState();
        }
        private void nickNameTMP_onValueChanged(string value)
        {
            nickNameValidation = false;

            Regex regex = new Regex("^[ㄱ-ㅎ가-힣a-zA-Z0-9]*$");
            if (string.IsNullOrEmpty(value))
            {
                nickNameValidationTMP.text = LocalizationMGR.One.GetText("MESSAGE_7");
                //nickNameValidationTMP.color = validationRightColor;
            }
            else if (!regex.IsMatch(value))
            {
                nickNameValidationTMP.text = LocalizationMGR.One.GetText("MESSAGE_7");
                //nickNameValidationTMP.color = validationWrongColor;
            }
            else if (value.Length < 2)
            {
                nickNameValidationTMP.text = LocalizationMGR.One.GetText("MESSAGE_8");
                //nickNameValidationTMP.color = validationWrongColor;
            }
            else
            {
                nickNameValidationTMP.text = LocalizationMGR.One.GetText("MESSAGE_10");
                //nickNameValidationTMP.color = validationRightColor;

                nickNameValidation = true;
            }

            updateButtonState();
        }
        private void birthYearTMP_onValueChanged(int value)
        {
            var age = currentYear - int.Parse(birthYearTMP.options[value].text) + 1;
            ageTMP.text = LocalizationMGR.One.GetText("WORD_123", age);
        }
        private void courseBT_onClick()
        {
            LOG.Function(this);

            showCoursePopup().Forget();
        }
        private async UniTask validateId(string id)
        {
            var result = await LMS.One.ValidateChildID(id);
            if (id == idTMP.text && idValidation)
            {
                idValidation = result;
                if (result)
                    idValidationTMP.text = LocalizationMGR.One.GetText("MESSAGE_10");
                else
                    idValidationTMP.text = LocalizationMGR.One.GetText("MESSAGE_4");
            }
        }
        private void addBT_onClick()
        {
            LOG.Function(this);

            bool validation = idValidation && passwordValidation && nickNameValidation;
            if (validation)
                OnAdd?.Invoke();
            else
                SystemUI.One.ShowPopupAddUserWrongInput().Forget();
        }
        private void editBT_onClick()
        {
            LOG.Function(this);

            bool validation = idValidation && passwordValidation && nickNameValidation;
            if (validation)
                OnEdit?.Invoke(childData);
            else
                SystemUI.One.ShowPopupAddUserWrongInput().Forget();
        }
        private void childSlot_OnClick()
        {
            childSlot.HideMenu();
        }
        private void childSlot_OnChangePhoto(UserDataChild childData, Texture2D texture)
        {
            hasChangedPhoto = true;
            photo = texture;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject titleAdd = null;
        [SerializeField] private GameObject titleEdit = null;
        [SerializeField] private ChildSlot childSlot = null;
        [SerializeField] private Button addBT = null;
        [SerializeField] private Button editBT = null;
        [SerializeField] private Toggle passwordVisibleTG = null;
        [SerializeField] private Button resetPWBT = null;
        [SerializeField] private TMP_InputField idTMP = null;
        [SerializeField] private TMP_InputField passwordTMP = null;
        [SerializeField] private TMP_InputField nickNameTMP = null;
        [SerializeField] private TMP_Text idValidationTMP = null;
        [SerializeField] private TMP_Text passwordValidationTMP = null;
        [SerializeField] private TMP_Text nickNameValidationTMP = null;
        [SerializeField] private TMP_Dropdown birthYearTMP = null;
        [SerializeField] private TMP_Text ageTMP = null;
        [SerializeField] private Button courseBT = null;
        [SerializeField] private TMP_Text courseNameTMP = null;
        [Header("★ Config")]
        [SerializeField] private Color validationRightColor = new(72 / 255f, 198 / 255f, 240 / 255f);
        [SerializeField] private Color validationWrongColor = Color.red;

        // Unity Messages
        private void Awake()
        {
            addBT.onClick.AddListener(() => addBT_onClick());
            editBT.onClick.AddListener(() => editBT_onClick());
            resetPWBT.onClick.AddListener(() =>
            {
                resetPWBT.gameObject.SetActive(false);
                passwordTMP.text = "";
                passwordTMP.enabled = true;
                passwordVisibleTG.gameObject.SetActive(true);
            });

            idTMP.onValueChanged.AddListener(idTMP_onValueChanged);
            passwordTMP.onValueChanged.AddListener(passwordTMP_onValueChanged);
            nickNameTMP.onValueChanged.AddListener(nickNameTMP_onValueChanged);
            courseBT.onClick.AddListener(() => courseBT_onClick());

            birthYearTMP.options.Clear();
            currentYear = int.Parse(DateTime.Now.ToString("yyyy"));
            for (int i = 0; i < 15; i++)
            {
                birthYearTMP.options.Add(new TMP_Dropdown.OptionData((currentYear - i).ToString()));
            }
            birthYearTMP.onValueChanged.AddListener(birthYearTMP_onValueChanged);
        }
        private void Start()
        {
        }
        protected void OnEnable()
        {
            childSlot.OnClick += childSlot_OnClick;
            childSlot.OnChangePhoto += childSlot_OnChangePhoto;
        }
        protected void OnDisable()
        {
            childSlot.OnClick -= childSlot_OnClick;
            childSlot.OnChangePhoto -= childSlot_OnChangePhoto;
        }
        private void OnDestroy()
        {
        }

    }
}