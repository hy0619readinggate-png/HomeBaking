using beyondi.Util;
using System.Linq;
using System.Collections;
using UnityEngine;
using DoDoEng.Common;

// devBOX - 삭제해야 함
#pragma warning disable 0414

namespace DoDoEng.MyRoom.Behavior
{
	public class CreatureBookBox : MonoBehaviour
	{
		// Definitions
		// Properties
		public int Index => index;

		// Methods
		public void Init(int idxKind, bool[] openData)
		{
			LOG.Function(this, $"| idxKind={idxKind}");

            for (int i = 0; i < tiers.Length; i++)
            {
                tiers[i].SetActive(Mathf.Floor(idxKind / 4) == i);
            }

            foreach (var (slot, i) in slots.Select((v, i) => (v, i)))
            {
                slot.Init(idxKind, i + 1, true, !openData[i]);
            }
        }



        // Fields : caching

        // Fields
        private int index;

		// Functions
		// Event Handlers
		// Overrides



		// Unity Inspectors
		[Header("★ Bindings")]
		[SerializeField] private GameObject newGO = null;
        [SerializeField] private CollectionSlot[] slots = null;
        [SerializeField] private GameObject[] tiers = null;

        // Unity Messages
        private void Awake()
		{
            newGO.SetActive(false);
        }
		private void Start()
		{
		   
		}

		// Unity Coroutine
	}
}