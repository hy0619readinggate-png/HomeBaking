using Spine.Unity;
using System;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Common
{
    [RequireComponent(typeof(SkeletonGraphic))]
    public class SpineAudioClips : MonoBehaviour
    {
        // Methods
        public AudioClip ClipOf(string animation)
        {
            LOG.Info($"ClipOf() | {animation}", this);

            return items.SingleOrDefault(it => it.animation == animation)?.clip;
        }



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private SpineAudioClips_Item[] items;
    }

    [Serializable]
    public class SpineAudioClips_Item
    {
        [SpineAnimation]
        public string animation;
        public AudioClip clip;
    }
}