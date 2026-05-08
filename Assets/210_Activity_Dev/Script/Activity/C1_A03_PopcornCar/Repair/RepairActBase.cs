using beyondi.Coroutine;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C1_A03
{
    public class RepairActBase : MonoBehaviour, ICompletable
    {
        // Methods
        public void Show()
        {
            LOG.Info($"Show()", this);

            gameObject.SetActive(true);
        }
        public void Hide()
        {
            LOG.Info($"Hide()", this);

            gameObject.SetActive(false);
        }

        public void StartRepair()
        {
            onStartPepair();
        }
        public void FinishRepair()
        {
            onFinishPepair();
        }



        // Virtual
        protected virtual void onStartPepair() { }
        protected virtual void onFinishPepair() { }



        // Fields
        protected bool isComplete = false;



        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }



        // Interface : ICompletable
        bool ICompletable.IsComplete => isComplete;
    }
}