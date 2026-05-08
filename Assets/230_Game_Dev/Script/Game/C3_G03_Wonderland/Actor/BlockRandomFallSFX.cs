using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Game.C3_G03
{
    public class BlockRandomFallSFX : MonoBehaviour
    {
        // Properties
        [HideInInspector] public int BlockType = 0;



        // Event Handlers
        public void RandomFallSFX()
        {
            var rnd = Random.Range(0, 3);
            AudioMGR.One.PlayEffect(_FallSFX[BlockType + rnd]);
        }



        // Unity Inspectors
        [Header("★ Sound")]
        [SerializeField] private AudioClip[] _FallSFX = null;
    }
}