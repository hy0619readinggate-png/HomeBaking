using beyondi.Coroutine;
using beyondi.Util;
using DoDoEng.Common;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C2_A12
{
    public class Problem : MonoBehaviour, ICompletable
    {
        // Properties
        public Subject[] Subjects => subjects;

        // Methods
        public void Setup(ProblemData pData)
        {
            LOG.Info($"Setup() | {string.Join(", ", pData.Subjects.Select(ex => ex.Word))}]", this);

            subjects.ForEach((i, s) => s.Setup(pData.Subjects[i]));
        }



        // Fields : caching
        private Subject[] subjects_ = null;
        private Subject[] subjects => subjects_ ??= GetComponentsInChildren<Subject>(true);



        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }



        // Interface : ICompletable
        bool ICompletable.IsComplete => subjects.Where(s => !s.IsComplete).Count() == 0;
    }
}