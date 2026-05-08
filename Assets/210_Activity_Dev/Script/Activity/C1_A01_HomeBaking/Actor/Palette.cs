using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C1_A01
{
    public class Palette : MonoBehaviour
    {
        // Methods
        public void SelectDefault()
        {
            LOG.Info($"SelectDefault()", this);

            items[0].Selected = true;
            scrollRect.verticalNormalizedPosition = 1f;

            changePaletteItem(items[0]);
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }



        // Fields : caching
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();
        private PaletteItem[] items_ = null;
        private PaletteItem[] items => items_ ??= GetComponentsInChildren<PaletteItem>(true);
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Functions
        private void changePaletteItem(PaletteItem item)
        {
            deco.ChangeTool(item.ToolType, item.BrushInfo, item.ID);
        }

        // Event Handlers
        private void item_OnSelected(PaletteItem item)
        {
            LOG.Info($"item_OnSelected() | {item}", this);

            AudioMGR.One.PlayEffectLL(changeToolCLIP);

            changePaletteItem(item);
        }
        private void OnScrollUp_Click()
        {
            LOG.Info($"OnScrollUp_Click()", this);

            var destPosY = scrollRect.content.anchoredPosition.y - scrollAmount;
            scrollRect.content.DOAnchorPosY(destPosY, scrollDuration);
        }
        private void OnScrollDown_Click()
        {
            LOG.Info($"OnScrollDown_Click()", this);

            var destPosY = scrollRect.content.anchoredPosition.y + scrollAmount;
            scrollRect.content.DOAnchorPosY(destPosY, scrollDuration);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Deco deco = null;
        [SerializeField] private Button scrollUp = null;
        [SerializeField] private Button scrollDown = null;
        [SerializeField] private ScrollRect scrollRect = null;
        [Header("★ Audios")]
        [SerializeField] private AudioClip changeToolCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float scrollAmount = 160f;
        [SerializeField] private float scrollDuration = 0.2f;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;

            items.AutoFillID();
            changePaletteItem(items[0]);

            scrollUp.onClick.AddListener(OnScrollUp_Click);
            scrollDown.onClick.AddListener(OnScrollDown_Click);
        }
        private void Start()
        {
        }
        private void OnEnable()
        {
            items.ForEach(i => i.OnSelected += item_OnSelected);
        }
        private void OnDisable()
        {
            items.ForEach(i => i.OnSelected -= item_OnSelected);
        }

    }
}