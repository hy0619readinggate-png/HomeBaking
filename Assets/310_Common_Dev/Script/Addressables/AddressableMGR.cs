using beyondi.Behaviour;
using Cysharp.Threading.Tasks;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Exceptions;

namespace DoDoEng.Common
{
    public enum HostServerType
    {
        Production,
        Test,
        Development
    }

    public class AddressableMGR : BYDSingleton<AddressableMGR>
    {
        // Properties
        public static string HostURL { get; private set; }
        public static HostServerType HostServerType { get; private set; }

        // Methods
        public static string GetErrorMessage(AsyncOperationHandle handle)
        {
            RemoteProviderException remoteException;

            var e = handle.OperationException;
            while (e != null)
            {
                remoteException = e as RemoteProviderException;
                if (remoteException != null)
                    return remoteException.WebRequestResult.Error;

                e = e.InnerException;
            }

            return handle.OperationException.Message;
        }
        public void SwitchTo(HostServerType serverType, bool clearCache = true)
        {
            LOG.Addressable($"SwitchTo() | {serverType}", this);

            var host = hosts.SingleOrDefault(h => h.ServerType == serverType);
            if (host != null)
            {
                HostURL = host.URL;
                HostServerType = serverType;
                LOG.Addressable($"Host : {HostServerType} | {HostURL}", this);
            }

            if (clearCache)
                ClearCache();
        }
        public void ClearCache()
        {
            LOG.Addressable($"ClearCache()", this);

            var path = Path.Combine(Application.persistentDataPath, "com.unity.addressables");
            if (Directory.Exists(path))
                Directory.Delete(path, true);

            LOG.Addressable($"Delete : {path}", this);

            //AssetBundle.UnloadAllAssetBundles(false);
            //LOG.Addressable("Unload all asset bundles.", this);

            // if (Caching.ClearCache())
            //     LOG.Addressable("Successfully cleaned all caches.", this);
            // else LOG.Addressable("Cache was in use.", this);
        }

        // Methods
        public async UniTask UpdateCatalogWithCheck()
        {
            using (LOG.Coroutine($"UpdateCatalogWithCheck()", this))
            {
                var hCheck = Addressables.CheckForCatalogUpdates();
                await hCheck.Task;
                var catalogs = hCheck.Result;

                if (catalogs.Count > 0)
                {
                    foreach (var c in catalogs)
                        LOG.Addressable($"Catalog to update : {c}", this);

                    var hUpdate = Addressables.UpdateCatalogs(catalogs);
                    await hUpdate.Task;

                    Addressables.Release(hUpdate);
                }
                else LOG.Addressable($"No catalog to update", this);

                Addressables.Release(hCheck);
            }
        }
        public async UniTask UpdateCatalog()
        {
            using (LOG.Coroutine($"UpdateCatalog()", this))
            {
                var hUpdate = Addressables.UpdateCatalogs(true, null, true);
                await hUpdate.Task;
            }
        }

        // Methods
        static AddressableMGR()
        {

        }



        // Functions
        private void editWebRequestURL(UnityWebRequest request)
        {
            LOG.Addressable($"REQ Before: {request.url}", this);
            request.url = request.url.Replace(profileRemoteHostURL, HostURL);
            LOG.Addressable($"REQ : {request.url}", this);
        }

        // Event Handlers
        private void customExceptionHandler(AsyncOperationHandle handle, Exception exception)
        {
            LOG.Warning($"customExceptionHandler() | {exception.Message}", this);
        }


        // Unity Inspectors
        [Header("�� Config")]
        [SerializeField] private HostServerTypeToURL[] hosts = null;
        [SerializeField] private HostServerType defaultHostType = HostServerType.Development;
        [SerializeField] private string profileRemoteHostURL = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            // https://docs.unity3d.com/Packages/com.unity.addressables@1.19/manual/TransformInternalId.html
            Addressables.WebRequestOverride = editWebRequestURL;

            SwitchTo(defaultHostType, false);
        }
        private void Start()
        {
            ResourceManager.ExceptionHandler = customExceptionHandler;
        }
    }

    [Serializable]
    public class HostServerTypeToURL
    {
        public HostServerType ServerType;
        public string URL;
    }
}