using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;
using System;
using Cysharp.Threading.Tasks;
using DoDoEng.MyRoom.Behavior;
using UnityEngine.Playables;

namespace DoDoEng.MyRoom.UI.Popup
{
	public class LevelUpPopup : MonoBehaviour
	{
        // Definitions
        // Properties

        // Methods
        public async UniTask Show(UserDataPet data)
        {
            LOG.Function(this, $"Show({data})");

            gameObject.SetActive(true);

            evaluateTimeline(showTL);

            petPrev.Init(data.IdxKind, data.Level - 1, true);
            petNext.Init(data, true);

            await playTimeline(showTL).ToUniTask();

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
        protected IEnumerator stopTimeline(PlayableDirector timeline)
        {
            timeline.time = timeline.duration;
            timeline.Evaluate();
            timeline.Stop();
            yield return null;
        }

        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private MyRoomPet petPrev = null;
        [SerializeField] private MyRoomPet petNext = null;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector showTL = null;

        // Unity Messages
        private void Awake()
		{
        }
		private void Start()
		{
		}
        protected void OnEnable()
        {
        }
        protected void OnDisable()
        {
        }

        // Unity Coroutine
	}
}