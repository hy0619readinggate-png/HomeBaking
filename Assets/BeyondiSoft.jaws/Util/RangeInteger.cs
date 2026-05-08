using UnityEngine;

namespace beyondi.Util
{
    [System.Serializable]
    public class RangeInteger
    {
        // Properties
        public int Min => min;
        public int Max => max;

        // Methods
        public int RandomOne()
        {
            return UtilArray.RandomOne(min, max);
        }
        public int[] Randome(int count)
        {
            return UtilArray.Random(min, max, count);
        }

        // Methods : ctor.
        public RangeInteger()
        {
        }
        public RangeInteger(int minV, int maxV)
        {
            min = minV; max = maxV;
        }



        // Unity Inspectors
        [SerializeField] private int min = 1;
        [SerializeField] private int max = 2;
    }
}
