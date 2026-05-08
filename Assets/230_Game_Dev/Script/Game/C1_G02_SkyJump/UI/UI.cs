using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Game.C1_G02
{
    public class UI : MonoBehaviour
    {
        // Methods
        public void ShowBoostEffect()
        {
            LOG.Info($"ShowBoostEffect()", this);

            windFxGO.SetActive(true);
        }
        public void ShowCompleteEffect()
        {
            LOG.Info($"ShowCompleteEffect()", this);

            AudioMGR.One.PlayEffect(completeCLIP);
            completeFxGO.SetActive(true);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject windFxGO = null;
        [SerializeField] private GameObject completeFxGO = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip completeCLIP = null;

        // Unity Messages
        private void Awake()
        {
            windFxGO.SetActive(false);
            completeFxGO.SetActive(false);
        }
        private void Start()
        {
        }
    }
}