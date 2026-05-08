using beyondi.Util;
using DoDoEng.Common;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C1_A05
{
    public class Bar : MonoBehaviour
    {
        // Properties
        public Transform[] AvailableClampTRs
        {
            get
            {
                var currentClamps = pNO % 2 == 1 ? p1Clamps : p2Clamps;
                return currentClamps
                        .Where(c => !c.IsPlateLoaded)
                        .Select(b => b.transform)
                        .ToArray();
            }
        }

        // Methods
        public void Setup(int pNO, Leoni leoni)
        {
            LOG.Info($"Setup() | {pNO}", this);

            this.pNO = pNO;
            this.leoni = leoni;

            p1Clamps.SetActiveAll(pNO % 2 == 1);
            p2Clamps.SetActiveAll(pNO % 2 == 0);

            p1Clamps.ForEach(c => c.Reset());
            p2Clamps.ForEach(c => c.Reset());
        }
        public Coroutine StartWaitPlates(int pNO)
        {
            LOG.Info($"StartWaitPlates()", this);

            p1GO.SetActive(pNO % 2 == 1);
            p2GO.SetActive(pNO % 2 == 0);

            crWaitAnswer = StartCoroutine(coWaitAnswer());
            return crWaitAnswer;
        }
        public void StopWaitPlates()
        {
            LOG.Info($"StopWaitPlates()", this);

            this.StopCoroutineSafe(ref crWaitAnswer);
        }

        // Events
        public event Action OnCorrect;



        // Fields
        private Coroutine crWaitAnswer = null;
        private int pNO = 0;
        private Leoni leoni = null;

        // Event Handlers
        private void clamp_OnCorrect(BarClamp clamp)
        {
            LOG.Info($"clamp_OnCorrect() | {clamp.gameObject.name}", this);

            vfxCorrectGO.transform.position = clamp.transform.position;
            vfxCorrectGO.SetActive(true);

            var ani = UtilArray.ExtractOne(
                    new LeoniAnimation[] {
                            LeoniAnimation.Correct0_1,
                            LeoniAnimation.Correct0_2,
                            LeoniAnimation.Correct0_3 }
                    );
            leoni.PlayAnimation(ani);

            OnCorrect?.Invoke();
        }
        private void clamp_OnWrong(BarClamp clamp)
        {
            LOG.Info($"clamp_OnWrong() | {clamp.gameObject.name}", this);

            vfxWrongGO.transform.position = clamp.transform.position;
            vfxWrongGO.SetActive(true);

            var ani = UtilArray.ExtractOne(
                    new LeoniAnimation[] {
                            LeoniAnimation.Wrong1,
                            LeoniAnimation.Wrong2 }
                    );
            leoni.PlayAnimation(ani);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject p1GO = null;
        [SerializeField] private GameObject p2GO = null;
        [SerializeField] private BarClamp[] p1Clamps = null;
        [SerializeField] private BarClamp[] p2Clamps = null;
        [SerializeField] private GameObject vfxCorrectGO = null;
        [SerializeField] private GameObject vfxWrongGO = null;

        // Unity Messages
        private void Awake()
        {
            p1GO.SetActive(false);
            p2GO.SetActive(false);

            p1Clamps.SetActiveAll(true);
            p1Clamps.AutoFillID();
            p2Clamps.SetActiveAll(false);
            p2Clamps.AutoFillID();

            vfxCorrectGO.SetActive(false);
            vfxWrongGO.SetActive(false);
        }
        private void Start()
        {
        }
        private void OnEnable()
        {
            p1Clamps.ForEach(c => c.OnCorrect += clamp_OnCorrect);
            p1Clamps.ForEach(c => c.OnWrong += clamp_OnWrong);
            p2Clamps.ForEach(c => c.OnCorrect += clamp_OnCorrect);
            p2Clamps.ForEach(c => c.OnWrong += clamp_OnWrong);
        }
        private void OnDisable()
        {
            p1Clamps.ForEach(c => c.OnCorrect -= clamp_OnCorrect);
            p1Clamps.ForEach(c => c.OnWrong -= clamp_OnWrong);
            p2Clamps.ForEach(c => c.OnCorrect -= clamp_OnCorrect);
            p2Clamps.ForEach(c => c.OnWrong -= clamp_OnWrong);
        }

        // Unity Coroutine
        IEnumerator coWaitAnswer()
        {
            using (LOG.Coroutine($"coWaitAnswer()", this))
            {
                yield return new WaitForSeconds(1);

                var currentClamps = pNO % 2 == 1 ? p1Clamps : p2Clamps;
                yield return new WaitUntil(() => currentClamps.All(c => c.IsPlateLoaded));
            }
        }
    }
}