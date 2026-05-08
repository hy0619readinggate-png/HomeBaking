using System;

namespace beyondi.Util
{
    public static class InfiniteLoopDetector
    {
        private static string prevPoint = "";
        private static int detectionCount = 0;
        private const int DetectionThreshold = 100000;

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Run(
            int? thresholdCount = null,
            [System.Runtime.CompilerServices.CallerMemberName] string mn = "",
            [System.Runtime.CompilerServices.CallerFilePath] string fp = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int ln = 0
        )
        {
            var currentPoint = $"{fp}:{ln}, {mn}()";

            if (prevPoint == currentPoint)
                detectionCount++;
            else detectionCount = 0;

            var threshold = thresholdCount ?? DetectionThreshold;
            if (detectionCount > threshold)
                throw new Exception($"Infinite Loop Detected: \n{currentPoint}\n\n");

            prevPoint = currentPoint;
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void Init()
        {
            UnityEditor.EditorApplication.update += () =>
            {
                detectionCount = 0;
            };
        }
#endif
    }
}