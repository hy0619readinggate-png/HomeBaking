using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C1_A10
{
    public class Maze : AffBase
    {
        // Methods
        public void Init(CameraMGR cameraMGR, Friend[] friends)
        {
            LOG.Info($"Init()", this);

            this.cameraMGR = cameraMGR;
            this.friends = friends;
        }
        public void PrepareMaze()
        {
            LOG.Info($"PrepareMaze()", this);

            Dodo.One.StopMoving();

            var dodoStartPosition = enterTR.position + Vector3.left * enterDistance;
            Dodo.One.TeleportTo(dodoStartPosition);

            cameraMGR.MoveTo(cameraTR.position);

            var random = new RangeInteger(1, keys.Length).RandomOne();
            foreach (var (key, i) in keys.Select((k, i) => (k, i)))
            {
                key.Reset();
                key.gameObject.SetActive(i == random - 1);
            }
            Block.RandomizeBlocks(blocksTR);

            exitBlockTR.gameObject.SetActive(true);
            lockAnim.SetTrigger("closed");
            exitTriggerGO.SetActive(false);
        }
        public Coroutine StartMaze()
        {
            LOG.Info($"StartMaze()", this);

            Dodo.One.OnGetKey += dodo_OnGetKey;
            Dodo.One.OnTryExit += dodo_OnTryExit;

            isMazeComplete = false;

            crMaze = StartCoroutine(coMaze());
            return crMaze;
        }
        public void FinishMaze()
        {
            LOG.Info($"FinishMaze()", this);

            this.StopCoroutineSafe(ref crMaze);
            this.StopCoroutineSafe(ref crGetKey);
            this.StopCoroutineSafe(ref crExit);

            PlayerController.One.EnableInteraction(false);
            Dodo.One.StopMoving();

            Dodo.One.OnGetKey -= dodo_OnGetKey;
            Dodo.One.OnTryExit -= dodo_OnTryExit;

            cameraMGR.FinishLookOverMaze();
            cameraMGR.FinishFollow();

            activateAff = false;
            affTargetGO.SetActive(false);

            lockAnim.SetTrigger("close");
        }



        // Fields : caching
        private Key[] keys_ = null;
        private Key[] keys => keys_ ??= keySetTR.GetComponentsInChildren<Key>();

        // Fields
        private bool isMazeComplete = false;
        private Coroutine crMaze = null;
        private Coroutine crGetKey = null;
        private Coroutine crExit = null;
        private CameraMGR cameraMGR = null;
        private bool activateAff = false;
        private Friend[] friends = null;

        // Event Handlers
        private void dodo_OnGetKey(Key key)
        {
            LOG.Info($"dodo_OnGetKey() | {key.name}", this);

            exitTriggerGO.SetActive(true);
            crGetKey = StartCoroutine(coGetKey(key));
        }
        private void dodo_OnTryExit()
        {
            LOG.Info($"dodo_OnTryExit()", this);

            exitTriggerGO.SetActive(false);
            crExit = StartCoroutine(coExit());
        }

        // Overrides
        protected override IEnumerator onStartAff()
        {
            if (activateAff)
            {
                var dodoPosition = Dodo.One.Position + affDistance;
                affTargetGO.transform.position = new Vector3(dodoPosition.x, dodoPosition.y, transform.position.z);
                affTargetGO.SetActive(true);
            }
            yield return null;
        }
        protected override IEnumerator onFinishAff()
        {
            affTargetGO.SetActive(false);
            yield return null;
        }


        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Animator lockAnim = null;
        [SerializeField] private Transform blocksTR = null;
        [SerializeField] private Transform keySetTR = null;
        [SerializeField] private Transform enterTR = null;
        [SerializeField] private Transform enterBlockTR = null;
        [SerializeField] private Transform exitBlockTR = null;
        [SerializeField] private Transform cameraTR = null;
        [SerializeField] private Transform[] cameraLookOverPositions = null;
        [SerializeField] private GameObject exitTriggerGO = null;
        [Header("★ Config")]
        [SerializeField] private float enterDistance = 2;
        [SerializeField] private float exitDistance = 8;
        [SerializeField] private Vector3 affDistance;
        [SerializeField] private float keyDelay = 0.1f;
        [Header("★ Sound")]
        [SerializeField] private AudioClip getKeyCLIP = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            activateAff = false;
            affTargetGO.SetActive(false);
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coMaze()
        {
            using (LOG.Coroutine($"coGetKey()", this))
            {
                PlayerController.One.EnableInteraction(false);
                yield return null;

                yield return cameraMGR.StartLooKOverMaze(cameraLookOverPositions.Select(p => p.position).ToArray());

                cameraMGR.FinishLookOverMaze();
                cameraMGR.StartFollow();
                yield return null;

                enterBlockTR.gameObject.SetActive(false);
                yield return Dodo.One.MoveAndWait(enterTR.position);
                yield return null;

                enterBlockTR.gameObject.SetActive(true);
                yield return null;

                activateAff = true;
                PlayerController.One.EnableInteraction(true);
                yield return null;

                yield return new WaitUntil(() => isMazeComplete);
                yield return null;
            }
        }
        IEnumerator coGetKey(Key key)
        {
            using (LOG.Coroutine($"coGetKey()", this))
            {
                PlayerController.One.IgnoreCurrentControl();
                yield return new WaitForSeconds(keyDelay);

                Dodo.One.GetKey();
                friends.ForEach(f => f.Key());
                yield return null;

                AudioMGR.One.PlayEffect(getKeyCLIP);
                yield return null;

                key.gameObject.SetActive(false);
                yield return ActivityUI.One.GetKey();
                yield return null;

                lockAnim.SetTrigger("open");
                yield return new WaitForSeconds(1.5f);

                exitBlockTR.gameObject.SetActive(false);
                yield return null;
            }
        }
        IEnumerator coExit()
        {
            using (LOG.Coroutine($"coExit()", this))
            {
                activateAff = false;
                affTargetGO.SetActive(false);
                PlayerController.One.EnableInteraction(false);
                Dodo.One.StopMoving();
                yield return null;

                var exitPosition = exitBlockTR.position + Vector3.right * exitDistance;
                yield return Dodo.One.MoveAndWait(exitPosition);

                Dodo.One.StopMoving();
                yield return null;

                isMazeComplete = true;
                yield return null;
            }
        }
    }
}