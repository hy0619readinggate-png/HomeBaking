using System.Linq;
using UnityEngine;


namespace beyondi.Coroutine
{
    public class WaitForCompleteAll : CustomYieldInstruction
    {
        // Methods : ctor.
        public WaitForCompleteAll(MonoBehaviour mono, params ICompletable[] completables)
        {
            this.completables = completables;
        }



        // Fields
        private ICompletable[] completables;

        // Overrides
        public override bool keepWaiting { get { return completables.Count(c => !c.IsComplete) > 0; } }
    }
}