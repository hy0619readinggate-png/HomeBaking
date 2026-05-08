using UnityEngine;

namespace DoDoEng.Activity.C1_A01
{
    public class TraceHandle : MonoBehaviour
    {
        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;

        }
    }
}