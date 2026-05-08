using TMPro;
using UnityEngine;

namespace DoDoEng.Common
{
    public class ToastMessage : MonoBehaviour
    {
        // Methods
        public void Show(string message)
        {
            LOG.Info($"Show() | {message}", this);

            txt.text = message;
            timeout = autoHideDuration;

            gameObject.SetActive(true);
        }



        // Fields
        private float timeout = 0;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI txt;
        [Header("★ Config")]
        [SerializeField] private float autoHideDuration = 3;

        // Unity Messages
        private void Awake()
        {

        }
        private void Start()
        {

        }
        private void Update()
        {
            if (timeout > 0)
            {
                timeout -= Time.unscaledDeltaTime;
                if (timeout <= 0)
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }
}