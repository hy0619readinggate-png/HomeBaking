using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using SRDebugger;
using System;
using UnityEngine;

namespace DoDoEng.EBook.Framework
{
    public abstract class EBookPlayAllBase : MonoBehaviour
    {
        // Methods
        public async UniTask Prepare(EBookPlayAllIndex ebIDX)
        {
            LOG.Info($"Prepare() | {ebIDX}", this);

            EBIndex = ebIDX;
            await onPrepare(ebIDX);
        }
        public void StartEBook()
        {
            LOG.Info($"StartEBook()", this);
#if !DISABLE_SRDEBUGGER
            srOptionContainer = new DynamicOptionContainer();
            onBuildOption(srOptionContainer);
            SRDebug.Instance.AddOptionContainer(srOptionContainer);
#endif

            onStartEBook();
        }
        public void FinishEBook()
        {
            LOG.Info($"FinishEBook()", this);
#if !DISABLE_SRDEBUGGER
            if (SRDebug.Instance != null && srOptionContainer != null)
            {
                SRDebug.Instance.RemoveOptionContainer(srOptionContainer);
                srOptionContainer = null;
            }
#endif

            onFinishEBook();
        }

        // Events
        public Action OnEBookComplete { get; internal set; }



        // Virtual
        protected virtual async UniTask onPrepare(EBookPlayAllIndex ebIDX)
        {
            await UniTask.Delay(10);
        }
        protected virtual void onStartEBook()
        {
        }
        protected virtual void onFinishEBook()
        {
            DataLoader.One.ReleaseHandles();
        }
#if !DISABLE_SRDEBUGGER
        protected virtual void onBuildOption(DynamicOptionContainer srOptionContainer)
        {
        }
#endif

        // Properties : for concrete
        protected EBookPlayAllIndex EBIndex { get; private set; }

        // Functions : for concrete
        protected void complete()
        {
            OnEBookComplete?.Invoke();
        }

        // Fields
#if !DISABLE_SRDEBUGGER
        private DynamicOptionContainer srOptionContainer;
#endif

        // Event Handlers
        private void systemEventManager_OnSystemButtonClicked(SystemButtonType buttonType)
        {
        }



        // Unity Messages
        protected virtual void Awake()
        {
        }
        protected virtual void Start()
        {
        }
        protected virtual void Update()
        {
        }
        protected virtual void OnEnable()
        {
            SystemEventManager.OnSystemButtonClicked += systemEventManager_OnSystemButtonClicked;
        }
        protected virtual void OnDisable()
        {
            SystemEventManager.OnSystemButtonClicked -= systemEventManager_OnSystemButtonClicked;
        }
    }
}