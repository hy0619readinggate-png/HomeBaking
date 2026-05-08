using beyondi.Util;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace DoDoEng.Activity.C3_A09
{
    public class Tray : MonoBehaviour
    {
        // Definitions
        private const int SlotMinCount = 2;

        // Methods
        public void Setup(ProblemData pData)
        {
            LOG.Info($"Setup()", this);

            this.pData = pData;

            foreach (var key in slots.Keys)
                slots[key].GroupGO.SetActive(key == pData.SubjectCount);

            currentSlots.ForEach((i, s) => s.Setup(pData.Subjects[i]));
            currentTrayWords.ForEach((i, w) => w.Setup(pData.Subjects[i].Text));

            sentenceTXT.text = pData.Sentence;
        }
        public Coroutine StartWaitComplete()
        {
            LOG.Info($"StartWaitComplete()", this);

            crWaitComplete = StartCoroutine(coStartWaitComplete());
            return crWaitComplete;
        }
        public void FinishWaitComplete()
        {
            LOG.Info($"FinishWaitComplete()", this);

            this.StopCoroutineSafe(ref crWaitComplete);
            this.StopCoroutineSafe(ref crWrong);

            foreach (var (word, i) in currentTrayWords.Select((p, i) => (p, i)))
                word.Idle();
        }
        public void HideTrayWord()
        {
            LOG.Info($"HideTrayWord()", this);

            foreach (var (word, i) in currentTrayWords.Select((p, i) => (p, i)))
                word.Hidden();
        }

        // Methods
        public BreadSlot FindSlot(string text)
        {
            LOG.Info($"FindSlot()", this);

            return currentSlots.FirstOrDefault(s => s.Text == text);
        }



        // Fields
        private ProblemData pData = null;
        private Dictionary<int, BreadSlotInfo> slots = new Dictionary<int, BreadSlotInfo>();  // KEY : Bread Count 
        private Coroutine crWaitComplete = null;
        private Coroutine crWrong = null;

        // Functions
        private BreadSlot[] currentSlots => slots[pData.SubjectCount].BreadSlots;
        private TrayWord[] currentTrayWords => slots[pData.SubjectCount].TrayWords;
        private bool isComplete => currentSlots.All(s => s.IsComplete);

        // Event Handlers
        private void breadSlot_onBreadWrong(Bread bread, Transform breadParentTR)
        {
            LOG.Info($"breadSlot_onBreadWrong()", this);

            ActivityProgress.One.Wrong();

            crWrong = StartCoroutine(coBreadWrong(bread, breadParentTR));
        }
        private void breadSlot_onBreadCorrect(Bread bread, Transform breadParentTR)
        {
            LOG.Info($"breadSlot_onBreadCorrect()", this);

            Customer.One.Correct();
            bread.Correct(breadParentTR);

            var trayWord = currentTrayWords.FirstOrDefault(w => w.Text == bread.Text);
            if (trayWord != null)
            {
                trayWord.Show();
                bread.HideText();
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject[] groups = null;
        [SerializeField] private TextMeshProUGUI sentenceTXT = null;
        [SerializeField] private BreadMGR breadMGR = null;

        // Unity Messages
        private void Awake()
        {
            foreach (var (g, i) in groups.Select((g, i) => (g, i)))
            {
                var info = new BreadSlotInfo();
                var groupGO = groups[i];
                var breadSlots = groupGO.GetComponentsInChildren<BreadSlot>(true);
                breadSlots.ForEach(s => s.VfxCorrectTR.SetParent(groupGO.transform));
                var trayWords = groupGO.GetComponentsInChildren<TrayWord>(true);
                info.GroupGO = groupGO;
                info.BreadSlots = breadSlots;
                info.TrayWords = trayWords;
                slots[i + SlotMinCount] = info;
            }

        }
        private void Start()
        {
        }
        private void OnEnable()
        {
            foreach (var key in slots.Keys)
            {
                var slots = this.slots[key].BreadSlots;
                foreach (var slot in slots)
                {
                    slot.onBreadCorrect += breadSlot_onBreadCorrect;
                    slot.onBreadWrong += breadSlot_onBreadWrong;
                }
            }
        }
        private void OnDisable()
        {
            foreach (var key in slots.Keys)
            {
                var slots = this.slots[key].BreadSlots;
                foreach (var slot in slots)
                {
                    slot.onBreadCorrect -= breadSlot_onBreadCorrect;
                    slot.onBreadWrong -= breadSlot_onBreadWrong;
                }
            }
        }

        // Unity Coroutine
        IEnumerator coStartWaitComplete()
        {
            using (LOG.Coroutine($"coStartWaitComplete()", this))
            {
                yield return new WaitUntil(() => isComplete);

                yield return new WaitForSeconds(1);
            }
        }
        IEnumerator coBreadWrong(Bread bread, Transform breadParentTR)
        {
            using (LOG.Coroutine($"coBreadWrong()", this))
            {
                breadMGR.EnableInteraction(false);
                yield return null;

                Customer.One.Wrong();
                yield return bread.Wrong(breadParentTR);
                yield return new WaitForSeconds(0.3f);

                yield return AudioMGR.One.PlayNarrationAndWait(pData.SentenceCLIP);

                breadMGR.EnableInteraction(true);
                yield return null;
            }
        }



        // Inner Class
        private class BreadSlotInfo
        {
            public GameObject GroupGO = null;
            public BreadSlot[] BreadSlots = null;
            public TrayWord[] TrayWords = null;
        }
    }
}