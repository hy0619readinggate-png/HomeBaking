using DoDoEng.Game.C3_G02;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DoDoEng
{
    public class JellyHint : MonoBehaviour
    {
        // Definitions
        // Properties
        [HideInInspector] public List<GameObject> JellyList = new();
        // Methods
        public void ResetJellyHint()
        {
            JellyList.Clear();
        }
        public void SetCorrectHintJelly(int idx)
        {
            JellyList[idx].GetComponent<JellyObject>().IsCorrectHintSet = true;
        }
        // Events



        // Fields : caching
        // Fields
        // Functions
        // Event Handlers
        // Overrides



        // Unity Inspectors
        // Unity Messages
        private void Awake()
        {

        }
        private void Start()
        {

        }

        // Unity Coroutine
    }
}