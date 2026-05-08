using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C1_A09
{
    public class MachineCorn : MonoBehaviour
    {
        // Properties
        public int LoadedCorns { get; private set; }
        public bool IsEmpty => LoadedCorns == 0;
        public bool IsFull => LoadedCorns == transform.childCount;

        // Methods
        public void Empty()
        {
            LOG.Info($"Empty()", this);

            LoadedCorns = 0;
            updateCorns();
        }
        public void AddCorn()
        {
            LOG.Info($"AddCorn()", this);

            if (!IsFull)
            {
                LoadedCorns++;
                updateCorns();
            }
        }



        // Functions
        private void updateCorns()
        {
            for (var i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(i < LoadedCorns);
        }
    }
}