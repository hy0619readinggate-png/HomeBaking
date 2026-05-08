using UnityEngine;
using UnityEngine.Splines;

namespace DoDoEng.Activity.C1_A01
{
    public class TraceAlphabetOld : MonoBehaviour
    {
        // Properties
        public GameObject[] StrokesGO => strokesGO;
        public Transform OuterLineTR => outerLineTR;
        public SplineContainer[] PathsSC => pathsSC;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject[] strokesGO = null;
        [SerializeField] private Transform outerLineTR = null;
        [SerializeField] private SplineContainer[] pathsSC = null;
    }
}