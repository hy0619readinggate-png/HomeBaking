using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Common
{
    [RequireComponent(typeof(Image))]
    public class ImageAlphaHit : MonoBehaviour
    {
        // Fields : caching
        private Image image_ = null;
        private Image image => image_ ??= GetComponent<Image>();



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private float alphaHitTestMinThreshold = 0.5f;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
            image.alphaHitTestMinimumThreshold = alphaHitTestMinThreshold;
        }
    }
}