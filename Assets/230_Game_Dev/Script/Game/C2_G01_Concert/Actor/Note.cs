using beyondi.Behaviour;
using DoDoEng.Common;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;

namespace DoDoEng.Game.C2_G01
{
    public enum NoteType { Music, Text }
    public class Note : MonoBehaviour, IBYDPooledObject<Note>
    {
        // Properties
        public NoteType NoteType { get; private set; } = NoteType.Music;
        public int ColorType { get; private set; }
        public bool IsFever { get; private set; }
        public int PNO { get; private set; }
        public bool IsAnswer { get; private set; }
        public int ProblemID { get; private set; }
        public bool IsLastSeq { get; private set; }
        public bool IsAlive { get; private set; }

        // Methods
        public void InitForMusic(float floorY)
        {
            limitY = floorY;

            NoteType = NoteType.Music;
            ColorType = Random.Range(0, normalGO.Length) + 1;
            IsFever = false;
            PNO = 0;
            IsAnswer = false;
            IsAlive = true;

            updateForNoteType();
        }
        public void InitForText(float floorY, int pNO, string phonics, AudioClip phonicsClip, bool isAnswer, int problemID, bool isLastSeq)
        {
            limitY = floorY;

            NoteType = NoteType.Text;
            ColorType = Random.Range(0, normalGO.Length) + 1;
            IsFever = false;
            PNO = pNO;
            IsAnswer = isAnswer;
            ProblemID = problemID;
            IsLastSeq = isLastSeq;
            IsAlive = true;

            phonicsTXT.text = phonics;
            this.phonicsCLIP = phonicsClip;

            updateForNoteType();
        }
        public void InitForFever(float floorY)
        {
            limitY = floorY;

            NoteType = NoteType.Music;
            ColorType = Random.Range(0, normalGO.Length) + 1;
            IsFever = true;
            PNO = 0;
            IsAnswer = false;
            IsAlive = true;

            updateForNoteType();
        }
        public void ChangeToFever()
        {
            IsFever = true;
            updateForNoteType();
        }
        public void ChangeToMusic()
        {
            LOG.Info($"{NoteType}, {phonicsTXT.text}", this);

            NoteType = NoteType.Music;
            ColorType = Random.Range(0, normalGO.Length) + 1;
            IsFever = false;
            PNO = 0;
            IsAnswer = false;
            updateForNoteType();
        }
        public void ChangeToNormal()
        {
            IsFever = false;
            updateForNoteType();
        }

        // Methods
        public void Hit()
        {
            LOG.Info($"Hit()", this);

            IsAlive = false;
            anim.SetTrigger("Correct");
        }
        public void Correct()
        {
            LOG.Info($"{nameof(Correct)}()", this);

            IsAlive = false;
            anim.SetTrigger("Correct");

            if (phonicsCLIP != null)
            {
                AudioMGR.One.PlayEffect(phonicsCLIP);
            }
        }
        public void Wrong()
        {
            LOG.Info($"{nameof(Wrong)}()", this);

            IsAlive = false;
            anim.SetTrigger("Wrong");
        }



        // Fields
        private float limitY = 0;
        private AudioClip phonicsCLIP = null;

        // Functions
        private void updateForNoteType()
        {
            for (var i = 0; i < normalGO.Length; i++)
                normalGO[i].SetActive(!IsFever && ColorType - 1 == i);
            feverGO.SetActive(IsFever);

            noteGO.SetActive(NoteType == NoteType.Music);
            phonicsTXT.gameObject.SetActive(NoteType == NoteType.Text && !IsFever);

            correctFxMusicGO.SetActive(NoteType == NoteType.Music);
            correctFxTextGO.SetActive(NoteType == NoteType.Text);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Animator anim = null;
        [SerializeField] private GameObject[] normalGO = null;
        [SerializeField] private GameObject feverGO = null;
        [SerializeField] private GameObject noteGO = null;
        [SerializeField] private TextMeshProUGUI phonicsTXT = null;
        [SerializeField] private GameObject correctFxMusicGO = null;
        [SerializeField] private GameObject correctFxTextGO = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
        private void Update()
        {
            if (transform.position.y < limitY)
            {
                if (IsAlive && NoteType == NoteType.Text && !IsFever && IsAnswer)
                    EventBus.Raise<EventBus.NoteFloor>(this);
                Pool.Release(this);
            }
        }



        // Interface : IBYDPooledObject<T>
        public IObjectPool<Note> Pool { get; set; }
    }
}