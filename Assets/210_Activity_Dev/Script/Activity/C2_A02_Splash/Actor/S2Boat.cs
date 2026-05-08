using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C2_A02
{
    public class S2Boat : MonoBehaviour
    {
        // Methods
        public void Setup(int[] characterIds)
        {
            LOG.Info($"Setup()", this);

            this.seatCount = characterIds.Length;

            seat2.gameObject.SetActive(seatCount == 2);
            seat3.gameObject.SetActive(seatCount == 3);

            activeSeat.Cheese(characterIds);
        }
        public void PlayCheeseSound()
        {
            LOG.Info($"PlayCheeseSound()", this);

            activeSeat.PlayCheeseSound();
        }
        public void StopCheeseSound()
        {
            LOG.Info($"StopCheeseSound()", this);

            activeSeat.StopCheeseSound();
        }



        // Fields
        private int seatCount;

        // Functions
        private S1Seat activeSeat => seatCount == 2 ? seat2 : seat3;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private S1Seat seat2 = null;
        [SerializeField] private S1Seat seat3 = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}