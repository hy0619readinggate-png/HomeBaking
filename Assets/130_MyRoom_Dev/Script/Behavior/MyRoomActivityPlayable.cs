using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using Spine.Unity;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;

// devBOX - 삭제해야 함
#pragma warning disable 0414

namespace DoDoEng.MyRoom.Behavior
{
    public class MyRoomActivityPlayable : MonoBehaviour
    {
        // Definitions
        // Properties
        public bool IsPlaying {
            get
            {
                if (playPet) return playPet.IsActiveSelf;
                return false;
            }
        }
        public int MinLevel => minLevel;

        // Methods
        public void ShowFX(bool value)
        {
            if (activityFX != null) activityFX.SetActive(value);
        }
        public async UniTask Play(MyRoomPet dragPet)
		{
			LOG.Function(this, $"{dragPet}");

            dragPet.Activate(false);

            if (playFX != null ) playFX.SetActive(true);

            if (playPet != null)
            {
                playPet.Activate(true);

			    playPet.Init(dragPet.IdxKind, dragPet.Level, true);

			    playPet.PlayAni(petAniName, true);
			    if (sg) sg.AnimationState.SetAnimation(0, sg.startingAnimation, true);

                if (petAniName == "pooing")
                {
                    await UniTask.Delay(time - 1400);
                    animator.SetTrigger("poop");
                    await UniTask.Delay(400);
                    playPet.ToiletFX().Forget();
                    await UniTask.Delay(1000);
                }
                else if (petAniName == "sleep")
                {
                    await UniTask.Delay(time - 1000);
                    playFX.SetActive(false);
                    playPet.WakeUp();
                    await UniTask.Delay((int)(playPet.AnimationDuration * 1000));
                }
                else if (petAniName == "ride_slide" || petAniName == "ride_viking" || petAniName == "ride_seesaw")
                {
                    await UniTask.Delay(time - 2000);
                    playPet.PlayFX().Forget();
                    await UniTask.Delay(2000);
                }
                else
                    await UniTask.Delay(time);

                if (sg) sg.AnimationState.SetAnimation(0, sg.startingAnimation, true);
                if (sg) sg.AnimationState.GetCurrent(0).TimeScale = 0;

                playPet.Activate(false);
                dragPet.Activate(true);
			    dragPet.Drop();

                if (dragPet.CompleteWant(want))
                {
                    dragPet.PlayUp(5).Forget();
                    //dragPet.PlayUp(100).Forget();
                }
            }
            else
                playCallback?.Invoke(dragPet);

            if (playFX != null) playFX.SetActive(false);
        }



        // Fields : caching
        private Animator animator_;
        private Animator animator => animator_ ??= GetComponent<Animator>();

        // Fields
        // Functions
        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject activityFX = null;
        [SerializeField] private UnityEvent<MyRoomPet> playCallback = null;
        [SerializeField] private SkeletonGraphic sg = null;
		[SerializeField] private MyRoomPet playPet = null;
        [SerializeField] private MyRoomPet.Wants want = MyRoomPet.Wants.Eat;
        [SerializeField] private GameObject playFX = null;
        [Header("★ Config")]
        [SerializeField] private int minLevel = 1;
        [SerializeField] private int time = 3000;
        [SerializeField] private string petAniName = "idle";

        // Unity Messages
        private void Awake()
		{
            if (activityFX != null) activityFX.SetActive(false);
            if (playPet != null) playPet.Activate(false);
			if (sg) sg.AnimationState.GetCurrent(0).TimeScale = 0;
            if (playFX != null) playFX.SetActive(false);
        }
		private void Start()
		{
		   
		}

		// Unity Coroutine
	}
}