using DoDoEng.Common;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C1_A06
{
    public class RidePlay : MonoBehaviour
    {
        // Methods
        public void Setup(int[] characters)
        {
            LOG.Info($"Setup() | {string.Join(",", characters)}", this);

            foreach (var (seat, i) in seats.Select((s, i) => (s, i)))
            {
                if (characters[i] > 0)
                {
                    seat.Occupy(characters[i]);
                    seat.RidePlay();
                }
            }
        }



        // Fields : cachinge
        private Seat[] seats_ = null;
        private Seat[] seats => seats_ ??= GetComponentsInChildren<Seat>(true);



        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}