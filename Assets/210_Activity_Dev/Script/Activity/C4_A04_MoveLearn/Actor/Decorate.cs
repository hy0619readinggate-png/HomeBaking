using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C4_A04
{
    public class Decorate : MonoBehaviour
    {
        // Methods
        public void Setup(SpaceshipStudent student)
        {
            LOG.Info($"Setup() | {student}", this);

            currentStudent = student;
            isFinished = false;
            nextBTN.interactable = false;
            aff.SetSelected(false);
        }
        public Coroutine StartMenuSetting()
        {
            LOG.Info($"StartMenuSetting()", this);

            toggleGroup.SetAllTogglesOff();

            crMenuSetting = StartCoroutine(coMenuSetting());
            return crMenuSetting;
        }
        public void FinishMenuSetting()
        {
            LOG.Info($"FinishMenuSetting()", this);

            this.StopCoroutineSafe(ref crMenuSetting);

            toggleGroup.SetAllTogglesOff();
            menuAnim.SetTrigger("Shown");
        }
        public Coroutine StartWaitDecorate()
        {
            LOG.Info($"StartWaitDecorate()", this);

            cg.blocksRaycasts = true;
            decorating = true;

            currentStudent.StartDecorate();

            crWaitDecorate = StartCoroutine(coWaitDecorate());
            return crWaitDecorate;
        }
        public void StopWaitDecorate()
        {
            LOG.Info($"StopWaitDecorate()", this);

            cg.blocksRaycasts = false;
            decorating = false;

            currentStudent.StopDecorate();

            this.StopCoroutineSafe(ref crWaitDecorate);
        }
        public Coroutine StartHideMenu()
        {
            LOG.Info($"StartHideMenu()", this);

            crHideMenu = StartCoroutine(coHideMenu());
            return crHideMenu;
        }
        public void FinishHideMenu()
        {
            LOG.Info($"FinishHideMenu()", this);

            this.StopCoroutineSafe(ref crHideMenu);

            menuAnim.SetTrigger("Hidden");
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private DecorateToggle[] toggles_ = null;
        private DecorateToggle[] toggles => toggles_ ??= toggleGroup.GetComponentsInChildren<DecorateToggle>(true);

        // Fields
        private SpaceshipStudent currentStudent = null;
        private Coroutine crMenuSetting = null;
        private Coroutine crWaitDecorate = null;
        private Coroutine crHideMenu = null;
        private bool decorating = false;
        private bool isFinished = false;

        // Event Handlers
        private void toggle_OnSelected(DecorateToggle toggle)
        {
            LOG.Info($"toggle_OnSelected() | {toggle} {decorating}", this);

            if (decorating)
            {
                AudioMGR.One.PlayEffect(touchCLIP);
                currentStudent?.SelectSkin(toggle.Color);
                nextBTN.interactable = true;
                aff.SetSelected(true);
            }
        }
        private void nextBTN_OnClicked()
        {
            LOG.Info($"nextBTN_OnClicked()", this);

            AudioMGR.One.PlayEffect(touchCLIP);
            isFinished = true;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Animator menuAnim = null;
        [SerializeField] private Button nextBTN = null;
        [SerializeField] private ToggleGroup toggleGroup = null;
        [SerializeField] private AudioClip touchCLIP = null;
        [SerializeField] private DecorateAff aff = null;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;

            nextBTN.onClick.AddListener(nextBTN_OnClicked);
            aff.Init(toggleGroup.transform, nextBTN.transform);
        }
        private void Start()
        {
        }
        private void OnEnable()
        {
            toggles.ForEach(t => t.OnSelected += toggle_OnSelected);
        }
        private void OnDisable()
        {
            toggles.ForEach(t => t.OnSelected -= toggle_OnSelected);
        }

        // Unity Coroutine
        IEnumerator coMenuSetting()
        {
            using (LOG.Coroutine($"coMenuSetting()", this))
            {
                menuAnim.SetTrigger("Show");

                yield return new WaitForSeconds(1f);
            };
        }
        IEnumerator coWaitDecorate()
        {
            using (LOG.Coroutine($"coWaitDecorate", this))
            {
                yield return new WaitUntil(() => isFinished);
            }
        }
        IEnumerator coHideMenu()
        {
            using (LOG.Coroutine($"coHideMenu", this))
            {
                menuAnim.SetTrigger("Hide");

                yield return new WaitForSeconds(1f);
            }
        }
    }
}