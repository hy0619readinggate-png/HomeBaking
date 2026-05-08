using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;
using System;
using DoDoEng.MyRoom.UI;
using TMPro;
using Cysharp.Threading.Tasks;

namespace DoDoEng.MyRoom.Popup
{
    public class GotchaPopup : MonoBehaviour
    {
        // Definitions
        // Properties

        // Methods
        public void Activate(bool active)
        {
            LOG.Info($"Activate() | {active}", this);

            gameObject.SetActive(active);
            UIMyRoom.One.VisibleHelpButton = !active;

            gotchaBT.interactable = active;
        }
        public void Show()
        {
            Activate(true);

            gotchaMachine.PlayAnimationLoop(GotchaMachineAnimation.Idle);
        }
        public void ShowCapsulesPanel()
        {
            opendCount = 0;

            AudioMGR.One.PlayEffect(capsuleInCLIP);
            capsulesPanel.SetActive(true);
            capsulesPanel.GetComponent<Animator>().SetTrigger("show");

            var colorIndice = UtilArray.Random(0, capsules.Length - 1);
            var badIndice = UtilArray.Random(0, capsules.Length - 1);
            var typeIndice = UtilArray.Random(0, capsules.Length - 1);
            bool hasPet = UnityEngine.Random.Range(0f, 1f) < PetChances;
            int petKind = -1;
            float randomClass = UnityEngine.Random.Range(0f, 1f);
            LOG.Info($"등급 랜덤 값 : {randomClass} (Special: 0~{SpecialChances}, Rare: {SpecialChances}~{SpecialChances+RareChances}, Common: {SpecialChances + RareChances}~1", this);
            if (UserData.One.Pets.Count == 0)
            {
                hasPet = true;
                petKind = UnityEngine.Random.Range(0, 4);
            }
            else if (randomClass <= SpecialChances)
                petKind = UnityEngine.Random.Range(8, 12);
            else if (randomClass <= SpecialChances + RareChances)
                petKind = UnityEngine.Random.Range(4, 8);
            else
                petKind = UnityEngine.Random.Range(0, 4);
            bool hasCandy = UnityEngine.Random.Range(0f, 1f) <= CandyChances;
            foreach (var (capsule, i) in capsules.Select((v, i) => (v, i)))
            {
                if (typeIndice[i] == 0 && hasPet)
                    capsule.Init((GotchaCapsuleSkin)colorIndice[i], GotchaCapsule.ItemType.Pet, 0, petKind);
                else if (typeIndice[i] == 1 && hasCandy && !LMS.One.IsMaxPlaygroundPlayCount)
                    capsule.Init((GotchaCapsuleSkin)colorIndice[i], GotchaCapsule.ItemType.Candy);
                else
                    capsule.Init((GotchaCapsuleSkin)colorIndice[i], GotchaCapsule.ItemType.Losing, badIndice[i]);
            }

            readyToTouchCapsules();
        }

        // Events
        public Action<int> OnGetPet;



        // Fields : caching

        // Fields
        private int opendCount;
        private Coroutine coAffordance = null;

        // Functions
        private void readyToTouchCapsules()
        {
            stopTouchCapsules();

            capsules.ForEach(capsule =>
            {
                if (!capsule.IsOpened)
                    capsule.Interactable = true;
            });

            coAffordance = StartCoroutine(affordance());
        }
        private void stopTouchCapsules()
        {
            if (coAffordance != null)
            {
                StopCoroutine(coAffordance);
                coAffordance = null;
            }

            capsules.ForEach(capsule =>
            {
                if (!capsule.IsOpened)
                {
                    capsule.Interactable = false;
                    capsule.Idle();
                }
            });
        }

        // Event Handlers
        private void gotchaBT_OnClick()
        {
            AudioMGR.One.PlayEffect(SfxMoment.Common_Click);
            if (UserData.One.Pets.Count < UserDataPet.MAX)
            {
                if (100 <= LMS.One.Coin)
                    startGotcha().Forget();
                else
                    SystemUI.One.ShowPopupMyPetNeedCoin();
            }
            else
                SystemUI.One.ShowPopupPetLimit();
        }
        private void capsule_OnOpening(GotchaCapsule capsule)
        {
            stopTouchCapsules();
        }
        private void capsule_OnOpened(GotchaCapsule capsule)
        {
            LOG.Info($"capsule_OnOpened() | {capsule}", this);

            opendCount++;

            if (capsule.Type == GotchaCapsule.ItemType.Pet)
                OnGetPet?.Invoke(capsule.IdxPet);
            else if (capsule.Type == GotchaCapsule.ItemType.Candy)
                LMS.One.GetCandy().Forget();

            if (opendCount < capsules.Length)
                readyToTouchCapsules();
            else
                StartCoroutine(closeCapsulesPanel());
        }

        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Button gotchaBT = null;
        [SerializeField] private GotchaMachine gotchaMachine = null;
        [SerializeField] private GameObject capsulesPanel = null;
        [SerializeField] private GotchaCapsule[] capsules = null;
        [SerializeField] private TMP_Text coinTMP = null;
        [SerializeField] private Animator coinANI = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip capsuleCLIP = null;
        [SerializeField] private AudioClip capsuleInCLIP = null;
        [SerializeField] private AudioClip capsuleOutCLIP = null;
        [SerializeField] private AudioClip capsulePupCLIP = null;
        [Header("★ Config")]
        [SerializeField] public float PetChances = 0.3f;
        [SerializeField] public float CandyChances = 0.8f;
        [SerializeField] public float SpecialChances = 0.2f;
        [SerializeField] public float RareChances = 0.3f;

        // Unity Messages
        private void Awake()
        {
            gotchaBT.onClick.AddListener(() => gotchaBT_OnClick());

            capsulesPanel.SetActive(false);

            gotchaMachine.PlayAnimationLoop(GotchaMachineAnimation.Idle);
        }
        private void Start()
        {
        }
        protected void OnEnable()
        {
            capsules.ForEach(capsule =>
            {
                capsule.OnOpening += capsule_OnOpening;
                capsule.OnOpened += capsule_OnOpened;
            });
        }
        protected void OnDisable()
        {
            capsules.ForEach(capsule =>
            {
                capsule.OnOpening -= capsule_OnOpening;
                capsule.OnOpened -= capsule_OnOpened;
            });
        }

        // Unity Coroutine
        private async UniTask startGotcha()
        {
            UIMyRoom.One.VisibleBackButton = false;
            gotchaBT.interactable = false;
            int price = await LMS.One.SaveReward(10014);
            coinTMP.text = $"{price}";
            coinANI.SetTrigger("Start");
            LMS.One.Coin += price;
            await UniTask.Delay(1000);

            gotchaMachine.PlayAnimationLoop(GotchaMachineAnimation.Shuffle);
            AudioMGR.One.PlayEffect(capsuleCLIP);
            
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
            await UniTask.Delay(1000);
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
            await UniTask.Delay(500);
            AudioMGR.One.PlayEffect(capsuleCLIP);
            await UniTask.Delay(500);
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
            await UniTask.Delay(1000);

            var idxAni = UnityEngine.Random.Range(0, 6);
            gotchaMachine.PlayAnimation((GotchaMachineAnimation)idxAni, GotchaMachineAnimation.Idle);
            //yield return new WaitForSeconds(0.5f);
            AudioMGR.One.PlayEffect(capsulePupCLIP);
            await UniTask.Delay(800);
            AudioMGR.One.PlayEffect(capsulePupCLIP);
            await UniTask.Delay(800);
            AudioMGR.One.PlayEffect(capsulePupCLIP);
            await UniTask.Delay(1000);

            ShowCapsulesPanel();
        }
        private IEnumerator closeCapsulesPanel()
        {
            yield return new WaitForSeconds(2.0f);
            capsulesPanel.GetComponent<Animator>().SetTrigger("hide");
            capsules.ForEach(capsule => capsule.Close());
            yield return new WaitForSeconds(1.0f);
            AudioMGR.One.PlayEffect(capsuleOutCLIP);
            yield return new WaitForSeconds(1.0f);
            capsulesPanel.SetActive(false);
            gotchaBT.interactable = true;
            UIMyRoom.One.VisibleBackButton = true;
        }
        private IEnumerator affordance()
        {
            yield return new WaitForSeconds(3.0f);

            capsules.ForEach(capsule =>
            {
                if (!capsule.IsOpened)
                {
                    capsule.Affordance();
                }
            });
        }
	}
}