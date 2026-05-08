using beyondi.Util;
using UnityEngine;

namespace beyondi.Behaviour
{
    public class BYDLayoutRebuilder : MonoBehaviour
    {
        // Unity Messages
        protected virtual void Awake()
        {
            this.RebuildLayout();
        }
    }
}