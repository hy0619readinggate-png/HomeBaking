using beyondi.Util;
using System.Collections.Generic;
using UnityEngine;

namespace DoDoEng.Game.C3_G02
{
    public class Slot : MonoBehaviour
    {
        // Methods
        public void SetObstaclePosition(int itemNum, int jellyNum)
        {
            setObstaclePosition(itemNum, jellyNum);

        }
        public List<GameObject> GetJelly()
        {
            return jelly;
        }
        public List<GameObject> GetItem()
        {
            return item;
        }
        public void ClearSlot()
        {
            clearSlot();
        }



        // Fields
        private List<GameObject> jelly = new List<GameObject>();
        private List<GameObject> item = new List<GameObject>();

        // Functions
        private void setObstaclePosition(int itemNum, int jellyNum)
        {
            item.Clear();
            jelly.Clear();

            int[] rndItemIdx = UtilArray.Random(0, itemSet.Length - 1, itemNum);
            int[] rndJellyIdx = UtilArray.Random(0, jellySet.Length - 1, jellyNum);

            for (int j = 0; j < itemNum; j++)
            {
                int rndNum = Random.Range(0, 2);

                var itemBlock = Instantiate(itemPf[rndNum], itemSet[rndItemIdx[j]].transform);
                item.Add(itemBlock);
                itemBlock.transform.localPosition = Vector3.zero;
            }

            for (int i = 0; i < jellyNum; i++)
            {
                var jellyBlock = Instantiate(jellyPf, jellySet[rndJellyIdx[i]].transform);
                jelly.Add(jellyBlock);
                jellyBlock.transform.localPosition = Vector3.zero;
            }

        }
        private void clearSlot()
        {
            foreach (GameObject j in jelly) Destroy(j);
            foreach (GameObject i in item) Destroy(i);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject[] itemSet = null;
        [SerializeField] private GameObject[] jellySet = null;
        [Header("★ Items")]
        [SerializeField] private GameObject[] itemPf;
        [SerializeField] private GameObject jellyPf;

        // Unity Messages
        private void Awake()
        {

        }
        private void Start()
        {

        }
    }
}