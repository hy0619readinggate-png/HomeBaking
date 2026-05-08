using beyondi.Coroutine;
using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Activity.C1_A03
{
    public class RepairMGR : MonoBehaviour
    {
        // Methods
        public Coroutine StartRepair(int repairID)
        {
            LOG.Info($"StartRepair() | {repairID}", this);

            popup.gameObject.SetActive(true);

            stopRepair();
            currentRepairAct = repairActs[repairID - 1];
            crRepair = StartCoroutine(coRepair(currentRepairAct));
            return crRepair;
        }
        public Coroutine FinishRepair(bool closePopup)
        {
            LOG.Info($"FinishRepair() | {closePopup}", this);
            stopRepair();

            crRepairFinish = StartCoroutine(coRepairFinish(closePopup));
            return crRepairFinish;
        }



        // Fields
        private RepairActBase currentRepairAct = null;
        private Coroutine crRepair = null;
        private Coroutine crRepairFinish = null;

        // Functions
        private void stopRepair()
        {
            if (crRepair != null)
                StopCoroutine(crRepair);
            if (crRepairFinish != null)
                StopCoroutine(crRepairFinish);
            crRepair = null;
            crRepairFinish = null;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Animator popup = null;
        [SerializeField] private RepairActBase[] repairActs = null;


        // Unity Messages
        private void Awake()
        {
            repairActs.ForEach(r => r.Hide());
        }
        private void Start()
        {

        }

        // Unity Coroutine
        IEnumerator coRepair(RepairActBase repairAct)
        {
            repairAct.Show();
            popup.SetBool("Show", true);
            yield return new WaitForSeconds(1f);
            yield return null;

            repairAct.StartRepair();
            yield return new WaitForComplete(this, repairAct);
            yield return null;
        }
        IEnumerator coRepairFinish(bool closePopup)
        {
            if (closePopup)
            {
                popup.SetBool("Show", false);
                yield return new WaitForSeconds(1f);

                currentRepairAct.Hide();
                yield return null;
            }
            else yield return new WaitForSeconds(1f);
        }
    }
}