using beyondi.Behaviour;
using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace DoDoEng.Common
{
    public class InAppPurchaseMGR : BYDSingleton<InAppPurchaseMGR>, IDetailedStoreListener
    {
        // Methods
        public void PurchaseProduct(string productID, Action<bool, Product, string> onPurchased)
        {
            LOG.Info($"PurchaseProduct() | {productID}", this);

            var product = storeController.products.WithID(productID);
            if (product != null && product.availableToPurchase)
            {
                this.purchaseCallback = onPurchased;
                storeController.InitiatePurchase(product);
            }
            else LOG.Error($"Product[${productID}] is not exist or is not avavilable.", this);
        }



        // Fields
        private IStoreController storeController;
        
        // Fields
        private Action<bool, Product, string> purchaseCallback;

        // Functions
        private async void initialize()
        {
            LOG.Function(this);

            var success = await initUnityServices();
            if (success)
            {
                initUnityIAP();
            }
            else LOG.Error($"Can't init UnityIAP for fail of UnityServices", this);
        }
        private async Task<bool> initUnityServices()
        {
            LOG.Function(this);

            try
            {
                var options = new InitializationOptions();
                options.SetEnvironmentName(environment);

                await UnityServices.InitializeAsync(options);
                LOG.Info($"UnityServices Initialized", this);

                return true;
            }
            catch (Exception exception)
            {
                LOG.Error($"Initializing UnityServices is failed : {exception.Message}", this);
                return false;
            }
        }
        private void initUnityIAP()
        {
            LOG.Function(this);

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            var catalog = ProductCatalog.LoadDefaultCatalog();

            foreach (var product in catalog.allProducts)
            {
                ProductType type = (ProductType)product.type;

                // 스토어 ID 매핑 (있으면)
                StoreSpecificIds ids = null;

                if (product.allStoreIDs != null && product.allStoreIDs.Count > 0)
                {
                    ids = new StoreSpecificIds();

                    foreach (var storeID in product.allStoreIDs)
                    {
                        ids.Add(storeID.id, storeID.store);
                    }
                }

                if (ids != null)
                    builder.AddProduct(product.id, type, ids);
                else
                    builder.AddProduct(product.id, type);
            }


            UnityPurchasing.Initialize(this, builder);
        }

        // Functions
        private async void processPurchasing(Product product)
        {
            // LMS 서버에 검증 및 구매 확인 요청
            await Task.Delay(1000);

            // 성공시
            finishPurchasingWith(product, true);

            // 실패시
            //purchaseFinishWith(product, false, "에러 메시지");
        }
        private void finishPurchasingWith(Product product, bool success, string message = null)
        {
            if (success)
                storeController.ConfirmPendingPurchase(product);

            purchaseCallback?.Invoke(success, product, message);
            purchaseCallback = null;
        }


        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private string environment = "production";

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();
        }
        private void Start()
        {
            initialize();
        }



        // Interface : IDetailedStoreListener
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            LOG.Function(this);

            this.storeController = controller;
        }
        public void OnInitializeFailed(InitializationFailureReason error)
        {
            LOG.Error($"OnInitializeFailed() | {error}", this);
        }
        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            LOG.Error($"OnInitializeFailed() | {error}, {message}", this);
        }
        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            LOG.Error($"OnPurchaseFailed()", this);
            finishPurchasingWith(product, false, failureDescription.message);
        }
        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            LOG.Error($"OnPurchaseFailed() | {failureReason}", this);
            finishPurchasingWith(product, false, failureReason.ToString());
        }
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
        {
            LOG.Function(this);

            var product = purchaseEvent.purchasedProduct;
            LOG.Info($"transactionID : {product.transactionID}", this);
            LOG.Info($"metadata : {product.metadata}", this);
            LOG.Info($"id : {product.definition.id}", this);
            LOG.Info($"payout : {product.definition.payout}", this);
            LOG.Info($"receipt : {product.receipt}", this);

            // LMS 서버에 검증 및 구매 확인 요청
            processPurchasing(product);

            return PurchaseProcessingResult.Pending;
        }
    }
}