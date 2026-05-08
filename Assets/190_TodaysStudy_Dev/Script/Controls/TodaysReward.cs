using beyondi.Util;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine.Playables;
using DoDoEng.Common;

namespace DoDoEng.TodaysStudy.Controls
{
	public class TodaysReward : MonoBehaviour
	{
        // Definitions
        // Properties

        // Methods
        public async UniTask PlayFirstSignInReward(int coin)
        {
            await PlayGetCoin(LocalizationMGR.One.GetText("MESSAGE_82"), coin, firstLoginOnTL, firstLoginOffTL);
        }
        public async UniTask PlayStampReward(int[] coins)
        {
            await PlayGetCoin(LocalizationMGR.One.GetText("MESSAGE_83", coins.Length), coins.Sum(), eventOnTL, eventOffTL);
        }
        public async UniTask PlayDayReward(int coin)
        {
            await PlayGetCoin("Today's Lesson Complete!", coin, dayComOnTL, dayComOffTL);
        }
        public async UniTask PlayStageReward(int coin)
        {
            await PlayGetCoin("Stage Complete!", coin, stageComOnTL, stageComOffTL);
        }
        public async UniTask PlayCourseReward(int coin)
        {
            await PlayGetCoin("Course Complete!", coin, courseComOnTL, courseComOffTL);
        }
        public async UniTask PlayGetCoin(string message, int coin, PlayableDirector onTL, PlayableDirector offTL)
        {
            gameObject.SetActive(true);
            completeTMP.text = message;
            coinTMP.text = $"+{coin}";
            await playTimeline(onTL).ToUniTask();
            await UniTask.Delay((int)(coinPreDelay * 1000));

            //coinANI.SetTrigger("Get");

            await coinInfo.StartGetCoin(coinFXStartTR, coin);

            await UniTask.Delay((int)(coinPostDelay * 1000));

            await playTimeline(offTL).ToUniTask();
            gameObject.SetActive(false);
        }
        public async UniTask PlayStageEmblem(int stage)
        {
            gameObject.SetActive(true);
            stage1TMP.text = stage.ToString();
            stage2TMP.text = stage.ToString();
            await playTimeline(stageTL);
            gameObject.SetActive(false);
        }
        public async UniTask PlayCourseEmblem(int course)
        {
            gameObject.SetActive(true);
            course1TMP.text = course.ToString();
            course2TMP.text = course.ToString();
            await playTimeline(courseTL);
            gameObject.SetActive(false);
        }

        // Events



        // Fields : caching
        // Fields
        // Functions

        // Functions : timeline
        protected void evaluateTimeline(PlayableDirector timeline, double time = 0)
        {
            timeline.time = time;
            timeline.Evaluate();
            timeline.Stop();
        }
        protected IEnumerator playTimeline(PlayableDirector timeline, float delay = 0.3f)
        {
            timeline.time = 0;
            timeline.Play();
            yield return new WaitForSeconds((float)timeline.duration);
            yield return new WaitForSeconds(delay);
        }

        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TMP_Text completeTMP = null;
        [SerializeField] private TMP_Text stage1TMP = null;
        [SerializeField] private TMP_Text stage2TMP = null;
        [SerializeField] private TMP_Text course1TMP = null;
        [SerializeField] private TMP_Text course2TMP = null;
        [SerializeField] private TMP_Text coinTMP = null;
        [SerializeField] private CoinInfo coinInfo = null;
        [SerializeField] private Transform coinFXStartTR = null;
        [Header("★ Config")]
        [SerializeField] private float coinPreDelay = 0.4f;
        [SerializeField] private float coinPostDelay = 1.0f;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector courseTL = null;
        [SerializeField] private PlayableDirector stageTL = null;
        [SerializeField] private PlayableDirector coinOnTL = null;
        [SerializeField] private PlayableDirector coinOffTL = null;
        [SerializeField] private PlayableDirector dayComOnTL = null;
        [SerializeField] private PlayableDirector dayComOffTL = null;
        [SerializeField] private PlayableDirector stageComOnTL = null;
        [SerializeField] private PlayableDirector stageComOffTL = null;
        [SerializeField] private PlayableDirector courseComOnTL = null;
        [SerializeField] private PlayableDirector courseComOffTL = null;
        [SerializeField] private PlayableDirector eventOnTL = null;
        [SerializeField] private PlayableDirector eventOffTL = null;
        [SerializeField] private PlayableDirector firstLoginOnTL = null;
        [SerializeField] private PlayableDirector firstLoginOffTL = null;

        // Unity Messages
        private void Awake()
		{
            evaluateTimeline(courseTL);
            evaluateTimeline(stageTL);
            evaluateTimeline(coinOnTL);
            evaluateTimeline(coinOffTL);
            evaluateTimeline(dayComOnTL);
            evaluateTimeline(dayComOffTL);
            evaluateTimeline(stageComOnTL);
            evaluateTimeline(stageComOffTL);
            evaluateTimeline(courseComOnTL);
            evaluateTimeline(courseComOffTL);
            evaluateTimeline(eventOnTL);
            evaluateTimeline(eventOffTL);
            evaluateTimeline(firstLoginOnTL);
            evaluateTimeline(firstLoginOffTL);
        }
		private void Start()
		{
		}

		// Unity Coroutine
	}
}