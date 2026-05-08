using System.Collections;
using UnityEngine;


namespace beyondi.Coroutine
{
    public class WaitForComplete : CustomYieldInstruction
    {
        // Properties
        public ICompletable Completed { get; private set; } = null;

        // Methods : ctor.
        public WaitForComplete(MonoBehaviour mono, params ICompletable[] completables)
        {
            Completed = null;
            this.completables = completables;
            mono.StartCoroutine(coWait());
        }



        // Fields
        private ICompletable[] completables;

        // Overrides
        public override bool keepWaiting { get { return Completed == null; } }



        // Unity Coroutine
        IEnumerator coWait()
        {
            while (Completed == null)
            {
                foreach (var c in completables)
                {
                    if (c.IsComplete)
                        Completed = c;
                }

                yield return null;
            }
        }
    }

    public interface ICompletable
    {
        bool IsComplete { get; }
    }
}