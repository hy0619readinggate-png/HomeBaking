using beyondi.Util;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C1_A08
{
    public class Pachinko : MonoBehaviour
    {
        // Methods
        public void Setup(Sprite answerSprite, Sprite[] examSprites)
        {
            LOG.Info($"Setup()", this);

            var exams = UtilArray.Extract(examSprites, 6);
            var queue = new Queue<Sprite>(exams);

            foreach (var (slot, idx) in slots.Select((slot, idx) => (slot, idx)))
            {
                slots[idx].Setup(
                    idx / 3 == 1
                    ? answerSprite
                    : queue.Dequeue());
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private PachinkoSlot[] slots = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}