using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C2_A02
{
    public class S1Entrance : MonoBehaviour, IID
    {
        // Properties
        public S1ExampleText ExampleText => exampleText;
        public Character Character => character;
        public bool IsComplete { get; private set; }

        // Methods
        public void Setup(ExampleData exam, int characterID)
        {
            LOG.Info($"Setup() | {exam.Text}, {characterID}", this);

            character.Setup(characterID);
            character.gameObject.SetActive(true);
            exampleText.Setup(exam);
            exampleText.gameObject.SetActive(true);

            doorAnim.SetTrigger("Idle");
        }
        public void Correct()
        {
            LOG.Info($"Correct()", this);

            doorAnim.SetTrigger("Opened");
        }
        public void Wrong()
        {
            LOG.Info($"Wrong()", this);

            vfxWrongGO.SetActive(false);
            vfxWrongGO.SetActive(true);
        }



        // Fields
        private int id = 0;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Character character = null;
        [SerializeField] private S1ExampleText exampleText = null;
        [SerializeField] private GameObject vfxWrongGO = null;
        [SerializeField] private Animator doorAnim = null;

        // Unity Messages
        private void Awake()
        {
            vfxWrongGO.SetActive(false);
        }
        private void Start()
        {
        }



        // Interface : IID
        public int ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
                exampleText.ID = id;
            }
        }
    }
}