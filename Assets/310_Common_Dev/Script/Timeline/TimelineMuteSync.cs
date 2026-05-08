using beyondi.Util;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace DoDoEng.Common
{
    public class TimelineMuteSync : MonoBehaviour
    {
        // Fields : caching
        private PlayableDirector timeline_ = null;
        private PlayableDirector timeline => timeline_ ??= GetComponent<PlayableDirector>();

        // Fields
        private Dictionary<TimelineMuteSync_Source, TimelineMuteSync_Track> mapSourceVsTrack = new Dictionary<TimelineMuteSync_Source, TimelineMuteSync_Track>();
        private bool isGraphModified = false;
        private bool isApplicationQuitting = false;

        // Functions
        private GameObject getBindingGameObject(TrackAsset track)
        {
            var obj = timeline.GetGenericBinding(track);

            var component = obj as Component;
            if (component != null)
                return component.gameObject;

            return obj as GameObject;
        }
        private void buildBindingMap()
        {
            var timelineAsset = (TimelineAsset)timeline.playableAsset;
            var tracks = timelineAsset.GetOutputTracks();

            foreach (var syncTrackName in syncTrackNames)
            {
                var track = tracks.SingleOrDefault(t => t.name == syncTrackName);
                var go = getBindingGameObject(track);

                if (track != null && go != null)
                {

                    var source = go.AddComponent<TimelineMuteSync_Source>();
                    source.OnMuteChanged += source_OnMuteChanged;

                    var originTrackMuted = track.muted;
                    track.muted = source.Mute;

                    mapSourceVsTrack[source] = new TimelineMuteSync_Track { Track = track, OriginMuted = originTrackMuted };
                }
            }
        }
        private void rebuildGraph()
        {
            double t = timeline.time;
            timeline.RebuildGraph();
            timeline.time = t;
        }

        // Event Handlers
        private void source_OnMuteChanged(TimelineMuteSync_Source source)
        {
            LOG.Info($"source_OnMuteChanged() | {source.name} {source.Mute}", this);

            if (isApplicationQuitting)
                return;

            if (!mapSourceVsTrack.ContainsKey(source))
                LOG.Error($"No TrackAsset Binding {source.gameObject.name}", this);

            var trackData = mapSourceVsTrack[source];
            if (trackData.Track.muted != source.Mute)
            {
                trackData.Track.muted = source.Mute;
                isGraphModified = true;
            }
        }



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private string[] syncTrackNames = null;

        // Unity Messages
        private void Awake()
        {
            isGraphModified = false;
            buildBindingMap();
        }
        private void Start()
        {
        }
        private void LateUpdate()
        {
            if (isGraphModified)
            {
                rebuildGraph();
                isGraphModified = false;
            }
        }
        private void OnDestroy()
        {
            mapSourceVsTrack.Keys.ForEach(s => s.OnMuteChanged -= source_OnMuteChanged);
        }
        private void OnApplicationQuit()
        {
            isApplicationQuitting = true;

            mapSourceVsTrack.Values.ForEach(tData => tData.MuteOrigin());
        }
    }



    // Inner Class
    public class TimelineMuteSync_Track
    {
        // Properties
        public bool OriginMuted { get; set; }
        public TrackAsset Track { get; set; }

        // Methods
        public void MuteOrigin()
        {
            LOG.Info($"MuteOrigin()", this);

            Track.muted = OriginMuted;
        }
    }
}