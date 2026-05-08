using UnityEngine;

namespace DoDoEng.Activity.C1_A08
{
    public enum ContactType { NoHitZone, FloorZone, Player }

    public class RainDropContact : MonoBehaviour
    {
        // Properties
        public ContactType ContactType => contactType;



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private ContactType contactType;
    }
}