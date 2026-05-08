using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Game.C2_G01
{
    public class Chello : MonoBehaviour
    {
        // Methods
        public void PlayDance() => ani.PlayDance();
        public void PlayIdle() => ani.PlayIdle();

        public void DoFever()
        {
            LOG.Function(this);

            isFever = true;

            ani.PlayFever();
        }
        public void DoNormal()
        {
            LOG.Function(this);

            isFever = false;

            ani.PlayIdle();
        }


        // Fields
        private bool isFever = false;

        // Event Handlers
        private void note_OnHit(Note note)
        {
            LOG.Info($"note_OnHit()", this);

            if (!isFever)
                ani.PlayAction();
        }
        private void note_OnCorrect(Note note)
        {
            LOG.Info($"{nameof(note_OnCorrect)}()", this);

            if (!isFever)
            {
                var clip = UtilArray.ExtractOne(correctCLIP);
                AudioMGR.One.PlayEffect(clip);

                ani.PlayAction();
            }
        }
        public void note_OnWrong(Note note)
        {
            LOG.Info($"{nameof(note_OnWrong)}()", this);

            if (!isFever)
            {
                var clip = UtilArray.ExtractOne(wrongCLIP);
                AudioMGR.One.PlayEffect(clip);

                ani.PlayWrong();
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private ChelloAni ani = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip[] correctCLIP = null;
        [SerializeField] private AudioClip[] wrongCLIP = null;


        // Unity Messages
        private void Awake()
        {
            isFever = false;
        }
        private void Start()
        {
        }
        private void OnEnable()
        {
            EventBus.Subscribe<EventBus.NoteHit>(note_OnHit);
            EventBus.Subscribe<EventBus.NoteCorrect>(note_OnCorrect);
            EventBus.Subscribe<EventBus.NoteWrong>(note_OnWrong);
        }
        private void OnDisable()
        {
            EventBus.Unsubscribe<EventBus.NoteHit>(note_OnHit);
            EventBus.Unsubscribe<EventBus.NoteCorrect>(note_OnCorrect);
            EventBus.Unsubscribe<EventBus.NoteWrong>(note_OnWrong);
        }
    }
}