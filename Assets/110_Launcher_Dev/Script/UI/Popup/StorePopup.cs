using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using Newtonsoft.Json.Linq;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

namespace DoDoEng.Launcher.UI
{
    public class StorePopup : PopupBase<SimplePopupResult>
    {
        // Definitions
        private static string PRODUCT_PASS = "hidodo_1month";

        // Methods
        public async UniTask<SimplePopupResult> ShowPopup()
        {
            LOG.Function(this);

            return await showPopup();
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Event Handlers
        private void purchasePassBTN_OnClicked()
        {
            LOG.Function(this);

            cg.blocksRaycasts = false;

            InAppPurchaseMGR.One.PurchaseProduct(PRODUCT_PASS, (bool success, Product product, string message) => onPurchaseResult(success, product, message).Forget());
        }
        private void showPassBTN_OnClicked()
        {
            LOG.Function(this);


        }
        private async UniTask onPurchaseResult(bool success, Product product, string message)
        {
            LOG.Function(this, $"{success}, {message}");

            if (success)
            {
                var receipt = JObject.Parse(product.receipt);
                var quantity = 1;
                var orderId = "";
                try
                {
                    var payloadStr = receipt.Value<string>("Payload");
                    //var payloadStr = "{\"json\":\"{\\\"orderId\\\":\\\"GPA.3351-0770-9103-98833\\\",\\\"packageName\\\":\\\"com.readinggate.hidodo\\\",\\\"productId\\\":\\\"hidodo_1month\\\",\\\"purchaseTime\\\":1721796977076,\\\"purchaseState\\\":0,\\\"purchaseToken\\\":\\\"mhicaoppkmbmcjpknbjncckn.AO-J1Oy5_5fOoM5X8OTLafuT7TWS1eELv8ovOu9QZ2zXaPEOCYv-TlGdaK1qUSwh3Z7kvqr0mO2ZDpDDe3AWCfAQd29tVXLdfP4_obG0eMwqzlXuEKodhUQ\\\",\\\"quantity\\\":3,\\\"acknowledged\\\":false}\",\"signature\":\"jmi9ikquPvK6SekGwNkGea6IY5p9P75sKaLyt49K2kahBeCdevg1G7lW4lvMUcPbBpfgk8J73ltf7dxVkCdDJvor3sTdIBFdptC4pRy+YBpk3CkPtWAsm8bxOLheXRV3hldr6K5IyqZQWFkQNdRNBCjkHUc1Yv2Mss+ctqQKge7CjhqqezqCgvMmnYmM1pziDc1PE0USEnmI38/2XTmhbl23a3R+jybQGU38skUkgUZHTwS1Yvs3ueJWewJ6YEXvPnNYcujKL0jcbe9wKDo2JJk4Dxxe7rbKUE+DTa4CCBUYQZuzMT9KXhTu+IAZzIDj8yHDi1OdybuHODGcytknRA==\",\"skuDetails\":[\"{\\\"productId\\\":\\\"pass_month01\\\",\\\"type\\\":\\\"inapp\\\",\\\"title\\\":\\\"\\\\ud558\\\\uc774\\\\ub3c4\\\\ub3c4 1\\\\uac1c\\\\uc6d4 \\\\uc774\\\\uc6a9\\\\uad8c (com.readinggate.hidodo (unreviewed))\\\",\\\"name\\\":\\\"\\\\ud558\\\\uc774\\\\ub3c4\\\\ub3c4 1\\\\uac1c\\\\uc6d4 \\\\uc774\\\\uc6a9\\\\uad8c\\\",\\\"description\\\":\\\"\\\\ud558\\\\uc774\\\\ub3c4\\\\ub3c4 1\\\\uac1c\\\\uc6d4 \\\\uc774\\\\uc6a9\\\\uad8c\\\\uc785\\\\ub2c8\\\\ub2e4.\\\",\\\"price\\\":\\\"\\\\u20a930,000\\\",\\\"price_amount_micros\\\":\\\"30000000000\\\",\\\"price_currency_code\\\":\\\"KRW\\\"}\"]}";
                    var payload = JObject.Parse(payloadStr);
                    var jsonStr = payload.Value<string>("json");
                    var json = JObject.Parse(jsonStr);
                    quantity = json.Value<int>("quantity");
                    orderId = json.Value<string>("orderId");
                }
                catch (Exception ex) { LOG.Warning(ex.Message, this); }
#if UNITY_EDITOR
                await LMS.One.SaveUserTicket(9, receipt.Value<string>("TransactionID"), quantity);
#else
#if UNITY_ANDROID
                    await LMS.One.SaveUserTicket(9, receipt.Value<string>("TransactionID"), quantity, orderId);
#elif UNITY_IOS
                    await LMS.One.SaveUserTicket(10, receipt.Value<string>("TransactionID"), quantity);
#endif
#endif
                await SystemUI.One.MessagePU.ShowPopupOK("PAYMENT_3");
                CloseWithResult(SimplePopupResult.Okay);
            }
            else
            {
                SystemUI.One.MessagePU.ShowPopupOK("PAYMENT_5").Forget();
                CloseWithResult(SimplePopupResult.Back);
            }

            cg.blocksRaycasts = true;
        }


        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Button purchasePassBTN = null;
        [SerializeField] private Button showPassBTN = null;

        // Unity Messages
        private void Awake()
        {
            purchasePassBTN.onClick.AddListener(() => purchasePassBTN_OnClicked());
            showPassBTN.onClick.AddListener(showPassBTN_OnClicked);
        }
        private void Start()
        {

        }
    }
}