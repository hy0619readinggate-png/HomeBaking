using DoDoEng.Common;
using UnityEngine;


namespace DoDoEng
{
    public class TestCoin : MonoBehaviour
    {
        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private CoinMGR coinMGR = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                coinMGR.StartGetCoin(5);
            }
        }
    }
}