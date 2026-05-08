using beyondi.Behaviour;
using UnityEngine;

namespace DoDoEng.Library.UI
{
    public class UILibraryMovieCommon : BYDSingleton<UILibraryMovieCommon>
    {
        // Properties
        public LibraryMoviePlayerPopup PlayerPU => playerPU;



        // Unity Inspectors
        [Header("Bindings")]
        [SerializeField] private LibraryMoviePlayerPopup playerPU = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            playerPU.gameObject.SetActive(false);
        }
        private void Start()
        {
        }
    }
}