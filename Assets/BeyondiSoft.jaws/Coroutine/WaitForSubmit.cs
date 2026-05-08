using System.Collections;
using UnityEngine;


// Reference
// http://answers.unity.com/answers/1651736/view.html
// https://pastebin.com/xAfP1bCN

namespace beyondi.Coroutine
{
    public class WaitForSubmit : CustomYieldInstruction
    {
        // Properties
        public ISubmitable Submited { get; private set; } = null;

        // Methods : ctor.
        public WaitForSubmit(MonoBehaviour mono, params ISubmitable[] submitables)
        {
            Submited = null;
            this.submitables = submitables;
            mono.StartCoroutine(coWait());
        }



        // Fields
        private ISubmitable[] submitables;

        // Overrides
        public override bool keepWaiting { get { return Submited == null; } }



        // Unity Coroutine
        IEnumerator coWait()
        {
            while (Submited == null)
            {
                foreach (var s in submitables)
                {
                    if (s.IsSubmit)
                        Submited = s;
                }

                yield return null;
            }
        }
    }

    public interface ISubmitable
    {
        bool IsSubmit { get; }
    }
}