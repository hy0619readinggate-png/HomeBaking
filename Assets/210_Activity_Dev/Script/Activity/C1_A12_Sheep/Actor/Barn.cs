using beyondi.Coroutine;
using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DoDoEng.Activity.C1_A12
{
    [RequireComponent(typeof(Animator))]
    public class Barn : MonoBehaviour, IID, ICompletable
    {
        // Properties
        public int ID { get; set; }
        public Transform EntranceTR => getEntranceTR();
        public bool IsAnswer { get; private set; } = false;
        public bool IsOpen { get; private set; } = false;
        public bool IsFeedback { get; private set; } = false;

        // Methods
        public void Setup(ExampleData exam, int sheepCount)
        {
            LOG.Info($"Setup() | {exam} {sheepCount}", this);

            wordTXT.text = exam.Text;
            wordSR.sprite = exam.WordSPR;

            sheepCurrentCount = 0;
            sheepNeeds = sheepCount;

            IsAnswer = exam.IsAnswer;
            IsOpen = false;
            anim.SetTrigger("reset");
        }
        public void Open()
        {
            LOG.Info($"Open()", this);

            anim.SetTrigger("openNow");
            IsOpen = true;
        }
        public void DoSuccess()
        {
            LOG.Info($"DoSuccess()", this);

            anim.SetTrigger("success");
        }
        public void DoWrong()
        {
            LOG.Info($"DoWrong()", this);

            anim.SetTrigger("fail");
        }



        // Fields : caching
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();

        // Fields
        private int sheepNeeds;
        private int sheepCurrentCount;
        private Queue<Transform> entranceQ = new Queue<Transform>();

        // Functions
        private Transform getEntranceTR()
        {
            if (entranceQ.Count == 0)
            {
                var trs = UtilArray.Shuffled(entranceTRs);
                trs.ForEach(tr => entranceQ.Enqueue(tr));
            }

            return entranceQ.Dequeue();
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshPro wordTXT = null;
        [SerializeField] private SpriteRenderer wordSR = null;
        [SerializeField] private Transform[] entranceTRs = null;

        // Unity Messages
        private void Awake()
        {

        }
        private void Start()
        {

        }
        private void OnTriggerStay2D(Collider2D collision)
        {
            //if (IsFeedback) return;

            var sheep = collision.GetComponent<Sheep>();
            if (sheep != null && sheep.IsFollowing)
            {
                if (IsAnswer)
                    StartCoroutine(coCorrect(sheep));
                else StartCoroutine(coWrong(sheep));
            }
        }

        // Unity Coroutine
        IEnumerator coCorrect(Sheep sheep)
        {
            using (LOG.Coroutine($"coCorrect()", this))
            {
                IsFeedback = true;
                yield return sheep.DoCorrect(this);
                yield return null;

                IsFeedback = false;
                yield return null;

                sheepCurrentCount++;
            }
        }
        IEnumerator coWrong(Sheep sheep)
        {
            using (LOG.Coroutine($"coWrong()", this))
            {
                IsFeedback = true;
                yield return sheep.DoWrong(this);
                yield return null;

                IsFeedback = false;
            }
        }



        // Implementation
        bool ICompletable.IsComplete => sheepNeeds == sheepCurrentCount;
    }
}