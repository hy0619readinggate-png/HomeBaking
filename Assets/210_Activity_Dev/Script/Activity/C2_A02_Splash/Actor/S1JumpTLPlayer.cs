using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

namespace DoDoEng
{
    public class S1JumpTLPlayer : MonoBehaviour
    {
        // Methods
        public void FinishPlaying()
        {
            LOG.Info($"FinishPlaying()", this);

            tl.time = tl.duration;
            tl.Evaluate();
            tl.Stop();
        }
        public IEnumerator PlayAndWait(int characterID)
        {
            LOG.Info($"PlayAndWait() | {characterID}", this);

            muteAll();

            var indices = characterID switch
            {
                1 => ginoInTrackIndices,
                2 => gomaInTrackIndices,
                3 => LeoniInTrackIndices,
                _ => null
            };

            tl.UnmuteTrack(indices);

            tl.time = 0;
            tl.Play();
            yield return new WaitForSeconds((float)tl.duration);
        }



        // Fields : caching
        private PlayableDirector tl_ = null;
        private PlayableDirector tl => tl_ ??= GetComponent<PlayableDirector>();

        // Functions
        private void muteAll()
        {
            tl.MuteTrack(ginoInTrackIndices);
            tl.MuteTrack(gomaInTrackIndices);
            tl.MuteTrack(LeoniInTrackIndices);
        }



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private int[] ginoInTrackIndices = null;
        [SerializeField] private int[] gomaInTrackIndices = null;
        [SerializeField] private int[] LeoniInTrackIndices = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}