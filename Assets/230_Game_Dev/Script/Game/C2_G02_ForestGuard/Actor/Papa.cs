using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DoDoEng.Game.C2_G02.EventBus;

// #475 happyCLIP 미사용
#pragma warning disable 0414 

namespace DoDoEng.Game.C2_G02
{
    public class Papa : MonoBehaviour
    {
        // Fields
        private Queue<int> laneForMagic = new Queue<int>();
        private bool isCastingMagic = false;
        private Coroutine crHappy = null;
        private bool isCaution = false;

        // Functions
        private bool isIdleOrCaution => ani.IsAnimationPlaying(PapaAnimation.Idle) || ani.IsAnimationPlaying(PapaAnimation.Caution);

        // Event Handlers
        private void monster_OnDefend(Monster m)
        {
            LOG.Info($"monster_OnDefend()", this);

            if (!isCastingMagic)
            {
                AudioMGR.One.StopEffectLL();
                var clip = UtilArray.ExtractOne(wrongCLIP);
                AudioMGR.One.PlayEffectLL(clip);

                ani.PlayAnimation(PapaAnimation.Wrong);
                isCaution = false;
            }
        }
        private void monster_OnDied(Monster m)
        {
            LOG.Info($"monster_OnDied()", this);

            if (!isCastingMagic)
            {
                AudioMGR.One.StopEffectLL();
                this.StopCoroutineSafe(ref crHappy);
                crHappy = StartCoroutine(coHappy());
            }
        }
        private void plant_OnBeAttacked(int lane)
        {
            LOG.Info($"plant_OnBeAttacked() | {lane}", this);

            if (!laneForMagic.Contains(lane))
                laneForMagic.Enqueue(lane);
        }
        private void plant_OnDied()
        {
            LOG.Info($"plant_OnDied()", this);

            StopAllCoroutines();
            ani.PlayAnimation(PapaAnimation.Wrong, PapaAnimation.Idle);
            isCaution = false;
        }
        private void cannon_OnBeAttacked()
        {
            LOG.Info($"cannon_OnBeAttacked()", this);

            if (!isCastingMagic)
            {
                AudioMGR.One.StopEffectLL();
                var clip = UtilArray.ExtractOne(damageCLIP);
                AudioMGR.One.PlayEffectLL(clip);

                ani.PlayAnimation(PapaAnimation.Damage, PapaAnimation.Idle);
                isCaution = false;
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private PapaAni ani = null;
        [SerializeField] private Magic[] magics = null;
        [SerializeField] private MonsterMGR monsterMGR = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip[] correctCLIP = null;
        [SerializeField] private AudioClip[] wrongCLIP = null;
        [SerializeField] private AudioClip[] damageCLIP = null;
        [SerializeField] private AudioClip magicCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float magicPreDelay = 0.3f;
        [SerializeField] private float magicDelay = 0.3f;

        // Unity Messages
        private void Awake()
        {
            magics.ForEach(m => m.gameObject.SetActive(true));
        }
        private void Start()
        {
        }
        private void Update()
        {
            if (!isCastingMagic && laneForMagic.Count > 0)
            {
                this.StopCoroutineSafe(ref crHappy);
                StartCoroutine(coMagic());

            }

            if (isIdleOrCaution)
            {
                if (!isCaution && monsterMGR.IsCaution)
                {
                    isCaution = true;
                    ani.PlayAnimationLoopT2(PapaAnimation.Caution);
                }
                else if(isCaution && !monsterMGR.IsCaution)
                {
                    isCaution = false;
                    ani.PlayAnimationLoopT2(PapaAnimation.Idle);
                }

            }
        }
        private void OnEnable()
        {
            EventBus.Subscribe<EventBus.MonsterDiedEvent>(monster_OnDied);
            EventBus.Subscribe<EventBus.MonsterDefendEvent>(monster_OnDefend);
            EventBus.Subscribe<EventBus.PlantBeAttackedEvent>(plant_OnBeAttacked);
            EventBus.Subscribe<EventBus.PlantDiedEvent>(plant_OnDied);
            EventBus.Subscribe<EventBus.CannonBeAttackedEvent>(cannon_OnBeAttacked);

        }
        private void OnDisable()
        {
            EventBus.Unsubscribe<EventBus.MonsterDiedEvent>(monster_OnDied);
            EventBus.Unsubscribe<EventBus.MonsterDefendEvent>(monster_OnDefend);
            EventBus.Unsubscribe<EventBus.PlantBeAttackedEvent>(plant_OnBeAttacked);
            EventBus.Unsubscribe<EventBus.PlantDiedEvent>(plant_OnDied);
            EventBus.Unsubscribe<EventBus.CannonBeAttackedEvent>(cannon_OnBeAttacked);
        }

        // Unity Coroutine
        IEnumerator coHappy()
        {
            using (LOG.Coroutine($"coHappy()", this))
            {
                // #475 정답시 그린썸 좋아할 때, 효과음 제거
                // #3744 정답시 효과음 추가
                var clip = UtilArray.ExtractOne(correctCLIP);
                AudioMGR.One.PlayEffectLL(clip);
                yield return ani.PlayAnimationAndWait(PapaAnimation.Correct);

                // #475 정답시 그린썸 좋아할 때, 효과음 제거
                //AudioMGR.One.PlayEffect(happyCLIP);
                //yield return ani.PlayAnimationAndWait(PapaAnimation.Correct);
                isCaution = false;
            }
        }
        IEnumerator coMagic()
        {
            using (LOG.Coroutine($"coMagic()", this))
            {
                if (laneForMagic.TryPeek(out var lane))
                {
                    isCastingMagic = true;
                    yield return new WaitForSeconds(magicPreDelay);

                    AudioMGR.One.PlayEffect(magicCLIP);
                    yield return magics[lane - 1].PlayMagic();
                    // PlayMagic()에 통합됨
                    //yield return ani.PlayAnimationAndWait(PapaAnimation.Skill);

                    isCaution = false;
                    yield return new WaitForSeconds(magicDelay);

                    laneForMagic.Dequeue();
                    isCastingMagic = false;
                    yield return null;
                }
                else LOG.Warning($"No lane for magic", this);
            }
        }
    }
}