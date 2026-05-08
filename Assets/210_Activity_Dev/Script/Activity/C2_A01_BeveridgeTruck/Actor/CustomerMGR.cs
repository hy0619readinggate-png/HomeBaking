using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C2_A01
{
    public class CustomerMGR : MonoBehaviour
    {
        // Methods
        public void Setup(int characterID)
        {
            LOG.Info($"Select() | {characterID}", this);

            this.activeCustomerID = characterID;

            characters.ForEach((i, c) => c.gameObject.SetActive(i == characterID));
        }
        public void Idle()
        {
            LOG.Info($"Idle()", this);

            activeCharacter.Idle();
        }
        public void Wrong()
        {
            LOG.Info($"Wrong()", this);

            AudioMGR.One.PlayEffect(worngClip);
            activeCharacter.Wrong();
        }



        // Fields
        private int activeCustomerID = 0;

        // Functions
        private Character activeCharacter => characters[activeCustomerID];



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Character[] characters = null;
        [Header("★ Audios")]
        [SerializeField] private AudioClip worngClip = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}