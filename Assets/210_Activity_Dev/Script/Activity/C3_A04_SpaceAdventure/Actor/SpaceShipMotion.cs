using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Activity.C3_A04
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class SpaceShipMotion : MonoBehaviour
    {
        // Methods
        public void Init()
        {
            LOG.Info($"Init()", this);

            originPosition = transform.position;
        }
        public void StartMoving(Vector2 position)
        {
            LOG.Info($"StartMoving() | {position}", this);

            startMoving();
            targetPosition = position;
        }
        public void MoveTo(Vector2 position)
        {
            targetPosition = position;
        }
        public void StopMoving()
        {
            LOG.Info($"StopMoving()", this);

            stopMoving();
        }
        public void ForceStop()
        {
            LOG.Info($"ForceStop()", this);

            stopMoving();
        }

        // Methods
        public Coroutine MoveAndWait(Vector2 position)
        {
            LOG.Info($"MoveAndWait() | {position}", this);

            // 테스트는 못했음.
            return StartCoroutine(coMoveTo(position));
        }
        public void Bounce(Vector2 power)
        {
            LOG.Info($"Bounce() | {power}", this);

            rb.velocity = power * bouncePower;
        }
        public void ResetToOrigin()
        {
            LOG.Info($"ResetToOrigin()", this);

            transform.position = originPosition;
        }
        public void GoOut()
        {
            LOG.Info($"GoOut()", this);

            AudioMGR.One.PlayEffect(goOutCLIP);
            startMoving();
            targetPosition = (Vector2)transform.position + new Vector2(0, 20);
        }



        // Fields : caching
        private Rigidbody2D rb_ = null;
        private Rigidbody2D rb => rb_ ??= GetComponent<Rigidbody2D>();

        // Fields
        private Vector3 originPosition;
        private Vector2 targetPosition;
        private bool isMoving = false;

        // Functions
        private void startMoving()
        {
            if (movingCLIP != null)
                AudioMGR.One.PlayEffectLL(movingCLIP, true);
            isMoving = true;
        }
        private void stopMoving()
        {
            if (movingCLIP != null)
                AudioMGR.One.StopEffectLL(true);
            isMoving = false;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Animator rotateANIM = null;
        [Header("★ Config")]
        [SerializeField] private float moveSpeed = 3; // m/sec
        [SerializeField] private float moveThreshold = 0.2f;
        [SerializeField] private float arriveThreshold = 0.1f;
        [SerializeField] private float accerlation = 2f;
        [SerializeField] private float deaccerlation = 3f;
        [SerializeField] private float bouncePower = 1.5f;
        [Header("★ Sound")]
        [SerializeField] private AudioClip movingCLIP = null;
        [SerializeField] private AudioClip goOutCLIP = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
        private void Update()
        {
            if (isMoving)
            {
                var displacement = targetPosition - (Vector2)transform.position;
                if (displacement.magnitude > moveThreshold)
                    rb.velocity = Vector2.Lerp(
                        rb.velocity,
                        displacement.normalized * moveSpeed,
                        accerlation * Time.deltaTime);
            }
            else
            {
                rb.velocity = Vector2.Lerp(
                    rb.velocity,
                    Vector2.zero,
                    deaccerlation * Time.deltaTime);
            }

            var angle = rb.velocity.x / moveSpeed;
            rotateANIM.SetFloat("Angle", angle);
        }

        // Unity Coroutine
        IEnumerator coMoveTo(Vector2 position)
        {
            using (LOG.Coroutine($"coMoveTo() | {position}", this))
            {
                targetPosition = position;

                startMoving();

                while (true)
                {
                    // 다른 기능을 통해 목표지점을 해제할 경우 종료
                    if (targetPosition == null)
                    {
                        LOG.Warning($"targetPosition lost!", this);
                        break;
                    }

                    // 목표지점에 도착했을 경우 종료
                    var distance = Vector2.Distance(transform.position, targetPosition);
                    if (distance < arriveThreshold)
                        break;

                    yield return null;
                }

                stopMoving();
            }
        }
    }
}