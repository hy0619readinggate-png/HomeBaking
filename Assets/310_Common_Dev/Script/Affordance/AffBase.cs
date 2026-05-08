using beyondi.Util;
using System;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Common
{
    public abstract class AffBase : MonoBehaviour
    {
        // Properties
        public bool EnableAff
        {
            get => enableAff;
            set
            {
                enableAff = value;

                if (!enableAff)
                    AbortAffordance();
            }
        }
        public Func<bool> Enabler;

        // Methods
        public void StartAffordance()
        {
            if (gameObject.activeInHierarchy 
                && !isRunning
                && enableAff 
                && (Enabler == null || Enabler.Invoke()))
            {
                isRunning = true;
                affCR = StartCoroutine(coStartAff());
            }
        }
        public void AbortAffordance()
        {
            this.StopCoroutineSafe(ref affCR);
            finishAff();
        }

        // Events
        public event Action<GameObject> OnAffStart;
        public event Action<GameObject> OnAffFinish;



        // Fields
        private Coroutine affCR = null;
        private bool enableAff = true;
        private bool isRunning = false;

        // Functions
        protected GameObject affTargetGO => targetGO;
        protected void finishAff()
        {
            if (isRunning)
            {
                if (gameObject.activeInHierarchy && enableAff)
                    StartCoroutine(coFinishAff());
                isRunning = false;
            }
        }

        // Overrides
        protected abstract IEnumerator onStartAff();
        protected abstract IEnumerator onFinishAff();



        // Unity Inspectors
        [Header("¡Ú AffBase")]
        [SerializeField] private GameObject targetGO = null;
        [SerializeField] private float duration = 2f;

        // Unity Messages
        protected virtual void Awake()
        {
            AffordanceMGR.One.RegisterAff(this);
        }
        protected virtual void OnDestroy()
        {
            AffordanceMGR.One?.UnregisterAff(this);
        }

        // Unity Coroutine
        IEnumerator coStartAff()
        {
            OnAffStart?.Invoke(targetGO);

            yield return onStartAff();
            yield return null;

            if (duration > 0)
            {
                yield return new WaitForSeconds(duration);

                finishAff();
                yield return null;
            }
        }
        IEnumerator coFinishAff()
        {
            OnAffFinish?.Invoke(targetGO);

            yield return onFinishAff();
        }
    }
}