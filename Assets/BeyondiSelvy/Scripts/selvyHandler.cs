using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

namespace BeyondiSelvy
{
    using FFmpegUnityBind2;
    public class selvyHandler : IFFmpegCallbacksHandler
    {
        Action<int, long, string> callback = null;
        public selvyHandler(Action<int, long, string> cb)
        {
            callback = cb;
        }


        public void OnStart(long executionId)
        {
            if (callback != null) callback(1, executionId, null);
        }

        public void OnSuccess(long executionId)
        {
            if (callback != null) callback(2, executionId, null);
        }

        public void OnCanceled(long executionId)
        {
            if (callback != null) callback(3, executionId, null);
        }

        public void OnFail(long executionId)
        {
            if (callback != null) callback(4, executionId, null);
        }

        public void OnLog(long executionId, string message)
        {
            if (callback != null) callback(5, executionId, message);
        }

        public void OnWarning(long executionId, string message)
        {
            if (callback != null) callback(-1, executionId, message);
        }

        public void OnError(long executionId, string message)
        {
            if (callback != null) callback(-2, executionId, message);
        }
    }
}