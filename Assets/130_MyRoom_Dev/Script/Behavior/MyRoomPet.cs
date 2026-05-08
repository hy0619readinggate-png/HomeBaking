using beyondi.Util;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DoDoEng.Common;
using DoDoEng.MyRoom.UI;
using Spine.Unity;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DoDoEng.MyRoom.Behavior
{
    public class MyRoomPet : RaycastTarget
    {
        // Definitions
        public enum Wants
        {
            Eat,
            Shower,
            Healing,
            Sleep,
            Toilet,
            Play,
        }
        public enum Ranks
        {
            Common,
            Rare,
            Special
        }
        private static readonly Wants[] wantsList1 =
        {
            Wants.Eat, Wants.Sleep, Wants.Play,
        };
        private static readonly Wants[] wantsList2 =
        {
            Wants.Eat, Wants.Sleep, Wants.Toilet, Wants.Shower, Wants.Play,
        };
        private static readonly Wants[] wantsList3 =
        {
            Wants.Eat, Wants.Sleep, Wants.Toilet, Wants.Shower, Wants.Healing, Wants.Play,
        };

        // Properties
        public bool IsActiveSelf => gameObject.activeSelf;
        public UserDataPet Data => data;
        public int KindCount => pets.Length;
        public int IdxKind => idxKind;
        public Ranks Rank
        {
            get
            {
                if (idxKind < 4)
                    return Ranks.Common;
                else if (idxKind < 8)
                    return Ranks.Rare;
                else
                    return Ranks.Special;
            }
        }
        public int Level => level;
        public float PosX => rt.anchoredPosition.x;
        public Wants CurrentWant => wantsList[currentWant];
        public bool IsWanting => topIcons[(int)CurrentWant].activeSelf;
        public float AnimationDuration => currentPetSG.AnimationState.GetCurrent(0).Animation.Duration;

        // Methods
        public void Activate(bool active)
        {
            LOG.Function(this, $"{active}");

            gameObject.SetActive(active);
        }
        public void Init(UserDataPet data, bool idle = false, bool idleHalf = false)
        {
            LOG.Info($"InitFromData | data={data} | idle={idle} | idleHalf={idleHalf}", this);

            this.data = data;
            if (data != null)
            {
                Init(data.IdxKind, data.Level, idle, idleHalf);

                updateLevelSlide();
            }
        }
		public void Init(int idxKind, int level = 1, bool idle = false, bool idleHalf = false)
		{
            LOG.Function(this, $"| idxKind={idxKind} | level={level} | idle={idle} | idleHalf={idleHalf}");

            Activate(true);

            dirtyEffects.ForEach(effect => effect.SetActive(false));

            this.idxKind = idxKind;
            int levelLimited = Mathf.Min(level, 3);
            if (this.level != levelLimited)
            {
                if (levelLimited == 1) wantsList = wantsList1;
                else if (levelLimited == 2) wantsList = wantsList2;
                else wantsList = wantsList3;
                wantsList = UtilArray.Shuffled(wantsList);
                this.level = levelLimited;
            }

            anim.SetTrigger($"Level{levelLimited}");

            foreach (var (pet, i) in pets.Select((v, i) => (v, i)))
			{
                if (i == idxKind)
                    currentPetSG = pet;

				pet.gameObject.SetActive(i == idxKind);
			}
            topIconBFG.skeletonGraphic = currentPetSG;
            topIconBFG.SetBone("icon_bone");
            levelBFG.skeletonGraphic = currentPetSG;
            levelBFG.SetBone("heart_bone");
            newBFG.skeletonGraphic = currentPetSG;
            newBFG.SetBone("icon_bone");
            dirtyBFG.skeletonGraphic = currentPetSG;
            dirtyBFG.SetBone("root");
            upBFG.skeletonGraphic = currentPetSG;
            upBFG.SetBone("heart_bone");
            topIcons.ForEach(icon => icon.SetActive(false));
            playIcons.ForEach(icon => icon.SetActive(false));
            newBFG.gameObject.SetActive(false);

            currentPetSG.Skeleton.SetSkin(currentPetSG.SkeletonData.Skins.ToArray()[levelLimited]);
            //currentPetSG.AnimationState.SetEmptyAnimation(0, 0);
            if (idleHalf) IdleHalf();
            else Idle();
            if (!idle)
                currentPetSG.AnimationState.GetCurrent(0).TimeScale = 0;
            //LOG.Info($"Init | Name={currentPetSG.AnimationState.GetCurrent(0).Animation.Name}", this);
            currentPetSG.Skeleton.SetSlotsToSetupPose();
        }
        public void SetNew()
        {
            newBFG.gameObject.SetActive(true);
        }
        public void InitMovable(float minX, float maxX, float minY, float maxY)
        {
            this.minX = minX;
            this.maxX = maxX;
            this.minY = minY;
            this.maxY = maxY;

            rt.anchoredPosition = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));

            if (GetComponent<EventTrigger>() == null)
            {
                var entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.BeginDrag;
                entry.callback.AddListener((data) => trigger_onBeginDrag((PointerEventData)data));

                var entry2 = new EventTrigger.Entry();
                entry2.eventID = EventTriggerType.Drag;
                entry2.callback.AddListener((data) => trigger_onDrag((PointerEventData)data));

                var entry3 = new EventTrigger.Entry();
                entry3.eventID = EventTriggerType.EndDrag;
                entry3.callback.AddListener((data) => trigger_onEndDrag((PointerEventData)data));

                var entry4 = new EventTrigger.Entry();
                entry4.eventID = EventTriggerType.PointerClick;
                entry4.callback.AddListener((data) => trigger_onPointerClick((PointerEventData)data));

                var trigger = gameObject.AddComponent<EventTrigger>();
                trigger.triggers.Add(entry);
                trigger.triggers.Add(entry2);
                trigger.triggers.Add(entry3);
                trigger.triggers.Add(entry4);
            }
        }
		public void Move()
		{
            moveTween();

            PlayAni("walk", true);

            waitWant();
		}
        public void MoveTo(Vector2 ptScreen)
        {
            //LOG.Info($"MoveTo() | {ptScreen}", this);
            
            rt.anchoredPosition = UtilTransform.ScreenToLocal(ptScreen, rt.parent as RectTransform, GetComponentInParent<Canvas>());
        }
        public void JumpTo(float x, float y)
        {
            rt.DOKill();
            rt.anchoredPosition = new Vector2(x, y);

            if (actionCoroutine != null) StopCoroutine(actionCoroutine);
            actionCoroutine = StartCoroutine(coJump());

            rectTransform.SetAsLastSibling();
        }
        public void Drop(bool refuse = false)
        {
            actionCoroutine = StartCoroutine(coDrop(refuse));
        }
        public void Idle()
        {
            PlayAni("idle", true);
        }
        public void IdleHalf()
        {
            PlayAni("idle_half", true);
        }
        public void Hungry()
        {
            PlayAni("hungry", true);
        }
        public void Sleepy()
        {
            PlayAni("sleepy", true);
        }
        public void Dirty()
        {
            PlayAni("dirt", true);
        }
        public void PooSig()
        {
            PlayAni($"poo_sig{Random.Range(1, 3)}", true);
        }
        public void EatingIdle()
        {
            PlayAni("food_hold", true);
        }
        public void EatingFood()
        {
            PlayAni("food_eat");
        }
        public void EatingRefuse()
        {
            PlayAni("food_refuse");
        }
        public void Injury()
        {
            PlayAni("injury", true);
        }
        public void HealingFeather()
        {
            PlayAni("treatment_feather");
        }
        public void HealingMusic()
        {
            PlayAni("treatment_music");
        }
        public void HealingPortion()
        {
            PlayAni("treatment_portion");
        }
        public void Washing()
        {
            PlayAni("washing", true);
        }
        public void WashingAfter()
        {
            PlayAni("washingAfter");
        }
        public void WakeUp()
        {
            PlayAni("wakeup");
        }
        public async UniTask ToiletFX()
        {
            toiletFX.SetActive(true);
            await UniTask.Delay(1000);
            toiletFX.SetActive(false);
        }
        public async UniTask PlayFX()
        {
            playFX.SetActive(true);
            await UniTask.Delay(2000);
            playFX.SetActive(false);
        }
        public void PlayAni(string aniName, bool loop = false)
        {
            //LOG.Function(this, $"{aniName} | {loop}");
            int levelLimited = Mathf.Min(level, 3);
            currentPetSG.AnimationState.SetAnimation(0, $"{levelLimited}/{aniName}", loop);
        }
        public bool CompleteWant(Wants want)
        {
            LOG.Function(this, $"| Data={Data.ID} | CurrentWant={CurrentWant} | want={want} | currentWant={currentWant}");
            if (CurrentWant == want)
            {
                dirtyEffects.ForEach(effect => effect.SetActive(false));
                topIcons[(int)CurrentWant].SetActive(false);
                currentWant = (currentWant + 1) % wantsList.Length;
                return true;
            }
            return false;
        }
        public void NextDesire()
        {
            LOG.Function(this, $"| Data={Data.ID} | CurrentWant={CurrentWant} | icon={topIcons[(int)CurrentWant].activeSelf}| NextWant={wantsList[(currentWant + 1) % wantsList.Length]} | currentWant={currentWant}");
            if (wantCoroutine != null) StopCoroutine(wantCoroutine);
            if (topIcons[(int)CurrentWant].activeSelf)
            {
                topIcons[(int)CurrentWant].SetActive(false);
                currentWant = (currentWant + 1) % wantsList.Length;
            }
            LOG.Function(this, $"| Data={Data.ID} | CurrentWant={CurrentWant}");
            topIcons[(int)CurrentWant].SetActive(true);

            wantAni();
        }
        public async UniTask PlayUp(int point)
        {
            LOG.Function(this, $"| point={point}");

            upTMP.text = $"+ {point}";
            upFX.SetActive(true);
            levelGO.SetActive(true);

            LOG.Info($"Affection: {Data.Affection} + {point / 100.0f} = {Data.Affection + (point / 100.0f)}", this);
            int prevLevel = Data.Level;

            float rankedPoint = point * commonPointRate;
            if (Rank == Ranks.Rare)
                rankedPoint = point * rarePointRate;
            else if (Rank == Ranks.Special)
                rankedPoint = point * specialPointRate;
            float dest = rankedPoint / 100.0f;
            LOG.Info($"Affection: {Data.Affection} + {dest} = {Data.Affection + dest}", this);
            float value = dest / 10;
            for (int i = 0; i < 10; i++)
            {
                Data.Affection += value;
                updateLevelSlide();
                await UniTask.WaitForSeconds(0.1f);
            }
            LMS.One.ChangeMyPet(Data.ID, Data.Name, Data.Affection).Forget();
            await UniTask.WaitForSeconds(1.0f);
            levelGO.SetActive(false);
            upFX.SetActive(false);

            if (Data.Level <= 3 && prevLevel != Data.Level)
            {
                await UIMyRoom.One.LevelUpPU.Show(Data);
            }

            Init(Data, true);
        }
        public async UniTask AffectionUp(int point)
        {
            LOG.Function(this, $"| point={point}");

            upTMP.text = $"+ {point}";
            upFX.SetActive(true);
            levelGO.SetActive(true);

            float rankedPoint = point * commonPointRate;
            if (Rank == Ranks.Rare)
                rankedPoint = point * rarePointRate;
            else if (Rank == Ranks.Special)
                rankedPoint = point * specialPointRate;
            float dest = rankedPoint / 100.0f;
            LOG.Info($"Affection: {Data.Affection} + {dest} = {Data.Affection + dest}", this);
            float value = dest / 10;
            for (int i = 0; i < 10; i++)
            {
                Data.Affection += value;
                updateLevelSlide();
                await UniTask.WaitForSeconds(0.1f);
            }
            await UniTask.WaitForSeconds(1.0f);
            levelGO.SetActive(false);
            upFX.SetActive(false);
        }

        // Events
        public event System.Action<MyRoomPet, PointerEventData> OnBeginDrag;
        public event System.Action<MyRoomPet, PointerEventData> OnDrag;
        public event System.Action<MyRoomPet, PointerEventData> OnDrop;
        public event System.Action<MyRoomPet> OnClickIcon;
        public event System.Action<MyRoomPet, int> OnPlayIcon;



        // Fields : caching
        private RectTransform rt_;
		private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private Animator anim_;
        private Animator anim => anim_ ??= GetComponent<Animator>();

        // Fields
        private UserDataPet data;
        private int idxKind;
        private float minX;
		private float maxX;
		private float minY;
		private float maxY;
        private bool isDown = false;
        private SkeletonGraphic currentPetSG = null;
        private int level = 0;
        private Coroutine actionCoroutine = null;
        private Coroutine wantCoroutine = null;
        private int currentWant;
        private Wants[] wantsList;

        // Functions
        private void moveTween()
		{
			float destX = rt.anchoredPosition.x + Random.Range(-300.0f, 300.0f);
			destX = Mathf.Min(destX, maxX);
			destX = Mathf.Max(destX, minX);
            float destY = rt.anchoredPosition.y + Random.Range(-100.0f, 100.0f);
            destY = Mathf.Min(destY, minY);
            destY = Mathf.Max(destY, maxY);

            if (Mathf.Abs(rt.anchoredPosition.x - destX) < 50)
                Idle();
            else
                PlayAni("walk", true);

            currentPetSG.Skeleton.ScaleX = destX > rt.anchoredPosition.x ? -1 : 1;

            if (topIcons[(int)CurrentWant].activeSelf)
                rt.DOAnchorPos(new Vector2(destX, destY), Random.Range(3.0f, 10.0f)).OnComplete(() => wantAni());
            else
                rt.DOAnchorPos(new Vector2(destX, destY), Random.Range(3.0f, 10.0f)).OnComplete(() => moveTween());
        }
        private void waitWant()
        {
            if (wantCoroutine != null) StopCoroutine(wantCoroutine);
            if (!IsWanting) wantCoroutine = StartCoroutine(coWaitWant());
        }
        private void wantAni()
        {
            rt.DOKill();
            dirtyEffects.ForEach(effect => effect.SetActive(false));
            if (CurrentWant == Wants.Eat) Hungry();
            else if (CurrentWant == Wants.Sleep) Sleepy();
            else if (CurrentWant == Wants.Shower)
            {
                dirtyEffects[level - 1].SetActive(true);
                Dirty();
            }
            else if (CurrentWant == Wants.Toilet) PooSig();
            else if (CurrentWant == Wants.Healing) Injury();
            else Move();

            if (CurrentWant != Wants.Play)
            {
                rt.DOAnchorPos(rt.anchoredPosition, 5.0f).OnComplete(() => moveTween());
            }
        }
        private void updateLevelSlide()
        {
            if (data != null)
            {
                if (data.Level < 4)
                {
                    levelTMP.text = data.Level.ToString();
                    levelSlider.value = data.Affection - (float)data.Level + 1.0f;
                    maxGO.SetActive(false);
                }
                else
                {
                    levelTMP.text = "3";
                    levelSlider.value = 1.0f;
                    maxGO.SetActive(true);
                }
            }
        }

        // Event Handlers
        void trigger_onBeginDrag(PointerEventData eventData)
        {
            LOG.Info($"trigger_onBeginDrag() | {eventData.position}", this);

            if (actionCoroutine != null) StopCoroutine(actionCoroutine);

            AudioMGR.One.PlayEffect(dragCLIP);

            newBFG.gameObject.SetActive(false);
            levelGO.SetActive(false);
            upFX.SetActive(false);
            playIcons.ForEach(icon => icon.SetActive(levelGO.activeSelf));

            isDown = true;
            rt.DOKill();

            PlayAni("drag", true);

            OnBeginDrag?.Invoke(this, eventData);
        }
        void trigger_onDrag(PointerEventData eventData)
        {
            if (isDown)
            {
                //MoveTo(eventData.position);
                OnDrag?.Invoke(this, eventData);
            }
        }
        void trigger_onEndDrag(PointerEventData eventData)
        {
            LOG.Info($"trigger_onEndDrag() | {eventData.position}", this);

            AudioMGR.One.PlayEffect(dropCLIP);

            levelGO.SetActive(false);
            playIcons.ForEach(icon => icon.SetActive(levelGO.activeSelf));

            isDown = false;

            OnDrop?.Invoke(this, eventData);
        }
        void trigger_onPointerClick(PointerEventData eventData)
        {
            LOG.Info($"trigger_onPointerClick() | {eventData.position}", this);

            newBFG.gameObject.SetActive(false);
            levelGO.SetActive(!levelGO.activeSelf);

            playIcons.ForEach(icon => icon.SetActive(levelGO.activeSelf));
        }
        void iconBT_onClick()
        {
            LOG.Info($"iconBT_onClick()", this);

            newBFG.gameObject.SetActive(false);
            levelGO.SetActive(false);
            playIcons.ForEach(icon => icon.SetActive(levelGO.activeSelf));

            OnClickIcon?.Invoke(this);
        }
        void playBT_onClick(int index)
        {
            LOG.Info($"playBT_onClick({index})", this);

            newBFG.gameObject.SetActive(false);
            levelGO.SetActive(false);
            playIcons.ForEach(icon => icon.SetActive(levelGO.activeSelf));

            OnPlayIcon?.Invoke(this, index);
        }

        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private SkeletonGraphic[] pets = null;
        [SerializeField] private GameObject[] topIcons = null;
        [SerializeField] private GameObject[] playIcons = null;
        [SerializeField] private GameObject levelGO = null;
        [SerializeField] private TMP_Text levelTMP = null;
        [SerializeField] private Slider levelSlider = null;
        [SerializeField] private GameObject toiletFX = null;
        [SerializeField] private GameObject playFX = null;
        [SerializeField] private BoneFollowerGraphic topIconBFG = null;
        [SerializeField] private BoneFollowerGraphic levelBFG = null;
        [SerializeField] private BoneFollowerGraphic newBFG = null;
        [SerializeField] private BoneFollowerGraphic dirtyBFG = null;
        [SerializeField] private BoneFollowerGraphic upBFG = null;
        [SerializeField] private GameObject upFX = null;
        [SerializeField] private TMP_Text upTMP = null;
        [SerializeField] private GameObject maxGO = null;
        [SerializeField] private GameObject[] dirtyEffects = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip dragCLIP = null;
        [SerializeField] private AudioClip dropCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float minWaitSec = 60.0f;
        [SerializeField] private float maxWaitSec = 300.0f;
        [SerializeField] private float commonPointRate = 1.0f;
        [SerializeField] private float rarePointRate = 0.7f;
        [SerializeField] private float specialPointRate = 0.5f;



        // Unity Messages
        protected override void Awake()
		{
            base.Awake();

            currentWant = 0;

            levelGO.SetActive(false);
            toiletFX.SetActive(false);
            playFX.SetActive(false);
            newBFG.gameObject.SetActive(false);
            maxGO.SetActive(false);

            topIcons.ForEach(icon => icon.GetComponent<Button>().onClick.AddListener(() => iconBT_onClick()));
            playIcons.ForEach(icon => icon.GetComponent<Button>().onClick.AddListener(() => playBT_onClick(5)));
        }
        protected override void Start()
		{
            base.Start();
        }

        // Unity Coroutine
        IEnumerator coWaitWant()
        {
            yield return new WaitForSeconds(Random.Range(minWaitSec, maxWaitSec));
            topIcons[(int)CurrentWant].SetActive(true);

            wantAni();
        }
        IEnumerator coDrop(bool refuse = false)
        {
            float destX = rt.anchoredPosition.x;
            destX = Mathf.Min(destX, maxX);
            destX = Mathf.Max(destX, minX);
            float destY = rt.anchoredPosition.y;
            destY = Mathf.Min(destY, minY);
            destY = Mathf.Max(destY, maxY);

            rt.anchoredPosition = new Vector2(destX, destY);


            if (refuse)
            {
                PlayAni("food_refuse");
                yield return new WaitForSeconds(1.0f);
            }
            else
            {
                PlayAni("drop");
                yield return new WaitForSeconds(0.9f);
            }

            if (topIcons[(int)CurrentWant].activeSelf)
            {
                wantAni();
            }
            else
            {
                Move();
            }
        }
        IEnumerator coJump()
        {
            float destX = rt.anchoredPosition.x;
            destX = Mathf.Min(destX, maxX);
            destX = Mathf.Max(destX, minX);
            float destY = rt.anchoredPosition.y;
            destY = Mathf.Min(destY, minY);
            destY = Mathf.Max(destY, maxY);

            rt.anchoredPosition = new Vector2(destX, destY);

            PlayAni("drop");

            yield return new WaitForSeconds(0.9f);

            if (topIcons[(int)CurrentWant].activeSelf)
                wantAni();
            else
            {
                Idle();

                yield return new WaitForSeconds(3.0f);

                Move();
            }
        }
    }
}