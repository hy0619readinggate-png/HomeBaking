using UnityEngine;

namespace beyondi.Util
{
    [System.Serializable]
    public class Range
    {
        // Properties
        public float Min => min;
        public float Max => max;

        // Methods
        public float RandomValue()
        {
            return Random.Range(Min, Max);
        }

        // Methods : ctor.
        public Range()
        {
        }
        public Range(float minV, float maxV)
        {
            min = minV; max = maxV;
        }



        // Unity Inspectors
        [SerializeField] private float min = 1;
        [SerializeField] private float max = 2;
    }
}
