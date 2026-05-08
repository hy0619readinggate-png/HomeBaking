using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using Spine.Unity;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DoDoEng.Launcher
{
    public class LobbyMenuButton : MonoBehaviour
    {
        // Properties
        public float ClickAniTime => aniSG.SkeletonData.FindAnimation("click").Duration;

        // Methods
        public async UniTask PlayAnimation(UnityAction call)
        {
            aniSG.AnimationState.SetAnimation(0, "click", false);
            LOG.Info($"time: {aniSG.SkeletonData.FindAnimation("click").Duration}", this);
            if (particleGO != null) particleGO.SetActive(true);

            await UniTask.Delay((int)(ClickAniTime * 1000));

            call.Invoke();

            aniSG.AnimationState.SetAnimation(0, "idle", false);
            if (particleGO != null) particleGO.SetActive(false);
        }

        // Events
        public event Action<LobbyMenuButton> OnClick;



        // Fields : caching
        private Button button_;
        private Button button => button_ ??= GetComponent<Button>();

        // Functions
        // Event Handlers



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private SkeletonGraphic aniSG = null;
        [SerializeField] private GameObject particleGO = null;

        // Unity Messages
        private void Awake()
        {
            button.onClick.AddListener(() => OnClick?.Invoke(this));
        }
        private void Start()
        {
            aniSG.AnimationState.SetAnimation(0, "idle", false);
            if (particleGO != null) particleGO.SetActive(false);
        }
    }
}
