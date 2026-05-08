using beyondi.Util;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DoDoEng.Playground.Behaviour;

namespace DoDoEng.Common
{
    public class PlayCounter : MonoBehaviour
    {
        // Methods
        public void Activate(bool active)
        {
            LOG.Info($"Activate() | {active}", this);

            gameObject.SetActive(active);
        }



        // Event Handlers
        private void lms_OnChangePlaygroundPlayCount(int count)
        {
            foreach (var (icon, i) in icons.Select((v, i) => (v, i)))
            {
                icon.SetActive(i < count);
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject[] icons;

        // Unity Messages
        private void Awake()
        {
            lms_OnChangePlaygroundPlayCount(LMS.One.PlaygroundPlayCount);
        }
        private void OnEnable()
        {
            LMS.One.OnChangePlaygroundPlayCount += lms_OnChangePlaygroundPlayCount;
        }
        private void OnDisable()
        {
            if (LMS.One != null)
                LMS.One.OnChangePlaygroundPlayCount -= lms_OnChangePlaygroundPlayCount;
        }
    }
}