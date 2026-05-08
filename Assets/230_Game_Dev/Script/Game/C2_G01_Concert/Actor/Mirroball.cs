using beyondi.Util;
using DoDoEng.Common;
using NaughtyAttributes;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

namespace DoDoEng.Game.C2_G01
{
    public class Mirroball : MonoBehaviour
    {
        // Methods
        public void Clear()
        {
            LOG.Info($"{nameof(Clear)}()", this);

            clearAll();
            gauge = 0;
        }



        // Fields : caching
        private Animator[] mirroballs_ = null;
        private Animator[] mirroballs => mirroballs_ ??= GetComponentsInChildren<Animator>(true);

        // Fields
        private int gauge = 0;
        private bool isFever = false;

        // Functions
        private void clearAll()
        {
            mirroballs.ForEach(b => b.SetTrigger("Off"));
        }
        private void increaseGauge()
        {
            if (gauge >= mirroballs.Length)
                return;

            mirroballs[gauge++].SetTrigger("On");

            if (gauge == mirroballs.Length)
                StartCoroutine(coFever());
        }
        private void decreaseGauge()
        {
            LOG.Info($"off() - {gauge}", this);
            if (gauge - 1 < 0)
                return;

            mirroballs[--gauge].SetTrigger("Off");
        }

        // Event Handlers
        private void note_OnCorrect(Note note)
        {
            LOG.Info($"{nameof(note_OnCorrect)}()", this);

            increaseGauge();
        }



        // Unity Inspectors
        [Header("¡Ú Bindings")]
        [SerializeField] private NoteMGR noteMGR = null;
        [SerializeField] private Chello chello = null;
        [Header("¡Ú TimeLine")]
        [SerializeField] private PlayableDirector feverTL = null;
        [SerializeField] private PlayableDirector normalTL = null;
        [Header("¡Ú Configs")]
        [SerializeField] private float feverDuration = 5f;

        // Unity Inspectors : button
        [Button("(DEV)IncreaseGauge", EButtonEnableMode.Playmode)]
        private void devIncreaseGauge()
        {
            if (!isFever)
                increaseGauge();
        }
        [Button("(DEV)DecreaseGauge", EButtonEnableMode.Playmode)]
        private void devDecreaseGauge()
        {
            if (!isFever)
                decreaseGauge();
        }

        // Unity Messages
        private void Awake()
        {
            clearAll();
        }
        private void Start()
        {
        }
        private void OnEnable()
        {
            EventBus.Subscribe<EventBus.NoteCorrect>(note_OnCorrect);
        }
        private void OnDisable()
        {
            EventBus.Unsubscribe<EventBus.NoteCorrect>(note_OnCorrect);
        }

        // Unity Coroutine
        IEnumerator coFever()
        {
            using (LOG.Coroutine($"{nameof(coFever)}()", this))
            {
                isFever = true;
                yield return null;

                noteMGR.DoFever();
                chello.DoFever();
                feverTL.time = 0;
                feverTL.Play();
                yield return null;

                var interval = feverDuration / (mirroballs.Length + 1);
                yield return new WaitForSeconds(interval);
                while (gauge > 0)
                {
                    decreaseGauge();
                    yield return null;

                    yield return new WaitForSeconds(interval);
                }

                feverTL.Stop();
                normalTL.time = 0;
                normalTL.Play();
                noteMGR.DoNormal();
                chello.DoNormal();
                yield return null;

                isFever = false;
                yield return null;
            }

        }
    }
}