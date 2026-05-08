using beyondi.Coroutine;
using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using TMPro;
using UnityEngine;

namespace DoDoEng.Activity.C1_A10
{
    [RequireComponent(typeof(Animator))]
    public class Example : MonoBehaviour, IID
    {
        // Properties
        public bool IsAnswer => exam?.IsAnswer ?? false;

        // Methods
        public void Setup(ExampleData exam)
        {
            LOG.Info($"Setup() | {exam}", this);

            this.exam = exam;
            phoneticTXT.text = exam.Text;

            anim.SetTrigger("Closed");
        }
        public void Correct()
        {
            LOG.Info($"Correct()", this);

            StartCoroutine(coCorrect());
        }
        public void Wrong()
        {
            LOG.Info($"Wrong()", this);

            StartCoroutine(coWrong());
        }
        public void HideText()
        {
            LOG.Info($"Wrong()", this);

            anim.SetTrigger("HideText");
        }
        public void Close()
        {
            LOG.Info($"Close()", this);

            anim.SetTrigger("Close");

            vfxCorrectGO.SetActive(false);
            vfxWrongGO.SetActive(false);
        }



        // Fields : caching
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();

        // Fields
        private ExampleData exam;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshPro phoneticTXT = null;
        [SerializeField] private GameObject vfxCorrectGO = null;
        [SerializeField] private GameObject vfxWrongGO = null;

        // Unity Messages
        private void Awake()
        {
            vfxWrongGO.SetActive(false);
            vfxCorrectGO.SetActive(false);
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coCorrect()
        {
            using (LOG.Coroutine($"coCorrect()", this))
            {
                anim.SetTrigger("Correct");
                yield return null;

                vfxCorrectGO.SetActive(false);
                vfxCorrectGO.SetActive(true);
                yield return null;
            }
        }
        IEnumerator coWrong()
        {
            using (LOG.Coroutine($"coWrong()", this))
            {
                anim.SetTrigger("Wrong");
                yield return new WaitForSeconds(0.3f);

                vfxWrongGO.SetActive(false);
                vfxWrongGO.SetActive(true);
                yield return null;
            }
        }

        // Interface : IID
        public int ID { get; set; }
    }
}