using beyondi.Coroutine;
using beyondi.Util;
using DoDoEng.Common;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DoDoEng.Activity.C1_A01
{
    public class Eat : MonoBehaviour, IPointerDownHandler, ICompletable
    {
        // Methods
        public void Setup(GameObject upperPB, GameObject lowerPB)
        {
            LOG.Info($"Setup()", this);

            Util.RemoveAllChildren(prefabUpperParentTR);
            {
                var go = Instantiate(upperPB, prefabUpperParentTR);
                DA = go.GetComponent<DecoAlphabet>();

                upperUserImage.SetParent(prefabUpperParentTR);
                upperUserImage.localScale = Vector2.one;
                upperUserImage.SetParent(DA.DecoAlphabetGO.transform);
                upperUserImage.localPosition = Vector2.zero;
                upperUserImage.gameObject.SetActive(true);

                DA.DecoAlphabetGO.transform.SetParent(DA.EatAlphabetGO.transform);
                DA.DecoAlphabetGO.transform.localPosition = Vector2.zero;
                DA.DecoAlphabetGO.transform.localScale = Vector2.one;
                DA.DecoAlphabetGO.transform.SetParent(alphabetArea.transform);

                DA.EatAlphabetGO.transform.SetParent(DA.DecoAlphabetGO.transform);
                DA.CrumbGO.transform.SetParent(maskCrumb.transform);

                var mask = DA.DecoAlphabetGO.AddComponent<Mask>();
                mask.showMaskGraphic = false;
                upperMaskBuilder = DA.DecoAlphabetGO.AddComponent<MaskBuilder>();
                upperMaskBuilder.Setup(MaskBuilder.Mode.Erase, brushTexture, brushScale);

                upperUserImage.SetAsLastSibling();
            }
            Util.RemoveAllChildren(prefabLowerParentTR);
            {
                var go = Instantiate(lowerPB, prefabLowerParentTR);
                DA = go.GetComponent<DecoAlphabet>();

                lowerUserImage.SetParent(prefabLowerParentTR);
                lowerUserImage.localScale = Vector2.one;
                lowerUserImage.SetParent(DA.DecoAlphabetGO.transform);
                lowerUserImage.localPosition = Vector2.zero;
                lowerUserImage.gameObject.SetActive(true);

                DA.DecoAlphabetGO.transform.SetParent(DA.EatAlphabetGO.transform);
                DA.DecoAlphabetGO.transform.localPosition = Vector2.zero;
                DA.DecoAlphabetGO.transform.localScale = Vector2.one;
                DA.DecoAlphabetGO.transform.SetParent(alphabetArea.transform);

                DA.EatAlphabetGO.transform.SetParent(DA.DecoAlphabetGO.transform);
                DA.CrumbGO.transform.SetParent(maskCrumb.transform);

                var mask = DA.DecoAlphabetGO.AddComponent<Mask>();
                mask.showMaskGraphic = false;
                lowerMaskBuilder = DA.DecoAlphabetGO.AddComponent<MaskBuilder>();
                lowerMaskBuilder.Setup(MaskBuilder.Mode.Erase, brushTexture, brushScale);

                lowerUserImage.SetAsLastSibling();
            }

            maskCrumb.Setup(MaskBuilder.Mode.Drawing, brushTexture, brushScale);
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }
        public void EatAll()
        {
            LOG.Info($"EatAll()", this);

            maskCrumb.DrawAll();
            upperMaskBuilder.DrawAll();
            lowerMaskBuilder.DrawAll();
        }

        // Events
        public event Action OnEat;



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        private DecoAlphabet DA = null;
        private MaskBuilder upperMaskBuilder = null;
        private MaskBuilder lowerMaskBuilder = null;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Transform prefabUpperParentTR = null;
        [SerializeField] private Transform prefabLowerParentTR = null;
        [SerializeField] private Transform upperUserImage = null;
        [SerializeField] private Transform lowerUserImage = null;
        [SerializeField] private MaskBuilder maskCrumb = null;
        [SerializeField] private Transform alphabetArea = null;
        [SerializeField] private Texture2D brushTexture = null;
        [Header("★ Audios")]
        [SerializeField] private AudioClip eatCLIP = null;
        [Header("★ Configs")]
        [SerializeField] private float completeRatio = 0.8f;
        [SerializeField] private float brushScale = 2f;




        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;
        }
        private void Start()
        {

        }

        // Interface : IPointerDownHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            LOG.Info($"OnPointerDown()", this);

            maskCrumb.Draw(eventData);
            upperMaskBuilder.Draw(eventData);
            lowerMaskBuilder.Draw(eventData);

            AudioMGR.One.PlayEffectLL(eatCLIP);

            OnEat?.Invoke();
        }

        // Interface : ICompletable
        bool ICompletable.IsComplete => upperMaskBuilder.Progress >= completeRatio && lowerMaskBuilder.Progress >= completeRatio;
    }
}