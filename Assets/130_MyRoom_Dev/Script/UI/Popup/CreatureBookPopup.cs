using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;
using DoDoEng.MyRoom.Behavior;

namespace DoDoEng.MyRoom.UI.Popup
{
	public class CreatureBookPopup : MonoBehaviour
	{
        // Definitions
        // Properties

        // Methods
        public void Activate(bool active)
        {
            LOG.Info($"Activate() | {active}", this);

            gameObject.SetActive(active);
        }
        public void Show()
        {
            LOG.Function(this);

            Activate(true);

            foreach (var (box, i) in boxes.Select((v, i) => (v, i)))
            {
                box.Init(i, UserData.One.PetBooks[i]);
            }

            scrollbar.value = 1.0f;
        }

        // Events



        // Fields : caching
        // Fields
        // Functions
        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Button closeBT = null;
        [SerializeField] private CreatureBookBox[] boxes = null;
        [SerializeField] private Scrollbar scrollbar = null;

        // Unity Messages
        private void Awake()
		{
            closeBT.onClick.AddListener(() =>
            {
                AudioMGR.One.PlayEffect(SfxMoment.Common_Click);
                Activate(false);
            });
        }
		private void Start()
		{
		}
        protected void OnEnable()
        {
        }
        protected void OnDisable()
        {
        }

        // Unity Coroutine
	}
}