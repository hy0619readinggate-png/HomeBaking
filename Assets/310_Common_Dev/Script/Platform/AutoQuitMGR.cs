using beyondi.Behaviour;
using System.Collections;
using UnityEngine;

// devBOX - 삭제해야 함
#pragma warning disable 0414

namespace DoDoEng.Common
{
    public class AutoQuitMGR : BYDSingleton<AutoQuitMGR>
    {
        // Definitions
        // Properties
        public static bool ShowDebugInfo
        {
            get => One?.showDebugInfo ?? false;
            set
            {
                if (One != null)
                    One.showDebugInfo = value;
            }
        }

        // Methods
        public void PauseMonitor()
        {
            LOG.Function(this);

            isMonitoring = false;
        }
        public void ResumeMonitor()
        {
            LOG.Function(this);

            isMonitoring = true;
        }
        // Events



        // Fields : caching
        // Fields
        private bool isMonitoring = false;
        private float duration = 1200f;
        private float remain = 0;
        private Coroutine monitorCR = null;

        // Functions
        private void startMonitor()
        {
            
        }
        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private float timeout = 1200f;
        [SerializeField] private bool showDebugInfo = false;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();
        }
        private void Start()
        {
            isMonitoring = true;
            this.duration = timeout;
            monitorCR = StartCoroutine(coMonitor());
        }
        private void Update()
        {
            if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
            {
                remain = duration;
            }

            if (Input.GetMouseButton(0))
                remain = duration;
        }
        private void OnGUI()
        {
            if (!showDebugInfo)
                return;

            var targetHeight = 700f;
            var scale = Screen.height / targetHeight;

            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1));

            GUILayout.BeginArea(new Rect(10, 540, 200, 150), GUI.skin.box);
            GUILayout.Label("AffManager");
            GUILayout.Label($"timeout    : {duration:f1} (sec)");
            GUILayout.Label($"remain     : {remain:f2}");
            GUILayout.EndArea();
        }

        // Unity Coroutine
        IEnumerator coMonitor()
        {
            remain = duration;

            while (true)
            {
                if (isMonitoring)
                    remain -= Time.deltaTime;

                if (remain < 0)
                {
                    LOG.Info($"Application Quit!!", this);
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                }

                yield return null;
            }
        }
    }
}