using DoDoEng.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DoDoEng.Activity.C4_A05
{
    public class BlocksAff : AffBase
    {
        // Methods
        public void Add(GameObject affGO)
        {
            LOG.Info($"Add()", this);

            affObjects.Add(affGO);
        }
        public void Clear()
        {
            LOG.Info($"Clear()", this);

            affObjects.Clear();
        }



        // Fields
        private List<GameObject> affObjects = new List<GameObject>();

        // Overrides
        protected override IEnumerator onStartAff()
        {
            LOG.Info($"onStartAff()", this);

            AudioMGR.One.PlayEffect(affordanceCLIP);
            affObjects.ForEach(a => a.SetActive(true));

            yield return new WaitForSeconds(delay);

            finishAff();
        }
        protected override IEnumerator onFinishAff()
        {
            LOG.Info($"onFinishAff()", this);

            affObjects.ForEach(a => a.SetActive(false));
            yield return null;
        }



        // Unity Inspectors
        [Header("★ Binding")]
        [SerializeField] private AudioClip affordanceCLIP = null;
        [Header("★ Config")]
        [SerializeField] private int delay = 2;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();
        }
        private void Start()
        {
        }
    }
}