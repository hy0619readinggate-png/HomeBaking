using System;
using UnityEngine;

namespace DoDoEng.Common
{
    public class TimelineMuteSync_Source : MonoBehaviour
    {
        // Properties
        public bool Mute => !gameObject.activeInHierarchy;

        // Events
        public event Action<TimelineMuteSync_Source> OnMuteChanged;



        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
        private void OnEnable()
        {
            OnMuteChanged?.Invoke(this);
        }
        private void OnDisable()
        {
            OnMuteChanged?.Invoke(this);
        }
    }
}