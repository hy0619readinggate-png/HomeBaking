using beyondi.Util;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C1_A05
{
    public class BarClamp : MonoBehaviour,
        IDropHandler, IID
    {
        // Properties
        public bool IsPlateLoaded => plateGO.activeSelf;

        // Methods
        public void Reset()
        {
            LOG.Info($"Reset()", this);

            plateGO.SetActive(false);
            affGO.SetActive(true);
        }

        // Events
        public event Action<BarClamp> OnCorrect;
        public event Action<BarClamp> OnWrong;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject plateGO = null;
        [SerializeField] private GameObject affGO = null;

        // Unity Messages
        private void Awake()
        {
            plateGO.SetActive(false);
            affGO.SetActive(true);
        }
        private void Start()
        {
        }



        // Interface : IDropHandler
        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            LOG.Info($"OnDrop() | {eventData.pointerDrag.name}", this);

            var plate = eventData.pointerDrag.GetComponent<Plate>();
            if (plate != null)
            {
                if (!IsPlateLoaded)
                {
                    if (plate.IsAnswer)
                    {
                        plateGO.SetActive(true);
                        affGO.SetActive(false);

                        plate.Correct();
                        OnCorrect?.Invoke(this);
                    }
                    else
                    {
                        ActivityProgress.One.Wrong();

                        plate.Wrong(transform, ID == 1 ? Plate.OutDirection.L : Plate.OutDirection.R);
                        OnWrong?.Invoke(this);
                    }

                    eventData.Use();
                }
            }
        }

        // Interface : IID
        public int ID { get; set; }
    }
}