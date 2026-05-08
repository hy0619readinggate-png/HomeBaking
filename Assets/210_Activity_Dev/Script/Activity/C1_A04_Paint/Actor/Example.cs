using beyondi.Coroutine;
using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C1_A04
{
    public class Example : MonoBehaviour, ISubmitable
    {
        // Properties
        public int ID { get; private set; }
        public bool IsAnswer { get; protected set; }
        public AudioClip WordCLIP { get; protected set; }

        // Methods
        public void Setup(int id, ExampleData examData)
        {
            LOG.Info($"Setup()", this);

            Util.RemoveAllChildren(examTR);

            ID = id;
            IsAnswer = examData.IsAnswer;
            WordCLIP = examData.WordCLIP;

            Instantiate(examData.WordPB, examTR);
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            if (enable)
                isPressed = false;
            btn.interactable = enable;
        }
        public void Feedback(bool correct)
        {
            LOG.Info($"Feedback() | {correct}", this);

            fxGO.SetActive(correct);

            select(true);

            if(!correct)
                anim.SetTrigger("Wrong");
        }
        public void Clear()
        {
            LOG.Info($"Clear()", this);

            select(false);
        }



        // Fields : caching
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();

        // Fields
        private bool isPressed = false;

        // Functions
        private void select(bool selected)
        {
            selectedGO.SetActive(selected);
        }



        // Unity Inspectors
        [Header("¡Ú Bindings")]
        [SerializeField] private Button btn = null;
        [SerializeField] private Transform examTR = null;
        [SerializeField] private GameObject selectedGO = null;
        [SerializeField] private GameObject fxGO = null;
        


        // Unity Messages
        private void Awake()
        {
            fxGO.SetActive(false);
            selectedGO.SetActive(false);

            btn.onClick.AddListener(() => isPressed = true);
        }
        private void Start()
        {

        }



        // Implementation : ISelectable
        public bool IsSubmit => isPressed;
    }
}