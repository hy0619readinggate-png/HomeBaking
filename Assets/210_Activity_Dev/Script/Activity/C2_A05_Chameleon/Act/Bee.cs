using beyondi.Coroutine;
using beyondi.Util;
using DoDoEng.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C2_A05
{
    [RequireComponent(typeof(Button))]
    public class Bee : MonoBehaviour, IID, ISubmitable
    {
        // Properties
        public bool IsAnswer => exam.IsAnswer;
        public bool IsAlive => model.activeSelf;

        // Methods
        public void Setup(ExampleData exam)
        {
            LOG.Info($"Setup() | {exam.Text}", this);

            this.exam = exam;

            IsSubmit = false;

            examTXT.text = exam.Text;
            model.SetActive(true);
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            IsSubmit = false;
            cg.blocksRaycasts = enable && IsAlive;
        }
        public void Take()
        {
            LOG.Info($"Take()", this);

            model.SetActive(false);
        }



        // Fields : caching
        private Button btn_ = null;
        private Button btn => btn_ ??= GetComponent<Button>();
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private GameObject model_ = null;
        private GameObject model => model_ ??= transform.GetChild(0).gameObject;

        // Fields
        private ExampleData exam = null;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI examTXT = null;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;

            btn.onClick.AddListener(() => IsSubmit = true);
        }
        private void Start()
        {
        }




        // Interface : IID
        public int ID { get; set; }

        // Interface : IID
        public bool IsSubmit { get; private set; }
    }
}