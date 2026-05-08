using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

namespace DoDoEng.Common
{
    public abstract class PopupBase<T> : MonoBehaviour where T : struct, IConvertible
    {
        // Properties 
        public T Result { get; private set; }

        // Methods
        public void CloseWithResult(T result = default(T))
        {
            LOG.Info($"CloseWithResult() | {result}", this);

            closePopup(result);
        }



        // Virtual
        protected virtual void onOpen() { }
        protected virtual void onClose(T result) { }
        protected virtual IEnumerator onClosing() { yield return null; }



        // Functions : for concrete
        protected async UniTask<T> showPopup()
        {
            gameObject.SetActive(true);
            onOpen();

            if (pauseAtShown)
                SystemManager.Pause();

            Result = default(T);
            await UniTask.WaitUntil(() => !Result.Equals(default(T)));

            return Result;
        }
        protected void closePopup(T result)
        {
            StartCoroutine(coClose(result));
        }

        // Functions : timeline
        protected void evaluateTimeline(PlayableDirector timeline, double time = 0)
        {
            timeline.time = time;
            timeline.Evaluate();
            timeline.Stop();
        }
        protected IEnumerator playTimeline(PlayableDirector timeline, float delay = 0.3f)
        {
            timeline.time = 0;
            timeline.Play();
            yield return new WaitForSeconds((float)timeline.duration);
            yield return new WaitForSeconds(delay);
        }
        protected IEnumerator stopTimeline(PlayableDirector timeline)
        {
            timeline.time = timeline.duration;
            timeline.Evaluate();
            timeline.Stop();
            yield return null;
        }


        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private bool pauseAtShown = true;
        [SerializeField] private bool leaveAsShownAtClose = false;
        [SerializeField] private bool resumeAtClose = true;

        // Unity Coroutine
        IEnumerator coClose(T result)
        {
            yield return onClosing();
            yield return null;

            Result = result;

            if (pauseAtShown && resumeAtClose)
                SystemManager.Resume();

            if (!leaveAsShownAtClose)
                gameObject.SetActive(false);
            onClose(result);
            yield return null;
        }
    }
}