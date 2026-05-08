using beyondi.Coroutine;
using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C1_A06
{
    public class Seat : MonoBehaviour,
        IDropHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        ISubmitable
    {
        // Properties
        public bool IsProblemSeat => problemSeat;
        public bool IsOccupied => activeCharacter != null;
        public bool IsAnswer { get; private set; } = false;
        public bool IsSubmit { get; private set; }
        public int CharacterID { get; private set; }
        public Balloon Balloon { get; set; }
        public Vector3 CenterPosition => centerTR.position;

        // Methods
        public void Initialize(bool snapCharacter)
        {
            LOG.Info($"Initialize()", this);

            this.snapCharacter = snapCharacter;
        }
        public void Setup(ExampleData examData)
        {
            LOG.Info($"Setup() | {examData}", this);

            IsSubmit = false;
            IsAnswer = examData.IsAnswer;
            phoneticCLIP = examData.PhoneticCLIP;

            characters.SetActiveAll(false);
            activeCharacter = null;
        }
        public void Occupy(int characterID)
        {
            LOG.Info($"Occupy() | {characterID}", this);

            CharacterID = characterID;

            activeCharacter = characters.SetActiveOnly(characterID - 1);
            if (CharacterID <= 3)
                activeCharacter.PlayAnimationLoopT1(CharacterAnimation.SitIdle1);
            else activeCharacter.PlayAnimationLoopT1(CharacterAnimation.Idle1);
        }
        public void CheerUp(bool submit)
        {
            LOG.Info($"CheerUp() | {submit}", this);

            if (activeCharacter != null)
            {
                activeCharacter.Correct(submit);
            }
        }
        public void Idle()
        {
            LOG.Info($"Idle()", this);

            if (activeCharacter != null)
            {
                if (CharacterID <= 3)
                    activeCharacter.PlayAnimationLoopT1(CharacterAnimation.SitIdle1);
                else activeCharacter.PlayAnimationLoopT1(CharacterAnimation.Idle1);
            }
        }
        public void Sad()
        {
            LOG.Info($"Sad()", this);

            if (IsOccupied)
                StartCoroutine(coSad());
        }
        public void RidePlay()
        {
            LOG.Info($"RidePlay()", this);

            activeCharacter.PlayAnimationLoopT1(CharacterAnimation.Idle2);
        }
        public Coroutine ThrowAway(int characterID)
        {
            LOG.Info($"ThrowAway() | {characterID}", this);

            return StartCoroutine(coThrowAway(characterID));
        }
        public Coroutine StartTryWrongShake(int characterID)
        {
            LOG.Info($"StartTryWrongShake()", this);

            crTryWrongShake = StartCoroutine(coTryWrongShake(characterID));
            return crTryWrongShake;
        }
        public void FinishTryWrongShake()
        {
            LOG.Info($"TryWrongShake()", this);

            this.StopCoroutineSafe(ref crTryWrongShake);
        }



        // Fields
        private CharacterAni activeCharacter = null;
        private AudioClip phoneticCLIP = null;
        private Coroutine crTryWrongShake = null;
        private bool snapCharacter = false;

        // Functions
        private void clearCharacterOver(Character character)
        {
            if (glowGO != null)
                glowGO.SetActive(false);

            if (snapCharacter)
            {
                character.Snapped(false);
                characters.SetActiveAll(false);
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private CharacterAni[] characters = null;
        [SerializeField] private GameObject glowGO = null;
        [SerializeField] private Transform centerTR = null;
        [SerializeField] private Animator shakeAnim = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip dropCLIP = null;
        [SerializeField] private AudioClip shakeCLIP = null;
        [Header("★ Config")]
        [SerializeField] private bool problemSeat = false;

        // Unity Messages
        private void Awake()
        {
            if (glowGO != null)
                glowGO?.SetActive(false);
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coSad()
        {
            using (LOG.Coroutine("coSad()", this))
            {
                if (CharacterID <= 3)
                {
                    yield return activeCharacter.PlayAnimationAndWait(CharacterAnimation.SitWrong);

                    activeCharacter.PlayAnimationLoopT1(CharacterAnimation.SitIdle1);
                    yield return null;
                }
                else
                {
                    yield return activeCharacter.PlayAnimationAndWait(CharacterAnimation.Wrong);

                    activeCharacter.PlayAnimationLoopT1(CharacterAnimation.Idle1);
                    yield return null;
                }
            }
        }
        IEnumerator coThrowAway(int characterID)
        {
            using (LOG.Coroutine($"coThrowAway() | {characterID}", this))
            {
                var active = characters.SetActiveOnly(characterID - 1);
                yield return active.PlayAnimationAndWait(CharacterAnimation.ThrowR);

                characters.SetActiveAll(false);
            }
        }
        IEnumerator coTryWrongShake(int characterID)
        {
            using (LOG.Coroutine($"coTryWrongShake() | {shakeAnim != null}", this))
            {
                var active = characters.SetActiveOnly(characterID - 1);
                yield return active.PlayAnimationAndWait(CharacterAnimation.Sit);

                AudioMGR.One.PlayEffect(shakeCLIP);
                if (shakeAnim != null)
                    shakeAnim.SetTrigger("wrong");

                active.PlayWrongShakeSFX();
                yield return active.PlayAnimationAndWait(CharacterAnimation.Shake1);
                yield return active.PlayAnimationAndWait(CharacterAnimation.Shake2);
                yield return null;
                yield return null;
            }
        }



        // IDropHandler
        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            LOG.Info($"OnDrop() | {eventData.pointerDrag.name}", this);

            var character = eventData.pointerDrag.GetComponent<Character>();
            if (character != null && !IsOccupied)
            {
                AudioMGR.One.PlayEffect(dropCLIP);

                clearCharacterOver(character);

                eventData.Use();
                IsSubmit = true;
            }
        }

        // IPointerEnterHandler
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            LOG.Info($"OnPointerEnter()", this);

            if (!IsOccupied && eventData.pointerDrag != null)
            {
                if (glowGO != null)
                {
                    glowGO.SetActive(true);
                    AudioMGR.One.PlayNarration(phoneticCLIP);
                }

                if (snapCharacter)
                {
                    var character = eventData.pointerDrag.GetComponent<Character>();
                    var c = characters.SetActiveOnly(character.CharacterID - 1);
                    c.PlayAnimationLoopT1(CharacterAnimation.SitIdle1);
                    character.Snapped(true);
                }
            }
        }

        // IPointerExitHandler
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            LOG.Info($"OnPointerExit()", this);

            if (!IsOccupied && eventData.pointerDrag != null)
            {
                var character = eventData.pointerDrag.GetComponent<Character>();
                clearCharacterOver(character);
            }
        }
    }
}