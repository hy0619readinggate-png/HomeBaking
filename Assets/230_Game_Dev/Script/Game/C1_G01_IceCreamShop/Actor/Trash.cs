using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Game.C1_G01
{
    [RequireComponent(typeof(Animator))]
    public class Trash : MonoBehaviour, IDropHandler
    {
        // Fields : caching
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Transform iceCreamPosTR = null;
        [SerializeField] private TrashAni trashAni = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip dropCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float throwAwayDuration = 1f;

        // Unity Messages
        private void Awake()
        {
            Util.RemoveAllChildren(iceCreamPosTR);
        }
        private void Start()
        {
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            LOG.Info($"OnTriggerEnter2D() | {collision.gameObject.name}", this);

            // 근접시 애니메이션 없음 20230901 by veramocor
            //anim.SetBool("Over", true);
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            LOG.Info($"OnTriggerExit2D() | {collision.gameObject.name}", this);

            // 근접시 애니메이션 없음 20230901 by veramocor
            //anim.SetBool("Over", false);
        }

        // Unity Coroutine
        IEnumerator coThrowAway(IceCreamCup cup)
        {
            using (LOG.Coroutine($"coThrowAway()", this))
            {
                cup.transform.SetParent(iceCreamPosTR);
                cup.transform.localPosition = Vector3.zero;
                anim.SetTrigger("Trash");
                var eatIDX = cup.IceCreamCount > 0 ? cup.IceCreamCount - 1 : 0;
                trashAni.PlayAnimation(TrashAnimation.Eat1 + eatIDX);
                yield return new WaitForSeconds(throwAwayDuration);

                cup.Respawn();
                yield return null;
            }
        }



        // Interface : IDropHandler
        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            LOG.Info($"OnDrop() | {eventData.pointerDrag.name}", this);

            var cup = eventData.pointerDrag.GetComponent<IceCreamCup>();
            if (cup != null && !cup.IsRespawning)
            {
                AudioMGR.One.PlayEffect(dropCLIP);
                eventData.Use();

                StartCoroutine(coThrowAway(cup));
            }
        }
    }
}