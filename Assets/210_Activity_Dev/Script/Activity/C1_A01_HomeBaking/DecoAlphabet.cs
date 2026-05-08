using UnityEngine;

namespace DoDoEng.Activity.C1_A01
{
    public class DecoAlphabet : MonoBehaviour
    {
        // Properties
        public GameObject CrumbGO => crumbGO;
        public GameObject DecoAlphabetGO => decoAlphabetGO;
        public GameObject EatAlphabetGO => eatAlphabetGO;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject crumbGO = null;
        [SerializeField] private GameObject decoAlphabetGO = null;
        [SerializeField] private GameObject eatAlphabetGO = null;
    }
}