using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using UnityEngine;


namespace DoDoEng.Activity.C2_A07
{
    public class CartGroup : MonoBehaviour
    {
        // Properties
        public Cart CurrentDropedCart { get; private set; }
        public Cart[] Carts => carts;
        public Transform[] EmptyCarts => carts
                                        .Where(c => c.IsEmpty)
                                        .Select(c => c.AffPos)
                                        .ToArray();

        // Methods
        public Coroutine StartWaitLuggages()
        {
            LOG.Info($"StartWaitLuggages()", this);

            waitSubmit = StartCoroutine(WaitSubmit());
            return waitSubmit;
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            foreach (Cart c in carts)
                c.EnableInteraction(enable);
        }



        // Fields
        private Coroutine waitSubmit = null;
        private bool isSubmit = false;

        // Event Handlers
        private void cart_OnSubmit(Cart cart)
        {
            LOG.Info($"cart_OnSubmit() | {cart.gameObject.name}", this);

            CurrentDropedCart = cart;
            isSubmit = true;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Cart[] carts = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
        private void OnEnable()
        {
            carts.ForEach(c => c.OnSubmit += cart_OnSubmit);
        }
        private void OnDisable()
        {
            carts.ForEach(c => c.OnSubmit -= cart_OnSubmit);
        }

        // Unity Coroutine
        IEnumerator WaitSubmit()
        {
            using (LOG.Coroutine($"WaitSubmit()", this))
            {
                isSubmit = false;
                yield return new WaitUntil(() => isSubmit);
            }
        }
    }
}