using beyondi.Behaviour;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Library.UI
{
    public class UILibraryEBookCommon : BYDSingleton<UILibraryEBookCommon>
    {
        // Properties
        public LibraryEBookStartPopup StartPU => startPU;
        public LibraryEBookPlayerPopup PlayerPU => playerPU;
        public SimplePopup DeleteConfirmPU => deleteConfirmPU;
        public SimplePopup RerecordPU => rerecordPU;



        // Unity Inspectors
        [Header("Bindings")]
        [SerializeField] private LibraryEBookStartPopup startPU = null;
        [SerializeField] private LibraryEBookPlayerPopup playerPU = null;
        [SerializeField] private SimplePopup deleteConfirmPU = null;
        [SerializeField] private SimplePopup rerecordPU = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            startPU.Hide();
            playerPU.gameObject.SetActive(false);
            deleteConfirmPU.gameObject.SetActive(false);
            rerecordPU.gameObject.SetActive(false);
        }
        private void Start()
        {
        }
    }
}