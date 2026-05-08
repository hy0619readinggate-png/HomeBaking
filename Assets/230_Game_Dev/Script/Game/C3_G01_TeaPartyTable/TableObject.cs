using System.Collections;
using UnityEngine;


namespace DoDoEng.Game.C3_G01
{
    public class TableObject : MonoBehaviour
    {

        // Properties
        public ObstacleType ObstacleType => _ObstacleType;
        public bool IsText => _IsText;
        public bool IsBroom => _IsBroom;
        public bool IsMacaron => _IsMacaron;

        public char? Letter => letter;



        // Methods
        public void Setup(float speed, char? letter)
        {
            moveSpeed = speed;
            setup(letter);
        }
        public void Move()
        {
            StartCoroutine(coMove());
        }
        public void Halt(bool halt, float speed = 0)
        {
            if (halt)
                _MoveANI.speed = 0f;
            else _MoveANI.speed = speed;
        }
        public void Clear()
        {
            if (hitCoroutine == null)
                StartCoroutine(coClear());
        }



        // Fields : cachings
        private TableMGR tableMGR_ = null;
        private TableMGR tableMGR => tableMGR_ ??= FindObjectOfType<TableMGR>();


        // Fields
        private Coroutine hitCoroutine = null;

        private float moveSpeed = 1f;
        protected char? letter = null;

        // Fields
        private readonly int hashkey_Move = Animator.StringToHash("Forward");
        private readonly int hashkey_Hit = Animator.StringToHash("Hit");
        private readonly int hashkey_Correct = Animator.StringToHash("Correct");
        private readonly int hashkey_Wrong = Animator.StringToHash("Wrong");
        private readonly int hashkey_Broom = Animator.StringToHash("Broomstick");



        // Functions
        protected virtual void setup(char? value) { }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Animator _MoveANI = null;
        [SerializeField] private Animator _FeedbackANI = null;
        [SerializeField] private GameObject _ColliderGO = null;
        [Header("★ Config")]
        [SerializeField] private ObstacleType _ObstacleType = ObstacleType.AppleRed_Normal;
        [SerializeField] private bool _IsText = false;
        [SerializeField] private bool _IsBroom = false;
        [SerializeField] private bool _IsMacaron = false;

        // Unity Messages
        private void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody != null)
            {
                var checker = other.attachedRigidbody.GetComponent<PlayerTableObjectChecker>();
                if (checker != null)
                    hitCoroutine = StartCoroutine(coHit());
            }
        }


        // Unity Coroutine
        IEnumerator coMove()
        {
            _MoveANI.SetTrigger(hashkey_Move);
            _MoveANI.speed = moveSpeed;
            yield return null;

            yield return new WaitUntil(() => _MoveANI.GetCurrentAnimatorStateInfo(0).IsTag("Move"));
            yield return new WaitUntil(() => _MoveANI.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f);

            Destroy(gameObject);
        }
        IEnumerator coHit()
        {
            _MoveANI.speed = 0f;
            _ColliderGO.SetActive(false);

            bool rightAnswer = tableMGR.CheckAnswer(letter); // << Hit 시 알파벳 정답판정 (기존 : 알파벳 세팅 시)

            if (IsText)
            {
                if (rightAnswer)
                {
                    _FeedbackANI.SetTrigger(hashkey_Correct);
                    yield return new WaitUntil(() => _FeedbackANI.GetCurrentAnimatorStateInfo(0).IsTag("Correct"));
                    yield return new WaitUntil(() => _FeedbackANI.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f);
                }
                else
                {
                    _FeedbackANI.SetTrigger(hashkey_Wrong);
                    yield return new WaitUntil(() => _FeedbackANI.GetCurrentAnimatorStateInfo(0).IsTag("Wrong"));
                    yield return new WaitUntil(() => _FeedbackANI.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f);
                }
            }
            else
            {
                if (IsBroom)
                {
                    _FeedbackANI.SetTrigger(hashkey_Broom);
                    yield return new WaitUntil(() => _FeedbackANI.GetCurrentAnimatorStateInfo(0).IsTag("Broom"));
                    yield return new WaitUntil(() => _FeedbackANI.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f);
                }
                if (IsMacaron)
                {
                    _FeedbackANI.SetTrigger(hashkey_Correct);
                    yield return new WaitUntil(() => _FeedbackANI.GetCurrentAnimatorStateInfo(0).IsTag("Correct"));
                    yield return new WaitUntil(() => _FeedbackANI.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f);
                }
                else
                {
                    _FeedbackANI.SetTrigger(hashkey_Hit);
                    yield return new WaitUntil(() => _FeedbackANI.GetCurrentAnimatorStateInfo(0).IsTag("Hit"));
                    yield return new WaitUntil(() => _FeedbackANI.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f);
                }
            }

            Destroy(gameObject);
        }
        IEnumerator coClear()
        {
            _MoveANI.speed = 0f;
            _ColliderGO.SetActive(false);

            _FeedbackANI.SetTrigger(hashkey_Broom);
            yield return new WaitUntil(() => _FeedbackANI.GetCurrentAnimatorStateInfo(0).IsTag("Broom"));
            yield return new WaitUntil(() => _FeedbackANI.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f);

            Destroy(gameObject);
        }

    }
}