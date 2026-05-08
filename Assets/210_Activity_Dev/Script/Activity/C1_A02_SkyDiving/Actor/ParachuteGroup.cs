using beyondi.Util;
using DoDoEng.Common;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C1_A02
{
    public class ParachuteGroup : MonoBehaviour
    {
        // Methods
        public void Setup(ExampleData[] exams, int characterID)
        {
            LOG.Info($"Setup() | {characterID}", this);

            foreach (var (p, i) in parachutes.Select((p, i) => (p, i)))
                p.Setup(exams[i], characterID);
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }
        public void Show()
        {
            LOG.Info($"Show()", this);

            this.StopCoroutineSafe(ref crShowHide);
            crShowHide = StartCoroutine(coShow());
        }
        public void Correct()
        {
            LOG.Info($"Correct()", this);

            parachutes.ForEach(c => c.Correct());
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Parachute[] parachutes_ = null;
        private Parachute[] parachutes => parachutes_ ??= GetComponentsInChildren<Parachute>(true);

        // Fields
        private Coroutine crShowHide = null;



        // Unity Inspectors
        [Header("★ Timing")]
        [SerializeField] private float showInterval = 0.3f;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coShow()
        {
            using (LOG.Coroutine($"coShow()", this))
            {
                var shuffledChutes = UtilArray.Extract(parachutes, parachutes.Length);
                foreach (var chute in shuffledChutes)
                {
                    chute.Show();
                    yield return new WaitForSeconds(showInterval);
                }
            }
        }
    }
}