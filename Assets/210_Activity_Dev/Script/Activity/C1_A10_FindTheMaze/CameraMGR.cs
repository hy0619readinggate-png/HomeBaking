using beyondi.Util;
using Com.LuisPedroFonseca.ProCamera2D;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C1_A10
{
    [RequireComponent(typeof(ProCamera2D))]
    public class CameraMGR : MonoBehaviour
    {
        // Methods
        public void MoveTo(Vector3 position)
        {
            LOG.Info($"MoveTo()", this);

            procamera2D.enabled = false;
            transform.position = position;
        }
        public void StartFollow()
        {
            LOG.Info($"StartFollow()", this);

            procamera2D.enabled = true;
            procamera2D.Reset(false);
        }
        public void FinishFollow()
        {
            LOG.Info($"FinishFollow()", this);

            procamera2D.enabled = false;
        }
        public Coroutine StartLooKOverMaze(Vector3[] positions)
        {
            LOG.Info($"StartLooKOverMaze()", this);

            crLookOver = StartCoroutine(coLookOver(positions));
            return crLookOver;
        }
        public void FinishLookOverMaze()
        {
            LOG.Info($"FinishLookOverMaze()", this);

            transform.DOKill();
            this.StopCoroutineSafe(ref crLookOver);
        }


        // Fields : caching
        private ProCamera2D procamera2D_ = null;
        private ProCamera2D procamera2D => procamera2D_ ??= GetComponent<ProCamera2D>();

        // Fields
        private Coroutine crLookOver = null;



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private float lookOverMoveSpeed = 3f;
        [SerializeField] private float lookOverMoveDelay = 0.5f;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coLookOver(Vector3[] positions)
        {
            transform.position = positions[0];

            foreach (var (p, i) in positions.Select((p, i) => (p, i)).Where((p, i) => i > 0))
            {
                var distance = Vector2.Distance(p, transform.position);
                var duration = distance / lookOverMoveSpeed;
                yield return transform.DOMove(p, duration).SetEase(Ease.Linear).WaitForCompletion();

                yield return new WaitForSeconds(lookOverMoveDelay);

            }
        }
    }
}