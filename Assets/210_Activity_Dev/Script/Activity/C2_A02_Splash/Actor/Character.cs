using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using static Unity.VisualScripting.Member;

namespace DoDoEng.Activity.C2_A02
{
    public class Character : MonoBehaviour
    {
        // Properties
        public int CharacterID => characterID;

        // Methods
        public void Setup(int characterID)
        {
            LOG.Info($"Setup() | {characterID}", this);

            characterModels.SetActiveOnly(characterID - 1);
            this.characterID = characterID;

            activeAnim.PlayAnimationLoopT2(CharacterAnimation.Idle1, CharacterAnimation.Idle2);
        }
        public void Empty()
        {
            LOG.Info($"Empty()", this);

            characterID = -1;
        }
        public void Idle()
        {
            LOG.Info($"Idle()", this);

            activeAnim.PlayAnimationLoopT2(CharacterAnimation.Idle1, CharacterAnimation.Idle2);
        }
        public void IdleOnShip()
        {
            LOG.Info($"IdleOnShip()", this);

            activeAnim.PlayAnimationLoop(CharacterAnimation.IdleOnShip);
        }
        public void Cheese()
        {
            LOG.Info($"Cheese()", this);

            activeAnim.Cheese();
        }
        public void Wrong()
        {
            LOG.Info($"Wrong()", this);

            activeAnim.Wrong();
        }
        public void PlayCheeseSound()
        {
            LOG.Info($"PlayCheeseSound()", this);

            crPlayCheeseSound = StartCoroutine(coPlayCheeseSound());
        }
        public void StopCheeseSound()
        {
            LOG.Info($"StopCheeseSound()", this);

            source.Stop();
            this.StopCoroutineSafe(ref crPlayCheeseSound);
        }



        // Fields : caching
        private AudioSource source_ = null;
        private AudioSource source => source_ ??= GetComponent<AudioSource>();


        // Fields
        private int characterID = 1;
        private CharacterAni activeAnim => characterModels[characterID - 1];
        private Coroutine crPlayCheeseSound = null;


        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private CharacterAni[] characterModels = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coPlayCheeseSound()
        {
            using (LOG.Coroutine($"coPlayCheeseSound()", this))
            {
                yield return null;

                while (true)
                {
                    var clip = UtilArray.ExtractOne(activeAnim.CheeseClips);
                    source.clip = clip;
                    source.Play();
                    yield return new WaitForSeconds(clip.length);
                }
            }

        }




    }
}