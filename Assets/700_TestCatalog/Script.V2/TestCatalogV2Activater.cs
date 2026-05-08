using UnityEngine;

namespace DoDoEng.Tester
{
    public class TestCatalogV2Activater : MonoBehaviour
    {
        // Unity Messages
        private void Awake()
        {

        }
        private void Start()
        {
            TestCatalogV2.One.Activate(true);
        }
        private void OnDestroy()
        {
            TestCatalogV2.One?.Activate(false);
        }
    }
}