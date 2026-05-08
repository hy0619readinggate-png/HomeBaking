using beyondi.Util;
using DoDoEng.Common;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Game.C1_G01
{
    public class Order : MonoBehaviour
    {
        // Methods
        public void Setup(CustomerData data)
        {
            LOG.Info($"Setup() | {data}", this);

            setupItems(data);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }
        public void ShowAnswer()
        {
            LOG.Info($"ShowAnswer()", this);

            showAnswerItems();
        }



        // Fields : caching
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();

        // Functions
        private void setupItems(CustomerData data)
        {
            items.ForEach(it => it.gameObject.SetActive(false));
            thiefOrderGO.SetActive(false);

            if (!data.IsThief)
            {
                for (var i = 0; i < data.OrderIceCreams.Length; i++)
                {
                    var item = items[i];
                    item.Setup(data.OrderIceCreams[i]);
                    item.gameObject.SetActive(true);
                }
            }
            else thiefOrderGO.SetActive(true);
        }
        private void showAnswerItems()
        {
            foreach (var item in items.Where(it => it.gameObject.activeSelf))
                item.ShowAnswer();
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private OrderItem[] items = null;
        [SerializeField] private GameObject thiefOrderGO = null;

        // Unity Messages
        private void Awake()
        {
            thiefOrderGO.SetActive(false);
        }
        private void Start()
        {

        }
    }
}