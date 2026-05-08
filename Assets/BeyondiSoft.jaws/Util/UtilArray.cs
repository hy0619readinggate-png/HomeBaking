using System;
using System.Collections.Generic;
using System.Linq;

namespace beyondi.Util
{
    public class UtilArray
    {
        public static T[] Shuffle<T>(T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = UnityEngine.Random.Range(0, n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }

            return array;
        }
        public static T[] Shuffled<T>(T[] array)
        {
            var clone = array.Clone() as T[];
            return Shuffle(clone);
        }
        public static T[] Shuffled<T>(T[] src, params T[] firstAvoid)
        {
            var list = new List<T>(src);
            firstAvoid.ForEach(f => list.Remove(f));

            var first = Shuffle(list.ToArray());
            var second = Shuffle(firstAvoid);

            return Enumerable.Concat(first, second).ToArray();
        }

        public static T[] Extract<T>(T[] source, int count, bool shuffle = true)
        {
            var queue = new Queue<T>();
            var extracted = new List<T>();
            for (var i = 0; i < count; i++)
            {
                if (queue.Count == 0)
                {
                    if (shuffle)
                        Shuffled(source).ForEach(c => queue.Enqueue(c));
                    else source.ForEach(c => queue.Enqueue(c));
                }

                if (extracted.Count > 0)    // 순서상 겹치는 문제 해결
                {
                    var last = extracted.LastOrDefault();
                    var peek = queue.Peek();
                    if (EqualityComparer<T>.Default.Equals(last, peek))
                        queue.Enqueue(queue.Dequeue());
                }

                extracted.Add(queue.Dequeue());
            }

            return extracted.ToArray();
        }
        public static T[] ExtractWithRemain<T>(T[] source, int count, out T[] remains)
        {
            var copy = (T[])source.Clone();
            var shuffled = Shuffle(copy);

            var extractCount = Math.Min(count, source.Length);
            var extractList = new List<T>();
            var remainList = new List<T>();

            for (var i = 0; i < source.Length; i++)
            {
                if (i < extractCount)
                    extractList.Add(shuffled[i]);
                else remainList.Add(shuffled[i]);
            }

            remains = remainList.ToArray();
            return extractList.ToArray();
        }
        public static T[] ExtractOnlyInSourceCount<T>(T[] source, int count)
        {
            var shuffled = Shuffled(source);

            var resultCount = Math.Min(count, source.Length);
            var result = new T[resultCount];
            for (var i = 0; i < resultCount; i++)
                result[i] = shuffled[i];

            return result;
        }

        public static T ExtractOne<T>(T[] source)
        {
            var extracted = Extract(source, 1);
            if (extracted.Length > 0)
                return extracted[0];
            else return default(T);
        }
        public static T ExtractOne<T>(T[] source, out T[] remains)
        {
            var extracted = ExtractWithRemain(source, 1, out remains);
            if (extracted.Length > 0)
                return extracted[0];
            else return default(T);
        }

        public static int[] Zero(int count)
        {
            var arr = new int[count];
            for (int i = 0; i < count; i++)
                arr[i] = 0;

            return arr;
        }
        public static int[] SingleValue(int value, int count)
        {
            var arr = new int[count];
            for (int i = 0; i < count; i++)
                arr[i] = value;

            return arr;
        }
        public static int[] Sequential(int s, int fInclusive)
        {
            var count = fInclusive - s + 1;
            var arr = new int[count];
            for (int i = 0; i < count; i++)
                arr[i] = s + i;

            return arr;
        }
        public static int[] Random(int s, int fInclusive)
        {
            var src = Sequential(s, fInclusive);
            return Shuffle(src);
        }
        public static int[] Random(int s, int fInclusive, int extractCount)
        {
            var src = Sequential(s, fInclusive);
            return Extract(src, extractCount);
        }
        public static int[] Random(int s, int fInclusive, int extractCount, int answer)
        {
            var arr1 = Sequential(s, fInclusive);
            var arr2 = Extract(arr1, extractCount);

            var list = new List<int>(arr2);
            if (!list.Contains(answer))
            {
                list.RemoveAt(0);
                list.Add(answer);
                return Shuffle(list.ToArray());
            }
            else return arr2;
        }

        public static int RandomOne(int s, int fInclusive)
        {
            var src = Sequential(s, fInclusive);
            return Shuffle(src)[0];
        }


        public static int[] Distribute(int sourceCount, int count)
        {
            var q = count / sourceCount;
            var r = count % sourceCount;

            // 몫으로 채우기
            var frequencies = UtilArray.SingleValue(q, sourceCount);

            // 나머지를 랜덤하게 나눠주기
            var indicesPool = UtilArray.Sequential(0, sourceCount - 1);
            var indices = UtilArray.Extract(indicesPool, r);

            foreach (var idx in indices)
                frequencies[idx] += 1;

            return frequencies;
        }


        public static List<List<T>> Divide<T>(T[] source, int count)
        {
            List<List<T>> result = new List<List<T>>();
            for (int i = 0; i < count; i++)
                result.Add(new List<T>());

            for (int i = 0; i < source.Length; i++)
                result[i % count].Add(source[i]);

            return result;
        }
        public static T[] Reorder<T>(T[] arr, int[] indices)
        {
            if (arr == null) return null;
            if (arr.Length < 2) return arr;

            var list = new List<T>();
            foreach (var idx in indices)
                list.Add(arr[idx]);
            return list.ToArray();
        }
    }
}
