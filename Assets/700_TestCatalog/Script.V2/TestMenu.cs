using DoDoEng.Common;
using DoDoEng.Launcher;
using Udar.SceneManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DoDoEng.Tester
{
    public class TestMenu : MonoBehaviour
    {
        // Event Handlers
        private async void launcheerProdServerBTN_OnClick()
        {
            LOG.Function(this);

            SystemUI.One.TestServerModeMarker = false;
            API.One.SwitchTo(APIServer.Production);
            AddressableMGR.One.SwitchTo(HostServerType.Production, false);
            await AddressableMGR.One.UpdateCatalog();

            SceneManager.LoadScene(launcherScene.BuildIndex);
        }
        private async void launcherTestServerBTN_OnClick()
        {
            LOG.Function(this);

            SystemUI.One.TestServerModeMarker = true;
            API.One.SwitchTo(APIServer.Test);
            AddressableMGR.One.SwitchTo(HostServerType.Test, false);
            await AddressableMGR.One.UpdateCatalog();

            SceneManager.LoadScene(launcherScene.BuildIndex);
        }
        private async void catalogProdServerBTN_OnClick()
        {
            LOG.Function(this);

            SystemUI.One.TestServerModeMarker = false;
            API.One.SwitchTo(APIServer.Production);
            AddressableMGR.One.SwitchTo(HostServerType.Production, false);
            await AddressableMGR.One.UpdateCatalog();

            SceneManager.LoadScene(catalogScene.BuildIndex);
        }
        private async void catalogTestServerBTN_OnClick()
        {
            LOG.Function(this);

            SystemUI.One.TestServerModeMarker = true;
            API.One.SwitchTo(APIServer.Test);
            AddressableMGR.One.SwitchTo(HostServerType.Test, false);
            await AddressableMGR.One.UpdateCatalog();

            SceneManager.LoadScene(catalogScene.BuildIndex);
        }
        private void clearCacheBTN_OnClick()
        {
            LOG.Function(this);

            DataLoader.One.ReleaseHandles();
            AddressableMGR.One.ClearCache();
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Button launcherProdServerBTNmainTestServerBTN = null;
        [SerializeField] private Button launcherTestServerBTN = null;
        [SerializeField] private Button catalogProdServerBTN = null;
        [SerializeField] private Button catalogTestServerBTN = null;
        [SerializeField] private Button clearCacheBTN = null;
        [Header("★ Config")]
        [SerializeField] private SceneField launcherScene = null;
        [SerializeField] private SceneField catalogScene = null;

        // Unity Messages
        private void Awake()
        {
            launcherProdServerBTNmainTestServerBTN.onClick.AddListener(launcheerProdServerBTN_OnClick);
            launcherTestServerBTN.onClick.AddListener(launcherTestServerBTN_OnClick);
            catalogProdServerBTN.onClick.AddListener(catalogProdServerBTN_OnClick);
            catalogTestServerBTN.onClick.AddListener(catalogTestServerBTN_OnClick);
            clearCacheBTN.onClick.AddListener(clearCacheBTN_OnClick);

            if (TestCatalogV2.One != null)
                Destroy(TestCatalogV2.One.gameObject);
        }
        private void Start()
        {
        }
    }
}