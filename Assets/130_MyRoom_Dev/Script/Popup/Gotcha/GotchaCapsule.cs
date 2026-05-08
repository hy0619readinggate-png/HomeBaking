using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using System;
using DoDoEng.MyRoom.Behavior;
using UnityEngine.Playables;

namespace DoDoEng.MyRoom.Popup
{
    public enum GotchaCapsuleSkin
    {
        Gotcha_Capsule_Bule,
        Gotcha_Capsule_Red,
        Gotcha_Capsule_Sky,
    }

    public class GotchaCapsule : MonoBehaviour
	{
        // Definitions
        public enum ItemType
        {
            Losing,
            Candy,
            Pet
        }

        // Properties
        public bool IsOpened => isOpened;
        public int IdxPet => petKind;
        public ItemType Type => type;
        public bool Interactable
        {
            get => capsuleBT.interactable;
            set => capsuleBT.interactable = value;
        }

        // Methods
        public void Init(GotchaCapsuleSkin skin, ItemType type, int idxBad = 0, int petKind = -1)
        {
            LOG.Function(this, $"| skin={skin} | type={type} | idxBad={idxBad} | petKind={petKind}");

            isOpened = false;
            this.type = type;
            this.petKind = UnityEngine.Random.Range(0, pet.KindCount);
            if (petKind != -1)
            {
                this.petKind = petKind;
            }

            capsuleSG.Skeleton.SetSkin(skin.ToString());
            capsuleSG.AnimationState.SetEmptyAnimation(0, 0);
            capsuleSG.Skeleton.SetSlotsToSetupPose();

            candy.SetActive(type == ItemType.Candy);
            pet.gameObject.SetActive(type == ItemType.Pet);
            if (type == ItemType.Pet) pet.Init(this.petKind);
            for (int i = 0; i < badGO.Length; i++)
            {
                badGO[i].SetActive(type == ItemType.Losing && i == idxBad);
            }

            failFX.SetActive(false);
            candyFX.SetActive(false);
            petFX.SetActive(false);
            badFX.SetActive(false);

            Idle();
        }
        public void Idle()
        {
            capsuleSG.AnimationState.SetAnimation(0, "Capsule_idle", true);
        }
        public void Affordance()
        {
            capsuleSG.AnimationState.SetAnimation(0, "Capsule_Emphasis", true);
        }
        public void Close()
        {
            failFX.SetActive(false);
            candyFX.SetActive(false);
            petFX.SetActive(false);
            badFX.SetActive(false);
        }

        // Events
        public event Action<GotchaCapsule> OnOpening;
        public event Action<GotchaCapsule> OnOpened;



        // Fields : caching
        // Fields
        private bool isOpened = false;
        private ItemType type;
        private int petKind = -1;

        // Functions

        // Event Handlers
        private void capsuleBT_OnClick()
        {
            LOG.Info($"capsuleBT_OnClick()", this);

            if (!isOpened)
            {
                AudioMGR.One.PlayEffect(SfxMoment.Common_Click);

                isOpened = true;
                OnOpening?.Invoke(this);

                StartCoroutine(openCapsule());
            }
        }

        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
		[SerializeField] private SkeletonGraphic capsuleSG = null;
		[SerializeField] private MyRoomPet pet = null;
		[SerializeField] private GameObject candy = null;
        [SerializeField] private Button capsuleBT = null;
        [SerializeField] private GameObject failFX = null;
        [SerializeField] private GameObject candyFX = null;
        [SerializeField] private GameObject petFX = null;
        [SerializeField] private GameObject[] badGO = null;
        [SerializeField] private GameObject badFX = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip rewardCLIP = null;
        [SerializeField] private AudioClip rewardFailCLIP = null;
        [SerializeField] private AudioClip rewardFailJellyCLIP = null;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector petCapsuleTouchTL = null;

        // Unity Messages
        private void Awake()
		{
            capsuleBT.gameObject.SetActive(true);
            capsuleBT.onClick.AddListener(() => capsuleBT_OnClick());
        }
		private void Start()
		{
        }

        // Unity Coroutine
        private IEnumerator openCapsule()
        {
            capsuleSG.AnimationState.SetAnimation(0, "Capsule_Open", false);

            if (Type == ItemType.Pet)
            {
                yield return new WaitForSeconds(0.5f);

                petCapsuleTouchTL.time = 0;
                petCapsuleTouchTL.Play();

                yield return new WaitForSeconds((float)petCapsuleTouchTL.duration);

                AudioMGR.One.PlayEffect(rewardCLIP);
                petFX.SetActive(true);
            }
            else if (Type == ItemType.Losing)
            {
                yield return new WaitForSeconds(0.9f);
                AudioMGR.One.PlayEffect(rewardFailJellyCLIP);
                failFX.SetActive(true);
                yield return new WaitForSeconds(1.0f);
                AudioMGR.One.PlayEffect(rewardFailCLIP);
                badFX.SetActive(true);
                badGO.ForEach(bad => bad.SetActive(false));
            }
            else if (Type == ItemType.Candy)
            {
                yield return new WaitForSeconds(0.9f);
                AudioMGR.One.PlayEffect(rewardCLIP);
                candyFX.SetActive(candy.activeSelf);
            }

            OnOpened?.Invoke(this);
        }
    }
}