using beyondi.Behaviour;
using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DoDoEng.Activity.C1_A12
{
    [RequireComponent(typeof(PapaMotion))]
    public class Papa : BYDSingleton<Papa>
    {
        // Properties
        public float MoveSpeed => motion.MoveSpeed;

        // Methods
        public void MoveTo(Vector3 position) => motion.MoveTo(position);
        public void StopMoving() => motion.StopMoving();
        public void DoCorrect() => motion.DoCorrect();
        public void DoWrong() => motion.DoWrong();
        public Coroutine MoveAndWait(Vector2 position) => motion.MoveAndWait(position);
        public Coroutine MoveToCenterAndWait()
        {
            LOG.Info($"MoveToCenterAndWait()", this);

            return motion.MoveAndWait(origin);
        }
        public void MoveToCenter()
        {
            LOG.Info($"MoveToCenter()", this);

            transform.position = origin;
        }

        // Methods
        public void Setup()
        {
            LOG.Info($"Setup()", this);

            var positions = UtilArray.Shuffled(sheepPositions);

            transform.position = initialPosition;

            seatQ.Clear();
            positions.ForEach(p => seatQ.Enqueue(p));
        }
        public Transform TakeSeat()
        {
            LOG.Info($"TakeSeat()", this);

            return seatQ.Dequeue();
        }
        public void ReleaseSeat(Transform tr)
        {
            LOG.Info($"ReleaseSeat()", this);

            seatQ.Enqueue(tr);
        }



        // Fields : caching
        private PapaMotion motion_ = null;
        private PapaMotion motion => motion_ ??= GetComponent<PapaMotion>();

        // Fields
        private Queue<Transform> seatQ = new Queue<Transform>();
        private Vector2 origin;




        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Transform[] sheepPositions = null;
        [SerializeField] private Vector2 initialPosition;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            origin = transform.position;
        }
        private void Start()
        {

        }
    }
}