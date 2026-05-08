using beyondi.Util;
using System;
using UnityEngine;

namespace DoDoEng.Common
{
    public abstract class AnimationMecanim<T> : MonoBehaviour where T : struct, IConvertible
    {
        // Methods
        public void PlayAnimation(T animation)
        {
            var aniEnum = animation as Enum;
            var aniName = aniEnum.GetAttribute<AnimationAttribute>().Name;
            anim.SetTrigger(aniName);
        }
        public void AbortAnimation()
        {
            var animation = DefaultAnimationAttribute.GetDefault<T>();
            PlayAnimation(animation);
        }



        // Unity Inspectors
        [SerializeField] private Animator anim;

        // Unity Messages
        protected virtual void Awake()
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("T must be an enumerated type");

            if (anim == null)
                anim = GetComponent<Animator>();
        }
    }
}