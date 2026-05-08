using System.Collections;
using UnityEngine;

namespace DoDoEng.Game.C4_G02
{
    public enum BubbleType { Monster, Text, None = 999 }

    public class Bubble : MonoBehaviour
    {

        // Properties
        public BubbleType BubbleType => _BubbleType;



        // Methods
        public void SetSpeed(float value)
        {
            speed = value;
        }
        public void Pop()
        {
            pop();
        }



        // Fields
        protected float speed = 1f;
        protected Coroutine runCoroutine = null;

        protected readonly int hashKey_Show = Animator.StringToHash("Show");
        protected readonly int hashKey_Pop = Animator.StringToHash("Pop");
        protected readonly int hashKey_ForcePop = Animator.StringToHash("ForcePop");



        // Functions
        protected virtual void pop() { }


        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private BubbleType _BubbleType = BubbleType.None;

        // Unity Messages
        protected virtual void Start()
        {
            runCoroutine = StartCoroutine(coRun());
        }
        protected virtual void OnDestroy()
        {
            StopAllCoroutines();
        }


        // Unity Coroutine
        protected virtual IEnumerator coRun() { yield return null; }

    }
}