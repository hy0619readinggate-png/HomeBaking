using beyondi.Util;
using DoDoEng.Common;
using TMPro;
using UnityEngine;

namespace DoDoEng.Activity.C2_A03
{
    public class Boat : MonoBehaviour
    {
        // Methods
        public void Setup(ProblemData problem)
        {
            LOG.Info($"Setup() | {problem}", this);

            updateBoatType(problem.BoatType);
        }



        // Functions
        private void updateBoatType(int type)
        {
            boatType1GO.ForEach(go => go.SetActive(type == 1));
            boatType2GO.ForEach(go => go.SetActive(type == 2));
            boatType3GO.ForEach(go => go.SetActive(type == 3));
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject[] boatType1GO = null;
        [SerializeField] private GameObject[] boatType2GO = null;
        [SerializeField] private GameObject[] boatType3GO = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {

        }
    }
}