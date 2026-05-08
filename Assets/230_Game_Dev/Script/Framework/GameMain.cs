using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using SRDebugger;
using UnityEngine;

namespace DoDoEng.Game.Framework
{
    public abstract class GameMain<T0, T1> : GameBase
        where T0 : MonoBehaviour
        where T1 : GameData, new()
    {
        // Definition
        private string PATH_DEBUG_VIEWER = "prefab/DebugTableViewer";

        // Properties
        public static T0 Instance
        {
            get
            {
                if (isApplicationQuitting)
                    return null;

                lock (lockObj)
                {
                    if (instance == null)
                    {
                        instance = GameObject.FindObjectOfType<T0>();
                        if (instance == null)
                            LOG.Error($"can't find object of type '{typeof(T0).Name}'", typeof(GameMain<T0, T1>));
                    }
                }
                return instance;
            }
        }
        public bool Show_TableDebugViewer
        {
            get => tableDebugViewer != null;
            set => showTables(value);
        }




        // Properties : for concrete
        protected TableVersion VERSION { get; private set; }
        protected GameList CURRICULUM { get; private set; }
        protected T1[] TABLES { get; private set; }



        // Fields : singleton
        private static object lockObj = new Object();
        private static bool isApplicationQuitting = false;
        private static T0 instance = null;

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
        protected override async UniTask onPrepare(GameIndex gameIDX)
        {
            await base.onPrepare(gameIDX);

            var result = await GameTableLoader.One.LoadTable<T1>(gameIDX);
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
                "Game", 400));
        }
#endif



        // Unity Messages
        protected virtual void OnApplicationQuit()
        {
            isApplicationQuitting = true;
            instance = null;
        }
    }
}