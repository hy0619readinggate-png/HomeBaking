using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DoDoEng.Common
{
    public enum FinishAction
    {
        // Common
        Retry, Return, Next,

        // EBook
        EBook_Read, EBook_Record, EBook_MyEBook, EBook_Quiz,

        // Movie
        Movie_Record, Movie_MyMovie
    }
    public abstract class SceneRunnerBase : MonoBehaviour
    {
        // Methods
        public async UniTask Prepare()
        {
            using (LOG.Coroutine($"Prepare()", this))
            {
                await onPrepare();
            }
        }
        public void Run()
        {
            LOG.Info($"Run()", this);

            onRun();
        }



        // Function : for concrete class
        protected void finish(FinishAction finishAction = FinishAction.Return)
        {
            LOG.Info($"finish() | {finishAction}", this);
            onFinish(finishAction);
        }



        // Overrides
        protected virtual async UniTask onPrepare() { await UniTask.Yield(); }
        protected abstract void onRun();
        protected abstract void onFinish(FinishAction finishAction);
    }
}