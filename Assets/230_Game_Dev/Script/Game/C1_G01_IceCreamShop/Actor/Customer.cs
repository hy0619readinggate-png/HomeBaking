using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Game.C1_G01
{
    [RequireComponent(typeof(Animator))]
    public class Customer : MonoBehaviour, IDropHandler, IPointerDownHandler
    {
        // Properties
        public bool IsVisit { get; private set; }
        public bool IsWaiting { get; private set; }
        public bool IsThief => customerData.IsThief;
        public static bool IsExistAvails => characterAvailables.Any(c => c);

        // Methods
        public void Visit(CustomerData data)
        {
            LOG.Info($"Visit() | {data}", this);

            customerData = data;

            if (customerData.IsThief)
                crVisit = StartCoroutine(coThiefVisit());
            else crVisit = StartCoroutine(coCustomerVisit());
        }
        public void ResetVisit()
        {
            LOG.Info($"ResetVisit()", this);

            if (IsVisit)
            {
                IsVisit = false;
                IsWaiting = false;
                timer.StopTimer();
                if (isOrderShown)
                    anim.SetTrigger("OrderHide");
                isOrderShown = false;

                pang.gameObject.SetActive(false);
                clearCharacter();
            }
            this.StopCoroutineSafe(ref crVisit);
        }

        // Events
        public event System.Action<Customer> OnVisit;
        public event System.Action<Customer> OnSuccess;
        public event System.Action<Customer> OnFail;



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();

        // Fields : static
        private static bool[] characterAvailables = null;

        // Fields
        private int currentCharacterIdx = -1;
        private Character currentCharacter = null;
        private CustomerData customerData = null;
        private Coroutine crVisit = null;
        private bool isOrderShown = false;

        // Functions : static
        private static void initCharacterAvails(int n)
        {
            characterAvailables = Enumerable.Repeat(true, n).ToArray();
        }
        private static int getCharacterFromAvails()
        {
            LOG.Info<Customer>($"getCharacterFromAvails()");
            LOG.Info<Customer>($"characterAvailables = {string.Join(",", characterAvailables)}");

            var availIndices =
                characterAvailables
                    .Select((avail, idx) => (avail, idx))
                    .Where(it => it.avail)
                    .Select(it => it.idx).ToArray();

            var idx = UtilArray.ExtractOne(availIndices);
            characterAvailables[idx] = false;

            LOG.Info<Customer>($"characterAvailables = {string.Join(",", characterAvailables)}");

            return idx;
        }
        private static void releaseCharacterToAvails(int idx)
        {
            characterAvailables[idx] = true;

            LOG.Info<Customer>($"characterAvailables = {string.Join(",", characterAvailables)}");
        }

        // Functions
        private void setupCharacter()
        {
            characters.ForEach(ch => ch.gameObject.SetActive(false));

            currentCharacterIdx = getCharacterFromAvails();
            currentCharacter = characters[currentCharacterIdx];
        }
        private void clearCharacter()
        {
            if (currentCharacterIdx != -1)
            {
                currentCharacter.gameObject.SetActive(false);

                releaseCharacterToAvails(currentCharacterIdx);
                currentCharacterIdx = -1;
                currentCharacter = null;
            }
        }
        private bool isCorrect(IceCreamCup cup)
        {
            var orders = customerData.OrderIceCreams.Select(ic => ic.ColorID);
            var cups = cup.ColorIDs;

            var ordersStr = string.Join(",", orders);
            var cupsStr = string.Join(",", cups);

            var result = Enumerable.SequenceEqual(orders, cups);

            LOG.Important($"ORDER : [{ordersStr}] | CUP : [{cupsStr}]  ==> {result}", this);

            return result;
        }
        private int getCoinEffectIndex()
        {
            return customerData.OrderIceCreams.Length switch
            {
                1 => 0,
                2 => 1,
                3 => 1,
                4 => 2,
                _ => 2
            };
        }

        // Event Handlers
        private void timer_OnStateChanged(TimerState state)
        {
            LOG.Info($"timer_OnStateChanged() | {state}", this);

            if (!customerData.IsThief)
            {
                switch (state)
                {
                    case TimerState.Yellow: currentCharacter.PlayAnimationLoop(CharacterAnimation.Unpleasant); break;
                    case TimerState.Red: currentCharacter.PlayAnimationLoop(CharacterAnimation.Annoyed); break;

                    case TimerState.TimeOut:
                        StartCoroutine(coCustomerFail());
                        break;
                }
            }
            else
            {
                if (state == TimerState.TimeOut)
                    StartCoroutine(coThiefFail());
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Character[] characters = null;
        [SerializeField] private Pang pang = null;
        [SerializeField] private Order order = null;
        [SerializeField] private Timer timer = null;
        [SerializeField] private GameObject hammerGO = null;
        [SerializeField] private GameObject[] coinGO = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip visitCLIP = null;
        [SerializeField] private AudioClip successCLIP = null;
        [SerializeField] private AudioClip failCLIP = null;
        [SerializeField] private AudioClip thiefVisitCLIP = null;
        [SerializeField] private AudioClip thiefSuccessCLIP = null;
        [SerializeField] private AudioClip thiefFailCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float cupRespawnDelay = 0.5f;
        [SerializeField] private float sucessDuration = 1f;
        [SerializeField] private float thiefMagicDuration = 2f;
        [SerializeField] private float thiefMagicDelay = 0.5f;

        // Unity Messages
        private void Awake()
        {
            initCharacterAvails(characters.Length);

            cg.blocksRaycasts = false;

            IsVisit = false;
            IsWaiting = false;
            characters.ForEach(ch => ch.gameObject.SetActive(false));
            pang.gameObject.SetActive(false);
            hammerGO.SetActive(false);
            coinGO.SetActiveAll(false);
        }
        private void Start()
        {
        }
        private void OnEnable()
        {
            timer.OnStateChanged += timer_OnStateChanged;
        }
        private void OnDisable()
        {
            timer.OnStateChanged -= timer_OnStateChanged;
        }

        // Unity Coroutine
        IEnumerator coCustomerVisit()
        {
            using (LOG.Coroutine($"coCustomerVisit()", this))
            {
                IsVisit = true;
                IsWaiting = true;
                setupCharacter();
                yield return null;

                AudioMGR.One.PlayEffect(visitCLIP);
                currentCharacter.gameObject.SetActive(true);
                yield return currentCharacter.PlayAnimationAndWait(CharacterAnimation.Appear);
                yield return null;

                OnVisit?.Invoke(this);
                yield return null;

                timer.ResetTimer();
                order.Setup(customerData);
                isOrderShown = true;
                anim.SetTrigger("OrderShow");
                yield return new WaitForSeconds(0.5f);

                timer.StartTimer(customerData.Duration);
                yield return null;

                cg.blocksRaycasts = true;
            }
        }
        IEnumerator coCustomerSuccess()
        {
            using (LOG.Coroutine($"coCustomerSuccess()", this))
            {
                IsWaiting = false;
                cg.blocksRaycasts = false;
                timer.StopTimer();
                yield return null;

                OnSuccess?.Invoke(this);
                yield return null;

                AudioMGR.One.PlayEffect(successCLIP);
                order.ShowAnswer();

                var coinEffectIdx = getCoinEffectIndex();
                coinGO.SetActiveOnly(coinEffectIdx);

                //currentCharacter.PlayAnimation(CharacterAnimation.Idle, false);
                currentCharacter.PlayAnimation(CharacterAnimation.ReactionCorrect);
                yield return new WaitForSeconds(sucessDuration);

                isOrderShown = false;
                anim.SetTrigger("OrderHide");
                yield return currentCharacter.PlayAnimationAndWait(CharacterAnimation.Correct, false);
                yield return new WaitForSeconds(0.5f);

                clearCharacter();
                IsVisit = false;
                yield return null;
            }
        }
        IEnumerator coCustomerFail()
        {
            using (LOG.Coroutine($"coCustomerFail()", this))
            {
                IsWaiting = false;
                cg.blocksRaycasts = false;
                timer.StopTimer();
                yield return null;

                AudioMGR.One.PlayEffect(failCLIP);
                isOrderShown = false;
                anim.SetTrigger("OrderHide");
                yield return new WaitForSeconds(0.5f);

                yield return currentCharacter.PlayAnimationAndWait(CharacterAnimation.ReactionWrong, false);
                yield return currentCharacter.PlayAnimationAndWait(CharacterAnimation.Wrong, false);

                clearCharacter();
                IsVisit = false;
                yield return null;

                OnFail?.Invoke(this);
                yield return null;
            }
        }
        IEnumerator coThiefVisit()
        {
            using (LOG.Coroutine($"coThiefVisit()", this))
            {
                IsVisit = true;
                yield return null;

                AudioMGR.One.PlayEffect(thiefVisitCLIP);
                pang.gameObject.SetActive(true);
                yield return pang.PlayAnimationAndWait(PangAnimation.Appear);
                yield return null;

                timer.ResetTimer();
                order.Setup(customerData);
                isOrderShown = true;
                anim.SetTrigger("OrderShow");
                yield return new WaitForSeconds(0.5f);

                hammerGO.SetActive(true);
                timer.StartTimer(customerData.Duration);
                yield return null;

                cg.blocksRaycasts = true;
            }
        }
        IEnumerator coThiefSuccess()
        {
            using (LOG.Coroutine($"coThiefSuccess()", this))
            {
                hammerGO.SetActive(false);
                cg.blocksRaycasts = false;
                timer.StopTimer();
                yield return null;

                AudioMGR.One.PlayEffect(thiefSuccessCLIP);
                isOrderShown = false;
                anim.SetTrigger("OrderHide");
                yield return pang.PlayAnimationAndWait(PangAnimation.Hammer, false);
                yield return pang.PlayAnimationAndWait(PangAnimation.SadOut, false);

                pang.gameObject.SetActive(false);
                IsVisit = false;
                yield return null;
            }
        }
        IEnumerator coThiefFail()
        {
            using (LOG.Coroutine($"coThiefFail()", this))
            {
                hammerGO.SetActive(false);
                cg.blocksRaycasts = false;
                timer.StopTimer();
                yield return null;

                isOrderShown = false;
                anim.SetTrigger("OrderHide");
                yield return new WaitForSeconds(0.5f);

                pang.PlayAnimation(PangAnimation.Wand, false);
                yield return new WaitForSeconds(thiefMagicDelay);

                AudioMGR.One.PlayEffect(thiefFailCLIP);
                IceCreamCup.One.ExplodeAndRespawn();
                yield return new WaitForSeconds(thiefMagicDuration - thiefMagicDelay);

                yield return pang.PlayAnimationAndWait(PangAnimation.SmileOut, false);

                pang.gameObject.SetActive(false);
                IsVisit = false;
                yield return null;
            }
        }


        // Interface : IDropHandler
        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            LOG.Info($"OnDrop() | {eventData.pointerDrag.name}", this);

            if (customerData.IsThief)
                return;

            var cup = eventData.pointerDrag.GetComponent<IceCreamCup>();
            if (cup != null)
            {
                if (isCorrect(cup))
                    StartCoroutine(coCustomerSuccess());
                else StartCoroutine(coCustomerFail());

                cup.Respawn(cupRespawnDelay);

                eventData.Use();
            }
        }

        // Interface : IPointerDownHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            LOG.Info($"OnPointerDown()", this);

            if (customerData.IsThief && timer.IsRunning)
                StartCoroutine(coThiefSuccess());
        }
    }
}