using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C2_A11
{
    public class PositionGroup : MonoBehaviour
    {
        // Properties
        public Transform[] Positions => currentHolders.Select(h => h.transform).ToArray();

        // Methods
        public void Setup(int setID)
        {
            LOG.Info($"Setup() | {setID}", this);

            this.setID = setID;
        }



        // Fields
        private int setID = 1;

        // Functions
        private PositionHolder[] currentHolders => setID switch
        {
            1 => holderSet1,
            2 => holderSet2,
            3 => holderSet3,
            _ => null
        };



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private PositionHolder[] holderSet1 = null;
        [SerializeField] private PositionHolder[] holderSet2 = null;
        [SerializeField] private PositionHolder[] holderSet3 = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}