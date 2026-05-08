using beyondi.Util;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DoDoEng.Game.C3_G02
{
    public class SlotManager : MonoBehaviour
    {
        // Methods
        public void SetSlot(int idx, int jellyNum, int itemNum)
        {
            setSlot(idx, jellyNum, itemNum);
        }
        public List<GameObject> getAllJelly()
        {
            jelly.Clear();
            List<Slot> slotList = new();

            foreach (GameObject g in slotSettings)
            {
                var slot = g.GetComponentInChildren<Slot>();
                slotList.Add(slot);
            }

            var slots = UtilArray.Shuffle(slotList.ToArray());

            for (int i = 0; i < slots.Length; i++)
            {
                var slotJelly = slots[i].GetJelly();
                foreach (var j in slotJelly) jelly.Add(j);
            }

            return jelly;
        }
        public List<GameObject> getAllItem()
        {
            item.Clear();

            List<Slot> slotList = new();

            foreach (GameObject g in slotSettings)
            {
                var slot = g.GetComponentInChildren<Slot>();
                slotList.Add(slot);
            }

            var slots = UtilArray.Shuffle(slotList.ToArray());

            for (int i = 0; i < slots.Length; i++)
            {
                var slotItem = slots[i].GetItem();
                foreach (var j in slotItem) item.Add(j);
            }

            return item;
        }
        public void ClearSlots()
        {
            foreach (GameObject g in slotSettings)
            {
                var slots = g.GetComponentsInChildren<Slot>();
                foreach (Slot s in slots) s.ClearSlot();
            }
        }



        // Fields
        private string[] itemSetValue = null;
        private List<GameObject> jelly = new List<GameObject>();
        private List<GameObject> item = new List<GameObject>();

        // Functions
        private void setSlot(int idx, int jellyNum, int itemNum)
        {
            if (setText[idx] != null)
                itemSetValue = setText[idx].text.Split("\n");

            var rndIdx = UtilArray.Random(0, itemSetValue.Length - 1, 2);

            var rndItemSet = itemSetValue[rndIdx[0]].Split(',');
            var rndJellySet = itemSetValue[rndIdx[1]].Split(',');

            int slotIdx = 0;

            foreach (var ss in slotSettings)
            {
                var ItemSetList = ss.GetComponentsInChildren<Slot>();
                foreach (var Item in ItemSetList) Item.gameObject.SetActive(false);

                int setListIdx = UnityEngine.Random.Range(0, ItemSetList.Length - 1);

                ItemSetList[setListIdx].SetObstaclePosition(Int32.Parse(rndItemSet[slotIdx]), Int32.Parse(rndJellySet[slotIdx]));
                ItemSetList[setListIdx].gameObject.SetActive(true);

                slotIdx++;
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject[] slotSettings;
        [Header("★ SetTexts")]
        [SerializeField] private TextAsset[] setText;

        // Unity Messages
        private void Awake()
        {

        }
        private void Start()
        {

        }
    }
}