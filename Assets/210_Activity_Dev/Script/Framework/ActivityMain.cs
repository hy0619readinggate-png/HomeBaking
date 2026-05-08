using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using SRDebugger;
using System;
using UnityEngine;
using UnityEngine.Playables;

namespace DoDoEng.Activity.Framework
{
    public abstract class ActivityMain<T> : ActivityBase where T : ActivityData, new()
    {
        // Definition
        private string PATH_DEBUG_VIEWER = "prefab/DebugTableViewer";

        // Properties
        public bool Show_TableDebugViewer
        {
            get => tableDebugViewer != null;
            set => showTables(value);
        }



        // Properties : for concrete
        protected TableVersion VERSION { get; private set; }
        protected ActivityList CURRICULUM { get; private set; }
        protected T[] TABLES { get; private set; }



        // Fields
        private DebugTableViewer tableDebugViewer;

        // Functions
        private void showTables(bool show)
        {
            if (show)
            {
                var pb = Resources.Load<GameObject>(PATH_DEBUG_VIEWER);
                var go = Instantiate(pb, transform);
                tableDebugViewer = go.GetComponent<DebugTableViewer>();
                tableDebugViewer.ShowTable(VERSION, CURRICULUM, TABLES);
            }
            else Destroy(tableDebugViewer.gameObject);
        }

        // Overrides
        protected override async UniTask onPrepare(ActivityIndex actIDX)
        {
            await base.onPrepare(actIDX);

            var result = await ActivityTableLoader.One.LoadTable<T>(actIDX);
            VERSION = result.Version;
            CURRICULUM = result.List;
            TABLES = result.Tables;
        }
#if !DISABLE_SRDEBUGGER
        protected override void onBuildOption(DynamicOptionContainer srOptionContainer)
        {
            base.onBuildOption(srOptionContainer);

            srOptionContainer.AddOption(
                OptionDefinition.Create("Show Table",
                () => this.Show_TableDebugViewer,
                (newValue) => this.Show_TableDebugViewer = newValue,
                "Activity", 400));
        }
#endif
    }
}