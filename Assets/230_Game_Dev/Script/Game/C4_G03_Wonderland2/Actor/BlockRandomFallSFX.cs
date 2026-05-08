using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Game.C4_G03
{
    public class BlockRandomFallSFX : MonoBehaviour
    {
        // Event Handlers
        public void RandomFallSFX()
        {
            var rnd = Random.Range(0, _FallSFX.Length);
            AudioMGR.One.PlayEffect(_FallSFX[rnd]);
        }



        // Unity Inspectors
        [Header("★ Sound")]
        [SerializeField] private AudioClip[] _FallSFX = null;
    }
}