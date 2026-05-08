using beyondi.Util;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace DoDoEng.Common
{
    public static class Extensions
    {
        // Timeline
        public static void MuteTrack(this PlayableDirector tl, params int[] trackIndices)
        {
            // Get track from TimelineAsset
            var timelineAsset = (TimelineAsset)tl.playableAsset;

            // Mute
            trackIndices.ForEach(tIdx => timelineAsset.GetOutputTrack(tIdx).muted = true);

            // Rebuild Graph
            double t = tl.time;
            tl.RebuildGraph();
            tl.time = t;
        }
        public static void UnmuteTrack(this PlayableDirector tl, params int[] trackIndices)
        {
            // Get track from TimelineAsset
            var timelineAsset = (TimelineAsset)tl.playableAsset;

            // Mute
            trackIndices.ForEach(tIdx => timelineAsset.GetOutputTrack(tIdx).muted = false);

            // Rebuild Graph
            double t = tl.time;
            tl.RebuildGraph();
            tl.time = t;
        }
    }
}