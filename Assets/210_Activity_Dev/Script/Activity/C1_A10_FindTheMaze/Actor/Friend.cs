using beyondi.Util;
using DoDoEng.Activity.C1_A10;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng
{
    [RequireComponent(typeof(CharacterMotion))]
    [RequireComponent(typeof(CharacterFollower))]
    public class Friend : MonoBehaviour
    {
        // Properties
        public int CharacterID => characters.FindIndex(activeCharacter) + 1;
        public bool IsFollowing => follewer.IsFollowing;

        // Methods
        public void Setup(int characterID)
        {
            LOG.Info($"Setup() | {characterID}", this);

            activeCharacter = characters.SetActiveOnly(characterID - 1);
            motion.SetCharacter(activeCharacter);
        }

        // Methods
        public void TeleportTo(Vector3 position) => motion.TeleportTo(position);
        public void MoveTo(Vector3 position) => motion.MoveTo(position);
        public void StopMoving() => motion.StopMoving();
        public void ChangeSpeed(bool highSpeed) => motion.ChangeSpeed(highSpeed);
        public Coroutine MoveAndWait(Vector2 position, bool forceForward = false) => motion.MoveAndWait(position, forceForward);
        public bool IsMoving => motion.IsMoving;
        public void SetFollow(CharacterLeader leader, float distance) => follewer.SetFollow(leader, distance);

        // Methods
        public void Appear()
        {
            LOG.Info($"Appear()", this);

            activeCharacter.PlayAnimation(CharacterAnimation.Appear);
        }
        public void Idle()
        {
            LOG.Info($"Idle()", this);

            activeCharacter.PlayAnimationLoop(CharacterAnimation.Idle);
        }
        public void IdleForward()
        {
            LOG.Info($"IdleForward()", this);

            activeCharacter.PlayAnimationLoop(CharacterAnimation.Idle);
            activeCharacter.FlipX(true);
        }
        public void Direction(bool right = true)
        {
            LOG.Info($"Direction()", this);

            activeCharacter.FlipX(right);
        }
        public void Correct()
        {
            LOG.Info($"Correct()", this);

            activeCharacter.PlayAnimationLoop(CharacterAnimation.Correct1);
            AudioMGR.One.PlayEffect(correctClip[CharacterID - 1]);
        }
        public void Wrong()
        {
            LOG.Info($"Wrong()", this);

            activeCharacter.PlayAnimation(CharacterAnimation.Wrong1);
        }
        public void Join()
        {
            LOG.Info($"Join()", this);

            activeCharacter.FlipX(true);
            activeCharacter.PlayAnimation(CharacterAnimation.Join);
        }
        public void Gogo()
        {
            LOG.Info($"Gogo()", this);

            activeCharacter.PlayAnimation(CharacterAnimation.Gogo);
        }
        public void LookAt(Vector3 position)
        {
            LOG.Info($"LookAt()", this);

            var flipX = position.x >= transform.position.x;
            activeCharacter.FlipX(flipX);
        }

        public void JustLook()
        {
            LOG.Info($"JustLook()", this);

            activeCharacter.PlayAnimationLoop(CharacterAnimation.IdleR);
        }
        public void Key()
        {
            LOG.Info($"Key()", this);

            activeCharacter.PlayAnimation(CharacterAnimation.Key);
        }



        // Fields : caching
        private CharacterMotion motion_ = null;
        private CharacterMotion motion => motion_ ??= GetComponent<CharacterMotion>();
        private CharacterFollower follewer_ = null;
        private CharacterFollower follewer => follewer_ ??= GetComponent<CharacterFollower>();

        // Fields
        private CharacterAni activeCharacter = null;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private CharacterAni[] characters = null;
        [Header("★ Audios")]
        [SerializeField] private AudioClip[] correctClip = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}