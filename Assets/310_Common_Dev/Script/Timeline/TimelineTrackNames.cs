using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

#pragma warning disable 0414

// 타임라인의 트랙이름을 인스펙터에 출력
namespace DoDoEng
{
    [ExecuteInEditMode]
    public class TimelineTrackNames : MonoBehaviour
    {
        // Methods
        public void Refresh()
        {
            refresh();

        }



        // Fields
        private PlayableDirector prevTimeline = null;

        // Functions
        private void refresh()
        {
#if UNITY_EDITOR
            if (timeline != null)
            {
                var timelineAsset = (TimelineAsset)timeline.playableAsset;
                var tracks = timelineAsset.GetOutputTracks();

                var list = new List<TimelineTrackData>();
                foreach (var track in tracks)
                {
                    list.Add(new TimelineTrackData { TrackName = track.name, bindingGameObject = getBindingGameObject(track) });
                }
                timelineTrackDatas = list.ToArray();
            }
            else timelineTrackDatas = null;
#endif
        }
        private GameObject getBindingGameObject(TrackAsset track)
        {
            var obj = timeline.GetGenericBinding(track);

            var component = obj as Component;
            if (component != null)
                return component.gameObject;

            return obj as GameObject;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField][ReadOnly] private TimelineTrackData[] timelineTrackDatas = null;
        [SerializeField] private PlayableDirector timeline = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
        private void Update()
        {
#if UNITY_EDITOR
            if (prevTimeline != timeline)
            {
                refresh();

                prevTimeline = timeline;
            }
#endif
        }
    }

    [Serializable]
    public class TimelineTrackData
    {
        public string TrackName;
        public GameObject bindingGameObject;
    }
}
