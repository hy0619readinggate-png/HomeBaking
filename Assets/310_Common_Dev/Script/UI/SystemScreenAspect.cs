using UnityEngine;

namespace DoDoEng
{
    public class SystemScreenAspect : MonoBehaviour
    {
#if UNITY_STANDALONE_WIN
        // Fields
        private int frame = 0;
        private int lastWidth = 0;
        private int lastHeight = 0;



        // Unity Inspectors
        [SerializeField] private int widthAspect = 1920;
        [SerializeField] private int heightAspect = 1080;
        [Tooltip("After every how many frames to rescale the windows size.")]
        [SerializeField] private int updateAspectDelay = 100;

        // Unity Messages
        private void Update()
        {
            frame++;

            if (frame % updateAspectDelay != 0) { return; }

            var width = Screen.width;
            var height = Screen.height;

            if (lastWidth != width) // if the user is changing the width
            {
                // update the height
                float heightAccordingToWidth = (float)width / widthAspect * heightAspect;
                Screen.SetResolution(width, (int)Mathf.Round(heightAccordingToWidth), false);
            }
            else if (lastHeight != height) // if the user is changing the height
            {
                // update the width
                float widthAccordingToHeight = (float)height / heightAspect * widthAspect;
                Screen.SetResolution((int)Mathf.Round(widthAccordingToHeight), height, false);
            }

            lastWidth = width;
            lastHeight = height;
        }
#endif
    }
}