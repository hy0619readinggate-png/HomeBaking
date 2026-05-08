using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C1_A01
{
    public class BakeMGR : MonoBehaviour
    {
        // Methods
        public void Idle()
        {
            LOG.Info($"Idle()", this);
            oven.PlayAnimationLoop(OvenAnimation.Idle);
        }
        public Coroutine StartBake()
        {
            LOG.Info($"StartPop()", this);

            crBake = StartCoroutine(coBake());
            return crBake;
        }
        public void FinishBake()
        {
            LOG.Info($"FinishBake()", this);

            if (crBake != null)
                StopCoroutine(crBake);
            crBake = null;

            affodanceHit.SetActive(false);
            affodanceOpen.SetActive(false);
            startBTN.interactable = false;
            openBTN.interactable = false;
        }

        // Fields
        private bool isBaked = false;
        private bool isOpened = false;

        // Fields
        private Coroutine crBake = null;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Oven oven = null;
        [SerializeField] private GameObject affodanceHit = null;
        [SerializeField] private GameObject affodanceOpen = null;
        [SerializeField] private Button startBTN = null;
        [SerializeField] private Button openBTN = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip bakeCLIP = null;
        [SerializeField] private AudioClip bakedCLIP = null;
        [SerializeField] private AudioClip openCLIP = null;

        // Unity Messages
        private void Awake()
        {
            affodanceHit.SetActive(false);
            affodanceOpen.SetActive(false);
            startBTN.interactable = false;
            openBTN.interactable = false;

            startBTN.onClick.AddListener(() => isBaked = true);
            openBTN.onClick.AddListener(() => isOpened = true);
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coBake()
        {
            using (LOG.Coroutine($"coBake()", this))
            {
                // Bake
                affodanceHit.SetActive(true);
                affodanceOpen.SetActive(false);
                startBTN.interactable = true;
                yield return null;

                isBaked = false;
                yield return new WaitUntil(() => isBaked == true);
                yield return null;

                affodanceHit.SetActive(false);
                startBTN.interactable = false;
                yield return null;

                AudioMGR.One.PlayEffect(bakeCLIP);
                yield return oven.PlayAnimationAndWait(OvenAnimation.Bake, false);

                AudioMGR.One.PlayEffect(bakedCLIP);
                oven.PlayAnimationLoop(OvenAnimation.BakedAff);
                yield return null;

                // Open
                openBTN.interactable = true;
                affodanceOpen.SetActive(true);
                yield return null;

                isOpened = false;
                yield return new WaitUntil(() => isOpened == true);
                yield return null;

                affodanceOpen.SetActive(false);
                AudioMGR.One.PlayEffect(openCLIP);
                yield return null;

                openBTN.interactable = false;
                yield return null;

                yield return oven.PlayAnimationAndWait(OvenAnimation.Open, OvenAnimation.Opened);
                yield return null;
            }
        }
    }
}