using DoDoEng.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DoDoEng.Game.C2_G01
{
    public class TouchPad : Graphic,
        IPointerDownHandler
    {
        // Methods
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            raycastTarget = enable;
        }



        // Functions
        private void press()
        {
            AudioMGR.One.PlayEffect(touchClip);
            pressButtonAnimation();

            var note = hitTest(hitTR.position);
            if (note != null && note.IsAlive)
            {
                LOG.VeryImportant($"HIT : {note.gameObject.name}", this);

                if (note.NoteType == NoteType.Text && !note.IsFever)
                {
                    if (note.IsAnswer)
                    {
                        note.Correct();
                        EventBus.Raise<EventBus.NoteCorrect>(note);
                    }
                    else
                    {
                        note.Wrong();
                        EventBus.Raise<EventBus.NoteWrong>(note);
                    }
                }
                else
                {
                    note.Hit();
                    EventBus.Raise<EventBus.NoteHit>(note);
                }
            }
        }
        private void pressButtonAnimation()
        {
            if (speaker1ANIM.gameObject.activeSelf)
                speaker1ANIM.SetTrigger("Push");
            if (speaker2ANIM.gameObject.activeSelf)
                speaker2ANIM.SetTrigger("Push");
            buutonANIM.SetTrigger("Pressed");
        }
        private Note hitTest(Vector2 pos)
        {
            var origin = pos - Vector2.right;
            var direction = Vector2.right;
            var hit = Physics2D.Raycast(origin, direction, 2);

            return hit.collider?.GetComponent<Note>();
        }

        // Overrides
        public override bool Raycast(Vector2 sp, Camera eventCamera)
        {
            // https://younitystudy.tistory.com/m/75
            return true;
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Transform hitTR = null;
        [SerializeField] private Animator speaker1ANIM = null;
        [SerializeField] private Animator speaker2ANIM = null;
        [SerializeField] private Animator buutonANIM = null;
        [Header("★ Audios")]
        [SerializeField] private AudioClip touchClip = null;
        [Header("★ Dev")]
        [SerializeField] private bool devDrawGizmos = false;
        [SerializeField] private KeyCode devKeyCode = KeyCode.LeftArrow;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            raycastTarget = false;
        }
        protected override void Start()
        {
            base.Start();
        }
        private void Update()
        {
            if (Input.GetKeyDown(devKeyCode))
                press();
        }
        private void OnDrawGizmos()
        {
            if (devDrawGizmos)
            {
                const int length = 2;
                var dir = hitTR.position.x > 0 ? Vector3.right : Vector3.left;
                var color = hitTest(hitTR.position) ? Color.red : Color.green;

                Gizmos.color = color;
                Gizmos.DrawRay(hitTR.position - dir * length, dir * 2 * length);
                Gizmos.DrawCube(hitTR.position + dir * 2, Vector3.one);
            }
        }



        // Interface : IPointerDownHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            LOG.Info($"OnPointerDown() | {eventData.position}", this);

            press();
        }
    }
}