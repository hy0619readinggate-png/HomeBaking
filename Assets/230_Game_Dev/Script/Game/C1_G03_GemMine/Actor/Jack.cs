using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Game.C1_G03
{
    public class Jack : MonoBehaviour
    {
        // Properties
        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = value;
        }

        // Methods
        public void Hide()
        {
            LOG.Function(this);
            gameObject.SetActive(false);
        }
        public void SetInitialPosition(Cell cell)
        {
            LOG.Function(this, $"{cell}");

            gameObject.SetActive(false);

            place(cell);
            jackAni.PlayAnimation(JackAnimation.Idle);
        }
        public Coroutine StartMove(Cell[] path)
        {
            LOG.Function(this);

            AudioMGR.One.PlayEffectLL(moveCLIP, true);
            crMove = StartCoroutine(coMove(path));
            return crMove;
        }
        public void FinishMove(Cell arrivedCell)
        {
            LOG.Function(this);

            AudioMGR.One.StopEffectLL();
            this.StopCoroutineSafe(ref crMove);
            transform.DOKill();

            place(arrivedCell);
        }
        public void GetOffForMine(Direction dir)
        {
            LOG.Info($"GetOffForMine(), {dir}", this);

            fxANIM.SetTrigger("Show");
            var ani = dir == Direction.B ?
                    JackAnimation.IdleEmptyFront :
                    JackAnimation.IdleEmpty;
            jackAni.PlayAnimationLoop(ani);
        }
        public void Ride(Direction dir)
        {
            LOG.Info($"Ride(), {dir}", this);

            fxANIM.SetTrigger("Show");
            var ani = dir == Direction.B ?
                    JackAnimation.IdleFront :
                    JackAnimation.Idle;
            jackAni.PlayAnimationLoop(ani);
        }
        public void TimeOut(Direction dir)
        {
            LOG.Function(this);

            var ani1 = dir == Direction.B ?
                    JackAnimation.TimeoutFront :
                    JackAnimation.Timeout;
            var ani2 = dir == Direction.B ?
                    JackAnimation.TimeoutIdleFront :
                    JackAnimation.TimeoutIdle;
            jackAni.PlayAnimation(ani1, ani2);
        }
        public void RideAndHappy(Direction dir)
        {
            LOG.Function(this);

            var ani = dir == Direction.B ?
                    JackAnimation.EndFront :
                    JackAnimation.End;
            jackAni.PlayAnimationLoop(ani);

            fxOutroANIM.SetTrigger("Show");
        }


        // Fields
        private Coroutine crMove = null;

        // Functions
        private void place(Cell cell)
        {
            var pos = Map.One.CellCenterOf(cell);
            transform.SetXYOnly(pos);
            Map.One.PlacePlayer(cell);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private JackAni jackAni = null;
        [SerializeField] private Animator fxANIM = null;
        [SerializeField] private Animator fxOutroANIM = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip moveCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float moveSpeed = 2f;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {

        }

        // Unity Coroutine
        IEnumerator coMove(Cell[] path)
        {
            using (LOG.Coroutine($"coMove()", this))
            {
                // 이동
                var cCell = path.First();
                foreach (var nCell in path.Skip(1))     // 현재 위치 제외
                {
                    // 애니메이션
                    var direction = Cell.DirectionOf(cCell, nCell);
                    jackAni.PlayAnimationOfMoving(direction);

                    // 이동
                    var cPos = Map.One.CellCenterOf(cCell);
                    var nPos = Map.One.CellCenterOf(nCell);
                    var duration = Vector3.Distance(cPos, nPos) / moveSpeed;
                    transform.DOMoveX(nPos.x, duration).SetEase(Ease.Linear);
                    transform.DOMoveY(nPos.y, duration).SetEase(Ease.Linear);
                    yield return new WaitForSeconds(duration);

                    cCell = nCell;
                }

                AudioMGR.One.StopEffectLL();
            }
        }
    }
}