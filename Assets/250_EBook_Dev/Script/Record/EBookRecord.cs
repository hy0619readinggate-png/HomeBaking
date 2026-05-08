using beyondi.FSM;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.EBook.Framework;
using DoDoEng.EBook.Record;
using SRDebugger;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace DoDoEng.EBoEBook.Record
{
    public class EBookRecord : EBookSingleBase
    {
        // Definitions
        private enum State
        {
            Permission,
            Intro,
            Listen, Wait, Record, MyVoice,
            Next, Goto,
            Closing, Save,
            Outro, Reward
        }



        // Fields
        private FSM<State> fsm = null;
        private int cNO = 1;
        private int cTOTAL = 1;
        private RecordData cDATA => RECORDS[cNO - 1];
        private int gotoNO = -1;
        private bool isPlayAllMode = false;
        private bool isPlayRecordComplete = false;
        private bool isPlayCurrentPage = false;

        // Fields
        private AudioClip[] recordCLIPs = null;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Permission,  E_Permission);
            fsm.AddState(State.Intro,       E_Intro,    X_Intro);
            fsm.AddState(State.Listen,      E_Listen,   X_Listen);
            fsm.AddState(State.Wait,        E_Wait,     X_Wait);
            fsm.AddState(State.Record,      E_Record,   X_Record);
            fsm.AddState(State.MyVoice,     E_MyVoice,  X_MyVoice);
            fsm.AddState(State.Next,        E_Next);
            fsm.AddState(State.Goto,        E_Goto,     X_Goto);
            fsm.AddState(State.Outro,       E_Outro,    X_Outro);
            fsm.AddState(State.Closing,     E_Closing,  X_Closing);
            fsm.AddState(State.Save,        E_Save,     X_Save);
            fsm.AddState(State.Reward,      E_Reward);
            #pragma warning restore format
        }
        private void checkMicrophone()
        {
            try
            {
                recorder.CheckMicrophone();
            }
            catch (Exception ex)
            {
                SystemUI.One.ShowToastMessage(ex.ToString());
            }
        }

        // Functions
        private bool isCurrentRecorded()
        {
            return recordCLIPs[cNO - 1] != null;
        }
        private bool isAllRecorded()
        {
            return recordCLIPs.All(c => c != null);
        }

        // Functions
        private void updateNavigationControl()
        {
            prevBTN.interactable = cNO > 1;
            nextBTN.interactable = cNO < cTOTAL && isCurrentRecorded();

            prevBTN.gameObject.SetActive(cNO > 1);
            nextBTN.gameObject.SetActive(cNO < cTOTAL);
        }
        private void updateController()
        {
            menu.EnableMyVoice = isCurrentRecorded();
            playAllTB.Interactable = isAllRecorded();
            saveAllBTN.interactable = isAllRecorded();
        }

        // Functions
        private void turnOffPlayAllMode()
        {
            if (isPlayAllMode)
            {
                isPlayAllMode = false;
                playAllTB.IsOn = true;
            }
        }

        // Event Handlers
        private void anywhereBTN_OnClick()
        {
            LOG.Info($"anywhereBTN_OnClick()", this);

            if (fsm.CurrentState == State.Intro)
            {
                if (introTL.time < introSkipTime)
                    StartCoroutine(coSkipIntro());
            }

            if (fsm.CurrentState == State.Closing)
            {
                if (closingTL.time < closingSkipTime)
                    StartCoroutine(coSkipClosing());
            }

            if (fsm.CurrentState == State.Listen ||
                fsm.CurrentState == State.MyVoice)
                fsm.PerformTransition(State.Wait);
        }
        private void prevBTN_OnClick()
        {
            LOG.Info($"prevBTN_OnClick()", this);

            AudioMGR.One.PlayEffect(movePageCLIP);

            gotoNO = cNO - 1;
            fsm.PerformTransition(State.Goto);
        }
        private void nextBTN_OnClick()
        {
            LOG.Info($"nextBTN_OnClick()", this);

            AudioMGR.One.PlayEffect(movePageCLIP);

            gotoNO = cNO + 1;
            fsm.PerformTransition(State.Goto);
        }
        private void playAllTB_OnValueChanged(bool isOn)
        {
            LOG.Info($"playAllTB_OnValueChanged() | {isOn}", this);

            AudioMGR.One.PlayEffect(SfxMoment.Common_Click);

            // On/Off가 바뀐듯 : ToggleButton의 기능이 혼돈스러운 상태임
            isPlayAllMode = !isOn;

            if (isPlayAllMode)
            {
                if (!isPlayCurrentPage)
                {
                    isPlayCurrentPage = true;
                    gotoNO = 1;
                    fsm.PerformTransition(State.Goto);
                }
                else fsm.PerformTransition(State.MyVoice);
            }
            else fsm.PerformTransition(State.Wait);
        }
        private void saveAllBTN_OnClick()
        {
            LOG.Info($"saveAllBTN_OnClick()", this);

            AudioMGR.One.PlayEffectUI(SfxMoment.Common_Click);

            fsm.PerformTransition(State.Save);
        }
        private void menu_OnButtonClick(Menu.Buttons button)
        {
            LOG.Info($"controller_OnButtonClick() | {button}", this);

            AudioMGR.One.PlayEffect(SfxMoment.Common_Click);

            turnOffPlayAllMode();

            switch (button)
            {
                case Menu.Buttons.NativeButton: fsm.PerformTransition(State.Listen); break;
                case Menu.Buttons.RecordButton:
                    if (fsm.CurrentState != State.Record)
                        fsm.PerformTransition(State.Record);
                    else fsm.PerformTransition(State.Wait);
                    break;
                case Menu.Buttons.MyVoiceButton: fsm.PerformTransition(State.MyVoice); break;
            }
        }

        // Overrides
        protected override async UniTask onPrepare(EBookSingleIndex ebIDX)
        {
            setAssetLoadFlag(AssetLoadFlag.LayerImage | AssetLoadFlag.NatvieNarration);
            await base.onPrepare(ebIDX);

            var pageSPR = LAYERS.Select(ld => ld.LayerSPR).ToArray();
            pageViewer.Setup(RECORDS, pageSPR);

            cNO = 1;
            cTOTAL = RECORDS.Length;
            recordCLIPs = new AudioClip[cTOTAL];

            updateNavigationControl();
            updateController();
            menu.SetProgress(0, cTOTAL, false);

            recordingANIM.gameObject.SetActive(true);

            evaluateTimeline(introTL);
        }
        protected override void onStartEBook()
        {
            base.onStartEBook();

            checkMicrophone();
            fsm.StartFSM(State.Permission, RunnerParam.SkipStateTo);
        }
        protected override void onFinishEBook()
        {
            base.onFinishEBook();

            fsm?.StopFSM();
            AudioMGR.One.StopAll();
            AffordanceMGR.One.Clear();
        }
#if !DISABLE_SRDEBUGGER
        protected override void onBuildOption(DynamicOptionContainer srOptionContainer)
        {
            base.onBuildOption(srOptionContainer);

            var sort = 400;

            srOptionContainer.AddOption(
                OptionDefinition.FromMethod("Record Done(Native->Record)", () =>
                {
                    for (var i = 0; i < recordCLIPs.Length; i++)
                        recordCLIPs[i] = RECORDS[i].SentenceCLIP;
                    updateNavigationControl();
                    updateController();

                }, "eBook", sort++));
        }
#endif
        protected override void onDebugForceFinish()
        {
            base.onDebugForceFinish();

            fsm.PerformTransition(State.Outro);
        }



        // FSM
        IEnumerator E_Permission()
        {
            var micPermitted = false;
            yield return permissionChecker.CheckMicrophonePermissionForced().ToCoroutine(ok => micPermitted = ok);

            if (!micPermitted)
                error();
            else fsm.PerformTransition(State.Intro);
        }
        IEnumerator E_Intro()
        {
            menu.EnableInteraction(false);

            yield return playTimeline(introTL);
            yield return null;

            fsm.PerformTransition(State.Listen);
        }
        IEnumerator X_Intro()
        {
            yield return stopTimeline(introTL);
            yield return null;

            menu.EnableInteraction(true);
        }
        IEnumerator E_Listen()
        {
            LOG.Important($"[cNO:{cNO}] {cDATA.Sentence}", this);

            menu.SetNativePlaying(true);
            yield return AudioMGR.One.PlayNarrationAndWait(cDATA.SentenceCLIP);
            yield return null;

            fsm.PerformTransition(State.Wait);
        }
        IEnumerator X_Listen()
        {
            menu.SetNativePlaying(false);
            AudioMGR.One.StopNarration();
            yield return null;
        }
        IEnumerator E_Wait()
        {
            AffordanceMGR.One.StartMonitor(affTimeout);
            yield return null;
        }
        IEnumerator X_Wait()
        {
            AffordanceMGR.One.StopMonitor();
            yield return null;
        }
        IEnumerator E_Record()
        {
            recordingANIM.SetTrigger("Show");
            yield return null;

            var duration = Mathf.FloorToInt(cDATA.SentenceCLIP.length + 2);
            yield return recorder.StartRecord(duration);

            recordCLIPs[cNO - 1] = recorder.RecordedClip;
            updateNavigationControl();
            updateController();
            yield return null;

            if (isAllRecorded() && !isPlayRecordComplete)
                fsm.PerformTransition(State.Closing);
            else fsm.PerformTransition(State.Wait);
        }
        IEnumerator X_Record()
        {
            if (!recorder.IsRecording)      // 녹음 완료
                menu.SetProgress(cNO, cTOTAL);
            recorder.StopRecord();
            recordingANIM.SetTrigger("Hide");
            yield return null;
        }
        IEnumerator E_MyVoice()
        {
            menu.SetMyVoicePlaying(true);

            var clip = recordCLIPs[cNO - 1];
            yield return AudioMGR.One.PlayMyVocieAndWait(clip);
            yield return null;

            if (isPlayAllMode)
                fsm.PerformTransition(State.Next);
            else fsm.PerformTransition(State.Wait);
        }
        IEnumerator X_MyVoice()
        {
            menu.SetMyVoicePlaying(false);
            AudioMGR.One.StopMyVoice();
            yield return null;
        }
        IEnumerator E_Next()
        {
            yield return null;

            if (cNO < cTOTAL)
            {
                gotoNO = cNO + 1;
                fsm.PerformTransition(State.Goto);
            }
            else
            {
                turnOffPlayAllMode();
                fsm.PerformTransition(State.Wait);
            }
        }
        IEnumerator E_Goto()
        {
            cNO = gotoNO;
            pageViewer.ChangeTo(cNO);
            yield return null;

            updateNavigationControl();
            updateController();
            yield return null;

            if (isPlayAllMode)
                fsm.PerformTransition(State.MyVoice);
            else fsm.PerformTransition(State.Listen);
        }
        IEnumerator X_Goto()
        {
            //navigation.Show(!isAutoMode);
            //pageTurnSubtitleOn.HideNow();
            //pageTurnSubtitleOff.HideNow();
            yield return null;
        }
        IEnumerator E_Closing()
        {
            menu.EnableInteraction(false);

            isPlayRecordComplete = true;
            isPlayCurrentPage = false;

            yield return playTimeline(closingTL);
            yield return null;

            fsm.PerformTransition(State.Wait);
        }
        IEnumerator X_Closing()
        {
            yield return stopTimeline(closingTL);
            yield return null;

            menu.EnableInteraction(true);
        }
        IEnumerator E_Save()
        {
            turnOffPlayAllMode();

            SystemUI.One.StandbyPU.ShowPopup().Forget();
            yield return LMS.One.SaveAudioRecords(int.Parse(EBIndex.Index), recordCLIPs);

            SystemUI.One.StandbyPU.CloseWithResult();

            fsm.PerformTransition(State.Outro);
        }
        IEnumerator X_Save()
        {
            yield return null;
        }
        IEnumerator E_Outro()
        {
            yield return new WaitForSeconds(1f);
            yield return null;

            fsm.PerformTransition(State.Reward);
        }
        IEnumerator X_Outro()
        {
            yield return null;
        }
        IEnumerator E_Reward()
        {
            backButtonGO.SetActive(false);
            yield return null;

            complete();
            yield return null;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private PageViewer pageViewer = null;
        [SerializeField] private Menu menu = null;
        [SerializeField] private Button anywhereBTN = null;
        [SerializeField] private Button prevBTN = null;
        [SerializeField] private Button nextBTN = null;
        [SerializeField] private GameObject backButtonGO = null;
        [SerializeField] private ToggleButton playAllTB = null;
        [SerializeField] private Button saveAllBTN = null;
        [SerializeField] private VoiceRecorder recorder = null;
        [SerializeField] private PermissionChecker permissionChecker = null;
        [SerializeField] private Animator recordingANIM = null;
        [Header("★ Bindings - Aff")]
        [SerializeField] private AffBase affRecord = null;
        [SerializeField] private AffBase affSave = null;
        [Header("★ Audios")]
        [SerializeField] private AudioClip movePageCLIP = null;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector introTL = null;
        [SerializeField] private PlayableDirector closingTL = null;
        [Header("★ Config")]
        [SerializeField] private float introSkipTime = 10f;
        [SerializeField] private float closingSkipTime = 10f;
        [SerializeField] private float affTimeout = 10f;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            affRecord.gameObject.SetActive(true);
            affRecord.Enabler = () => !isCurrentRecorded();
            affSave.gameObject.SetActive(true);
            affSave.Enabler = () => isAllRecorded();

            anywhereBTN.onClick.AddListener(anywhereBTN_OnClick);
            prevBTN.onClick.AddListener(prevBTN_OnClick);
            nextBTN.onClick.AddListener(nextBTN_OnClick);
            saveAllBTN.onClick.AddListener(saveAllBTN_OnClick);

            initFSM();
        }
        protected override void Start()
        {
            base.Start();
        }
        protected override void OnEnable()
        {
            base.OnEnable();

            menu.OnButtonClick += menu_OnButtonClick;
            playAllTB.OnValueChanged += playAllTB_OnValueChanged;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            menu.OnButtonClick -= menu_OnButtonClick;
            playAllTB.OnValueChanged -= playAllTB_OnValueChanged;
        }

        // Unity Coroutine
        IEnumerator coSkipIntro()
        {
            introTL.time = introSkipTime;
            yield return new WaitForSeconds((float)introTL.duration - introSkipTime);

            fsm.PerformTransition(State.Listen);
        }
        IEnumerator coSkipClosing()
        {
            closingTL.time = closingSkipTime;
            yield return new WaitForSeconds((float)closingTL.duration - closingSkipTime);

            fsm.PerformTransition(State.Wait);
        }
    }
}