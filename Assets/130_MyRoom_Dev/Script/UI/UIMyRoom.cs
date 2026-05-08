using beyondi.Behaviour;
using DoDoEng.Common;
using DoDoEng.MyRoom.UI.Popup;
using UnityEngine;

namespace DoDoEng.MyRoom.UI
{
    public class UIMyRoom : BYDSingleton<UIMyRoom>
    {
        // Properties
        public CollectionPopup CollectionPU => collectionPU;
        public EatingPopup EatingPU => eatingPU;
        public HealingPopup HealingPU => healingPU;
        public ShowerPopup ShowerPU => showerPU;
        public CreatureBookPopup CreatureBookPU => creatureBookPU;
        public TripPopup TripPU => tripPU;
        public LevelUpPopup LevelUpPU => levelUpPU;
        public NameChangePopup NameChangePU => nameChangePU;

        // Properties
        public bool VisibleBackButton
        {
            get => visibleBackButton;
            set
            {
                visibleBackButton = value;
                updateButtonVisible();
            }
        }
        public bool VisibleHelpButton
        {
            get => helpButtonGO.activeSelf;
            set => helpButtonGO.SetActive(value);
        }



        // Fields
        private bool visibleBackButton = false;

        // Functions
        private void updateButtonVisible()
        {
            backButtonGO.SetActive(visibleBackButton);
        }



        // Unity Inspectors
        [Header("Bindings")]
        [SerializeField] private GameObject backButtonGO = null;
        [SerializeField] private CoinInfo coinInfo = null;
        [SerializeField] private CollectionPopup collectionPU = null;
        [SerializeField] private EatingPopup eatingPU = null;
        [SerializeField] private HealingPopup healingPU = null;
        [SerializeField] private ShowerPopup showerPU = null;
        [SerializeField] private CreatureBookPopup creatureBookPU = null;
        [SerializeField] private TripPopup tripPU = null;
        [SerializeField] private LevelUpPopup levelUpPU = null;
        [SerializeField] private NameChangePopup nameChangePU = null;
        [SerializeField] private GameObject helpButtonGO = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            backButtonGO.SetActive(true);
            coinInfo.gameObject.SetActive(true);
            collectionPU.Activate(false);
            eatingPU.Activate(false);
            healingPU.Activate(false);
            showerPU.Activate(false);
            creatureBookPU.Activate(false);
            tripPU.Activate(false);
            levelUpPU.gameObject.SetActive(false);
            nameChangePU.gameObject.SetActive(false);
        }
        private void Start()
        {
        }
    }
}