using beyondi.Util;
using DoDoEng.Common;
using DoDoEng.Game.Framework;
using DoDoEng.Game.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Game.C1_G01
{
    public class CustomerMGR : MonoBehaviour
    {
        // Properties
        public bool IsSuccessAll => hp > 0;

        // Methods
        public void Init(int initalHP)
        {
            hp = initalHP;
            UIGameCommon.One.HealthBar.Setup(initalHP);
        }
        public Coroutine StartVisit(ProblemData problem, RoundConfig config)
        {
            LOG.Info($"StartVisit()", this);

            roundConfig = config;
            currentThiefCount = 0;

            crVisitThief = StartCoroutine(coVisitThief());
            return crVisitCustomers = StartCoroutine(coVisitCustomers(problem));
        }
        public void StopVisit()
        {
            LOG.Info($"FinishVisit()", this);

            this.StopCoroutineSafe(ref crVisitThief);
            this.StopCoroutineSafe(ref crVisitCustomers);
            customers.ForEach(c => c.ResetVisit());
        }

        // Events
        public event Action OnFail;



        // Fields
        private RoundConfig roundConfig = null;
        private Coroutine crVisitCustomers = null;
        private Coroutine crVisitThief = null;
        private int currentSeq = -1;
        private int lastThiefSeq = -1;
        private int hp = 0;
        private int currentThiefCount = 0;

        // Functions
        private Customer getAvailCustomer()
        {
            var avails = customers.Where(c => !c.IsVisit).ToArray();
            if (avails.Length == 0)
                return null;

            return UtilArray.ExtractOne(avails);
        }
        private bool isAllCustomersAvail()
        {
            return customers.All(c => !c.IsVisit);
        }
        private bool isAnyCustomerWait()
        {
            return customers.Any(c => c.IsWaiting);
        }
        private bool checkThiefVisitable(int seq)
        {
            // 순서가 아니면 NO
            if (seq < roundConfig.ThiefOrder.Min) return false;
            if (seq > roundConfig.ThiefOrder.Max) return false;

            // 설정된 도둑등장 수 이상 등장했으면 NO
            if (currentThiefCount >= roundConfig.ThiefCount) return false;

            // 기다리고 있는 손님이 없으면 NO
            if (!isAnyCustomerWait()) return false;

            // 최종 도둑팡 방문이후 2명이 나오지 않았으면 NO
            if (seq - lastThiefSeq <= 2) return false;

            // 확률에 따라 생성
            return UtilRandom.RandomSuccess(thiefRatio);
        }
        private CustomerData getThiefData()
        {
            return new CustomerData
            {
                OrderIceCreams = null,
                Duration = roundConfig.ThiefDuration,
                Interval = roundConfig.CustomerInterval
            };
        }

        // Event Handlers
        private void customer_OnVisit(Customer customer)
        {
            LOG.Info($"customer_OnVisit() | {customer.gameObject.name}", this);

            if (!customer.IsThief)
                UIGameCommon.One.Progress.Increase();
        }
        private void customer_OnSuccess(Customer customer)
        {
            LOG.Info($"customer_OnSuccess() | {customer.gameObject.name}", this);

            UIGameCommon.One.StarGauge.Success();
            GameProgress.One.Correct();
        }
        private void customer_OnFail(Customer customer)
        {
            LOG.Info($"customer_OnFail() | {customer.gameObject.name}", this);

            hp--;

            UIGameCommon.One.HealthBar.UpdateHP(hp);

            if (hp <= 0)
                OnFail?.Invoke();
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Customer[] customers = null;
        [Header("★ Config")]
        [SerializeField] private float visitPreDelay = 0.5f;
        [SerializeField] private float thiefCheckInterval = 2f;
        [SerializeField] private float thiefRatio = 1f;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
            
        }
        private void OnEnable()
        {
            customers.ForEach(c => c.OnVisit += customer_OnVisit);
            customers.ForEach(c => c.OnSuccess += customer_OnSuccess);
            customers.ForEach(c => c.OnFail += customer_OnFail);
        }
        private void OnDisable()
        {
            customers.ForEach(c => c.OnVisit -= customer_OnVisit);
            customers.ForEach(c => c.OnSuccess -= customer_OnSuccess);
            customers.ForEach(c => c.OnFail -= customer_OnFail);
        }

        // Unity Coroutine
        IEnumerator coWaitIntervalOrNoCustomer(float interval)
        {
            var wait = interval;
            while (wait > 0 && customers.Any(c => c.IsVisit))
            {
                yield return null;
                wait -= Time.deltaTime;
            }
        }
        IEnumerator coVisitCustomers(ProblemData problem)
        {
            using (LOG.Coroutine($"coVisitCustomers()", this))
            {
                yield return new WaitForSeconds(visitPreDelay);

                currentSeq = 1;
                lastThiefSeq = -1;

                var q = new Queue<CustomerData>(problem.Customers);
                while (q.Count > 0)
                {
                    var customer = getAvailCustomer();
                    if (customer != null && Customer.IsExistAvails)
                    {
                        LOG.Info($"currentSeq {currentSeq}, q.Count {q.Count}", this);

                        currentSeq++;

                        var data = q.Dequeue();
                        customer.Visit(data);
                        yield return coWaitIntervalOrNoCustomer(data.Interval);
                    }
                    else yield return null;
                }

                // 모든 손님이 정리되면 종료
                yield return new WaitUntil(() => isAllCustomersAvail());
                yield return null;
            }
        }
        IEnumerator coVisitThief()
        {
            using (LOG.Coroutine($"coVisitThief()", this))
            {
                while (true)
                {
                    var customer = getAvailCustomer();
                    if (customer != null)
                    {
                        if (checkThiefVisitable(currentSeq))
                        {
                            LOG.Info($"currentSeq {currentSeq}, lastThiefSeq {lastThiefSeq}, currentThiefCount {currentThiefCount}", this);
                            LOG.Info($"IsWaiting {string.Join(", ", customers.Select(c => c.IsWaiting))}", this);
                            LOG.Info($"IsVisit {string.Join(", ", customers.Select(c => c.IsVisit))}", this);

                            lastThiefSeq = currentSeq;
                            currentThiefCount++;

                            var data = getThiefData();
                            customer.Visit(data);
                        }
                    }

                    yield return new WaitForSeconds(thiefCheckInterval);
                }
            }
        }
    }
}