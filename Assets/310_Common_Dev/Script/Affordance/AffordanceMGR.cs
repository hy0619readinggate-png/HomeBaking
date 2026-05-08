using beyondi.Behaviour;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DoDoEng.Common
{
    public class AffordanceMGR : BYDSingleton<AffordanceMGR>
    {
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
        public static bool DisableMonitor
        {
            get => One?.disableMonitor ?? false;
            set
            {
                if (One != null)
                    One.disableMonitor = value;
            }
        }

        // Methods
        public void StartMonitor(float timeout = 10f)
        {
            LOG.Info($"StartMonitor()", this);

            isMonitoring = true;
            this.duration = timeout;
            this.remain = duration;

            monitorCR = StartCoroutine(coMonitor());
        }
        public void StopMonitor()
        {
            LOG.Info($"StopMonitor()", this);

            isMonitoring = false;
            if (monitorCR != null)
                StopCoroutine(monitorCR);
            abortAff();
        }
        public void Clear()
        {
            LOG.Info($"Clear()", this);

            if (monitorCR != null)
                StopCoroutine(monitorCR);
            abortAff();
            affObjects.Clear();
        }
        public void StartAffNow(float preDelay = 0)
        {
            LOG.Info($"StartAffNow()", this);

            DOVirtual.DelayedCall(preDelay,
                () =>
                {
                    remain = duration;
                    startAff();
                });
        }

        // Methods
        public void RegisterAff(AffBase aff)
        {
            affObjects.Add(aff);
        }
        public void UnregisterAff(AffBase aff)
        {
            affObjects.Remove(aff);
        }

        // Methods
        public void DEV_timeleap()
        {
            LOG.Info($"DEV_timeleap()", this);
            remain = 0.5f;
        }



        // Fields
        private List<AffBase> affObjects = new List<AffBase>();
        private bool isMonitoring = false;
        private float duration = 10f;
        private float remain = 0;
        private float remainPostDelay = 0;
        private Coroutine monitorCR = null;

        // Functions
        private void startAff()
        {
            affObjects.ForEach(aff => aff.StartAffordance());
        }
        private void abortAff()
        {
            remain = duration;
            affObjects.ForEach(aff => aff.AbortAffordance());
        }



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private float postDelay = 2f;
        [Header("★ DEBUG")]
        [SerializeField] private bool showDebugInfo = false;
        [SerializeField] private bool disableMonitor = false;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();
        }
        private void Start()
        {

        }
        private void Update()
        {
            if (!isMonitoring) return;

            if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
            {
                remain = duration;
                abortAff();
            }

            if (Input.GetMouseButton(0))
                remain = duration;


            // 코루틴 함수를 업데이트로 변경 20250102 by veramocor
            if (!disableMonitor)
            {
                if (remainPostDelay > 0)
                    remainPostDelay -= Time.deltaTime;
                else remain -= Time.deltaTime;
            }

            if (remain < 0)
            {
                startAff();
                remain = duration;
                remainPostDelay = postDelay;
            }
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
            // 코루틴 함수를 업데이트로 변경함으로 아래 코드는 사용하지 않음 20250102 by veramocor
            //remain = duration;

            //while (true)
            //{
            //    if (!disableMonitor)
            //        remain -= Time.deltaTime;

            //    if (remain < 0)
            //    {
            //        startAff();
            //        remain = duration;

            //        yield return new WaitForSeconds(postDelay);
            //    }

            //    yield return null;
            //}
            yield return null;
        }
    }
}