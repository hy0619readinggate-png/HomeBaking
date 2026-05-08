using UnityEngine;

namespace DoDoEng.Launcher.UI
{
	public class AttendanceCoin : MonoBehaviour
	{
        // Definitions
        // Properties
        public bool Enabled
        {
            get => enableGO.activeSelf;
            set 
            {
                disableGO.SetActive(!value);
                enableGO.SetActive(value);
            }
        }

        // Methods
        // Events



        // Fields : caching
        // Fields
        // Functions
        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject disableGO = null;
        [SerializeField] private GameObject enableGO = null;

        // Unity Messages
        private void Awake()
		{
            disableGO.SetActive(true);
            enableGO.SetActive(false);
        }
		private void Start()
		{
		}

		// Unity Coroutine
	}
}