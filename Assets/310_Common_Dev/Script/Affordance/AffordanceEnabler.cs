using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// devBOX - 삭제해야 함
#pragma warning disable 0414

namespace DoDoEng.Common
{
    public class AffordanceEnabler : MonoBehaviour
    {
        // Definitions
        // Properties
        public int EnableID { get; set; }

        // Methods
        // Events



        // Fields : caching
        private AffBase[] affs_ = null;
        private AffBase[] affs => affs_ ??= GetComponents<AffBase>();

        // Fields
        // Functions
        // Event Handlers
        private bool enableF(int i)
        {
            return false;
        }
        // Overrides



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private bool randomEnable;

        // Unity Messages
        private void Awake()
        {

        }
        private void Start()
        {

        }
        private void OnEnable()
        {
            affs.ForEach((i, aff) => aff.Enabler = () => enableF(i));
        }

        // Unity Coroutine
    }
}