using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C1_A07
{
    public class Example : MonoBehaviour
    {
        // Properties
        public bool IsAnswer => examData.IsAnswer;
        public bool IsColliderable => isColliderable;

        // Methods
        public void Setup(ExampleData examData)
        {
            LOG.Info($"Setup()", this);

            this.examData = examData;

            anim.SetTrigger("Ready");
            isMoving = false;

            examIMG.sprite = examData.Image;
            transform.position = originPosition;

            vfxCorrectGO.SetActive(false);
            if (vfxWrongGO != null)
                vfxWrongGO.SetActive(false);
        }
        public void Move()
        {
            LOG.Info($"Move()", this);

            anim.SetTrigger("Ready");
            isMoving = true;
        }
        public void EnableColliderable(bool isColliderable)
        {
            LOG.Info($"EnableColliderable() | {isColliderable}", this);

            this.isColliderable = isColliderable;
        }
        public void Hold()
        {
            LOG.Info($"Hold()", this);

            isMoving = false;
        }
        public void Correct()
        {
            LOG.Info($"Correct()", this);

            anim.SetTrigger("Correct");
            isMoving = false;

            vfxCorrectGO.SetActive(true);
        }
        public void Wrong()
        {
            LOG.Info($"Correct()", this);

            anim.SetTrigger("Wrong");
            isMoving = false;

            if (vfxWrongGO != null)
                vfxWrongGO.SetActive(true);
        }



        // Fields
        private Vector3 originPosition;
        private bool isMoving = false;
        private bool isColliderable = false;
        private ExampleData examData;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Animator anim = null;
        [SerializeField] private Image examIMG = null;
        [SerializeField] private GameObject vfxCorrectGO = null;
        [SerializeField] private GameObject vfxWrongGO = null;

        // Unity Messages
        private void Awake()
        {
            vfxCorrectGO.SetActive(false);
            if (vfxWrongGO != null)
                vfxWrongGO.SetActive(false);
        }
        private void Start()
        {
            originPosition = transform.position;
        }
        private void Update()
        {
            if (isMoving)
            {
                var currentPosition = transform.position;
                var distanceX = Track.One.ExampleSpeed * Time.deltaTime;
                transform.position = new Vector2(currentPosition.x - distanceX, currentPosition.y);
            }
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            LOG.Info($"OnTriggerEnter2D() | {collision.gameObject.name}", this);

            var edmond = collision.GetComponentInParent<Edmond>();
            if (edmond == null)
                isMoving = false;
        }
    }
}