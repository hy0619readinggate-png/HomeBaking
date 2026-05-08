using beyondi.Util;
using UnityEngine;

namespace DoDoEng.Game.C1_G03
{
    public class FlipRandom : MonoBehaviour
    {
        // Unity Messages
        private void OnEnable()
        {
            var flip = UtilRandom.RandomSuccess(0.5f) ? +1 : -1;
            transform.localScale =
                new Vector3(
                    transform.localScale.x * flip,
                    transform.localScale.y,
                    transform.localScale.z);
        }
    }
}