using beyondi.Util;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DoDoEng.Common;
using DoDoEng.MyRoom.Popup;
using DoDoEng.MyRoom.UI;
using SRDebugger;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DoDoEng.MyRoom.Behavior;

// devBOX - 삭제해야 함
#pragma warning disable 0414

namespace DoDoEng.MyRoom
{
    public class MyRoom : MonoBehaviour
    {
        // Definitions
        public enum DebugStep
        {
            Start,
            Gotcha,
            Capsules,
            Eating,
            Shower,
        }

        // Properties

        // Methods
        public async UniTask Prepare()
        {
            LOG.Info($"Prepare()", this);

            await onPrepare();
        }
        public void StartMyRoom()
        {
            LOG.Info($"StartMyRoom()", this);

#if !DISABLE_SRDEBUGGER
            srOptionContainer = new DynamicOptionContainer();
            onBuildOption(srOptionContainer);
            SRDebug.Instance.AddOptionContainer(srOptionContainer);
#endif

            DOVirtual.DelayedCall(startDelay, () => { onStartGame(); });
        }
        public void FinishMyRoom()
        {
            LOG.Info($"FinishMyRoom()", this);

#if !DISABLE_SRDEBUGGER
            if (SRDebug.Instance != null && srOptionContainer != null)
            {
                SRDebug.Instance.RemoveOptionContainer(srOptionContainer);
                srOptionContainer = null;
            }
#endif

            onFinishGame();
        }
        public bool Back()
        {
            LOG.Function(this);
            if (gotchaPopup.isActiveAndEnabled)
            {
                LOG.Info($"Gotcha Popup activated.", this);
                gotchaPopup.Activate(false);
                return false;
            }
            else if (UIMyRoom.One.EatingPU.isActiveAndEnabled)
            {
                LOG.Info($"Eating Popup activated.", this);
                UIMyRoom.One.EatingPU.Hide();
                return false;
            }
            else if (UIMyRoom.One.HealingPU.isActiveAndEnabled)
            {
                LOG.Info($"Healing Popup activated.", this);
                UIMyRoom.One.HealingPU.Hide();
                return false;
            }
            else if (UIMyRoom.One.ShowerPU.isActiveAndEnabled)
            {
                LOG.Info($"Shower Popup activated.", this);
                UIMyRoom.One.ShowerPU.Hide();
                return false;
            }

            return true;
        }
        public void Eating(MyRoomPet pet)
        {
            LOG.Function(this);
            UIMyRoom.One.EatingPU.Show(pet);
        }
        public void Healing(MyRoomPet pet)
        {
            LOG.Function(this);
            UIMyRoom.One.HealingPU.Show(pet);
        }
        public void Shower(MyRoomPet pet)
        {
            LOG.Function(this);
            UIMyRoom.One.ShowerPU.Show(pet);
        }



        // Virtual
        protected virtual void onInitMyRoom()
        {
            UserData.One.LoadLanguage();

            gotchaPopup.Activate(false);

            gotchaBT.onClick.AddListener(() => gotchaBT_onClick());
            collectionBT.onClick.AddListener(() => collectionBT_onClick());
            creatureBookBT.onClick.AddListener(() => creatureBookBT_onClick());
            tripBT.onClick.AddListener(() => tripBT_onClick());
        }
        protected virtual async UniTask onPrepare()
        {
            //UIPlaygroundCommon.One.VisibleBackButton = true;
            await LMS.One.LoadMyPet();
            await LMS.One.LoadPetBook();

            gotchaGuideANI.SetBool("affordance", true);
            foreach (var (data, i) in UserData.One.Pets.Select((v, i) => (v, i)))
            {
                addPet(i);
            }

            if (pets.Count > 0) {
                var desirePetsCount = Random.Range(1, pets.Count + 1);
                var randIndice = UtilArray.Random(0, pets.Count - 1, desirePetsCount);
                for (int i = 0; i < randIndice.Length; i++)
                {
                    var pet = pets[randIndice[i]];
                    pet.NextDesire();
                }
            }
            else

            await UniTask.Delay(0);

            scrollRect.horizontalScrollbar.value = scrollPos;
        }
        protected virtual void onStartGame()
        {
            if (debugStep == DebugStep.Gotcha)
            {
                gotchaPopup.Activate(true);
            }
            else if (debugStep == DebugStep.Capsules)
            {
                gotchaPopup.Activate(true);
                gotchaPopup.ShowCapsulesPanel();
            }
            else if (debugStep == DebugStep.Eating)
            {
                Eating(pets[0]);
            }
            else if (debugStep == DebugStep.Shower)
            {
                Shower(pets[0]);
            }
        }
        protected virtual void onFinishGame() { }
        protected virtual void onDebugNext() { }
#if !DISABLE_SRDEBUGGER
        protected virtual void onBuildOption(DynamicOptionContainer srOptionContainer)
        {
            var sort = 400;

            srOptionContainer.AddOption(
                OptionDefinition.Create(
                    "Pet Chances", 
                    () => gotchaPopup.PetChances, 
                    value => gotchaPopup.PetChances = value, 
                    "MyRoom", ++sort));
            srOptionContainer.AddOption(
                OptionDefinition.Create(
                    "Candy Chances",
                    () => gotchaPopup.CandyChances,
                    value => gotchaPopup.CandyChances = value,
                    "MyRoom", ++sort));
            srOptionContainer.AddOption(
                OptionDefinition.FromMethod("Add 100 Coins", async () =>
                {
                    var point = await LMS.One.SaveReward(90001);
                    LMS.One.Coin += point;
                }, "MyRoom", ++sort)
            );
            srOptionContainer.AddOption(
                OptionDefinition.FromMethod("Next Pet's Desire", () =>
                {
                    pets.ForEach(pet => pet.NextDesire());
                }, "MyRoom", ++sort)
            );
        }
#endif



        // Fields : caching

        // Fields
#if !DISABLE_SRDEBUGGER
        private DynamicOptionContainer srOptionContainer;
#endif
        private List<MyRoomPet> pets = new List<MyRoomPet>();
        private MyRoomActivityPlayable prevPlayable;
        private MyRoomPet dragPet;

        // Functions
        private async UniTask getPet(int idxKind)
        {
            int id = UserData.One.Pets.Count;
            var pet = new UserDataPet(id, idxKind, 0, true);
            pet.ID = await LMS.One.AddMyPet((PetCode)idxKind, pet.Name);
            UserData.One.Pets.Add(pet);

            var newPet = addPet(UserData.One.Pets.Count - 1);
            Vector3 screenCenterViewport = new Vector3(0.5f, 0.5f, Camera.main.nearClipPlane);
            Vector3 screenCenterWorld = Camera.main.ViewportToWorldPoint(screenCenterViewport);
            newPet.transform.position = screenCenterWorld;
            newPet.JumpTo(newPet.rectTransform.anchoredPosition.x, minPetAreaY);
            newPet.SetNew();
        }
        private MyRoomPet addPet(int idxPet)
        {
            gotchaGuideANI.SetBool("affordance", false);

            var pet = Instantiate(petPF, charactersRT);

            var data = UserData.One.Pets[idxPet];
            pet.Init(data);
            pet.InitMovable(minPetAreaX, maxPetAreaX, minPetAreaY, maxPetAreaY);
            pet.Move();
            pets.Add(pet);

            int levelLimited = Mathf.Min(data.Level, 3);
            for (int i = 0; i < levelLimited; i++)
            {// 임시로 현재 펫과 해당 펫의 낮은 레벨의 펫도 모은 것으로 적용
                UserData.One.PetBooks[data.IdxKind][i] = true;
            }

            pet.OnBeginDrag += pet_onBeginDrag;
            pet.OnDrag += pet_onDrag;
            pet.OnDrop += pet_onDrop;
            pet.OnClickIcon += pet_onClickIcon;
            pet.OnPlayIcon += pet_onPlayIcon;

            return pet;
        }
        private async UniTask goTripPets()
        {
            int point = 0;
            foreach (var pet in tripPets)
            {
                if (pet.IsActiveSelf)
                {
                    removePet(pet.Data);
                    UserData.One.Pets.Remove(pet.Data);
                    LMS.One.RemoveMyPet(pet.Data.ID).Forget();
                    if (pet.Data.Level == 1)
                        point += await LMS.One.SaveReward(10001);
                    else if (pet.Data.Level == 2)
                        point += await LMS.One.SaveReward(10002);
                    else
                        point += await LMS.One.SaveReward(10003);
                }
            }

            LMS.One.Coin += point;

            StartCoroutine(goTrip());
        }
        private void removePet(UserDataPet data)
        {
            var pet = pets.Find(pet => pet.Data == data);

            pet.OnBeginDrag -= pet_onBeginDrag;
            pet.OnDrag -= pet_onDrag;
            pet.OnDrop -= pet_onDrop;
            pet.OnClickIcon -= pet_onClickIcon;
            pet.OnPlayIcon -= pet_onPlayIcon;

            pets.Remove(pet);
            Destroy(pet.gameObject);

            gotchaGuideANI.SetBool("affordance", pets.Count == 0);
        }
        private void showTrip(int idxPet = -1)
        {
            AudioMGR.One.PlayEffect(SfxMoment.Common_Click);
            UIMyRoom.One.TripPU.Show(idxPet);
        }

        // Event Handlers
        private void gotchaBT_onClick()
        {
            AudioMGR.One.PlayEffect(SfxMoment.Common_Click);
            gotchaPopup.Show();
        }
        private void collectionBT_onClick()
        {
            AudioMGR.One.PlayEffect(SfxMoment.Common_Click);
            UIMyRoom.One.CollectionPU.Show();
        }
        private void creatureBookBT_onClick()
        {
            AudioMGR.One.PlayEffect(SfxMoment.Common_Click);
            UIMyRoom.One.CreatureBookPU.Show();
        }
        private void tripBT_onClick()
        {
            showTrip();
        }
        private void pet_onBeginDrag(MyRoomPet pet, PointerEventData eventData)
        {
            LOG.Function(this);

            dragPet = pet;
        }
        private void pet_onDrag(MyRoomPet pet, PointerEventData eventData)
        {
            List<RaycastResult> results = new List<RaycastResult>();
            graphicRaycaster.Raycast(eventData, results);

            MyRoomActivityPlayable hitPlayable = null;
            foreach (var result in results)
            {
                hitPlayable = result.gameObject.GetComponent<MyRoomActivityPlayable>();
                if (hitPlayable != null)
                {
                    break;
                }
            }
            if (hitPlayable != prevPlayable)
            {
                if (prevPlayable) prevPlayable.ShowFX(false);
                if (hitPlayable && !hitPlayable.IsPlaying && hitPlayable.MinLevel <= pet.Level) hitPlayable.ShowFX(true);
                prevPlayable = hitPlayable;
            }
        }
        private void pet_onDrop(MyRoomPet pet, PointerEventData eventData)
        {
            LOG.Info($"pet_onDrop() | {pet}, {eventData.position}", this);
            List<RaycastResult> results = new List<RaycastResult>();
            graphicRaycaster.Raycast(eventData, results);

            //results.ForEach(result => LOG.Info($"{result.gameObject}", this));

            MyRoomActivityPlayable hitPlayable = null;
            bool droped = false;
            foreach (var result in results)
            {
                if (tripBoatGO == result.gameObject)
                {
                    pet.Drop();
                    showTrip(pets.IndexOf(pet));
                    droped = true;
                    break;
                }

                hitPlayable = result.gameObject.GetComponent<MyRoomActivityPlayable>();
                if (hitPlayable != null)
                {
                    break;
                }
            }
            if (hitPlayable != null && !hitPlayable.IsPlaying)
            {
                if (hitPlayable.MinLevel <= pet.Level)
                //if (true)
                {
                    hitPlayable.ShowFX(false);
                    hitPlayable.Play(pet).Forget();
                }
                else
                {
                    pet.Drop(true);
                }
            }
            else if (!droped)
                pet.Drop();

            dragPet = null;
        }
        private void pet_onClickIcon(MyRoomPet pet)
        {
            LOG.Info($"pet_onClickIcon() | {pet}, {pet.CurrentWant}", this);

            AudioMGR.One.PlayEffect(SfxMoment.Common_Click);

            var activity = activities[(int)pet.CurrentWant];
            float mapSize = mapRT.sizeDelta.x;
            float screenSize = scrollRect.GetComponent<RectTransform>().sizeDelta.x;
            float activityX = activity.anchoredPosition.x;
            float dest = (activityX + (mapSize / 2.0f) - (screenSize / 2.0f)) / (mapSize - screenSize);
            DOTween.To(() => scrollRect.horizontalScrollbar.value, v => scrollRect.horizontalScrollbar.value = v, dest, 0.5f);
            pet.JumpTo(activityX, minPetAreaY);
        }
        private void pet_onPlayIcon(MyRoomPet pet, int index)
        {
            LOG.Info($"pet_onPlayIcon() | {pet}, {index}", this);

            AudioMGR.One.PlayEffect(SfxMoment.Common_Click);

            var activity = activities[index];
            float mapSize = mapRT.sizeDelta.x;
            float screenSize = scrollRect.GetComponent<RectTransform>().sizeDelta.x;
            float activityX = activity.anchoredPosition.x;
            float dest = (activityX + (mapSize / 2.0f) - (screenSize / 2.0f)) / (mapSize - screenSize);
            DOTween.To(() => scrollRect.horizontalScrollbar.value, v => scrollRect.horizontalScrollbar.value = v, dest, 0.5f);
            pet.JumpTo(activityX, minPetAreaY);
        }
        private void gotchaPopup_onGetPet(int idxKind)
        {
            getPet(idxKind).Forget();
        }
        private void collectionPopup_onChangeName(int index)
        {
            LOG.Info($"collectionPopup_onChangeName() | {index}", this);
        }
        private void collectionPopup_onGoToPet(int index)
        {
            LOG.Info($"collectionPopup_onGoToPet() | {index}", this);

            if (index < pets.Count)
            {
                var pet = pets[index];
                float mapSize = mapRT.sizeDelta.x;
                float screenSize = scrollRect.GetComponent<RectTransform>().sizeDelta.x;
                float dest = (pet.PosX + (mapSize / 2.0f) - (screenSize / 2.0f)) / (mapSize - screenSize);
                DOTween.To(() => scrollRect.horizontalScrollbar.value, v => scrollRect.horizontalScrollbar.value = v, dest, 0.5f);
            }
        }
        private void collectionPopup_onTrip(int index)
        {
            LOG.Info($"collectionPopup_onTrip() | {index}", this);

            if (index < pets.Count)
            {
                //var pet = pets[index];
                DOTween.To(() => scrollRect.horizontalScrollbar.value, v => scrollRect.horizontalScrollbar.value = v, 1.0f, 0.5f);
                UIMyRoom.One.TripPU.Show(index);
            }
        }
        private void tripPopup_onChecked(CollectionSlot slot)
        {
            LOG.Function(this, $"{slot}");

            if (slot.Checked)
            {
                foreach (var pet in tripPets)
                {
                    if (!pet.IsActiveSelf)
                    {
                        pet.Activate(true);
                        pet.Init(slot.Pet.Data, true);
                        break;
                    }
                }
            }
            else
            {
                var selectedPet = tripPets.ToList().Find(pet => pet.Data == slot.Pet.Data);
                if (selectedPet != null)
                    selectedPet.Activate(false);
            }

            var fieldPet = pets.Find(pet => pet.Data == slot.Pet.Data);
            fieldPet.Activate(!slot.Checked);
        }
        private void tripPopup_onClose()
        {
            LOG.Info($"tripPopup_onClose()", this);

            tripPets.ForEach(tripPet =>
            {
                if (tripPet.IsActiveSelf)
                {
                    var fieldPet = pets.Find(pet => pet.Data == tripPet.Data);
                    fieldPet.Activate(true);
                    tripPet.Activate(false);
                }
            });
        }
        private void tripPopup_onTrip()
        {
            LOG.Info($"tripPopup_onTrip()", this);

            goTripPets().Forget();
        }

        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GotchaPopup gotchaPopup = null;
        [SerializeField] private Button gotchaBT = null;
        [SerializeField] private Button collectionBT = null;
        [SerializeField] private Button creatureBookBT = null;
        [SerializeField] private Button tripBT = null;
        [SerializeField] private RectTransform[] activities = null;
        [SerializeField] private MyRoomActivityPlayable[] playGO = null;
        [SerializeField] private RectTransform charactersRT = null;
        [SerializeField] private MyRoomPet petPF = null;
        [SerializeField] private GraphicRaycaster graphicRaycaster = null;
        [SerializeField] private ScrollRect scrollRect = null;
        [SerializeField] private RectTransform mapRT = null;
        [SerializeField] private MyRoomPet[] tripPets = null;
        [SerializeField] private Animator tripANI = null;
        [SerializeField] private GameObject tripBoatGO = null;
        [SerializeField] private Animator gotchaGuideANI = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip bgmCLIP = null;
        [SerializeField] private AudioClip ambCLIP = null;
        [Header("★ Config")]
        [SerializeField] private DebugStep debugStep = DebugStep.Start;
        [SerializeField] private float startDelay = 0.5f;
        [SerializeField] private float scrollPos = 0.233f;
        [SerializeField] private float minPetAreaX = -2000.0f;
        [SerializeField] private float maxPetAreaX = 3000.0f;
        [SerializeField] private float minPetAreaY = -200.0f;
        [SerializeField] private float maxPetAreaY = -450.0f;
        [SerializeField] private float autoDragSpeed = 0.2f;

        // Unity Messages
        private void Awake()
        {
            onInitMyRoom();

            petPF.gameObject.SetActive(false);

            tripPets.ForEach(pet => pet.gameObject.SetActive(false));
        }
        private void Start()
        {
            AudioMGR.One.PlayBGM(bgmCLIP);
            AudioMGR.One.PlayAmbient(ambCLIP);
        }
        private void OnEnable()
        {
            gotchaPopup.OnGetPet += gotchaPopup_onGetPet;

            UIMyRoom.One.CollectionPU.OnGoToPet += collectionPopup_onGoToPet;
            UIMyRoom.One.CollectionPU.OnTrip += collectionPopup_onTrip;
            UIMyRoom.One.CollectionPU.OnChangeName += collectionPopup_onChangeName;

            UIMyRoom.One.TripPU.OnChecked += tripPopup_onChecked;
            UIMyRoom.One.TripPU.OnClose += tripPopup_onClose;
            UIMyRoom.One.TripPU.OnTrip += tripPopup_onTrip;
        }
        private void OnDisable()
        {
            gotchaPopup.OnGetPet -= gotchaPopup_onGetPet;

            if (UIMyRoom.One)
            {
                UIMyRoom.One.CollectionPU.OnGoToPet -= collectionPopup_onGoToPet;
                UIMyRoom.One.CollectionPU.OnTrip -= collectionPopup_onTrip;
                UIMyRoom.One.CollectionPU.OnChangeName -= collectionPopup_onChangeName;

                UIMyRoom.One.TripPU.OnChecked -= tripPopup_onChecked;
                UIMyRoom.One.TripPU.OnClose -= tripPopup_onClose;
                UIMyRoom.One.TripPU.OnTrip -= tripPopup_onTrip;
            }
        }
        private void Update()
        {
            if (dragPet != null)
            {
                //LOG.Info($"{Input.mousePosition.x}, {Screen.width}", this);
                float range = Screen.width * 0.07f;
                if (Input.mousePosition.x < range)
                {
                    float dest = scrollRect.horizontalScrollbar.value - autoDragSpeed * Time.deltaTime;
                    dest = Mathf.Max(0, dest);
                    scrollRect.horizontalScrollbar.value = dest;
                }
                else if (Input.mousePosition.x > Screen.width - range)
                {
                    float dest = scrollRect.horizontalScrollbar.value + autoDragSpeed * Time.deltaTime;
                    dest = Mathf.Min(1, dest);
                    scrollRect.horizontalScrollbar.value = dest;
                }
                dragPet.MoveTo(Input.mousePosition);
            }

            for (int i = 1; i < charactersRT.childCount; i++)
            {
                var pet = charactersRT.GetChild(i);
                for (int j = 0; j < i; j++)
                {
                    if (pet.localPosition.y > charactersRT.GetChild(j).localPosition.y)
                    {
                        pet.SetSiblingIndex(j);
                        break;
                    }
                }
            }
            if (dragPet)
                dragPet.rectTransform.SetAsLastSibling();
        }

        // Unity Coroutine
        private IEnumerator goTrip()
        {
            tripANI.SetTrigger("out");
            yield return new WaitForSeconds(2.0f);
            tripPets.ForEach(pet => pet.gameObject.SetActive(false));
            tripANI.SetTrigger("in");
            //yield return new WaitForSeconds(2.0f);
        }
    }
}