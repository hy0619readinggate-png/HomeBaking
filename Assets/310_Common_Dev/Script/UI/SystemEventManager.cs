using System;

namespace DoDoEng.Common
{
    public enum SystemButtonType
    {
        None, Home,
        Back,
        Pause,
        Resume,
        Speaker,

        // Activity
        Debug_Next = -1,
        Debug_NextStep = -2,
        Debug_NextProblem = -3,
        Debug_PrevProblem = -4,
        Debug_ForceFinish = -5,

        // EBook
        Debug_NextLayer = -11,
        Debug_PrevLayer = -12,
    };

    public class SystemEventManager
    {
        public static event Action<SystemButtonType> OnSystemButtonClicked;
        public static void SystemButtonClick(SystemButtonType type)
        {
            OnSystemButtonClicked?.Invoke(type);
        }
    }
}