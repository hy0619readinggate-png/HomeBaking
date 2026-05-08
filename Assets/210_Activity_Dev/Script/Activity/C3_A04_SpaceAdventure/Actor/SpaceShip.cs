using beyondi.Behaviour;
using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace DoDoEng.Activity.C3_A04
{
    [RequireComponent(typeof(SpaceShipMotion))]
    [RequireComponent(typeof(SpaceShipTargetChaser))]
    [RequireComponent(typeof(Leader))]
    public class SpaceShip : MonoBehaviour
    {
        // Methods
        public void Init() => motion.Init();
        public void StartMoving(Vector3 position) => motion.StartMoving(position);
        public void MoveTo(Vector3 position) => motion.MoveTo(position);
        public void StopMoving() => motion.StopMoving();
        public void GoOut() => motion.GoOut();

        // Methods
        public Coroutine StartExpedition(string text)
        {
            LOG.Info($"StartExpedition() | {text}", this);

            problems = text.ToCharArray();
            pSeq = 1;
            isCollidable = true;

            crExpedition = StartCoroutine(coExpedition());
            return crExpedition;
        }
        public void FinishExpedition()
        {
            LOG.Info($"FinishExpedition()", this);

            this.StopCoroutineSafe(ref crExpedition);
            isCollidable = false;
            motion.StopMoving();
        }
        public void Ready()
        {
            LOG.Info($"Ready()", this);

            motion.ResetToOrigin();
            leader.ClearFollowers();
        }
        public Coroutine MoveToExitPosition1()
        {
            var position = transform.position + new Vector3(1, 0, 0) * xDeltaForExit1;
            return motion.MoveAndWait(position);
        }
        public Coroutine MoveToExitPosition2()
        {
            var position = transform.position + new Vector3(1, 0, 0) * xDeltaForExit2;
            return motion.MoveAndWait(position);
        }



        // Fields : caching
        private SpaceShipMotion motion_ = null;
        private SpaceShipMotion motion => motion_ ??= GetComponent<SpaceShipMotion>();
        private SpaceShipTargetChaser chaser_ = null;
        private SpaceShipTargetChaser chaser => chaser_ ??= GetComponent<SpaceShipTargetChaser>();
        private Leader leader_ = null;
        private Leader leader => leader_ ??= GetComponent<Leader>();

        // Fields
        private char[] problems;
        private int pSeq;

        // Fields
        private bool isCollidable = false;
        private Coroutine crExpedition = null;

        // Functions
        private void problem_Begin()
        {
            EventBus.Raise<EventBus.SpaceShipReady>(pSeq);

            var planets = planetMGR.SetAnswerPlanet(problems[pSeq - 1]);
            var target = planets.First();
            chaser.StartChase(target.transform);
        }
        private void problem_Complete()
        {
            EventBus.Raise<EventBus.SpaceShipCorrect>(pSeq);

            planetMGR.ClearAnswerPlanet();
            chaser.FinishChase();
        }
        private bool problem_Next()
        {
            pSeq++;
            return (pSeq <= problems.Length);
        }



        // Unity Inspectors
        [Header("★ Bindings - inner")]
        [SerializeField] private LeoniAni leoni = null;
        [Header("★ Bindings")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlanetMGR planetMGR = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip[] correctCLIP = null;
        [SerializeField] private AudioClip[] wrongCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float correctMoveDelay = 0.5f;
        [SerializeField] private float wrongDuration = 0.3f;
        [SerializeField] private float xDeltaForExit1 = 6;
        [SerializeField] private float xDeltaForExit2 = -35;
        [SerializeField] private LeoniAnimation[] correctAnimations = null;
        [SerializeField] private LeoniAnimation[] wrongAnimations = null;


        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
            leoni.PlayAnimationLoopT2(
                    LeoniAnimation.Idle1,
                    LeoniAnimation.Idle2,
                    LeoniAnimation.Idle3,
                    LeoniAnimation.IdleEye);
        }
        private void OnCollisionEnter2D(Collision2D collision)
        {
            LOG.Info($"OnCollisionEnter2D() | {collision.gameObject.name}", this);

            if (!isCollidable)
                return;

            var planet = collision.gameObject.GetComponent<Planet>();
            if (planet != null)
                StartCoroutine(coWrong(planet, collision.relativeVelocity));
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            LOG.Info($"OnTriggerEnter2D() | {collision.gameObject.name}", this);

            if (!isCollidable)
                return;

            var planet = collision.gameObject.GetComponent<Planet>();
            if (planet != null)
                StartCoroutine(coCorrect(planet));
        }

        // Unity Coroutine
        IEnumerator coExpedition()
        {
            using (LOG.Coroutine($"coExpedition", this))
            {
                problem_Begin();
                yield return null;

                yield return new WaitUntil(() => pSeq > problems.Length);
            }
        }
        IEnumerator coCorrect(Planet planet)
        {
            using (LOG.Coroutine($"coCorrect() | {planet.gameObject.name}", this))
            {
                problem_Complete();
                isCollidable = false;
                yield return null;

                planet.Correct();
                yield return new WaitForSeconds(correctMoveDelay);

                leader.AddFollower(planet.gameObject);
                yield return null;

                if (problem_Next())
                {
                    isCollidable = true;
                    problem_Begin();
                }
                yield return null;

                var idx = UtilArray.RandomOne(0, correctAnimations.Length - 1);
                AudioMGR.One.PlayEffect(correctCLIP[idx]);
                yield return leoni.PlayAnimationAndWait(correctAnimations[idx]);
                yield return null;

                leoni.PlayAnimationLoopT2(
                    LeoniAnimation.Idle1,
                    LeoniAnimation.Idle2,
                    LeoniAnimation.Idle3,
                    LeoniAnimation.IdleEye);
                yield return null;
            }
        }
        IEnumerator coWrong(Planet planet, Vector2 relativeVelocity)
        {
            using (LOG.Coroutine($"coWrong() | {planet.gameObject.name} {relativeVelocity}", this))
            {
                playerController?.EnableInteraction(false);
                isCollidable = false;
                motion.ForceStop();
                yield return null;

                planet.Wrong();
                motion.Bounce(relativeVelocity);
                yield return new WaitForSeconds(wrongDuration);

                playerController?.EnableInteraction(true);
                isCollidable = true;
                yield return null;

                var idx = UtilArray.RandomOne(0, wrongAnimations.Length - 1);
                AudioMGR.One.PlayEffect(wrongCLIP[idx]);
                yield return leoni.PlayAnimationAndWait(wrongAnimations[idx]);
                leoni.PlayAnimationLoopT2(
                    LeoniAnimation.Idle1,
                    LeoniAnimation.Idle2,
                    LeoniAnimation.Idle3,
                    LeoniAnimation.IdleEye);
            }
        }
    }
}