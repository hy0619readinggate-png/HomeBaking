using beyondi.Behaviour;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.EBook.UI
{
    public class UIEBookCommon : BYDSingleton<UIEBookCommon>
    {
        // Properties
        public SimplePopup ConfirmExitPopup => confirmExitPU;
        public SimplePopup ConfirmRerecordPopup => confirmRerecordPU;
        public EBookReadCompletePopup CompleteReadPopup => completeReadPU;
        public EBookRecordCompletePopup CompleteRecordPopup => completeRecordPU;
        public EBookQuizCompletePopup CompleteQuizPopup => completeQuizPU;




        // Unity Inspectors
        [Header("Bindings")]
        [SerializeField] private SimplePopup confirmExitPU = null;
        [SerializeField] private SimplePopup confirmRerecordPU = null;
        [SerializeField] private EBookReadCompletePopup completeReadPU = null;
        [SerializeField] private EBookRecordCompletePopup completeRecordPU = null;
        [SerializeField] private EBookQuizCompletePopup completeQuizPU = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            confirmExitPU.gameObject.SetActive(false);
            completeReadPU.gameObject.SetActive(false);
            completeRecordPU.gameObject.SetActive(false);
            completeQuizPU.gameObject.SetActive(false);
        }
        private void Start()
        {
        }
    }
}