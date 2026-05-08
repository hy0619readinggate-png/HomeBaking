using beyondi.Util;
using System.Linq;
using DoDoEng.Common;
using UnityEngine;
using Coffee.UIExtensions;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using DoDoEng.MyRoom.Behavior;
using System.Threading;

namespace DoDoEng.MyRoom.UI.Popup
{
    public class EatingPopup : Graphic,
        IBeginDragHandler,
        IEndDragHandler,
        IDragHandler,
        IPointerDownHandler,
        IPointerUpHandler
    {
        // Definitions
        private int[] points = { 2, 5, 7, 10 };
        private int[] coins = { 20, 50, 70, 100 };

        // Properties

        // Methods
        public void Activate(bool active)
        {
            LOG.Info($"Activate() | {active}", this);

            gameObject.SetActive(active);
        }
        public void Show(MyRoomPet pet)
        {
            LOG.Function(this, $"{pet}");

            fieldPet = pet;

            Activate(true);

            eatingCount = 0;

            eatUP.cachedParticleSystem.Stop();
            upFX.SetActive(false);
            this.pet.Init(fieldPet.Data, true);
            if (fieldPet.IsWanting && fieldPet.CurrentWant == MyRoomPet.Wants.Eat)
                this.pet.Hungry();
            else
                this.pet.EatingIdle();
            dragFood.SetActive(false);

            if (!hasTouched)
                affordance().Forget();
        }
        public void Hide()
        {
            Activate(false);
            fieldPet.Activate(true);
            fieldPet.Drop();
        }

        // Events



        // Fields : caching
        private GraphicRaycaster graphicRaycaster_;
        private GraphicRaycaster graphicRaycaster => graphicRaycaster_ ??= GetComponent<GraphicRaycaster>();

        // Fields
        private MyRoomPet fieldPet;
        private bool nowTurningTable = false;
        private bool nowFeedback = false;
        private bool isSwipe = false;
        private Vector2 dragBeginPosition;
        private EatingPopupDish dragDish;
        private int eatingCount = 0;
        private bool hasTouched = false;
        protected CancellationTokenSource cancel = new CancellationTokenSource();

        // Functions
        private async UniTask eat(EatingPopupDish dish)
        {
            eatingCount++;

            UIMyRoom.One.VisibleBackButton = false;
            nowFeedback = true;

            int coin = await LMS.One.SaveReward(10004 + dish.Index);
            coinTMP.text = $"{coin}";
            coinANI.SetTrigger("Start");
            LMS.One.Coin += coin;

            fieldPet.CompleteWant(MyRoomPet.Wants.Eat);

            AudioMGR.One.PlayEffect(eatCLIP);
            pet.EatingFood();
            eatUP.cachedParticleSystem.Play();

            int prevLevel = fieldPet.Data.Level;

            pet.AffectionUp(dish.Point).Forget();
            //upTMP.text = $"{dish.Point} points";
            //upFX.SetActive(true);
            await UniTask.Delay((int)(pet.AnimationDuration * 1000));
            //upFX.SetActive(false);

            //fieldPet.Data.Affection += dish.Point / 100.0f;
            LMS.One.ChangeMyPet(fieldPet.Data.ID, fieldPet.Data.Name, fieldPet.Data.Affection).Forget();
            if (fieldPet.Data.Level <= 3 && prevLevel != fieldPet.Data.Level)
            {
                await UIMyRoom.One.LevelUpPU.Show(fieldPet.Data);
                pet.Init(fieldPet.Data, true);
                //UserData.One.PetBooks[fieldPet.Data.IdxKind][fieldPet.Data.Level - 1] = true;
            }
            if (eatingCount < maxCanEatingCount)
                pet.EatingIdle();
            else
                pet.Idle();

            fieldPet.Init(fieldPet.Data, true);
            if (fieldPet.Data.Level <= 3)
                UserData.One.PetBooks[fieldPet.Data.IdxKind][fieldPet.Data.Level - 1] = true;

            dragDish.ShowFood(true);
            dragDish = null;
            nowFeedback = false;
            UIMyRoom.One.VisibleBackButton = true;
        }
        private async UniTask refuse()
        {
            nowFeedback = true;
            pet.EatingRefuse();
            dragDish.ShowFood(true);
            dragDish = null;
            await UniTask.Delay(2000);
            if (eatingCount < maxCanEatingCount)
                pet.EatingIdle();
            else
                pet.Idle();
            nowFeedback = false;
        }
        private async UniTask affordance()
        {
            cancel?.Cancel();
            cancel = new();
            affordanceGO.SetActive(true);
            for (int i = 0; i < 3; i++)
            {
                float delay = 0.3f;
                await UniTask.WaitForSeconds(delay, cancellationToken: cancel.Token, cancelImmediately: true).SuppressCancellationThrow();
                if (cancel.IsCancellationRequested)
                    break;

                nowTurningTable = true;
                AudioMGR.One.PlayEffect(scrollCLIP);
                table.SwipeTable(EatingPopupTable.Swipe.Left);

                await UniTask.WaitForSeconds(1.34f - delay, cancellationToken: cancel.Token, cancelImmediately: true).SuppressCancellationThrow();
                if (cancel.IsCancellationRequested)
                    break;
            }
            affordanceGO.SetActive(false);
        }

        // Event Handlers
        private void table_onIdle()
        {
            LOG.Function(this);

            nowTurningTable = false;
        }
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private UIParticle eatUP = null;
        [SerializeField] private GameObject upFX = null;
        [SerializeField] private TMP_Text upTMP = null;
        [SerializeField] private MyRoomPet pet = null;
        [SerializeField] private EatingPopupDish[] dishes = null;
        [SerializeField] private EatingPopupDragFood dragFood = null;
        [SerializeField] private EatingPopupTable table = null;
        [SerializeField] private TMP_Text coinTMP = null;
        [SerializeField] private Animator coinANI = null;
        [SerializeField] private GameObject hitGO = null;
        [SerializeField] private GameObject affordanceGO = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip eatCLIP = null;
        [SerializeField] private AudioClip dragCLIP = null;
        [SerializeField] private AudioClip scrollCLIP = null;
        [Header("★ Config")]
        [SerializeField] private int maxCanEatingCount = 3;
        [SerializeField] private float swipeDistance = 20;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            table.Init(points, coins);
        }
        protected override void Start()
        {
            base.Start();
        }
        protected override void OnEnable()
        {
            base.OnEnable();

            table.OnIdle += table_onIdle;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            table.OnIdle -= table_onIdle;
        }



        // Implementation Interface
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            LOG.Info($"OnBeginDrag() | {eventData.position}", this);

            if (!dragFood.gameObject.activeSelf)
            {
                isSwipe = false;
                dragBeginPosition = eventData.position;
            }
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (dragFood.gameObject.activeSelf)
            {
                dragFood.MoveTo(eventData.position);
            }
            else
            {
                if (!nowTurningTable)
                {
                    var distH = eventData.position.x - dragBeginPosition.x;
                    if (!isSwipe && Mathf.Abs(distH) > swipeDistance)
                    {
                        isSwipe = true;
                        nowTurningTable = true;
                        AudioMGR.One.PlayEffect(scrollCLIP);
                        table.SwipeTable(distH < 0 ? EatingPopupTable.Swipe.Left : EatingPopupTable.Swipe.Right);
                    }
                }
            }
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            LOG.Info($"OnEndDrag() | {eventData.position}", this);

            isSwipe = false;
        }
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            LOG.Info($"OnPointerDown() | {eventData.position}", this);

            cancel?.Cancel();
            hasTouched = true;

            if (!nowTurningTable && !nowFeedback && !dragDish)
            {
                List<RaycastResult> results = new List<RaycastResult>();
                table.GetComponent<GraphicRaycaster>().Raycast(eventData, results);
                //results.ForEach(result => LOG.Info($"{result.gameObject}", this));
                if (results.Count > 0)
                {
                    foreach (var dish in dishes)
                    {
                        if (results[0].gameObject == dish.Food)
                        {
                            AudioMGR.One.PlayEffect(dragCLIP);
                            dragDish = dish;
                            dragDish.ShowFood(false);
                            dragFood.SetActive(true);
                            dragFood.Init(dragDish.Index);
                            dragFood.MoveTo(eventData.position);
                            break;
                        }
                    }
                }
            }
        }
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            LOG.Info($"OnPointerUp() | {eventData.position}", this);

            if (!nowTurningTable && !nowFeedback && dragDish && dragFood.gameObject.activeSelf)
            {
                List<RaycastResult> results = new List<RaycastResult>();
                graphicRaycaster.Raycast(eventData, results);
                results.ForEach(result => LOG.Info($"hit: {result.gameObject}", this));

                dragFood.SetActive(false);

                if (results.Count > 0 && results[0].gameObject == hitGO)
                {
                    if (dragDish.Coin <= LMS.One.Coin)
                    {
                        if (eatingCount < maxCanEatingCount)
                        {
                            eat(dragDish).Forget();
                        }
                        else
                        {
                            refuse().Forget();
                        }
                    }
                    else
                    {
                        dragDish.ShowFood(true);
                        dragDish = null;
                        SystemUI.One.ShowPopupMyPetNeedCoin();
                    }
                }
                else
                {
                    dragDish.ShowFood(true);
                    dragDish = null;
                }
            }
            //if (!isSwipe && !menu.IsShown)
            //    menu.ShowMenu();
        }
    }
}